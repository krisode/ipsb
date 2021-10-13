using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPSB.ExternalServices
{

    public interface IPushNotificationService
    {
        Task<string> SendMessage(string title, string body, string topic, Dictionary<String, String> additionalDatas);
    }
    public class PushNotificationService : IPushNotificationService
    {
        public async Task<string> SendMessage(string title, string body, string topic, Dictionary<String, String> additionalDatas)
        {
            var message = new Message()
            {
                Notification = new Notification()
                {
                    Title = title,
                    Body = body,
                    ImageUrl = "https://i.ibb.co/KxpKRXJ/logo.png",
                },
                Topic = "/topics/" + topic,
                Data = additionalDatas,
            };
            var messaging = FirebaseMessaging.DefaultInstance;
            return await messaging.SendAsync(message);
        }
    }
}
