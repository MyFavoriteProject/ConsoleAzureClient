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

        public async Task<UserInfo> CreateUserAsync(UserCreate userCreate)
        {
            string password = userCreate.Password ?? _passwordGenerator.GeneratePassword();

            /// Формирования объекта для создание пользователя
            User newUser = new User
            {
                DisplayName = userCreate.DisplayName,
                MailNickname = userCreate.MailNickname,
                UserPrincipalName = userCreate.UserPrincipalName,
                AccountEnabled = userCreate.AccountEnabled ?? true,
                PasswordProfile = new PasswordProfile()
                {
                    Password = password,
                    ForceChangePasswordNextSignIn = userCreate.ForceChangePasswordNextSignIn ?? true
                }
            }; 

            User user = await _graphClient.Users
                .Request()
                .AddAsync(newUser);

            /// Формирования объекта учётных данных пользователя
            UserInfo userInfo = new UserInfo
            {
                Id = user.Id,
                UserPrincipalName = user.UserPrincipalName,
                Password = password,
            }; 

            return userInfo;
        }

        public async Task UpdateUserAsync(UserInfo userInfo)
        {
            string userId = userInfo.Id;

            if (userInfo.AccountEnabled.HasValue)
            {
                await SetAccountEnabledAsync(userId, userInfo.AccountEnabled.Value);
            }

            if (userInfo.AboutMe != null || userInfo.Birthday.HasValue || userInfo.HireDate.HasValue ||
                userInfo.Interests != null || userInfo.MySite != null || userInfo.PastProjects != null ||
                userInfo.Responsibilities != null || userInfo.Schools != null || userInfo.Skills != null)
            {
                await UpdateUserAdditionalProps(userInfo);
            }

            User user = new User
            {
                UserPrincipalName = userInfo.UserPrincipalName,
                DisplayName = userInfo.DisplayName,
                MailNickname = userInfo.MailNickname,
                GivenName = userInfo.GivenName,
                BusinessPhones = userInfo.BusinessPhones,
                PasswordPolicies = userInfo.PasswordPolicies.ToString()
            };

            await _graphClient.Users[userId]
                .Request()
                .UpdateAsync(user);
        }
        
        public async Task<UserInfo> GetUserInfo(string userId)
        {
            User userStandardInfo = await _graphClient.Users[userId]
                .Request().GetAsync();
            User userAdditionalInfo = await GetUserAdditionalInfoAsync(userId);
            PasswordPolicies passwordPolicies = await GetUserPasswordPoliciesAsync(userId);

            return new UserInfo()
            {
                Id = userStandardInfo.Id,
                UserPrincipalName = userStandardInfo.UserPrincipalName,
                DisplayName = userStandardInfo.DisplayName,
                MailNickname = userStandardInfo.MailNickname,
                GivenName = userStandardInfo.GivenName,
                PasswordPolicies = passwordPolicies,
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

        public async Task<string> ResetUserPasswordAsync(string userId)
        {
            string password = _passwordGenerator.GeneratePassword(); 

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

        public async Task<string> CreateGroupAsync(GroupCreate groupCreate)
        {
            Group newGroup = new Group
            {
                DisplayName = groupCreate.DisplayName,
                MailNickname = groupCreate.MailNickname,
                MailEnabled = groupCreate.MailEnabled,
                SecurityEnabled = groupCreate.SecurityEnabled,
                GroupTypes = groupCreate.GroupTypes
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
        
        /// Возвращает SharePoint Online свойства 
        private async Task<User> GetUserAdditionalInfoAsync(string userId)
        {
            return await _graphClient.Users[userId].Request()
                .Select(
                    "aboutMe, birthday, hireDate, interests, mySite, pastProjects, preferredName, responsibilities, schools, skills"
                )
                .GetAsync();
        }

        private async Task<PasswordPolicies> GetUserPasswordPoliciesAsync(string userId)
        {
            User user = await _graphClient.Users[userId]
                .Request()
                .Select("passwordPolicies")
                .GetAsync();

            if (user.PasswordPolicies == PasswordPolicies.DisablePasswordExpiration.ToString())
            {
                return PasswordPolicies.DisablePasswordExpiration;
            }

            if (user.PasswordPolicies == PasswordPolicies.DisableStrongPassword.ToString())
            {
                return PasswordPolicies.DisableStrongPassword;
            }

            return PasswordPolicies.NaN;
        }
        
        private async Task SetAccountEnabledAsync(string userId, bool accountEnabled)
        {
            User user = new User
            {
                AccountEnabled = accountEnabled
            };
            await _graphClient.Users[userId]
                .Request()
                .UpdateAsync(user);
        }

        private async Task UpdateUserAdditionalProps(UserInfo userInfo)
        {
            string userId = userInfo.Id;

            User user = new User
            {
                AboutMe = userInfo.AboutMe,
                Birthday = userInfo.Birthday,
                HireDate = userInfo.HireDate,
                Interests = userInfo.Interests,
                MySite = userInfo.MySite,
                PastProjects = userInfo.PastProjects,
                Responsibilities = userInfo.Responsibilities,
                Schools = userInfo.Schools,
                Skills = userInfo.Skills
            };

            await _graphClient.Users[userId]
                .Request()
                .UpdateAsync(user);
        }
    }
}
