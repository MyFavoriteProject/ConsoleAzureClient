using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Models;
using Microsoft.Graph;
using File = System.IO.File;

namespace Azure
{
    class Program
    {
        private static AzureClient _azureClient;
        
        private static List<string> _repeadAfterUserIds = new List<string>();

        static void Main(string[] args)
        {
            _azureClient = new AzureClient();
            int takeUsers = 20;
            //CreateALotOfUsers().GetAwaiter().GetResult();
            //UpdateAllUsers().GetAwaiter().GetResult();
            //GetUsersInfo().GetAwaiter().GetResult();
            //GetUsersInfo1().GetAwaiter().GetResult();

            List<string> userIds = _azureClient.GetUserIds().GetAwaiter().GetResult();

            //Stopwatch sp = new Stopwatch();
            //sp.Start();

            Console.WriteLine("Get hash start");
            var hashs = PerformanceComparison(userIds, takeUsers).GetAwaiter().GetResult();

            Console.WriteLine("Hash count: " + hashs.Count);

            //Console.WriteLine("Get user info start");
            //var userInfos = PerformanceComparison1(userIds, takeUsers).GetAwaiter().GetResult();

            //Console.WriteLine("Get user info count: " + userInfos.Count);

            //Console.WriteLine("Get user info and hash start");
            //PerformanceComparison3(userIds).GetAwaiter().GetResult();

            //Console.WriteLine("Total get info time: " + sp.Elapsed);

            Console.ReadKey();
        }

        private static async Task<List<KeyValuePair<string, string>>> PerformanceComparison(List<string> userIds, int takeUsers)
        {
            List<List<string>> idsList = userIds.Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / takeUsers)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            List<KeyValuePair<string, string>> userIdByPhotoHash = new List<KeyValuePair<string, string>>();

            Stopwatch sp = new Stopwatch();
            sp.Start();

            try
            {
                for (int i = 0; i < idsList.Count; i++)
                {
                    if (i % 10 == 0)
                    {
                        Console.WriteLine(sp.Elapsed);
                        Console.WriteLine(i * takeUsers);
                    }
                    
                    List<KeyValuePair<string, string>> result = await _azureClient.GetUsersPhotoHashAsync(idsList[i]);

                    userIdByPhotoHash.AddRange(result);
                }
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return userIdByPhotoHash;
                }
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("Total time: " + sp.Elapsed);
            Console.WriteLine($"Users {userIdByPhotoHash.Count}");

            sp.Stop();

            return userIdByPhotoHash;
        }

        private static async Task<List<UserInfo>> PerformanceComparison1(List<string> userIds, int takeUsers)
        {
            List<List<string>> idsList = userIds.Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / takeUsers)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            List<UserInfo> userInfos = new List<UserInfo>();

