using Azure.Identity;
using Microsoft.Graph;

namespace MailContactForm.Data
{
    public class Register
    {
        private static IConfigurationRoot? _configuration;
        private static GraphServiceClient? _graphClient;
        //string newUserId = string.Empty;

        public Register(IConfiguration configuration)
        {
            _configuration = (IConfigurationRoot?)configuration;

        }
        public async Task InitialRegister(Settings settings)
        {

            var scopes = new[] { "User.Read" };
            var interactiveBrowserCredentialOptions = new InteractiveBrowserCredentialOptions
            {
                ClientId = settings.ClientId, //_configuration["settings:clientId"],
                TenantId = settings.TenantId
            };
            var tokenCredential = new InteractiveBrowserCredential(interactiveBrowserCredentialOptions);

            // GraphServiceClient constructor accepts tokenCredential
             _graphClient = new GraphServiceClient(tokenCredential, scopes);
             
        }

        public async Task<(string Id, string Email)> GetUserIdAndEmail()
        {
            var me = await _graphClient.Me
                .GetAsync();

            return (me?.Id ?? string.Empty, me?.Mail ?? string.Empty);
        }


        public async Task UpdateUserId(string userID, string userEmail)
        {
            _configuration.GetSection("settings:userID").Value = userID;
            _configuration.GetSection("settings:userEmail").Value = userEmail;
            _configuration.Reload();
        }

        /*public string GetUserId()
        {
            return _configuration["settings:userID"];
        }*/
    }
}
