using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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


        public async Task GetUsers(List<string> ids)
        {
            List<string> types = new List<String>()
            {
                "user"
            };

            IGraphServiceUsersCollectionPage users = await _graphClient.Users //.DirectoryObjects
                //.GetByIds(userIds, types)
                .Request()
                //.Select("id, photo")
                .Expand("photo")
                //.PostAsync();
                .GetAsync();
        }

        public async Task<UserInfo> CreateUserAsync(UserCreate userCreate)
        {
            string password = userCreate.Password;

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

        public async Task<List<string>> GetUserIds()
        {
            List<string> userIds = new List<string>();
            IGraphServiceUsersCollectionPage users = await _graphClient.Users
                .Request()
                .GetAsync();

            userIds.AddRange(users.Select(c => c.Id).ToList());

            while (users.NextPageRequest != null)
            {
                users = await users.NextPageRequest.GetAsync();
                userIds.AddRange(users.Select(c => c.Id).ToList());
            }

            return userIds;
        }

        public async Task<List<string>> GetUserIds1()
        {
            List<string> userIds = new List<string>();
            IGraphServiceUsersCollectionPage users = await _graphClient.Users
                .Request()
                .GetAsync();

            userIds.AddRange(users.Select(c => c.Id).ToList());

            //var result = String.Join(", ", userIds.ToArray());

            //while (users.NextPageRequest != null)
            //{
            //    users = await users.NextPageRequest.GetAsync();
            //    userIds.AddRange(users.Select(c => c.Id).ToList());
            //}

            return userIds;
        }

        public async Task<UserInfo> GetUserInfo(string userId)
        {
            User userAdditionalInfo = await GetUserAdditionalInfoAsync(userId);

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


        public void GetUserInfo2(string userId)
        {
            GetUserAdditionalInfoAsync1(userId);
        }

        public async Task<UserInfo> GetUserInfo1(string userId)
        {
            User userAdditionalInfo = await GetUserAdditionalInfoAsync(userId);

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

        public async Task UpdateUserAsync(List<UserInfo> userInfos)
        {
            List<string> requestIds = new List<string>();

            BatchRequestContent batchRequestContent = new BatchRequestContent();

            foreach (UserInfo userInfo in userInfos)
            {
                //User user = new User
                //{
                //    AboutMe = userInfo.AboutMe,
                //    Birthday = userInfo.Birthday,
                //    HireDate = userInfo.HireDate,
                //    Interests = userInfo.Interests,
                //    MySite = userInfo.MySite,
                //    PastProjects = userInfo.PastProjects,
                //    Responsibilities = userInfo.Responsibilities,
                //    Schools = userInfo.Schools,
                //    Skills = userInfo.Skills
                //};

                User user = new User
                {
                    BusinessPhones = new List<string> { "(057)937-99-92" },
                    GivenName = "PVP",
                };

                HttpContent jsonUser = _graphClient.HttpProvider.Serializer.SerializeAsJsonContent(user);
                
                HttpRequestMessage request = _graphClient.Users[userInfo.Id].Request().GetHttpRequestMessage();
                request.Method = HttpMethod.Patch;
                request.Content = jsonUser;
                request.Headers.Add("x-ms-throttle-priority", "Normal");

                string requestId = batchRequestContent.AddBatchRequestStep(request);

                requestIds.Add(requestId);
            }
            
            BatchResponseContent returnedResponse = await _graphClient.Batch.Request().PostAsync(batchRequestContent);

            try
            {
                foreach (string requestId in requestIds)
                {
                    HttpResponseMessage response = await returnedResponse.GetResponseByIdAsync(requestId);

                    if (!response.IsSuccessStatusCode)
                    {

                    }
                }
            }
            catch (ServiceException ex)
            {
                if (ex.Message.Contains(" 429 "))
                {
                    throw new HttpRequestException(ex.Message, ex.InnerException, HttpStatusCode.TooManyRequests);
                }
                //Console.WriteLine($"UserId: {requestId.UserId}");
                Console.WriteLine($"Get user failed: {ex.Error.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task CreateUserAsync(List<UserCreate> userCreates)
        {
            List<string> requestIds = new List<string>();
            
            BatchRequestContent batchRequestContent = new BatchRequestContent();

            foreach (UserCreate userCreate in userCreates)
            {
                PasswordProfile passwordProfile = new PasswordProfile()
                {
                    Password = userCreate.Password,
                    ForceChangePasswordNextSignIn = userCreate.ForceChangePasswordNextSignIn ?? true
                };


                string jsonPasswordProfile = JsonSerializer.Serialize(passwordProfile);

                List<QueryOption> queryOptions = new List<QueryOption>
                {
                    new QueryOption("displayName", userCreate.DisplayName),
                    new QueryOption("mailNickname", userCreate.MailNickname),
                    new QueryOption("userPrincipalName", userCreate.UserPrincipalName),
                    new QueryOption("accountEnabled", userCreate.AccountEnabled.ToString()),
                    new QueryOption("passwordProfile", jsonPasswordProfile),
                };

                IGraphServiceUsersCollectionRequest request = _graphClient.Users.Request(queryOptions);

                string requestId = batchRequestContent.AddBatchRequestStep(request);

                requestIds.Add(requestId);
            }

            BatchResponseContent returnedResponse = await _graphClient.Batch.Request().PostAsync(batchRequestContent);

            try
            {
                foreach (string requestId in requestIds)
                {
                    HttpResponseMessage response = await returnedResponse.GetResponseByIdAsync(requestId);
                }
            }
            catch (ServiceException ex)
            {
                if (ex.Message.Contains(" 429 "))
                {
                    throw new HttpRequestException(ex.Message, ex.InnerException, HttpStatusCode.TooManyRequests);
                }
                //Console.WriteLine($"UserId: {requestId.UserId}");
                Console.WriteLine($"Get user failed: {ex.Error.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        public async Task<List<UserInfo>> GetUsersInfoAsync2(List<string> userIds)
        {
            List<string> requestIds = new List<string>();
            List<UserInfo> userInfos = new List<UserInfo>();

            BatchRequestContent batchRequestContent = new BatchRequestContent();
            
            foreach (string id in userIds)
            {
                HttpRequestMessage request = _graphClient.Users[id].Request()
                    //.Select("id, givenName, businessPhones").GetHttpRequestMessage();
                    //.Select(
                    //    "aboutMe, birthday, hireDate, interests, mySite, pastProjects, preferredName, responsibilities, schools, skills, id")
                    .Expand("manager")
                    .GetHttpRequestMessage();

                request.Headers.Add("x-ms-throttle-priority", "Normal"); 
                request.Method = HttpMethod.Get;

                string requestId = batchRequestContent.AddBatchRequestStep(request);

                requestIds.Add(requestId);
            }

            BatchResponseContent returnedResponse = await _graphClient.Batch.Request().PostAsync(batchRequestContent);
            
            foreach (string requestId in requestIds)
            {
                try
                {
                    User user = await returnedResponse
                        .GetResponseByIdAsync<User>(requestId);

                    UserInfo userInfo = new UserInfo()
                    {
                        Id = user.Id,
                        AboutMe = user.AboutMe,
                        Birthday = user.Birthday,
                        HireDate = user.HireDate,
                        Interests = user.Interests,
                        MySite = user.MySite,
                        PastProjects = user.PastProjects,
                        PreferredName = user.PreferredName,
                        Responsibilities = user.Responsibilities,
                        Schools = user.Schools,
                        Skills = user.Skills,
                        GivenName = user.GivenName,
                        BusinessPhones = user.BusinessPhones
                    };

                    userInfos.Add(userInfo);
                }
                catch (ServiceException ex)
                {
                    if (ex.Message.Contains(" 429 "))
                    {
                        throw new HttpRequestException(ex.Message, ex.InnerException, HttpStatusCode.TooManyRequests);
                    }
                    //Console.WriteLine($"UserId: {requestId.UserId}");
                    Console.WriteLine($"Get user failed: {ex.Error.Message}");
                }
            }

            return userInfos;
        }


        public async Task<List<UserInfoAndPhotoHash>> GetUsersInfoAsync3(List<string> userIds)
        {
            List<UserIdAndRequestIds> requestIds = new List<UserIdAndRequestIds>();
            List<UserInfoAndPhotoHash> userInfoAndPhotoHashs = new List<UserInfoAndPhotoHash>();

            BatchRequestContent batchRequestContent = new BatchRequestContent();

            foreach (string id in userIds)
            {
                IUserRequest userInfoRequest = _graphClient.Users[id].Request()
                    .Select(
                        "aboutMe, birthday, hireDate, interests, mySite, pastProjects, preferredName, responsibilities, schools, skills, id");

                string userInfoRequstId = batchRequestContent.AddBatchRequestStep(userInfoRequest);

                IProfilePhotoRequest hashRequest = _graphClient.Users[id].Photo.Request();

                string hashRequestId = batchRequestContent.AddBatchRequestStep(hashRequest);

                requestIds.Add(new UserIdAndRequestIds
                {
                    UserId = id,
                    UserInfoRequestId = userInfoRequstId,
                    HashRequestId = hashRequestId
                });
            }

            BatchResponseContent returnedResponse = await _graphClient.Batch.Request().PostAsync(batchRequestContent);
            
            foreach (UserIdAndRequestIds requestId in requestIds)
            {
                UserInfoAndPhotoHash infoAndPhotoHash = new UserInfoAndPhotoHash();
                try
                {
                    User user = await returnedResponse
                        .GetResponseByIdAsync<User>(requestId.UserInfoRequestId);

                    UserInfo userInfo = new UserInfo()
                    {
                        Id = user.Id,
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


                    infoAndPhotoHash.UserInfo = userInfo;
                }
                catch (ServiceException ex)
                {
                    if (ex.Message.Contains(" 429 "))
                    {
                        throw new HttpRequestException(ex.Message, ex.InnerException, HttpStatusCode.TooManyRequests);
                    }
                    Console.WriteLine($"UserId: {requestId.UserId}");
                    Console.WriteLine($"Get user failed: {ex.Error.Message}");
                }

                try
                {
                    ProfilePhoto profilePhoto = await returnedResponse
                        .GetResponseByIdAsync<ProfilePhoto>(requestId.HashRequestId);


                    /// По @odata.mediaEtag получаем хеш тип которого JsonElement
                    JsonElement? value = profilePhoto.AdditionalData["@odata.mediaEtag"] as JsonElement?;

                    infoAndPhotoHash.PhotoHash = value.ToString();
                }
                catch (ServiceException ex)
                {
                    if (ex.Message.Contains(" 429 "))
                    {
                        throw new HttpRequestException(ex.Message, ex.InnerException, HttpStatusCode.TooManyRequests);
                    }
                    if (!ex.Message.Contains("ImageNotFound"))
                    {
                        Console.WriteLine($"UserId: {requestId.UserId}");
                        Console.WriteLine($"Get photo hash failed: {ex.Error.Message}");
                    }
                }
            }

            return userInfoAndPhotoHashs;
        }


        public async Task<List<UserInfoAndPhotoHash>> GetUsersInfoAsync4(List<string> userIds)
        {
            List<UserInfoAndPhotoHash> userInfoAndPhotoHash = new List<UserInfoAndPhotoHash>();
            List<string> userInfoRequestIds = new List<string>();
            Dictionary<string, string> userIdByHashRequestIds = new Dictionary<string, string>();

            BatchRequestContent batchRequestContent = new BatchRequestContent();

            foreach (string id in userIds)
            {
                IUserRequest userInfoRequest = _graphClient.Users[id].Request()
                    .Select(
                        "aboutMe, birthday, hireDate, interests, mySite, pastProjects, preferredName, responsibilities, schools, skills, id");

                string userInfoRequstId = batchRequestContent.AddBatchRequestStep(userInfoRequest);

                userInfoRequestIds.Add(userInfoRequstId);
            }

            foreach (string id in userIds)
            {
                IProfilePhotoRequest hashRequest = _graphClient.Users[id].Photo.Request();

                string hashRequestId = batchRequestContent.AddBatchRequestStep(hashRequest);
                userIdByHashRequestIds.Add(id, hashRequestId);
            }

            BatchResponseContent returnedResponse = await _graphClient.Batch.Request().PostAsync(batchRequestContent);
            
            foreach (string requestId in userInfoRequestIds)
            {
                UserInfoAndPhotoHash infoAndPhotoHash = new UserInfoAndPhotoHash();
                try
                {
                    User user = await returnedResponse
                        .GetResponseByIdAsync<User>(requestId);

                    UserInfo userInfo = new UserInfo()
                    {
                        Id = user.Id,
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


                    infoAndPhotoHash.UserInfo = userInfo;
                }
                catch (ServiceException ex)
                {
                    if (ex.Message.Contains(" 429 "))
                    {
                        throw new HttpRequestException(ex.Message, ex.InnerException, HttpStatusCode.TooManyRequests);
                    }
                    Console.WriteLine($"Get user failed: {ex.Error.Message}");
                }
                userInfoAndPhotoHash.Add(infoAndPhotoHash);
            }

            foreach (KeyValuePair<string, string> userIdByHashRequestId in userIdByHashRequestIds)
            {

                try
                {
                    ProfilePhoto profilePhoto = await returnedResponse
                        .GetResponseByIdAsync<ProfilePhoto>(userIdByHashRequestId.Value);
                    
                    /// По @odata.mediaEtag получаем хеш тип которого JsonElement
                    JsonElement? value = profilePhoto.AdditionalData["@odata.mediaEtag"] as JsonElement?;
                    
                    string photoHash = value.ToString();

                    UserInfoAndPhotoHash item = userInfoAndPhotoHash.FirstOrDefault(c => c.UserInfo.Id.Equals(userIdByHashRequestId.Key));

                    if (item != null)
                    {
                        item.PhotoHash = photoHash;
                    }
                    else
                    {
                        userInfoAndPhotoHash.Add(new UserInfoAndPhotoHash
                        {
                            PhotoHash = photoHash,
                            UserInfo = new UserInfo
                            {
                                Id = userIdByHashRequestId.Key
                            }
                        });
                    }
                }
                catch (ServiceException ex)
                {
                    if (ex.Message.Contains(" 429 "))
                    {
                        throw new HttpRequestException(ex.Message, ex.InnerException, HttpStatusCode.TooManyRequests);
                    }
                    if (!ex.Message.Contains("ImageNotFound"))
                    {
                        Console.WriteLine($"Get photo hash failed: {ex.Error.Message}");
                    }
                }
            }

            return userInfoAndPhotoHash;
        }
        
        public async Task<List<KeyValuePair<string, string>>> GetUsersPhotoHashAsync(List<string> userIds)
        {
            Dictionary<string,string> userIdByRequestIds = new Dictionary<string, string>();
            List<KeyValuePair<string, string>> hashDictionarys = new List<KeyValuePair<string, string>>();

            BatchRequestContent batchRequestContent = new BatchRequestContent();

            foreach (string id in userIds)
            {
                IProfilePhotoRequest request = _graphClient.Users[id].Photo.Request();

                string requestId = batchRequestContent.AddBatchRequestStep(request);

                userIdByRequestIds.Add(id, requestId);
            }

            BatchResponseContent returnedResponse = await _graphClient.Batch.Request().PostAsync(batchRequestContent);

            foreach (KeyValuePair<string, string> userIdByRequest in userIdByRequestIds)
            {
                try
                {
                    ProfilePhoto profilePhoto = await returnedResponse
                        .GetResponseByIdAsync<ProfilePhoto>(userIdByRequest.Value);
                    
                    /// По @odata.mediaEtag получаем хеш тип которого JsonElement
                    JsonElement? value = profilePhoto.AdditionalData["@odata.mediaEtag"] as JsonElement?;

                    hashDictionarys.Add(new KeyValuePair<string, string>(userIdByRequest.Key, value.ToString()));
                }
                catch (ServiceException e)
                {
                    if (!e.Message.Contains("ImageNotFound"))
                    {
                        Console.WriteLine($"Get user failed: {e.Error.Message}");
                    }
                    hashDictionarys.Add(new KeyValuePair<string, string>(userIdByRequest.Key, e.Error.Message));
                }
            }

            return hashDictionarys;
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

        private GraphServiceClient GetGraphServiceClient1()
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

            IList<DelegatingHandler> handlers = GraphClientFactory.CreateDefaultHandlers(authProvider);

            DelegatingHandler compressionHandler =
                handlers.Where(h => h is RetryHandler).FirstOrDefault();
            handlers.Remove(compressionHandler);
            
            HttpClient httpClient = GraphClientFactory.Create(handlers);

            return new GraphServiceClient(httpClient);
        }

        /// Возвращает SharePoint Online свойства 
        private async Task<User> GetUserAdditionalInfoAsync(string userId)
        {
            return await _graphClient.Users[userId].Request()
                .GetAsync();
        }

        private void GetUserAdditionalInfoAsync1(string userId)
        {
            _graphClient.Users[userId].Request()
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
