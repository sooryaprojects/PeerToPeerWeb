using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PeerToPeerWeb.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Newtonsoft.Json;
using PeerToPeerWeb.Utilities;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;

namespace PeerToPeerWeb.Controllers
{
    public class MessageController : Controller
    {
        private readonly ILogger<MessageController> _logger;
        private readonly IConfiguration _config;
        private GoogleCredential googleCredential;
        private PubSub pubSub;
        private CloudStorage cloudStorage;

        public MessageController(IConfiguration config ,ILogger<MessageController> logger)
        {
            _logger = logger;
            _config = config;
            pubSub = new PubSub( config.GetValue(typeof(string), "AppSettings:PubSubTopicId").ToString(), config.GetValue(typeof(string), "AppSettings:CloudProjectId").ToString(), config.GetValue(typeof(string), "AppSettings:PubSubSubscriptionId").ToString());
            cloudStorage = new CloudStorage(config.GetValue(typeof(string), "AppSettings:CloudStorageBucketName").ToString(), config.GetValue(typeof(string), "AppSettings:CloudProjectId").ToString());
        }


        public IActionResult Index()
        {

                return View();            
        }


        /// <summary>
        /// Return Message, needs to be signed in to excess the page
        /// </summary>
        /// <returns></returns>
        [GoogleScopedAuthorize("https://www.googleapis.com/auth/userinfo.email")]
        public IActionResult Message([FromServices] IGoogleAuthProvider auth)
        {
            //we can get token from the googel credentials we got
            googleCredential = auth. GetCredentialAsync().Result;
            
        
            return View(User);
        }

        /// <summary>
        /// Action to Compose a message
        /// </summary>
        /// <returns></returns>
        [GoogleScopedAuthorize("https://www.googleapis.com/auth/userinfo.email")]
        public IActionResult Compose()
        {
            return View();
        }

        /// <summary>
        /// ACtion to Watch the Queue
        /// </summary>
        /// <returns></returns>
        [GoogleScopedAuthorize("https://www.googleapis.com/auth/userinfo.email")]
        public IActionResult Fetch()
        {
            FetchMessage().Wait();
            return View();
        }

        /// <summary>
        /// Call the pub sub and fetch the message
        /// </summary>
        /// <returns></returns>
        public async Task FetchMessage()
        {
            try
            {
                var data = await pubSub.PullMessagesAsync();
                var message = JsonConvert.DeserializeObject<MessageModel>(data);
                ViewData["Data"] = message.Message;
                ViewData["Link"] = $"https://storage.cloud.google.com/peertopeermessaging/{message.FileName}";
            }
            catch(Exception e)
            {
                Console.WriteLine($"Following error occurred while fetching message from queue: {e.Message}");
            }
        }

        /// <summary>
        /// Redirect to the File Link
        /// </summary>
        /// <returns></returns>
        public IActionResult SeeContent()
        {
            return Redirect(ViewData["link"].ToString());
        }


        /// <summary>
        /// Send Message on Submit
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SendMessage(MessageModel messageModel)
        {
            try
            {
                var objectName = Guid.NewGuid().ToString();
                var file = Request.Form.Files[0];
                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.GetFullPath(fileName);
                var fileContentType = file.ContentType;
                messageModel.FileName = objectName;
                pubSub.PublishToTopicAsync(JsonConvert.SerializeObject(messageModel)).Wait();

                using (var fileStream = System.IO.File.Create(fileName))
                {
                    file.CopyTo(fileStream);
                }

                cloudStorage.UploadObject(objectName, filePath, fileContentType).Wait();

                //Delete the Server copy of the file
                System.IO.File.Delete(fileName);

                return RedirectToAction("Compose");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction("Compose");
            }
        }


        /// <summary>
        /// Sign Out and return to the main screen
        /// </summary>
        /// <returns></returns>
        public IActionResult SignOut([FromServices] IGoogleAuthProvider auth)
        {
            SignOutCurrentUser(auth).Wait();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Helper function To SIgnOut the current User
        /// </summary>
        public async Task SignOutCurrentUser(IGoogleAuthProvider auth)
        {
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
