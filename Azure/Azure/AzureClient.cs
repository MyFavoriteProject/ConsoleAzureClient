using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Models;
using Microsoft.Graph;

namespace Azure
{
    public class AzureClient
    {
        private readonly GraphServiceClient graphClient;
        private readonly PasswordGenerator passwordGenerator;

        public AzureClient()
        {
            passwordGenerator = new PasswordGenerator();
            this.graphClient = GetGraphServiceClient();
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

            User user = await graphClient.Users
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

        //private static readonly Expression<Func<User, object>> SignInActivityExpression =
        //    (user) => new
        //    {
        //        user.SignInActivity,
        //        user.Id
        //    };

        public async Task DeleteUsers()
        {
            Stopwatch stopwatch = new Stopwatch();
            List<string> userNames = new List<string>();
            for(int i = 40_000; i < 45_000; i++)
            {
                string userName = "AUser" + String.Format("{0:00000}", i);
                userNames.Add(userName);
            }

            stopwatch.Start();

            List<string> userIds = new List<string>();

            try
            {
                IGraphServiceUsersCollectionPage page = await graphClient.Users
                    .Request()
                    .GetAsync();

                User[] users = page.Where(c => userNames.Contains(c.DisplayName)).ToArray();

                userIds.AddRange(users.Select(c => c.Id));

                while (page.NextPageRequest != null)
                {
                    page = await page.NextPageRequest.GetAsync();

                    users = page.Where(c => userNames.Contains(c.DisplayName)).ToArray();

                    userIds.AddRange(users.Select(c => c.Id));

                    Console.WriteLine($"Users for delete ids index {userIds.Count}");
                    Console.WriteLine(stopwatch.Elapsed);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("Messege before delete");
            Console.WriteLine($"Users for delete ids index {userIds.Count}");

            List<string> notDeletedIds = new List<string>();

            for (int i = 0; i < userIds.Count; i++)
            {
                if (i % 100 == 0)
                {
                    Console.WriteLine($"User index {i}");
                    Console.WriteLine(stopwatch.Elapsed);
                }
                try
                {
                    await graphClient.Users[userIds[i]]
                        .Request()
                        .DeleteAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    notDeletedIds.Add(userIds[i]);
                }
            }

            for (int i = 0; i < userIds.Count; i++)
            {
                if (notDeletedIds.Contains(userIds[i]))
                {
                    continue;
                }
                if (i % 100 == 0)
                {
                    Console.WriteLine($"User index {i}");
                    Console.WriteLine(stopwatch.Elapsed);
                }
                try
                {
                    await graphClient.Directory.DeletedItems[userIds[i]]
                        .Request()
                        .DeleteAsync();
                }
                catch (ServiceException e)
                {
                    Console.WriteLine(e);
                    notDeletedIds.Add(userIds[i]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    notDeletedIds.Add(userIds[i]);
                }
            }

            Console.WriteLine($"Not deleted count {notDeletedIds.Count}");
            //Console.WriteLine($"Not deleted count {notDeletedIds.Count}");
        }

        public async Task DeleteGroups()
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            List<string> groupIds = new List<string>();

            try
            {
                IGraphServiceGroupsCollectionPage page = await graphClient.Groups
                    .Request()
                    .GetAsync();

                groupIds.AddRange(page.Select(c => c.Id));

                while (page.NextPageRequest != null)
                {
                    page = await page.NextPageRequest.GetAsync();

                    groupIds.AddRange(page.Select(c => c.Id));

                    Console.WriteLine($"Users for delete ids index {groupIds.Count}");
                    Console.WriteLine(stopwatch.Elapsed);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("Messege before delete");
            Console.WriteLine($"Users for delete ids index {groupIds.Count}");

            List<string> notDeletedIds = new List<string>();

            for (int i = 0; i < groupIds.Count; i++)
            {
                if (i % 100 == 0)
                {
                    Console.WriteLine($"User index {i}");
                    Console.WriteLine(stopwatch.Elapsed);
                }
                try
                {
                    await graphClient.Groups[groupIds[i]]
                        .Request()
                        .DeleteAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    notDeletedIds.Add(groupIds[i]);
                }
            }

            for (int i = 0; i < groupIds.Count; i++)
            {
                if (notDeletedIds.Contains(groupIds[i]))
                {
                    continue;
                }
                if (i % 100 == 0)
                {
                    Console.WriteLine($"User index {i}");
                    Console.WriteLine(stopwatch.Elapsed);
                }
                try
                {
                    await graphClient.Directory.DeletedItems[groupIds[i]]
                        .Request()
                        .DeleteAsync();
                }
                catch (ServiceException e)
                {
                    Console.WriteLine(e);
                    notDeletedIds.Add(groupIds[i]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    notDeletedIds.Add(groupIds[i]);
                }
            }

            Console.WriteLine($"Not deleted count {notDeletedIds.Count}");
            //Console.WriteLine($"Not deleted count {notDeletedIds.Count}");
        }

        public async Task DeleteDeletedObjs()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string ids = System.IO.File.ReadAllText(@"C:\DeletedIds\WriteLines.txt");

            List<string> userIds = ids.Split(new char[] { ',' }).ToList();

            //try
            //{
            //    //var queryOptions = new List<QueryOption>()
            //    //{
            //    //    new QueryOption("$count", "true")
            //    //};

            //    var page = await graphClient.Directory.DeletedItems
            //        //.Request(queryOptions)
            //        .Request()
            //        //.Header("ConsistencyLevel", "eventual")
            //        //.Select("id,displayName,deletedDateTime")
            //        .GetAsync();

            //    userIds.AddRange(page.Select(c => c.Id));

            //    while (page.NextPageRequest != null)
            //    {
            //        page = await page.NextPageRequest.GetAsync();

            //        userIds.AddRange(page.Select(c => c.Id));

            //        Console.WriteLine($"Users for delete ids index {userIds.Count}");
            //        Console.WriteLine(stopwatch.Elapsed);
            //    }
            //}
            //catch (ServiceException e)
            //{
            //    Console.WriteLine(e);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //}

            List<string> notDeletedIds = new List<string>();

            for (int i = 0; i < userIds.Count; i++)
            {
                if (i % 100 == 0)
                {
                    Console.WriteLine($"User index {i}");
                    Console.WriteLine(stopwatch.Elapsed);
                }
                try
                {
                    await graphClient.Directory.DeletedItems[userIds[i]]
                        .Request()
                        .DeleteAsync();
                }
                catch (ServiceException e)
                {
                    Console.WriteLine(e);
                    notDeletedIds.Add(userIds[i]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    notDeletedIds.Add(userIds[i]);
                }
            }

            Console.WriteLine($"Not deleted count {notDeletedIds.Count}");
        }

        public async Task CreateALOtOfGroups()
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();

            List<string> notCteatedGroups = new();

            for (int i = 0; i < 40_000; i++)
            {
                if (i % 100 == 0)
                {
                    Console.WriteLine(sp.Elapsed);
                    Console.WriteLine(i);
                }

                string groupName = "Group" + String.Format("{0:00000}", i);

                var group = new Group
                {
                    Description = $"Self help community for {groupName}",
                    DisplayName = groupName,
                    GroupTypes = new List<String>()
                    {
                        "Unified"
                    },
                    MailEnabled = true,
                    MailNickname = groupName,
                    SecurityEnabled = false
                };
                try
                {
                    await graphClient.Groups
                        .Request()
                        .AddAsync(group);
                }
                catch(ServiceException e)
                {
                    Console.WriteLine(e);
                    notCteatedGroups.Add(groupName);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    notCteatedGroups.Add(groupName);
                }
            }

            Console.ReadKey();
        }

        public async Task GetGroupInfo(string groupId)
        {
            try
            {
                Group group = await graphClient.Groups[groupId]
                    .Request()
                    .GetAsync();
            }
            catch(Exception e)
            {

            }

            try
            {
                DirectoryObject group = await graphClient.DirectoryObjects[groupId]
                    .Request()
                    .GetAsync();
            }
            catch (Exception e)
            {

            }
        }

        //public async Task<List<SignInActivityInfo>> GetSignInActivity(List<string> userIds)
        //{
        //    StringBuilder stringBuilder = new StringBuilder();
        //    List<SignInActivityInfo> signInActivityInfos = new List<SignInActivityInfo>();

        //    bool isFirstTime = true;
        //    stringBuilder.Append("id eq ");

        //    foreach (string userId in userIds)
        //    {
        //        if (isFirstTime)
        //        {
        //            stringBuilder.Append($"'{userId}'");
        //        }
        //        else
        //        {
        //            stringBuilder.Append($" or id eq '{userId}'");
        //        }
        //        isFirstTime = false;
        //    }

        //    try
        //    {
        //        IList<User> users = await graphClient.Users.Request().Filter(stringBuilder.ToString()).Select(SignInActivityExpression).GetAsync();

        //        foreach(User user in users)
        //        {
        //            signInActivityInfos.Add(new SignInActivityInfo
        //            {
        //                userId = user.Id,
        //                SignInActivity = user.SignInActivity
        //            });
        //        }
        //    }
        //    catch(Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }

        //    return signInActivityInfos;
        //}

        //public async Task GetSignInActivity1(List<string> userIds)
        //{
        //    List<SignInActivityInfo> signInActivityInfos = new List<SignInActivityInfo>();
        //    List<string> page = new List<string>();
        //    int iterator = 0;
        //    Stopwatch stopwatch = new Stopwatch();
        //    stopwatch.Start();

        //    //foreach (string userId in userIds)
        //    for(int i = 0; i < userIds.Count; i++)
        //    {
        //        page.Add(userIds[i]);
        //        iterator++;

        //        if (iterator != 0 && iterator % 100 == 0)
        //        {
        //            Console.WriteLine($"Index: {iterator}");
        //            Console.WriteLine($"stopwatch: {stopwatch.Elapsed}");
        //        }

        //        if (page.Count == 10 || i == userIds.Count - 1)
        //        {
        //            signInActivityInfos.AddRange(await GetSignInActivity(page));
        //            page.Clear();
        //        }
        //    }

        //    Console.WriteLine($"End stopwatch: {stopwatch.Elapsed}");
        //    stopwatch.Stop();

        //    var notNull = signInActivityInfos.Where(c => c.SignInActivity != null).ToList();

        //    if (notNull.Any())
        //    {
        //        Console.WriteLine($"Finish, not null signInActivity: {notNull.Count}");
        //    }
        //}

        //public async Task GetSignInActivity2()
        //{
        //    Stopwatch stopwatch = new Stopwatch();
        //    stopwatch.Start();

        //    DateTime dateTime = DateTime.MinValue.ToUniversalTime();

        //    string str = dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");

        //    Guid guid = new Guid();

        //    string filter = $"signInActivity/lastSignInDateTime ge {str} or signInActivity/lastNonInteractiveSignInDateTime ge {str}";
        //    //https://graph.microsoft.com/beta/users?$filter=signInActivity.lastSignInDateTime%20ge%202021-06-15T10:00:00Z

        //    List<SignInActivityInfo> signInActivityInfos = new List<SignInActivityInfo>();

        //    IGraphServiceUsersCollectionPage users = await graphClient.Users
        //        .Request()
        //        .Select(SignInActivityExpression)
        //        .Filter(filter)
        //        .GetAsync();

        //    signInActivityInfos.AddRange(users.Select(c => new SignInActivityInfo { userId = c.Id, SignInActivity = c.SignInActivity}).ToList());

        //    while (users.NextPageRequest != null)
        //    {
        //        try
        //        {
        //            users = await users.NextPageRequest.GetAsync();
        //            signInActivityInfos.AddRange(users.Select(c => new SignInActivityInfo { userId = c.Id, SignInActivity = c.SignInActivity }).ToList());

        //            if (signInActivityInfos.Count % 1000 == 0)
        //            {
        //                Console.WriteLine($"User ids index {signInActivityInfos.Count}");

        //                Console.WriteLine($"stopwatch: {stopwatch.Elapsed}");
        //            }
        //        }
        //        catch (ServiceException e)
        //        {
        //            Console.WriteLine(e);
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e);
        //        }
        //    }

        //    List<User> notDeletedUsers = new List<User>();
        //    List<DirectoryObject> deletedUsers = new List<DirectoryObject>();
        //    List<SignInActivityInfo> errorUsers = new List<SignInActivityInfo>();

        //    foreach (SignInActivityInfo signInActivityInfo in signInActivityInfos)
        //    {
        //        try
        //        {
        //            var directoryObject = await graphClient.Users[signInActivityInfo.userId]
        //                .Request()
        //                .GetAsync();

        //            if (directoryObject.DisplayName == null)
        //            {

        //            }

        //            if (directoryObject.DeletedDateTime != null)
        //            {

        //            }
        //            notDeletedUsers.Add(directoryObject);
        //        }
        //        catch(Exception e)
        //        {
        //            try
        //            {
        //                var directoryObject = await graphClient.Directory.DeletedItems[signInActivityInfo.userId]
        //                    .Request()
        //                    .GetAsync();
        //                deletedUsers.Add(directoryObject);
        //            }
        //            catch(Exception ex)
        //            {
        //                errorUsers.Add(signInActivityInfo);
        //            }
        //        }


        //    }

        //    stopwatch.Stop();

        //    Console.WriteLine($"stopwatch end: {stopwatch.Elapsed}");
        //    Console.WriteLine($"User ids index end {signInActivityInfos.Count}");
        //}

        public async Task UpdateUserAsync(UserInfo userInfo)
        {
            string userId = userInfo.Id;

            //if (userInfo.AccountEnabled.HasValue)
            //{
            //    await SetAccountEnabledAsync(userId, userInfo.AccountEnabled.Value);
            //}

            //if (userInfo.AboutMe != null || userInfo.Birthday.HasValue ||
            //    userInfo.HireDate.HasValue ||
            //    userInfo.Interests != null || userInfo.MySite != null ||
            //    userInfo.PastProjects != null ||
            //    userInfo.Responsibilities != null || userInfo.Schools != null ||
            //    userInfo.Skills != null)
            //{
                await UpdateUserAdditionalProps(userInfo);
            //}

            //User user = new User
            //{
            //    UserPrincipalName = userInfo.UserPrincipalName,
            //    DisplayName = userInfo.DisplayName,
            //    MailNickname = userInfo.MailNickname,
            //    GivenName = userInfo.GivenName,
            //    BusinessPhones = userInfo.BusinessPhones,
            //    PasswordPolicies = userInfo.PasswordPolicies.ToString()
            //};

            //await graphClient.Users[userId]
            //    .Request()
            //    .UpdateAsync(user);
        }

        public async Task UpdateUserAsync1(string userId, User user)
        {
            await graphClient.Users[userId]
                .Request()
                .UpdateAsync(user);
        }

        public static readonly Expression<Func<User, object>> SelectExternalResourcesExpression =
            (user) => new
            {
                user.AboutMe,
                user.Birthday,
                user.Interests,
                user.PastProjects,
                user.PreferredName,
                user.Responsibilities,
                user.Schools,
                user.Skills
            };

        public async Task GetUsers(List<string> userIds)
        {
            foreach(string userId in userIds)
            {
                try
                {
                    var info = await graphClient.Users[userId].Request().Select(SelectExternalResourcesExpression).GetAsync();
                }
                catch (ServiceException e)
                {

                }
                catch (Exception e)
                {

                }
            }
        }

        public async Task<List<string>> GetUserIds()
        {
            List<string> userIds = new List<string>();
            IGraphServiceUsersCollectionPage users = await graphClient.Users
                .Request()
                .GetAsync();

            userIds.AddRange(users.Select(c => c.Id).ToList());

            while (users.NextPageRequest != null)
            {
                users = await users.NextPageRequest.GetAsync();
                userIds.AddRange(users.Select(c => c.Id).ToList());

                if (userIds.Count % 1000 == 0)
                {
                    Console.WriteLine($"User ids index {userIds.Count}");
                }
            }

            return userIds;
        }


        public async Task<List<string>> GetGroupIds()
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            List<string> groupIds = new List<string>();
            var groups = await graphClient.Groups
                .Request()
                .GetAsync();

            groupIds.AddRange(groups.Select(c => c.Id).ToList());

            //while (groups.NextPageRequest != null)
            //{
            //    groups = await groups.NextPageRequest.GetAsync();
            //    groupIds.AddRange(groups.Select(c => c.Id).ToList());
            //
            //    if (groupIds.Count % 1000 == 0)
            //    {
            //        Console.WriteLine($"Group ids index {groupIds.Count}");
            //        Console.WriteLine(sp.Elapsed);
            //    }
            //}

            //var ids = String.Join(Environment.NewLine, groupIds);
            //System.IO.File.WriteAllText(@"C:\DeletedIds\WriteLines.txt", ids);

            return groupIds;
        }

        public async Task<List<GroupInfo>> GetExternalResourse(List<string> groupIds)
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();

            List<GroupInfo> groupInfos = new List<GroupInfo>();

            for (int i = 0; i < groupIds.Count; i++)
            {
                if (i % 100 == 0)
                {
                    Console.WriteLine(sp.Elapsed);
                    Console.WriteLine(i);
                }

                try
                {
                    Group group = await graphClient.Groups[groupIds[i]]
                        .Request()
                        .Select((group) => new
                        {
                            group.AllowExternalSenders,
                            group.AutoSubscribeNewMembers,
                            group.HideFromAddressLists,
                            group.HideFromOutlookClients
                        })
                        .GetAsync();

                    if(group.AllowExternalSenders != null ||
                        group.AutoSubscribeNewMembers != null ||
                        group.HideFromAddressLists != null || 
                        group.HideFromOutlookClients != null)
                    {
                        groupInfos.Add(new()
                        {
                            Id = group.Id,
                            AllowExternalSenders = group.AllowExternalSenders,
                            AutoSubscribeNewMembers = group.AutoSubscribeNewMembers,
                            HideFromAddressLists = group.HideFromAddressLists,
                            HideFromOutlookClients = group.HideFromOutlookClients
                        });
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            Console.WriteLine($"Groups count: {groupInfos.Count}");

            return groupInfos;
        }


        private static Expression<Func<Group, object>> groupExturnalResoursesExpression =
            (group) => new
            {
                Id = group.Id,
                AllowExternalSenders = group.AllowExternalSenders,
                AutoSubscribeNewMembers = group.AutoSubscribeNewMembers,
                HideFromAddressLists = group.HideFromAddressLists,
                HideFromOutlookClients = group.HideFromOutlookClients
            };


        public async Task<List<GroupInfo>> GetExternalResourse1(List<string> groupIds)
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();

            int pageLimit = 10;

            List<string> page = new List<string>();

            List<GroupInfo> groupInfos = new List<GroupInfo>();

            for (int i = 0; i < groupIds.Count; i++)
            {
                if (i % 100 == 0)
                {
                    Console.WriteLine(sp.Elapsed);
                    Console.WriteLine(i);
                }

                page.Add(groupIds[i]);

                if(page.Count == pageLimit || (i == page.Count - 1 && i < pageLimit))
                {
                    List<Group> groups = await GetExternalResourseByButch(page);

                    groupInfos.AddRange(groups.Select(c => new GroupInfo
                    {
                        Id = c.Id,
                        AllowExternalSenders = c.AllowExternalSenders,
                        AutoSubscribeNewMembers = c.AutoSubscribeNewMembers,
                        HideFromAddressLists = c.HideFromAddressLists,
                        HideFromOutlookClients = c.HideFromOutlookClients
                    }));

                    page.Clear();
                }
            }

            Console.WriteLine($"Groups count: {groupInfos.Count}");

            return groupInfos;
        }

        private async Task<List<Group>> GetExternalResourseByButch(IList<string> groupIds)
        {
            using BatchRequestContent batchRequestContent = new();
            string[] requestIds = new string[groupIds.Count];

            List<Group> groups = new List<Group>();

            for (int i = 0; i < groupIds.Count; i++)
            {
                IGroupRequest request = graphClient.Groups[groupIds[i]]
                    .Request()
                    .Select(groupExturnalResoursesExpression)
                    ;

                string requestId = batchRequestContent.AddBatchRequestStep(request);

                requestIds[i] = requestId;
            }

            BatchResponseContent responseContent =
                await graphClient.Batch.Request().PostAsync(batchRequestContent);

            for(int i = 0; i < groupIds.Count; i++)
            {
                try
                {
                    Group group = await responseContent
                        .GetResponseByIdAsync<Group>(requestIds[i]);

                    groups.Add(group);
                }
                catch (ServiceException e)
                {
                    var code = e.StatusCode;

                    if(code == HttpStatusCode.Unauthorized)
                    {

                    }

                    try
                    {
                        var request = await graphClient.Groups[groupIds[i]]
                            .Request()
                            .Select(groupExturnalResoursesExpression).GetAsync();
                    }
                    catch (ServiceException ex)
                    {

                    }
                    catch(Exception ex)
                    {

                    }

                    Console.WriteLine(e);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return groups;
        }

        public async Task<List<Group>> GetExternalResourse2(string groupId)
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();

            List<Group> groupInfos = new List<Group>();

            try
            {
                var groups = await graphClient.Groups
                    .Request()
                    .GetAsync();

                groupInfos.AddRange(groups);

                while (groups.NextPageRequest != null)
                {
                    groups = await groups.NextPageRequest.GetAsync();

                    groupInfos.AddRange(groups);

                    if (groupInfos.Count % 1000 == 0)
                    {
                        Console.WriteLine($"Group ids index {groupInfos.Count}");
                        Console.WriteLine(sp.Elapsed);
                    }
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Group group = groupInfos.FirstOrDefault(c => c.Id.Equals(groupId));

            return groupInfos;
        }

        public async Task UpdateGroupExternalResourses(List<string> groupIds)
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();

            Group group = new Group
            {
                //AllowExternalSenders = false,
                //AutoSubscribeNewMembers = true,
                HideFromAddressLists = false,
                HideFromOutlookClients = true,
            };

            List<string> notUpdatedIds = new List<string>();

            for(int i = 0; i < groupIds.Count; i++)
            {
                if (i % 100 == 0)
                {
                    Console.WriteLine(sp.Elapsed);
                    Console.WriteLine(i);
                }

                try
                {
                    await graphClient.Groups[groupIds[i]]
                        .Request()
                        .UpdateAsync(group);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    notUpdatedIds.Add(groupIds[i]);
                }
            }

            if (notUpdatedIds.Any())
            {
                var ids = String.Join(",", notUpdatedIds);
                System.IO.File.WriteAllText(@"C:\DeletedIds\WriteLines.txt", ids);
            }
        }

        public async Task<List<string>> GetUserIds1()
        {
            List<string> userIds = new List<string>();
            IGraphServiceUsersCollectionPage users = await graphClient.Users
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
            graphClient.Users[userId].Request().GetAsync();
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
                User user = new User
                {
                    AboutMe = userInfo.AboutMe,
                    Birthday = userInfo.Birthday,
                    PreferredName = userInfo.PreferredName,
                    Responsibilities = userInfo.Responsibilities,
                    Schools = userInfo.Schools,
                    Skills = userInfo.Skills,
                    PastProjects = userInfo.PastProjects,
                };

                HttpContent jsonUser =
                    graphClient.HttpProvider.Serializer.SerializeAsJsonContent(user);

                HttpRequestMessage request =
                    graphClient.Users[userInfo.Id].Request().GetHttpRequestMessage();
                request.Method = HttpMethod.Patch;
                request.Content = jsonUser;
                request.Headers.Add("x-ms-throttle-priority", "Normal");

                string requestId = batchRequestContent.AddBatchRequestStep(request);

                requestIds.Add(requestId);
            }

            BatchResponseContent returnedResponse =
                await graphClient.Batch.Request().PostAsync(batchRequestContent);

            try
            {
                foreach (string requestId in requestIds)
                {
                    HttpResponseMessage response =
                        await returnedResponse.GetResponseByIdAsync(requestId);

                    if (!response.IsSuccessStatusCode)
                    {
                    }
                }
            }
            catch (ServiceException ex)
            {
                if (ex.Message.Contains(" 429 "))
                {
                    throw new HttpRequestException(ex.Message, ex.InnerException,
                        HttpStatusCode.TooManyRequests);
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
                User user = new User()
                {
                    DisplayName = userCreate.DisplayName,
                    MailNickname = userCreate.MailNickname,
                    UserPrincipalName = userCreate.UserPrincipalName,
                    AccountEnabled = userCreate.AccountEnabled,
                    PasswordProfile = new PasswordProfile()
                    {
                        Password = userCreate.Password,
                        ForceChangePasswordNextSignIn =
                            userCreate.ForceChangePasswordNextSignIn ?? true
                    }
                };

                HttpContent jsonPasswordProfile =
                    graphClient.HttpProvider.Serializer.SerializeAsJsonContent(user);

                HttpRequestMessage request = graphClient.Users.Request().GetHttpRequestMessage();

                request.Content = jsonPasswordProfile;
                request.Method = HttpMethod.Post;

                string requestId = batchRequestContent.AddBatchRequestStep(request);

                requestIds.Add(requestId);
            }

            BatchResponseContent returnedResponse =
                await graphClient.Batch.Request().PostAsync(batchRequestContent);

            try
            {
                foreach (string requestId in requestIds)
                {
                    HttpResponseMessage response =
                        await returnedResponse.GetResponseByIdAsync(requestId);
                }
            }
            catch (ServiceException ex)
            {
                if (ex.Message.Contains(" 429 "))
                {
                    throw new HttpRequestException(ex.Message, ex.InnerException,
                        HttpStatusCode.TooManyRequests);
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
                HttpRequestMessage request = graphClient.Users[id].Request()
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

            BatchResponseContent returnedResponse =
                await graphClient.Batch.Request().PostAsync(batchRequestContent);

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
                        throw new HttpRequestException(ex.Message, ex.InnerException,
                            HttpStatusCode.TooManyRequests);
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
                IUserRequest userInfoRequest = graphClient.Users[id].Request()
                    .Select(
                        "aboutMe, birthday, hireDate, interests, mySite, pastProjects, preferredName, responsibilities, schools, skills, id");

                string userInfoRequstId = batchRequestContent.AddBatchRequestStep(userInfoRequest);

                IProfilePhotoRequest hashRequest = graphClient.Users[id].Photo.Request();

                string hashRequestId = batchRequestContent.AddBatchRequestStep(hashRequest);

                requestIds.Add(new UserIdAndRequestIds
                {
                    UserId = id,
                    UserInfoRequestId = userInfoRequstId,
                    HashRequestId = hashRequestId
                });
            }

            BatchResponseContent returnedResponse =
                await graphClient.Batch.Request().PostAsync(batchRequestContent);

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
                        throw new HttpRequestException(ex.Message, ex.InnerException,
                            HttpStatusCode.TooManyRequests);
                    }

                    Console.WriteLine($"UserId: {requestId.UserId}");
                    Console.WriteLine($"Get user failed: {ex.Error.Message}");
                }

                try
                {
                    ProfilePhoto profilePhoto = await returnedResponse
                        .GetResponseByIdAsync<ProfilePhoto>(requestId.HashRequestId);


                    /// По @odata.mediaEtag получаем хеш тип которого JsonElement
                    JsonElement? value =
                        profilePhoto.AdditionalData["@odata.mediaEtag"] as JsonElement?;

                    infoAndPhotoHash.PhotoHash = value.ToString();
                }
                catch (ServiceException ex)
                {
                    if (ex.Message.Contains(" 429 "))
                    {
                        throw new HttpRequestException(ex.Message, ex.InnerException,
                            HttpStatusCode.TooManyRequests);
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
                IUserRequest userInfoRequest = graphClient.Users[id].Request()
                    .Select(
                        "aboutMe, birthday, hireDate, interests, mySite, pastProjects, preferredName, responsibilities, schools, skills, id");

                string userInfoRequstId = batchRequestContent.AddBatchRequestStep(userInfoRequest);

                userInfoRequestIds.Add(userInfoRequstId);
            }

            foreach (string id in userIds)
            {
                IProfilePhotoRequest hashRequest = graphClient.Users[id].Photo.Request();

                string hashRequestId = batchRequestContent.AddBatchRequestStep(hashRequest);
                userIdByHashRequestIds.Add(id, hashRequestId);
            }

            BatchResponseContent returnedResponse =
                await graphClient.Batch.Request().PostAsync(batchRequestContent);

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
                        throw new HttpRequestException(ex.Message, ex.InnerException,
                            HttpStatusCode.TooManyRequests);
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
                    JsonElement? value =
                        profilePhoto.AdditionalData["@odata.mediaEtag"] as JsonElement?;

                    string photoHash = value.ToString();

                    UserInfoAndPhotoHash item = userInfoAndPhotoHash.FirstOrDefault(c =>
                        c.UserInfo.Id.Equals(userIdByHashRequestId.Key));

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
                        throw new HttpRequestException(ex.Message, ex.InnerException,
                            HttpStatusCode.TooManyRequests);
                    }

                    if (!ex.Message.Contains("ImageNotFound"))
                    {
                        Console.WriteLine($"Get photo hash failed: {ex.Error.Message}");
                    }
                }
            }

            return userInfoAndPhotoHash;
        }

        public async Task<List<KeyValuePair<string, string>>> GetUsersPhotoHashAsync(
            List<string> userIds)
        {
            Dictionary<string, string> userIdByRequestIds = new Dictionary<string, string>();
            List<KeyValuePair<string, string>> hashDictionarys =
                new List<KeyValuePair<string, string>>();

            BatchRequestContent batchRequestContent = new BatchRequestContent();

            foreach (string id in userIds)
            {
                IProfilePhotoRequest request = graphClient.Users[id].Photo.Request();

                string requestId = batchRequestContent.AddBatchRequestStep(request);

                userIdByRequestIds.Add(id, requestId);
            }

            BatchResponseContent returnedResponse =
                await graphClient.Batch.Request().PostAsync(batchRequestContent);

            foreach (KeyValuePair<string, string> userIdByRequest in userIdByRequestIds)
            {
                try
                {
                    object obj = await returnedResponse
                        .GetResponseByIdAsync<object>(userIdByRequest.Value);

                    var json = obj.ToString();

                    var type = JsonSerializer.Deserialize<ProfilePhoto>(json);
                }
                catch(Exception ex)
                {
                    
                }
                try
                {
                    ProfilePhoto profilePhoto = await returnedResponse
                        .GetResponseByIdAsync<ProfilePhoto>(userIdByRequest.Value);

                    /// По @odata.mediaEtag получаем хеш тип которого JsonElement
                    JsonElement? value =
                        profilePhoto.AdditionalData["@odata.mediaEtag"] as JsonElement?;

                    hashDictionarys.Add(
                        new KeyValuePair<string, string>(userIdByRequest.Key, value.ToString()));
                }
                catch (ServiceException e)
                {
                    if (!e.Message.Contains("ImageNotFound") && !e.Message.Contains("AadGraphCallFailed"))
                    {
                        Console.WriteLine($"Get user failed: {e.Error.Message}");
                    }

                    hashDictionarys.Add(
                        new KeyValuePair<string, string>(userIdByRequest.Key, e.Error.Message));
                }
            }

            return hashDictionarys;
        }

        public async Task<string> ResetUserPasswordAsync(string userId)
        {
            string password = passwordGenerator.GeneratePassword();

            User user = new User
            {
                PasswordProfile = new PasswordProfile
                {
                    Password = "mi59kLA7++",
                    ForceChangePasswordNextSignIn = false
                }
            };

            await graphClient.Users[userId]
                .Request()
                .UpdateAsync(user);

            return password;
        }

        public async Task DeleteUserAsync(string userId)
        {
            await graphClient.Users[userId]
                .Request()
                .DeleteAsync();
        }

        public async Task RestoreUserAsync(string userId)
        {
            await graphClient.Directory.DeletedItems[userId]
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

            Group group = await graphClient.Groups
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

            await graphClient.Groups[groupId].Members.References
                .Request()
                .AddAsync(user);
        }

        public async Task RemoveMemberFromGroupAsync(string groupId, string userId)
        {
            await graphClient.Groups[groupId].Members[userId].Reference
                .Request()
                .DeleteAsync();
        }

        string ptotoSize = "504x504";

        public async Task<string> GetPhotoHashAsync(string userId)
        {
            /// Получение информации о фото
            /// 

            var requestUrl = graphClient.Users[userId].Photos[ptotoSize].RequestUrl;

            //var newRequestUrl = graphClient.Users[userId].Photo.RequestUrl;

            ProfilePhoto profilePhoto = await graphClient.Users[userId].Photos[ptotoSize]
                .Request()
                .GetAsync();

            /// По @odata.mediaEtag получаем хеш тип которого JsonElement
            JsonElement? value = profilePhoto.AdditionalData["@odata.mediaEtag"] as JsonElement?;

            return value?.ToString();
        }

        public async Task<byte[]> GetUserPhotoAsync(string userId)
        {
            Stream stream = await graphClient.Users[userId].Photos[ptotoSize].Content
                .Request()
                .GetAsync();

            /// MemoryStream было использовано для получения byte[]
            await using MemoryStream memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        public async Task<Dictionary<string, string>> GetPhotoHashAsync1(string userId)
        {
            /// Получение информации о фото
            /// 

            Dictionary<string, string> sizeByPhotoHash = new Dictionary<string, string>();

            IList<ProfilePhoto> profilePhotos = await graphClient.Users[userId].Photos
                .Request()
                .GetAsync();

            foreach (ProfilePhoto profilePhoto in profilePhotos)
            {
                JsonElement? hash = profilePhoto.AdditionalData["@odata.mediaEtag"] as JsonElement?;
                sizeByPhotoHash.Add(profilePhoto.Id, hash.ToString());
            }

            return sizeByPhotoHash;
        }

        public async Task GetPhotoHashAsync2(List<string> ids)
        {
            /// Получение информации о фото
            /// 
            BatchRequestContent batchRequestContent = new();
            List<UserIdAndRequestId> userIdAndRequestIds = new List<UserIdAndRequestId>();

            foreach (string id in ids)
            {
                IUserPhotosCollectionRequest request = graphClient.Users[id].Photos
                    .Request();

                string requestId = batchRequestContent.AddBatchRequestStep(request);

                userIdAndRequestIds.Add(new UserIdAndRequestId
                {
                    UserId = id,
                    RequestId = requestId
                });
            }

            BatchResponseContent responseContent =
                await graphClient.Batch.Request().PostAsync(batchRequestContent);

            for (int i = 0; i < userIdAndRequestIds.Count; i++)
            {
                try
                {
                    IUserPhotosCollectionPage profilePhoto1;
                    IDictionary<string, JsonElement?> profilePhoto2;
                    object profilePhoto3;
                    object photos1;
                    UserPhotosCollectionPage profilePhoto4;

                    string json1;
                    string json2;
                    string json3;
                    Type type1;

                    try
                    {
                        UserPhotosCollectionResponse profilePhoto5 = await responseContent
                            .GetResponseByIdAsync<UserPhotosCollectionResponse>(userIdAndRequestIds[i].RequestId);

                        //profilePhoto5.Value;
                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {
                        photos1 = await graphClient.Users[userIdAndRequestIds[i].UserId]
                            .Photos.Request().GetAsync();
                        type1 = photos1.GetType();
                        json1 = JsonSerializer.Serialize(photos1);
                    }
                    catch (Exception ex)
                    {

                    }
                    //



                    try
                    {
                        GraphResponse<UserPhotosCollectionResponse> profilePhoto5 = await responseContent
                            .GetResponseByIdAsync<GraphResponse<UserPhotosCollectionResponse>>(userIdAndRequestIds[i].RequestId);

                        json3 = profilePhoto5.ToString();

                        var type = JsonSerializer.Deserialize<IUserPhotosCollectionPage>(json3);
                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {
                        var response = await responseContent
                            .GetResponseByIdAsync(userIdAndRequestIds[i].RequestId);

                        json2 = await response.Content.ReadAsStringAsync();

                        //var type = JsonSerializer.Deserialize<IList<ProfilePhoto>>(json);
                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {
                        profilePhoto3 = await responseContent
                            .GetResponseByIdAsync<object>(userIdAndRequestIds[i].RequestId);

                        json3 = profilePhoto3.ToString();

                        var type = JsonSerializer.Deserialize<IUserPhotosCollectionPage>(json3);
                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {
                        profilePhoto4 = await responseContent
                            .GetResponseByIdAsync<UserPhotosCollectionPage>(userIdAndRequestIds[i].RequestId);
                        var type = profilePhoto4.GetType();
                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {
                        profilePhoto1 = await responseContent
                            .GetResponseByIdAsync<IUserPhotosCollectionPage>(userIdAndRequestIds[i].RequestId);
                        var type = profilePhoto1.GetType();
                    }
                    catch (Exception ex)
                    {

                    }
                    try
                    {
                        profilePhoto2 = await responseContent
                            .GetResponseByIdAsync<IDictionary<string, JsonElement?>>(userIdAndRequestIds[i].RequestId);
                        var type = profilePhoto2.GetType();
                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {
                        IUserPhotosCollectionPage photos = await graphClient.Users[userIdAndRequestIds[i].UserId]
                            .Photos.Request().GetAsync();
                    }
                    catch (Exception ex)
                    {

                    }

                    //foreach(var photoInfo in profilePhoto)
                    //{
                    //    var type = photoInfo.Value.GetType();
                    //}

                    //JsonElement? jsonElement = profilePhoto["value"];

                    //IList<ProfilePhoto> profilePhotos = JsonSerializer.Deserialize<IList<ProfilePhoto>>(jsonElement.ToString());

                    //IList<ProfilePhoto> profilePhotos1 = profilePhotos.OrderBy(c => c.Id).ToList();
                    //IList<ProfilePhoto> profilePhotos2 = profilePhotos.OrderByDescending(c => c.Id).ToList();

                    //ProfilePhoto profilePhoto = await responseContent
                    //    .GetResponseByIdAsync<ProfilePhoto>(userIdAndRequestIds[i].RequestId);

                    /// По @odata.mediaEtag получаем хеш тип которого JsonElement
                    //JsonElement? photoHashJsonElement =
                    //    profilePhoto.AdditionalData[MediaEtag] as JsonElement?;

                    //azureADExternalResources.Add(new AzureADExternalResources
                    //{
                    //    Id = userIdAndRequestIds[i].UserId,
                    //    PhotoHash = photoHashJsonElement.ToString()
                    //});
                }
                catch (Exception e)
                {
                    /// TODO : JSG Сделать проверку на TooManyRequests 429

                }
            }
        }

        public async Task<Dictionary<string, byte[]>> GetUserPhotoAsync1(string userId, List<string> sizes)
        {
            Dictionary<string, byte[]> photosBytes = new Dictionary<string, byte[]>();

            foreach (string size in sizes)
            {
                Stream stream = await graphClient.Users[userId].Photos[size].Content
                    .Request()
                    .GetAsync();

                /// MemoryStream было использовано для получения byte[]
                await using MemoryStream memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);

                photosBytes.Add(size, memoryStream.ToArray());
            }

            return photosBytes;
        }

        public async Task UpdateUserPhotoAsync(string userId, byte[] imageBytes)
        {
            /// Приведение массива байтов к Stream
            await using MemoryStream stream = new MemoryStream(imageBytes);

            await graphClient.Users[userId].Photo.Content
                .Request()
                .PutAsync(stream);
        }

        private GraphServiceClient GetGraphServiceClient()
        {
            //const string tenantId = "5c66821f-81e8-4faa-a800-b3fa3f2e27c0";
            //const string clientId = "31d2d6a6-f6ff-4fcc-960b-e50979fe69d8";
            //const string clientSecret = "f2i7Q~JLJlWDGmfUcEnjpEbGlg3~OaTSEvkBa";

            //const string clientId = "dd065605-c1f4-4010-8865-d439d8755d36";
            //const string tenantId = "f5cdac32-7fd2-4de4-bbe3-dcbe75c73a7f";
            //const string clientSecret = "Aq08Q~TehRVQmuZDEwLJf9.lcFeAs~daTPr_tbaM";

            const string clientId = "4e9c21e1-c711-4643-ba09-efb9440014e7";
            const string tenantId = "a5885e21-2ba8-4195-9a88-3756d1fe9453";
            const string clientSecret = "ugd8Q~kIvOVjTIpMCf7a7IXSzjULv0dIVdgDscTr";

            /// "/.default" подтягивает perissions приложения
            string[] scopes = {"https://graph.microsoft.com/.default"};

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
            string[] scopes = {"https://graph.microsoft.com/.default"};

            TokenCredentialOptions options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            ClientSecretCredential clientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret, options);

            TokenCredentialAuthProvider authProvider = new TokenCredentialAuthProvider(
                clientSecretCredential, scopes);

            IList<DelegatingHandler> handlers =
                GraphClientFactory.CreateDefaultHandlers(authProvider);

            DelegatingHandler compressionHandler =
                handlers.Where(h => h is RetryHandler).FirstOrDefault();
            handlers.Remove(compressionHandler);

            HttpClient httpClient = GraphClientFactory.Create(handlers);

            return new GraphServiceClient(httpClient);
        }

        /// Возвращает SharePoint Online свойства 
        private async Task<User> GetUserAdditionalInfoAsync(string userId)
        {
            return await graphClient.Users[userId].Request()
                //.Select(SelectExternalResourcesExpression)
                .Select("preferredName")
                .GetAsync();
        }

        private void GetUserAdditionalInfoAsync1(string userId)
        {
            graphClient.Users[userId].Request()
                .GetAsync();
        }

        private async Task<PasswordPolicies> GetUserPasswordPoliciesAsync(string userId)
        {
            User user = await graphClient.Users[userId]
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
            await graphClient.Users[userId]
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

            await graphClient.Users[userId]
                .Request()
                .UpdateAsync(user);
        }

        private class UserIdAndRequestId
        {
            public string UserId { get; set; }
            public string RequestId { get; set; }
        }

    }


    //public class SignInActivityInfo
    //{
    //    public string userId { get; set; }
    //    public SignInActivity SignInActivity { get; set; }
    //}
}
