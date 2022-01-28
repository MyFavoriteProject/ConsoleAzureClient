using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Models;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Azure
{
    public class AzureClient
    {
        private readonly GraphServiceClient _graphClient;
        private readonly PasswordGenerator _passwordGenerator;

        public AzureClient()
        {
            _passwordGenerator = new PasswordGenerator();

            string tenantId = "5c66821f-81e8-4faa-a800-b3fa3f2e27c0";
            string clientId = "31d2d6a6-f6ff-4fcc-960b-e50979fe69d8";
            //string clientSecret = "Djn7Q~PUmHBCBxkRJbYiMeXNXGYtb_IBNk0sY";

            string[] scopes = new[]
            {
                "https://graph.microsoft.com/.default",
            };

            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            Func<DeviceCodeInfo, CancellationToken, Task> callback = (code, cancellation) =>
            {
                Console.WriteLine(code.Message);
                return Task.FromResult(0);
            };

            var deviceCodeCredential = new DeviceCodeCredential(
                callback, tenantId, clientId, options);

            _graphClient = new GraphServiceClient(deviceCodeCredential, scopes);
        }
        
        public async Task<UserCredentials> CreateUser(string displayName, string mailNickname, string userPrincipalName)
        {
            string password = _passwordGenerator.GetPassword();
            User newUser = new User
            {
                DisplayName = displayName,
                MailNickname = mailNickname,
                UserPrincipalName = userPrincipalName,
                AccountEnabled = true,
                PasswordProfile = new PasswordProfile()
                {
                    Password = password,
                    ForceChangePasswordNextSignIn = true
                }
            };

            User user = await _graphClient.Users.Request().AddAsync(newUser);

            UserCredentials result = new UserCredentials
            {
                Id = user.Id,
                Password = password,
            };

            return result;
        }

        public async Task UpdateUser(string userId, Dictionary<string, object> propertyValueDictionary)
        {
            User user = new User();
            var userType = typeof(User);

            foreach (var propValue in propertyValueDictionary)
            {
                userType.GetProperty(propValue.Key)?.SetValue(user, propValue.Value);
            }

            await _graphClient.Users[userId].Request().UpdateAsync(user);
        }

        public async Task<string> ResetUserPassword(string userId)
        {
            string password = _passwordGenerator.GetPassword();

            User user = new User()
            {
                PasswordProfile = new PasswordProfile()
                {
                    Password = password,
                    ForceChangePasswordNextSignIn = true
                }
            };

            await _graphClient.Users[userId].Request().UpdateAsync(user);

            return password;
        }

        public async Task SetAccountEnabled(string userId, bool accountEnabled)
        {
            User user = new User
            {
                AccountEnabled = accountEnabled
            };

            await _graphClient.Users[userId].Request().UpdateAsync(user);
        }

        public async Task<UserSharePoint> GetUserSharePoint(string userId)
        {
            User user = await _graphClient.Users[userId].Request()
                .Select(
                    "aboutMe, birthday, hireDate, interests, mySite, pastProjects, preferredName, responsibilities, schools, skills")
                .GetAsync();

            UserSharePoint userSharePoint = new UserSharePoint()
            {
                AboutMe = user.AboutMe,
                Birthday = user.Birthday,
                HireDate = user.HireDate,
                Interests = user.Interests,
                MySite = user.MySite,
                PastProjects = user.PastProjects,
                PreferredName = user.PreferredName,
                Responsibilities = user.Responsibilities,
                Schools = user.Schools,
                Skills = user.Skills
            };
            
            return userSharePoint;
        }

        public async Task<string> GetUserPasswordPolicies(string userId)
        {
            User user = await _graphClient.Users[userId].Request().Select("passwordPolicies").GetAsync();

            return user.PasswordPolicies;
        }

        public async Task DeleteUser(string userId)
        {
             await _graphClient.Users[userId].Request().DeleteAsync();
        }

        public async Task<string> RestoreUser(string userId)
        {
            DirectoryObject user = await _graphClient.Directory.DeletedItems[userId].Restore().Request().PostAsync();

            return user.Id;
        }

        public async Task<string> GreateGroup(string displayName, string mailNickname, bool mailEnabled, bool securityEnabled)
        {
            Group newGroup = new Group
            {
                DisplayName = displayName,
                MailNickname = mailNickname,
                MailEnabled = mailEnabled,
                SecurityEnabled = securityEnabled,
            };

            Group group = await _graphClient.Groups.Request().AddAsync(newGroup);
            return group.Id;
        }

        public async Task AddMemberInGroup(string groupId, string userId)
        {
            DirectoryObject directoryObject = new DirectoryObject
            {
                Id = userId
            };

            await _graphClient.Groups[groupId].Members.References.Request().AddAsync(directoryObject);
        }

        public async Task RemoveMemberFromGroup(string groupId, string userId)
        {
            DirectoryObject directoryObject = new DirectoryObject
            {
                Id = userId
            };

            await _graphClient.Groups[groupId].Members[userId].Request().DeleteAsync();
        }
    }
}
