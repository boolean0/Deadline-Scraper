using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
namespace ConsoleApp2
{
    class Program
    {
        private static bool IsExpired(DateTime date, int expiresInSeconds)
        {
            date.AddSeconds(expiresInSeconds - 120);
            // if the token expires soon, we should still refresh in case the authorization process hangs 
            //and the token expires in that time frame

            // if the expiration date is before the time now, token expired
            return DateTime.Compare(date, DateTime.UtcNow) < 0 ? false : true;
        }

        // Checks if token is expired, and if it is, it refreshes the token
        // Effects: May modify userCredential. If it does, it is done without notice
        private static void RefreshIfNeeded(ref UserCredential userCredential)
        {
            if (userCredential.Token.IsExpired(userCredential.Flow.Clock))
            {
                bool canRefreshToken = userCredential.RefreshTokenAsync(CancellationToken.None).Result;
                if (!canRefreshToken)
                {
                    Console.WriteLine("Cannot refresh token.");
                }
            }
        }
        static void Main(string[] args)
        {
            ClientSecrets clientSecrets = new ClientSecrets();
            clientSecrets.ClientId = "1086353785798-4fve0l6mhub9gereqa1g1esjskvoa94i.apps.googleusercontent.com";
            clientSecrets.ClientSecret = "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";

            FileDataStore dataStore = new FileDataStore("C:\\Users\\Matt.Huang\\AppData\\Roaming\\Google.Apis.Auth", true);
            TokenResponse tk = dataStore.GetAsync<TokenResponse>("user").Result; // this is the important info

            GoogleAuthorizationCodeFlow codeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer { ClientSecrets = clientSecrets });
            UserCredential userCredential = new UserCredential(codeFlow, "user", tk);

            RefreshIfNeeded(ref userCredential);

            var jwtPayload = GoogleJsonWebSignature.ValidateAsync(userCredential.Token.IdToken).Result;
            string fromEmail = jwtPayload.Email;
            string toEmail = "matt.huang@senstar.com";


            // message composition
            MimeMessage email = new MimeMessage();
            email.To.Add(new MailboxAddress("Matt Huang", toEmail));
            email.From.Add(new MailboxAddress("Senstar Test", fromEmail));
            email.Subject = "Test message";
            email.Body = new TextPart("plain")
            {
                Text = "Automated message with OAuth"
            };

            // use auth token
            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                var oauth2 = new SaslMechanismOAuth2(fromEmail, userCredential.Token.AccessToken);
                client.Authenticate(oauth2);
                Console.WriteLine("Authenticated.");
                client.Send(email);
                Console.WriteLine("Sent email. Disconnecting:");
                client.Disconnect(true);
            }


            // send email, DC afterwards


        }
    }
}
