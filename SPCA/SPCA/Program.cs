using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Net.Mail;
using System.IO;
using System.Threading;

namespace GmailQuickstart
{
    class Program
    {
        static string[] Scopes = 
            {
            GmailService.Scope.GmailSend,
            GmailService.Scope.GmailCompose,
        };
        static string ApplicationName = "SPCA Email sender";

        static void Main(string[] args)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/gmail-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");

            //Custom Code
            var msg = new AE.Net.Mail.MailMessage
            {
                Subject = "SPCA Notification",
                Body = "HTML Page content",
                From = new MailAddress("spcamd@gmail.com")
            };
            Console.WriteLine("Enter the destination email: ");
            string to = Console.ReadLine();
            msg.To.Add(new MailAddress(to));
            msg.ReplyTo.Add(msg.From);
            var msgStr = new StringWriter();
            msg.Save(msgStr);
            var send = service.Users.Messages.Send(new Message
            {
                Raw = Base64UrlEncode(msgStr.ToString())
            }, "me").Execute();
            Console.WriteLine("Notification sent. Email ID: " + send.Id);
            Console.ReadLine();

            /*
            The input data would come from a c# webservice through the html form 
            and finally rendered through another html page
            */
        }

        private static string Base64UrlEncode(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes).Replace('+', '-').Replace('/', '_').Replace("=", "");
        }
    }
}