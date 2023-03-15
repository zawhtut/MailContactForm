using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

namespace MailContactForm.Data
{
    public class GraphHelper
    {
        // Settings object
        private static Settings? _settings;
        // App-ony auth token credential
        private static ClientSecretCredential? _clientSecretCredential;
        // Client configured with app-only authentication
        public static GraphServiceClient? _appClient;

        public static async Task InitializeGraphForAppOnlyAuth(Settings settings)
        {
            _settings = settings;

            // Ensure settings isn't null
            _ = settings ??
                throw new System.NullReferenceException("Settings cannot be null");

            
            if (_clientSecretCredential == null)
            {
                
                var options = new TokenCredentialOptions
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                };
                _clientSecretCredential = new ClientSecretCredential(
                    _settings.TenantId, _settings.ClientId, _settings.ClientSecret, options);
            }

            if (_appClient == null)
            {
                

                _appClient = new GraphServiceClient(_clientSecretCredential,
                    // Use the default scope, which will request the scopes
                    // configured on the app registration
                    new[] { "https://graph.microsoft.com/.default" });
            }
        }

        public static async Task<string> GetAppOnlyTokenAsync()
        {
            // Ensure credential isn't null
            _ = _clientSecretCredential ??
                throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

            // Request token with given scopes
            var context = new TokenRequestContext(new[] { "https://graph.microsoft.com/.default" });
            var response = await _clientSecretCredential.GetTokenAsync(context);
            return response.Token;
        }

        public async Task SendMailAsync(string subject, string body)
        {
            // Ensure client isn't null
            _ = _appClient ??
                throw new System.NullReferenceException("Graph has not been initialized for user auth");


            var requestBody = new SendMailPostRequestBody
            {
                Message = new Message
                {
                    Subject = subject,
                    Body = new ItemBody
                    {
                        Content = body,
                        ContentType = BodyType.Text,
                    },
                    ToRecipients = new List<Recipient>()
                    {
                        new Recipient
                        {
                            EmailAddress = new EmailAddress
                            {
                                Address = _settings.UserEmail
                            }
                        }
                    }
                },
                SaveToSentItems = true
            };

            // GET https://graph.microsoft.com/v1.0/me/messages?$select=subject,sender&$filter=<some condition>&orderBy=receivedDateTime

            try
            {
                await _appClient
                    .Users[_settings.UserId] //45c0e3e1-cea8-44bd-ba00-fe465c72c437 //c9c76c89-c0f7-4477-8c90-2c69238a96d2"
                    .SendMail
                    .PostAsync(requestBody);

                //Console.WriteLine(messages);
            }
            catch (ODataError odataError)
            {
                Console.WriteLine(odataError.Error.Code);
                Console.WriteLine(odataError.Error.Message);
                throw;
            }


        }

    }
}
