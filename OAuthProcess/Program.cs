using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading; 
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
namespace ConsoleApp1
{
    class Program
    {

        static void Main(string[] args)
        {
            
            string[] scopes = new string[] { "https://www.googleapis.com/auth/userinfo.email", "https://mail.google.com" };
            // asking for permission 
            ClientSecrets clientSecrets = new ClientSecrets();
            clientSecrets.ClientId = "1086353785798-4fve0l6mhub9gereqa1g1esjskvoa94i.apps.googleusercontent.com";
            clientSecrets.ClientSecret = "redacted";
            UserCredential userCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets, scopes, "user", CancellationToken.None).Result;
            if (userCredential.Token.IsExpired(userCredential.Flow.Clock))
            {
                bool canRefreshToken = userCredential.RefreshTokenAsync(CancellationToken.None).Result; 
                if (!canRefreshToken)
                {
                    Console.WriteLine("Cannot refresh token.");
                }
            }
            Console.WriteLine("Successfuly authorized.");

        }
    }
}
