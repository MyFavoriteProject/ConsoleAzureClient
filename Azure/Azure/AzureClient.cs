using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Models;
using Microsoft.Graph;

namespace Azure
{
    public class AzureClient
    {
        private readonly GraphServiceClient _graphClient;
        private readonly PasswordGenerator _passwordGenerator;

        public AzureClient()
        {
            _passwordGenerator = new PasswordGenerator();
            _graphClient = GetGraphServiceClient();
        }

        public async Task<UserCredential> CreateUserAsync(string displayName, string mailNickname, string userPrincipalName)
        {
            string password = _passwordGenerator.GetPassword();

            /// Формирования объекта для создание пользователя
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

            User user = await _graphClient.Users
                .Request()
                .AddAsync(newUser);

            /// Формирования объекта для обращения к созданому пользователю
            UserCredential userCredential = new UserCredential
            {
                Id = user.Id,
                UserPrincipalName = user.UserPrincipalName,
                Password = password,
            }; 

            return userCredential;
        }

        public async Task UpdateUserAsync(string userId, Dictionary<string, object> propNameByValueDictionary)
        {
            User user = new User();

            /// Получение информации о типе для сетинга свойств 
            Type userType = typeof(User);

            /// Формирования объекта User по имени свойства и значению в словаре
            foreach (KeyValuePair<string, object> propNameValue in propNameByValueDictionary) 
            {
                userType.GetProperty(propNameValue.Key)
                    ?.SetValue(user, propNameValue.Value);
            }
            
            await _graphClient.Users[userId]
                .Request()
                .UpdateAsync(user);
        }
        
        public async Task<string> ResetUserPasswordAsync(string userId)
        {
            /// Генерация пароля 10 символов 
            string password = _passwordGenerator.GetPassword(); 

            User user = new User
            {
                PasswordProfile = new PasswordProfile
                {
                    Password = password,
                    ForceChangePasswordNextSignIn = true
                }
            };

            await _graphClient.Users[userId]
                .Request()
                .UpdateAsync(user);

            return password;
        }
        
        public async Task SetAccountEnabledAsync(string userId, bool accountEnabled)
        {
            User user = new User
            {
                AccountEnabled = accountEnabled
            };
            await _graphClient.Users[userId]
                .Request()
                .UpdateAsync(user);
        }

        /// Получает SharePoint Online свойства 
        public async Task<UserAdditionalInfo> GetUserAdditionalInfoAsync(string userId) 
        {
            User user = await _graphClient.Users[userId].Request()
                .Select(
                    "aboutMe, birthday, hireDate, interests, mySite, pastProjects, preferredName, responsibilities, schools, skills")
                .GetAsync();

            UserAdditionalInfo userAdditionalInfo = new UserAdditionalInfo()
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

            return userAdditionalInfo;
        }

        public async Task<string> GetUserPasswordPoliciesAsync(string userId)
        {
            User user = await _graphClient.Users[userId]
                .Request()
                .Select("passwordPolicies")
                .GetAsync();

            return user.PasswordPolicies;
        }

        public async Task DeleteUserAsync(string userId)
        {
            await _graphClient.Users[userId]
                .Request()
                .DeleteAsync();
        }

        public async Task RestoreUserAsync(string userId)
        {
            await _graphClient.Directory.DeletedItems[userId]
                .Restore()
                .Request()
                .PostAsync();
        }

        public async Task<string> CreateGroupAsync(string displayName, string mailNickname, bool mailEnabled,
            bool securityEnabled)
        {
            Group newGroup = new Group
            {
                DisplayName = displayName,
                MailNickname = mailNickname,
                MailEnabled = mailEnabled,
                SecurityEnabled = securityEnabled,
                GroupTypes = new List<string>
                {
                    "Unified" /// Указывает на Microsoft 365 group
                },
            };

            Group group = await _graphClient.Groups
                .Request()
                .AddAsync(newGroup);
            return group.Id;
        }

        public async Task AddMemberInGroupAsync(string groupId, string userId)
        {
            DirectoryObject user = new DirectoryObject
            {
                Id = userId
            };
            
            await _graphClient.Groups[groupId].Members.References
                .Request()
                .AddAsync(user);
        }

        public async Task RemoveMemberFromGroupAsync(string groupId, string userId)
        {
            await _graphClient.Groups[groupId].Members[userId].Reference
                .Request()
                .DeleteAsync();
        }

        public async Task<string> GetPhotoHashAsync(string userId)
        {
            /// Получение информации о фото
            ProfilePhoto profilePhoto = await _graphClient.Users[userId].Photo
                .Request()
                .GetAsync(); 

            /// По @odata.mediaEtag получаем хеш тип которого JsonElement
            JsonElement? value = profilePhoto.AdditionalData["@odata.mediaEtag"] as JsonElement?; 

            return value?.ToString();
        }

        public async Task<byte[]> GetUserPhotoAsync(string userId)
        {
            Stream stream = await _graphClient.Users[userId].Photo.Content
                .Request()
                .GetAsync();

            /// MemoryStream было использовано для получения byte[]
            await using MemoryStream memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray(); 
        }

        public async Task UpdateUserPhotoAsync(string userId, byte[] imageBytes)
        {
            /// Приведение массива байтов к Stream
            await using MemoryStream stream = new MemoryStream(imageBytes); 

            await _graphClient.Users[userId].Photo.Content
                .Request()
                .PutAsync(stream);
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

            return new GraphServiceClient(clientSecretCredential, scopes);
        }
    }
}
