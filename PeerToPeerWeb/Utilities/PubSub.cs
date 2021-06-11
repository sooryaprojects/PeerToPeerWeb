using Google.Cloud.PubSub.V1;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PeerToPeerWeb.Utilities
{
    public class PubSub
    {
        private string TopicId;
        private string ProjectId;
        private string SubscriptionId;

        public PubSub(string topicId, string projectId, string subscriptionId)
        {
            TopicId = topicId;
            ProjectId = projectId;
            SubscriptionId = subscriptionId;
        }


        /// <summary>
        /// Publish the message to the topic
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task PublishToTopicAsync(string message)
        {
            try
            {
                var topicName = new TopicName(ProjectId, TopicId);

                PublisherClient publisher = PublisherClient.Create(topicName);

                await publisher.PublishAsync(message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Following error occured while publishing message to Pub/Sub topic: {e.Message}");
            }
		}


        /// <summary>
        /// Pull messages from the topic subscription
        /// </summary>
        /// <returns></returns>
        public async Task<string> PullMessagesAsync()
        {
            SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(ProjectId, SubscriptionId);
            SubscriberClient subscriberClient = await SubscriberClient.CreateAsync(subscriptionName);
            string result = String.Empty;
            Task startTask = subscriberClient.StartAsync((PubsubMessage message, CancellationToken cancel) =>
            {
                result = System.Text.Encoding.UTF8.GetString(message.Data.ToArray());

                return Task.FromResult(true ? SubscriberClient.Reply.Ack : SubscriberClient.Reply.Nack);
            });

            await Task.Delay(5000);

            await subscriberClient.StopAsync(CancellationToken.None);

            await startTask;

            return result;


        }

    }
}
