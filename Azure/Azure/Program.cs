using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Models;
using Microsoft.Graph;
using File = System.IO.File;

namespace Azure
{
    class Program
    {
        private static AzureClient azureClient;
        //private static readonly List<string> usersId = new List<string>
        //{
        //    "afceaf0b-05a5-48df-8a8d-265643a63896",
        //    "afceaf0b-05a5-48df-8a8d-265643a63896",
        //    "8a98aaff-a016-4a1c-b35e-11221bf25bed",
        //    "ed6280f0-385c-4e80-963c-a726898c121d",
        //    "e00dbdd2-c807-4fd0-ab3a-bd6ccfa4d7c1",
        //    "4fe8903e-a4e3-44e7-b427-7cfeaa1c5443",
        //    "72062c8a-9033-4d63-a78e-63e0aef5220c",
        //    "222da428-e04f-4f0d-89e1-96005ef34d7a",
        //    "698fa2a6-596d-4a5c-b5be-a0d6228a3295",
        //    "698fa2a6-596d-4a5c-b5be-a0d6228a3295",
        //};

        private const string RootPath = @"C:\Users\d.bondarenko\Pictures\";

        private static List<string> pathes = new List<string>()
        {
            RootPath + "SouthPark.jpg",
            RootPath + "photo_2022-03-21_15-10-05.jpg",
            RootPath + "4k.jpg",
            RootPath + "8к.jpg",
        };

        //private static List<string> userIdsForUpdatePhoto = new List<string>
        //{
        //    "92a4b84b-dbd3-427a-a333-a1fb75cacd45",
        //    "56f5423e-3eee-4dbf-a286-baf96e795e8a",
        //    "055c4599-9a4a-4678-b64f-0d5d9841d8a2",
        //    "698fa2a6-596d-4a5c-b5be-a0d6228a3295"
        //};

        static void Main(string[] args)
        {
            #region Azure

            azureClient = new AzureClient();
            //int takeUsers = 20;
            //UpdateAllUsers().GetAwaiter().GetResult();
            //GetUsersInfo().GetAwaiter().GetResult();
            //GetUsersInfo1().GetAwaiter().GetResult();

            //ResetPassword("3daf8cdd-985a-47c7-8468-962a4680379c");

            //List<string> userIds = azureClient.GetUserIds().GetAwaiter().GetResult();

            //CreateALotOfUsers().GetAwaiter().GetResult();
            //azureClient.CreateALOtOfGroups().GetAwaiter().GetResult();

            List<string> groupIds = azureClient.GetGroupIds().GetAwaiter().GetResult();
            //azureClient.UpdateGroupExternalResourses(groupIds).GetAwaiter().GetResult();
            //azureClient.UpdateGroupExternalResourses(new List<string> { "fb30a0b3-1d03-4a14-89c2-30cec68e9398" }).GetAwaiter().GetResult();

            var groups = azureClient.GetExternalResourse(groupIds).GetAwaiter().GetResult();
            //var groups = azureClient.GetExternalResourse1(groupIds).GetAwaiter().GetResult();
            //var groups = azureClient.GetExternalResourse2("95038ebf-f5bb-4cdb-b32d-aabacfceea03").GetAwaiter().GetResult();

            //azureClient.DeleteUsers().GetAwaiter().GetResult();
            //azureClient.DeleteGroups().GetAwaiter().GetResult();
            //azureClient.CreateALOtOfGroups().GetAwaiter().GetResult();

            //azureClient.GetUsers(userIds).GetAwaiter().GetResult();
            //Stopwatch sp = new Stopwatch();
            //sp.Start();

            //UpdateAllUsers(userIds).GetAwaiter().GetResult();
            //UpdateUsers(userIds).GetAwaiter().GetResult();

            //Console.WriteLine("Get hash start");
            //var hashs = PerformanceComparison(userIds, takeUsers).GetAwaiter().GetResult();

            //Console.WriteLine("Get user info start");
            //List<UserInfo> userInfos = PerformanceComparison1(userIds, takeUsers).GetAwaiter().GetResult();
            //Console.WriteLine("Get user info count: " + userInfos.Count);


            //Console.WriteLine("Get user info and hash start");
            //PerformanceComparison3(userIds).GetAwaiter().GetResult();

            //Console.WriteLine("Total get info time: " + sp.Elapsed);

            //for (int i = 0; i < usersId.Count; i++)
            //{
            //    try
            //    {
            //        byte[] imageBytes = azureClient.GetUserPhotoAsync(usersId[i]).GetAwaiter()
            //            .GetResult();
            //        string photoHash = azureClient.GetPhotoHashAsync(usersId[i]).GetAwaiter()
            //            .GetResult();

            //        /// Вызов этой функции нужен для того чтобы проверить что полученное изображение соответствует тому что находится на странице пользователя
            //        File.WriteAllBytes(
            //            @$"C:\Users\d.bondarenko\Desktop\TestPhoto\ImageFromAzureAD{i}.JPG",
            //            imageBytes);
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}


