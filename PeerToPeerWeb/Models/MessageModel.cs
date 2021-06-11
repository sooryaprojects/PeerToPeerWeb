using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeerToPeerWeb.Models
{
    public class MessageModel
    {
        public string Message { get; set; }

        public string FileName { get; set; }
    }
}
