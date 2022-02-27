using Azure.Identity;
using Azure.Models;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Azure
{
    public class AzureClient2
    {
        private readonly GraphServiceClient graphClient;

        public AzureClient2()
        {
            graphClient = GetGraphServiceClient();
        }

        public async Task<List<UserInfo>> GetUsersInfo(List<string> usersId)
        {
            List<UserInfo> result = new List<UserInfo>();

            for(int i = 0; i < usersId.Count; i++)
            {
                try
                {
                    UserInfo userInfo = await GetUserInfo(usersId[i]);

                    result.Add(userInfo);
                }
                catch(Exception ex)
                {

                }
            }

            return result;
        }

        public async Task<List<UserHash>> GetUsersHash(List<string> usersId)
        {
            List<UserHash> result = new List<UserHash>();
            List<string> requestUsersId = new List<string>();

            for(int i = 0; i < usersId.Count; i++)
            {
                requestUsersId.Add(usersId[i]);

                if(requestUsersId.Count == 20)
                {
                    //// TO DO
                    ///

                    requestUsersId.Clear();
                }
            }


            return result;
        }

        public async Task<UserInfo> GetUserInfo(string userId)
        {
            User userAdditionalInfo = await graphClient.Users[userId]
                .Request()
                .Select("aboutMe, birthday, hireDate, interests, mySite, pastProjects, preferredName, responsibilities, schools, skills, id")
                .GetAsync();
            return new UserInfo()
            {
                Id = userAdditionalInfo.Id,
                AboutMe = userAdditionalInfo.AboutMe,
                Birthday = userAdditionalInfo.Birthday,
                HireDate = userAdditionalInfo.HireDate,
                Interests = userAdditionalInfo.Interests,
                MySite = userAdditionalInfo.MySite,
                PastProjects = userAdditionalInfo.PastProjects,
                PreferredName = userAdditionalInfo.PreferredName,
                Responsibilities = userAdditionalInfo.Responsibilities,
                Schools = userAdditionalInfo.Schools,
                Skills = userAdditionalInfo.Skills
            };
        }

        private async Task<List<UserHash>> GetUsersHashByBatch(List<string> usersId)
        {
            List<UserHash> result = new List<UserHash>();
            Dictionary<string, string> userIdByRequestIdDictionary = new Dictionary<string, string>();

            BatchRequestContent batchRequestContent = new BatchRequestContent();

            foreach(string userId in usersId)
            {
                IProfilePhotoRequest hashRequest = graphClient.Users[userId].Photo.Request();

                string hashRequestId = batchRequestContent.AddBatchRequestStep(hashRequest);

                userIdByRequestIdDictionary.Add(userId, hashRequestId);
            }

            BatchResponseContent returnedResponse =
                await graphClient.Batch.Request().PostAsync(batchRequestContent);

            foreach(KeyValuePair<string, string> keyValuePair in userIdByRequestIdDictionary)
            {
                try
                {

                }
                catch(Exception ex)
                {

                }
            }

            return result;
        }

        private GraphServiceClient GetGraphServiceClient()
        {
            const string tenantId = "5c66821f-81e8-4faa-a800-b3fa3f2e27c0";
            const string clientId = "31d2d6a6-f6ff-4fcc-960b-e50979fe69d8";
            const string clientSecret = "f2i7Q~JLJlWDGmfUcEnjpEbGlg3~OaTSEvkBa";

            /// "/.default" подтягивает perissions приложения
            string[] scopes = { "https://graph.microsoft.com/.default" };

            TokenCredentialOptions options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            ClientSecretCredential clientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret, options);

            TokenCredentialAuthProvider authProvider = new TokenCredentialAuthProvider(
                clientSecretCredential, scopes);

            List<DelegatingHandler> handlers = new List<DelegatingHandler>
            {
                new Handlers.ResponseHandler(),
                new AuthenticationHandler(authProvider),
                new CompressionHandler(),
                //new RetryHandler(),
                new RedirectHandler(),
            };

            HttpClient httpClient = GraphClientFactory.Create(handlers);

            return new GraphServiceClient(httpClient);
        }

    }
}