            //string userId = "1b101bb4-e143-437b-8a46-fda10b230f3f";

            #region 

            //foreach(var userId in userIds)
            //{
            //    try
            //    {
            //        byte[] imageBytes = azureClient.GetUserPhotoAsync(userId).GetAwaiter()
            //            .GetResult();

            //        string photoHash = azureClient.GetPhotoHashAsync(userId).GetAwaiter()
            //            .GetResult();

            //        /// Вызов этой функции нужен для того чтобы проверить что полученное изображение соответствует тому что находится на странице пользователя
            //        File.WriteAllBytes(
            //            @$"C:\Users\d.bondarenko\Desktop\TestPhoto\ImageFromAzureAD000.JPG",
            //            imageBytes);
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}

            #endregion

            #region one Photo many Sizes

            //try
            //{
            //    var photosHashs = azureClient.GetPhotoHashAsync1(userId).GetAwaiter()
            //        .GetResult();

            //    List<string> sizes = photosHashs.Select(c => c.Key).ToList();

            //    Dictionary<string, byte[]> imagesBytes = azureClient.GetUserPhotoAsync1(userId, sizes).GetAwaiter()
            //            .GetResult();

            //    foreach (KeyValuePair<string, byte[]> imageBytes in imagesBytes)
            //    {
            //        File.WriteAllBytes(
            //            @$"C:\Users\d.bondarenko\Desktop\TestPhoto\6ImageFromOutlook{imageBytes.Key}.JPG",
            //            imageBytes.Value);
            //    }


            //    /// Вызов этой функции нужен для того чтобы проверить что полученное изображение соответствует тому что находится на странице пользователя
            //    //File.WriteAllBytes(
            //    //    @$"C:\Users\d.bondarenko\Desktop\TestPhoto\ImageFromOutlook.JPG",
            //    //    imageBytes);
            //}
            //catch (Exception ex)
            //{

            //}

            #endregion

            #region many photo many sizes

            //for (int i = 0; i < userIds.Count; i++)
            //{
            //    try
            //    {
            //        var photosHashs = azureClient.GetPhotoHashAsync1(userIds[i]).GetAwaiter()
            //            .GetResult();

            //        List<string> sizes = photosHashs.Select(c => c.Key).ToList();

            //        Dictionary<string, byte[]> imagesBytes = azureClient.GetUserPhotoAsync1(userIds[i], sizes).GetAwaiter()
            //                .GetResult();

            //        foreach (KeyValuePair<string, byte[]> imageBytes in imagesBytes)
            //        {
            //            File.WriteAllBytes(
            //                @$"C:\Users\d.bondarenko\Desktop\TestPhoto\{i}ImageFromOutlook{imageBytes.Key}.JPG",
            //                imageBytes.Value);
            //        }