            Stopwatch sp = new Stopwatch();
            sp.Start();
            for (int i = 0; i < idsList.Count; i++)
            {
                if (i % 10 == 0)
                {
                    Console.WriteLine(sp.Elapsed);
                    Console.WriteLine(i * takeUsers);
                }

                try
                {
                    List<UserInfo> users = await _azureClient.GetUsersInfoAsync2(idsList[i]);
                    userInfos.AddRange(users);
                }
                catch (HttpRequestException e)
                {
                    if (e.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        return userInfos;
                    }
                    Console.WriteLine(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
            }
            
            Console.WriteLine("Total time: " + sp.Elapsed);
            Console.WriteLine($"Users {userInfos.Count}");

            sp.Stop();

            return userInfos;
        }

        private static async Task PerformanceComparison2(List<string> userIds)
        {
            int takeUsers = 8;

            List<List<string>> idsList = userIds.Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / takeUsers)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            List<UserInfoAndPhotoHash> userInfos = new List<UserInfoAndPhotoHash>();

            Stopwatch sp = new Stopwatch();
            sp.Start();

            try
            {
                for (int i = 0; i < idsList.Count; i++)
                {
                    if (i % 10 == 0)
                    {
                        Console.WriteLine(sp.Elapsed);
                        Console.WriteLine(i * takeUsers);
                    }

                    try
                    {
                        List<UserInfoAndPhotoHash> users = await _azureClient.GetUsersInfoAsync3(idsList[i]);
                        userInfos.AddRange(users);
                    }
                    catch (HttpRequestException e)
                    {
                        if (e.StatusCode == HttpStatusCode.TooManyRequests)
                        {
                            _repeadAfterUserIds.AddRange(idsList[i]);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("Total time: " + sp.Elapsed);
            Console.WriteLine($"Users {userInfos.Count}");

            sp.Stop();
        }


        private static async Task PerformanceComparison3(List<string> userIds)
        {
            int takeUsers = 8;

            List<List<string>> idsList = userIds.Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / takeUsers)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            List<UserInfoAndPhotoHash> userInfos = new List<UserInfoAndPhotoHash>();

            Stopwatch sp = new Stopwatch();
            sp.Start();

            try
            {
                for (int i = 0; i < idsList.Count; i++)
                {
                    if (i % 10 == 0)
                    {
                        Console.WriteLine(sp.Elapsed);
                        Console.WriteLine(i * takeUsers);
                    }

                    try
                    {
                        List<UserInfoAndPhotoHash> users = await _azureClient.GetUsersInfoAsync4(idsList[i]);
                        userInfos.AddRange(users);
                    }
                    catch (HttpRequestException e)
                    {
                        if (e.StatusCode == HttpStatusCode.TooManyRequests)
                        {
                            _repeadAfterUserIds.AddRange(idsList[i]);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("Total time: " + sp.Elapsed);
            Console.WriteLine($"Users {userInfos.Count}");

            sp.Stop();
        }

        private static async Task CreateALotOfUsers()
        {

            Stopwatch sp = new Stopwatch();
            sp.Start();

            //List<UserCreate> userCreates = new List<UserCreate>();
            for (int i = 6928; i < 15_000; i++) // 6928
            {
                try
                {
                    if (i % 10 == 0)
                    {
                        Console.WriteLine(sp.Elapsed);
                        Console.WriteLine(i);
                    }

                    string userName = "AUser" + String.Format("{0:00000}", i);
                    UserCreate userCreate = new UserCreate
                    {
                        DisplayName = userName,
                        MailNickname = userName,
                        UserPrincipalName = userName + "@dimabondarenko888gmail.onmicrosoft.com",
                        AccountEnabled = true,
                        ForceChangePasswordNextSignIn = true,
                        Password = "xWwvJ]6NMw+bWH-d"
                    };

                    await _azureClient.CreateUserAsync(userCreate);

                    //userCreates.Add(userCreate);

                    //if (userCreates.Count == 20)
                    //{
                    //    /// Входными параметрами передаются значения для создания юзера
                    //    await _azureClient.CreateUserAsync(userCreates);
                    //    userCreates.Clear();
                    //}
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            
            Console.WriteLine("Total time: " + sp.Elapsed);

            sp.Stop();
        }

        private static async Task UpdateAllUsers()
        {
            try
            {
                List<string> userIds = await _azureClient.GetUserIds();
                byte[] imageBytes = File.ReadAllBytes("C:\\Users\\d.bondarenko\\Documents\\GitHub\\ConsoleAzureClient\\Azure\\Azure\\Img\\IMG_3614.JPG");

                for (int i = 0; i < userIds.Count; i++)
                {
                    await UpdateUser1(userIds[i]);
                    //await UpdatePhoto1(userIds[i], imageBytes);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static async Task GetUsersInfo()
        {
            Console.WriteLine("GetUsersInfo start");

            DateTime startTime = DateTime.Now;

            List<string> userIds = await _azureClient.GetUserIds();
            List<UserInfoAndPhotoHash> userInfoAndHashes = new List<UserInfoAndPhotoHash>();

            Stopwatch sp = new Stopwatch();
            sp.Start();
            for (int i = 0; i < userIds.Count; i++)
            {
                if (i != 0 && i % 10 == 0)
                {
                    Console.WriteLine(sp.Elapsed);
                    Console.WriteLine(i);
                }
                UserInfoAndPhotoHash userInfoAndPhotoHash = new UserInfoAndPhotoHash();

                Task<UserInfo> taskUserInfo = _azureClient.GetUserInfo(userIds[i]);
                Task<string> taskPhotoHash = _azureClient.GetPhotoHashAsync(userIds[i]);

                try
                {
                    userInfoAndPhotoHash.UserInfo = await taskUserInfo;
                    //userInfoAndPhotoHash.UserInfo = await _azureClient.GetUserInfo1(userIds[i]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                try
                {
                    userInfoAndPhotoHash.PhotoHash = await taskPhotoHash;
                    userInfoAndPhotoHash.PhotoHash = await _azureClient.GetPhotoHashAsync(userIds[i]);
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("ImageNotFound"))
                    {
                        Console.WriteLine(e);
                    }
                }

                userInfoAndHashes.Add(userInfoAndPhotoHash);
            }

            TimeSpan deltaTime = DateTime.Now - startTime;

            Console.WriteLine("End operation, time : " + deltaTime.TotalMinutes);

            Console.ReadKey();
        }
        
        private static async Task UpdateUser1(string userId)
        {
            UserInfo userInfo = new UserInfo
            {
                Id = userId,
                BusinessPhones = new List<string> { "937-99-92" },
                GivenName = "Some One",
                PasswordPolicies = PasswordPolicies.DisableStrongPassword,
                AboutMe = "Im Dev",
                Skills = new List<string> { "C#", ".Net", "Drink coffee" },
                Birthday = DateTimeOffset.Now.AddYears(-20)
            };

            try
            {
                await _azureClient.UpdateUserAsync(userInfo);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static async Task UpdatePhoto1(string userId, byte[] imageBytes)
        {
            try
            {
                _azureClient.UpdateUserPhotoAsync(userId, imageBytes);

                Console.WriteLine("Updated User photo was success");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        private static void CreateUserAndGroup()
        {
            UserInfo userInfo = CreateUser();

            /// Если необходимо обратится к созданому пользователю
            string userId = userInfo.Id;

            try
            {
                GroupCreate groupCreate = new GroupCreate
                {
                    DisplayName = "Library Assist",
                    MailNickname = "library",
                    MailEnabled = true,
                    SecurityEnabled = false
                };
                /// Входными параметрами передаются значения для создания группы
                string groupId = _azureClient.CreateGroupAsync(groupCreate).GetAwaiter().GetResult();
                _azureClient.AddMemberInGroupAsync(groupId, userId).GetAwaiter().GetResult();
                _azureClient.RemoveMemberFromGroupAsync(groupId, userId).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            try
            {
                _azureClient.DeleteUserAsync(userId).GetAwaiter().GetResult();
                _azureClient.RestoreUserAsync(userId).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            ResetPassword(userId);

            /// Ожидает 2 минуты(120000 мс) для обновление фото 
            Task.Delay(120000).GetAwaiter().GetResult();

            /// Запись по id значений хеша картинок для сравнения сторого и нового хеша
            Dictionary<string, string> idByHashDictionary = new Dictionary<string, string>();

            /// Путь указан статической картинки для обновления у пользователя
            UpdateUserPhoto(userId, "C:\\Users\\d.bondarenko\\Documents\\GitHub\\ConsoleAzureClient\\Azure\\Azure\\Img\\IMG_3615.JPG");

            string photoHash = string.Empty;

            try
            {
                photoHash = _azureClient.GetPhotoHashAsync(userId).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            idByHashDictionary.Add(userId, photoHash);

            /// Путь указан статической картинки для обновления у пользователя
            UpdateUserPhoto(userId, "C:\\Users\\d.bondarenko\\Documents\\GitHub\\ConsoleAzureClient\\Azure\\Azure\\Img\\IMG_3614.JPG");

            try
            {
                string newPhotoHash = _azureClient.GetPhotoHashAsync(userId).GetAwaiter().GetResult();

                /// Получает значения хеша которое было записано ранее 
                if (idByHashDictionary.TryGetValue(userId, out string oldPhotoHash))
                {
                    /// Если хеши разные, идёт получение byte[] нового изображения и запись в файл
                    if (!newPhotoHash.Equals(oldPhotoHash))
                    {
                        byte[] imageBytes = _azureClient.GetUserPhotoAsync(userId).GetAwaiter().GetResult();

                        /// Вызов этой функции нужен для того чтобы проверить что полученное изображение соответствует тому что находится на странице пользователя
                        File.WriteAllBytes(@"C:\Users\d.bondarenko\Documents\GitHub\ConsoleAzureClient\Azure\Azure\Img\ImageFromAzureAD.JPG",
                            imageBytes);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            /// Обновление атрибутов SPO у пользователя лучше проводить спустя несколько минут после создания
            /// Issues: https://docs.microsoft.com/en-us/graph/known-issues#access-to-user-resources-is-delayed-after-creation

            UpdateUser(userId);

            try
            {
                UserInfo UserInfo = GetUserInfo(userId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static UserInfo CreateUser()
        {
            Console.WriteLine();

            UserInfo userInfo = default;

            try
            {
                UserCreate userCreate = new UserCreate
                {
                    DisplayName = "First user",
                    MailNickname = "FirstUser",
                    UserPrincipalName = "FirstUser@dimabondarenko888gmail.onmicrosoft.com",
                    AccountEnabled = true,
                    ForceChangePasswordNextSignIn = true,
                    Password = "xWwvJ]6NMw+bWH-d"
                };
                /// Входными параметрами передаются значения для создания юзера
                userInfo = _azureClient.CreateUserAsync(userCreate).GetAwaiter().GetResult();
                
                Console.WriteLine("Created user:"); 
                Console.WriteLine($"User Id - {userInfo.Id}");
                Console.WriteLine($"User PrincipalName - {userInfo.UserPrincipalName}");
                Console.WriteLine($"User password - {userInfo.Password}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine();

            return userInfo;
        }

        /// Обновление только Azure AD свойств 
        static void UpdateUser(string userId) 
        {
            Console.WriteLine();

            UserInfo userInfo = new UserInfo
            {
                Id = userId,
                BusinessPhones = new List<string> {"937-99-92"},
                GivenName = "Some One",
                PasswordPolicies = PasswordPolicies.DisableStrongPassword,
                AboutMe = "Im Dev",
                Skills = new List<string> { "C#", ".Net", "Drink coffee" },
                Birthday = DateTimeOffset.Now.AddYears(-20)
            };
            
            try
            {
                _azureClient.UpdateUserAsync(userInfo).GetAwaiter().GetResult();
                Console.WriteLine("UpdateUser was success");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine();
        }
        
        static UserInfo GetUserInfo(string userId)
        {
            Console.WriteLine();
            UserInfo UserInfo = default;
            try
            {
                UserInfo = _azureClient.GetUserInfo(userId).GetAwaiter().GetResult();
                Console.WriteLine("GetUserInfo was success");
                Console.WriteLine($"AboutMe - {UserInfo.AboutMe}");
                Console.WriteLine($"Birthday - {UserInfo.Birthday}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine();

            return UserInfo;
        }
        
        static void ResetPassword(string userId)
        {
            Console.WriteLine();
            try
            {
                string newPassword = _azureClient.ResetUserPasswordAsync(userId).GetAwaiter().GetResult();
                Console.WriteLine($"New user password: {newPassword}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine();
        }

        static void UpdateUserPhoto(string userId, string imgPath)
        {
            Console.WriteLine();
            try
            {
                /// Получение массива байтов фото по пути
                byte[] imageBytes = File.ReadAllBytes(imgPath);

                _azureClient.UpdateUserPhotoAsync(userId, imageBytes).GetAwaiter().GetResult();

                Console.WriteLine("Updated User photo was success");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine();
        }
    }
}