            //        /// Вызов этой функции нужен для того чтобы проверить что полученное изображение соответствует тому что находится на странице пользователя
            //        //File.WriteAllBytes(
            //        //    @$"C:\Users\d.bondarenko\Desktop\TestPhoto\ImageFromOutlook.JPG",
            //        //    imageBytes);
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}

            #endregion

            #region Batch many size to many photo

            //List<string> ids = new List<string>();

            //foreach (string id in userIds)
            //{
            //    ids.Add(id);

            //    if (ids.Count == 20)
            //    {
            //        try
            //        {
            //            //azureClient.GetUsersPhotoHashAsync(ids).GetAwaiter()
            //            //    .GetResult();
            //            azureClient.GetPhotoHashAsync2(ids).GetAwaiter()
            //                .GetResult();
            //        }
            //        catch (Exception ex)
            //        {

            //        }
            //    }
            //}

            #endregion

            //string userId = "e72bba5b-e22a-4f95-a6c0-4a32349334a6";

            //foreach (string path in pathes)
            //{
            //    try
            //    {
            //        byte[] imageBytes = File.ReadAllBytes(path);

            //        try
            //        {
            //            azureClient.UpdateUserPhotoAsync(userId, imageBytes).GetAwaiter().GetResult();
            //        }
            //        catch (Exception ex)
            //        {

            //        }
            //        try
            //        {
            //            var photosHashs = azureClient.GetPhotoHashAsync1(userId).GetAwaiter()
            //                .GetResult();
            //        }
            //        catch (Exception ex)
            //        {

            //        }
            //    }
            //    catch (Exception ex)
            //    {

            //    }

            //}

            //List<string> ids = new List<string>();

            //foreach(string userId in userIds)
            //{
            //    ids.Add(userId);
            //    if(ids.Count == 10)
            //    {
            //        azureClient.GetPhotoHashAsync2(ids).GetAwaiter().GetResult();
            //        ids.Clear();
            //    }
            //}

            //try
            //{
            //    byte[] imageBytes = File.ReadAllBytes(
            //        "C:\\Users\\d.bondarenko\\Documents\\GitHub\\ConsoleAzureClient\\Azure\\Azure\\Img\\IMG_3614.JPG");

            //    foreach (string userId in userIdsForUpdatePhoto)
            //    {
            //        UpdatePhoto1(userId, imageBytes).GetAwaiter().GetResult();
            //    }

            //}
            //catch (Exception ex)
            //{

            //}

            //foreach(string userId in userIdsForUpdatePhoto)
            //{
            //    try
            //    {
            //        var photosHashs = azureClient.GetPhotoHashAsync1(userId).GetAwaiter()
            //            .GetResult();
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}

            #endregion

            //List<string> page = new List<string>();

            ////for(int i = 0; i < userIds.Count; i++)
            //foreach(string userId in userIds)
            //{
            //    page.Add(userId);
            //    if (page.Count == 10)
            //    {
            //        azureClient.GetSignInActivity(page).GetAwaiter().GetResult();
            //        page.Clear();
            //    }
            //}


            //azureClient.GetSignInActivity1(userIds).GetAwaiter().GetResult();

            //try
            //{
            //    azureClient.GetSignInActivity2().GetAwaiter().GetResult();
            //}
            //catch(Exception e)
            //{
            //    Console.WriteLine(e);
            //}

            // azureClient.GetGroupInfo("95038ebf-f5bb-4cdb-b32d-aabacfceea03").GetAwaiter().GetResult();

            //string goupId = "b08406b1-836a-403b-bd5e-291228147bc9";

            //azureClient.UpdateGroupExternalResourses(new List<string> { "b08406b1-836a-403b-bd5e-291228147bc9"}).GetAwaiter().GetResult();

            Console.ReadKey();
        }

        private static async Task<List<KeyValuePair<string, string>>> PerformanceComparison(
            List<string> userIds, int takeUsers)
        {
            List<List<string>> idsList = userIds.Select((x, i) => new {Index = i, Value = x})
                .GroupBy(x => x.Index / takeUsers)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            List<KeyValuePair<string, string>> userIdByPhotoHash =
                new List<KeyValuePair<string, string>>();

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

                    List<KeyValuePair<string, string>> result =
                        await azureClient.GetUsersPhotoHashAsync(idsList[i]);

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


        private static async Task<List<KeyValuePair<string, string>>> PerformanceComparison2(
            List<string> userIds, int takeUsers)
        {
            List<List<string>> idsList = userIds.Select((x, i) => new {Index = i, Value = x})
                .GroupBy(x => x.Index / takeUsers)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            List<KeyValuePair<string, string>> userIdByPhotoHash =
                new List<KeyValuePair<string, string>>();

            Stopwatch sp = new Stopwatch();
            sp.Start();

            try
            {
                for (int i = idsList.Count - 1; i >= 0; i--)
                {
                    if (i % 10 == 0)
                    {
                        Console.WriteLine(sp.Elapsed);
                        Console.WriteLine(i * takeUsers);
                    }

                    List<KeyValuePair<string, string>> result =
                        await azureClient.GetUsersPhotoHashAsync(idsList[i]);

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

        private static async Task<List<UserInfo>> PerformanceComparison1(List<string> userIds,
            int takeUsers)
        {
            List<List<string>> idsList = userIds.Select((x, i) => new {Index = i, Value = x})
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
                    List<UserInfo> users = await azureClient.GetUsersInfoAsync2(idsList[i]);
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

            List<List<string>> idsList = userIds.Select((x, i) => new {Index = i, Value = x})
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
                        List<UserInfoAndPhotoHash> users =
                            await azureClient.GetUsersInfoAsync3(idsList[i]);
                        userInfos.AddRange(users);
                    }
                    catch (HttpRequestException e)
                    {
                        if (e.StatusCode == HttpStatusCode.TooManyRequests)
                        {
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

            List<List<string>> idsList = userIds.Select((x, i) => new {Index = i, Value = x})
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
                        List<UserInfoAndPhotoHash> users =
                            await azureClient.GetUsersInfoAsync4(idsList[i]);
                        userInfos.AddRange(users);
                    }
                    catch (HttpRequestException e)
                    {
                        if (e.StatusCode == HttpStatusCode.TooManyRequests)
                        {
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

            List<int> errorsIndexs = new List<int>();

            List<UserCreate> userCreates = new List<UserCreate>();
            for (int i = 0; i < 1_000; i++) // 6928
            {
                try
                {
                    if (i % 50 == 0)
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

                    userCreates.Add(userCreate);

                    if (userCreates.Count == 5)
                    {
                        /// Входными параметрами передаются значения для создания юзера
                        await azureClient.CreateUserAsync(userCreates);
                        userCreates.Clear();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    errorsIndexs.Add(i);
                }
            }

            Console.WriteLine("Total time: " + sp.Elapsed);

            sp.Stop();

            if (errorsIndexs.Any())
            {
                string result = String.Join(", ", errorsIndexs.ToArray());

                Console.WriteLine("Not created users indexs");
                Console.WriteLine(result);
            }
        }


        private static async Task UpdateAllUsers()
        {
            try
            {
                List<string> userIds = await azureClient.GetUserIds();
                byte[] imageBytes = File.ReadAllBytes(
                    "C:\\Users\\d.bondarenko\\Documents\\GitHub\\ConsoleAzureClient\\Azure\\Azure\\Img\\IMG_3614.JPG");

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

        private static async Task UpdateUsers(List<string> userIds)
        {
            //foreach(string userId in userIds)
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for(int i = 0; i < userIds.Count; i++)
            {
                if (i % 100 == 0)
                {
                    Console.WriteLine($"Index: {i}");
                    Console.WriteLine(stopwatch.Elapsed);
                }
                UpdateUser(userIds[i]);
            }
        }

        private static async Task UpdateAllUsers(List<string> userIds)
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            List<UserInfo> userInfos = new List<UserInfo>();
            for (int i = 0; i < userIds.Count; i++)
            {
                if (i % 100 == 0)
                {
                    Console.WriteLine(sp.Elapsed);
                    Console.WriteLine(i);
                }

                UserInfo userInfo = new UserInfo
                {
                    Id = userIds[i],
                    AboutMe = "Im Dev",
                    Birthday = DateTimeOffset.Now.AddYears(-20),
                    PreferredName = "JSG",
                    Skills = new List<string> { "C#", ".Net", "Drink coffee" },
                    Responsibilities = new List<string> { "Coding" },
                    Schools = new List<string> { "85" },
                    PastProjects = new List<string> { "Grids", "UWP" },
                };

                userInfos.Add(userInfo);

                if (userInfos.Count == 20)
                {
                    try
                    {
                        await azureClient.UpdateUserAsync(userInfos);
                    }
                    catch (HttpRequestException e)
                    {
                        Console.WriteLine(e);
                        if (e.StatusCode == HttpStatusCode.TooManyRequests)
                        {
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    userInfos.Clear();
                }
            }
        }

        private static async Task GetUsersInfo()
        {
            Console.WriteLine("GetUsersInfo start");

            DateTime startTime = DateTime.Now;

            List<string> userIds = await azureClient.GetUserIds();
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

                Task<UserInfo> taskUserInfo = azureClient.GetUserInfo(userIds[i]);
                Task<string> taskPhotoHash = azureClient.GetPhotoHashAsync(userIds[i]);

                try
                {
                    userInfoAndPhotoHash.UserInfo = await taskUserInfo;
                    //userInfoAndPhotoHash.UserInfo = await azureClient.GetUserInfo1(userIds[i]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                try
                {
                    userInfoAndPhotoHash.PhotoHash = await taskPhotoHash;
                    userInfoAndPhotoHash.PhotoHash =
                        await azureClient.GetPhotoHashAsync(userIds[i]);
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
                BusinessPhones = new List<string> {"937-99-92"},
                GivenName = "Some One",
                PasswordPolicies = PasswordPolicies.DisableStrongPassword,
                AboutMe = "Im Dev",
                Skills = new List<string> {"C#", ".Net", "Drink coffee"},
                Birthday = DateTimeOffset.Now.AddYears(-20)
            };

            try
            {
                await azureClient.UpdateUserAsync(userInfo);
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
                azureClient.UpdateUserPhotoAsync(userId, imageBytes);

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
                string groupId = azureClient.CreateGroupAsync(groupCreate).GetAwaiter()
                    .GetResult();
                azureClient.AddMemberInGroupAsync(groupId, userId).GetAwaiter().GetResult();
                azureClient.RemoveMemberFromGroupAsync(groupId, userId).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            try
            {
                azureClient.DeleteUserAsync(userId).GetAwaiter().GetResult();
                azureClient.RestoreUserAsync(userId).GetAwaiter().GetResult();
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
            UpdateUserPhoto(userId,
                "C:\\Users\\d.bondarenko\\Documents\\GitHub\\ConsoleAzureClient\\Azure\\Azure\\Img\\IMG_3615.JPG");

            string photoHash = string.Empty;

            try
            {
                photoHash = azureClient.GetPhotoHashAsync(userId).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            idByHashDictionary.Add(userId, photoHash);

            /// Путь указан статической картинки для обновления у пользователя
            UpdateUserPhoto(userId,
                "C:\\Users\\d.bondarenko\\Documents\\GitHub\\ConsoleAzureClient\\Azure\\Azure\\Img\\IMG_3614.JPG");

            try
            {
                string newPhotoHash =
                    azureClient.GetPhotoHashAsync(userId).GetAwaiter().GetResult();

                /// Получает значения хеша которое было записано ранее 
                if (idByHashDictionary.TryGetValue(userId, out string oldPhotoHash))
                {
                    /// Если хеши разные, идёт получение byte[] нового изображения и запись в файл
                    if (!newPhotoHash.Equals(oldPhotoHash))
                    {
                        byte[] imageBytes = azureClient.GetUserPhotoAsync(userId).GetAwaiter()
                            .GetResult();

                        /// Вызов этой функции нужен для того чтобы проверить что полученное изображение соответствует тому что находится на странице пользователя
                        File.WriteAllBytes(
                            @"C:\Users\d.bondarenko\Documents\GitHub\ConsoleAzureClient\Azure\Azure\Img\ImageFromAzureAD.JPG",
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
                userInfo = azureClient.CreateUserAsync(userCreate).GetAwaiter().GetResult();

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
            UserInfo userInfo = new UserInfo
            {
                Id = userId,
                AboutMe = "Im Dev",
                Birthday = DateTimeOffset.Now.AddYears(-20),
                PreferredName = null,
                Skills = new List<string> { "C#", ".Net", "Drink coffee" },
                Responsibilities = new List<string> { "Coding" },
                Schools = new List<string> { "85" },
                PastProjects = new List<string> { "Grids", "UWP" },
            };

            try
            {
                azureClient.UpdateUserAsync(userInfo).GetAwaiter().GetResult();
                //Console.WriteLine("UpdateUser was success");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        static UserInfo GetUserInfo(string userId)
        {
            Console.WriteLine();
            UserInfo UserInfo = default;
            try
            {
                UserInfo = azureClient.GetUserInfo(userId).GetAwaiter().GetResult();
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


        static void GetUserInfo1(List<string> userIds, Stopwatch sp)
        {
            Console.WriteLine();

            for (int i = 0; i < userIds.Count; i++)
            {
                if (i % 100 == 0)
                {
                    Console.WriteLine(sp.Elapsed);
                    Console.WriteLine(i);
                }

                try
                {
                    azureClient.GetUserInfo2(userIds[i]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }


            Console.WriteLine();
        }

        static void ResetPassword(string userId)
        {
            Console.WriteLine();
            try
            {
                string newPassword = azureClient.ResetUserPasswordAsync(userId).GetAwaiter()
                    .GetResult();
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

                azureClient.UpdateUserPhotoAsync(userId, imageBytes).GetAwaiter().GetResult();

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
