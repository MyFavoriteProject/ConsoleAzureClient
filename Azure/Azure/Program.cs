using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Models;

namespace Azure
{
    class Program
    {
        private static readonly AzureClient _azureClient = new AzureClient();

        static void Main(string[] args)
        {
            UserCredentials userCredentials = CreateUser().GetAwaiter().GetResult();

            string userId = userCredentials?.Id; /// Если необходимо обратится к созданому пользователю
            //string userId = "56f5423e-3eee-4dbf-a286-baf96e795e8a"; /// Если пользователь уже создан, и нужно обращение к нему

            UpdateUser(userId).GetAwaiter().GetResult();

            try
            {
                _azureClient.SetAccountEnabled(userId, false).GetAwaiter().GetResult();
                string userPasswordPolicies = _azureClient.GetUserPasswordPolicies(userId).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            try
            {
                string groupId = _azureClient.CreateGroup("Library Assist", "library", true, false).GetAwaiter().GetResult();
                _azureClient.AddMemberInGroup(groupId, userId).GetAwaiter().GetResult();
                _azureClient.RemoveMemberFromGroup(groupId, userId).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            try
            {
                _azureClient.DeleteUser(userId).GetAwaiter().GetResult();
                _azureClient.RestoreUser(userId).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            ResetPassword(userId);

            Task.Delay(120000).GetAwaiter().GetResult(); /// Ожидает 2 минуты(120000 мс) для обновление фото 

            Dictionary<string, string> idByHashDictionary = new Dictionary<string, string>(); /// Запись по id значений хеша картинок 

            UpdateUserPhoto(userId, "C:\\Users\\d.bondarenko\\Documents\\GitHub\\ConsoleAzureClient\\Azure\\Azure\\Img\\IMG_3615.JPG"); /// Путь указан статической картинки для обновления у пользователя

            string photoHash = string.Empty;

            try
            {
                photoHash = _azureClient.GetPhotoHash(userId).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            idByHashDictionary.Add(userId, photoHash);

            UpdateUserPhoto(userId, "C:\\Users\\d.bondarenko\\Documents\\GitHub\\ConsoleAzureClient\\Azure\\Azure\\Img\\IMG_3614.JPG"); /// Путь указан статической картинки для обновления у пользователя

            try
            {
                string newPhotoHash = _azureClient.GetPhotoHash(userId).GetAwaiter().GetResult();

                if (idByHashDictionary.TryGetValue(userId, out string oldPhotoHash)) /// Получает значения хеша которое было записано ранее 
                {
                    if (!newPhotoHash.Equals(oldPhotoHash)) /// Если хеши не соответсвуют, идёт получение byte[] нового изображения и запись в файл
                    {
                        byte[] imageBytes = _azureClient.GetUserPhoto(userId).GetAwaiter().GetResult();

                        File.WriteAllBytes(@"C:\Users\d.bondarenko\Documents\GitHub\ConsoleAzureClient\Azure\Azure\Img\ImageFromAzureAD.JPG",
                            imageBytes); /// Вызов этой функции нужен для того чтобы проверить что полученное изображение соответствует тому что находится на странице пользователя
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            UpdateAdditionalInfoUser(userId).GetAwaiter().GetResult(); /// Обновление атрибутов SPO у пользователя лучше проводить спустя несколько минут после создания
                                                                       /// Issues: https://docs.microsoft.com/en-us/graph/known-issues#access-to-user-resources-is-delayed-after-creation

            try
            {
                UserAdditionalInfo userAdditionalInfo = GetUserAdditionalInfo(userId).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static async Task<UserCredentials> CreateUser()
        {
            Console.WriteLine();

            UserCredentials userCredentials = default;

            try
            {
                userCredentials = await _azureClient.CreateUser("First user", "FirstUser", "FirstUser@dimabondarenko888gmail.onmicrosoft.com");

                Console.WriteLine("Created user:");
                Console.WriteLine($"User Id - {userCredentials.Id}");
                Console.WriteLine($"User PrincipalName - {userCredentials.UserPrincipalName}");
                Console.WriteLine($"User password - {userCredentials.Password}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine();

            return userCredentials;
        }

        static async Task UpdateUser(string userId) /// Обновление только Azure AD свойств 
        {
            Console.WriteLine();

            Dictionary<string, object> propNameByValueDictionary = new Dictionary<string, object>
            {
                { "BusinessPhones", new List<string>{ "937-99-92" }},
                { "GivenName", "Some One" },
                { "PasswordPolicies", "DisableStrongPassword" },
            }; /// Формирования словаря по имени свойства и значения этого свойства 
            try
            {
                await _azureClient.UpdateUser(userId, propNameByValueDictionary);
                Console.WriteLine("UpdateUser was success");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine();
        }

        static async Task UpdateAdditionalInfoUser(string userId) /// Обновление только SharePoint Online свойств 
        {
            Console.WriteLine();

            Dictionary<string, object> propNameByValueDictionary = new Dictionary<string, object>
            {
                { "AboutMe", "Im Dev" },
                { "Skills", new List<string>{"C#", ".Net", "Drink coffee"} },
                { "Birthday", DateTimeOffset.Now.AddYears(-20) },
            }; /// Формирования словаря по имени свойства и значения этого свойства 

            try
            {
                await _azureClient.UpdateUser(userId, propNameByValueDictionary);
                Console.WriteLine("UpdateAdditionalInfoUser was success");
            }
            catch (Exception e) /// 404 Not found Issues: https://docs.microsoft.com/en-us/graph/known-issues#access-to-user-resources-is-delayed-after-creation
            {
                Console.WriteLine(e);
            }

            Console.WriteLine();
        }
        
        static async Task<UserAdditionalInfo> GetUserAdditionalInfo(string userId)
        {
            Console.WriteLine();
            UserAdditionalInfo userAdditionalInfo = default;
            try
            {
                userAdditionalInfo = await _azureClient.GetUserAdditionalInfo(userId);
                Console.WriteLine("GetUserAdditionalInfo was success");
                Console.WriteLine($"AboutMe - {userAdditionalInfo.AboutMe}");
                Console.WriteLine($"Birthday - {userAdditionalInfo.Birthday}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine();

            return userAdditionalInfo;
        }
        
        static void ResetPassword(string userId)
        {
            Console.WriteLine();
            try
            {
                string newPassword = _azureClient.ResetUserPassword(userId).GetAwaiter().GetResult();
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
                byte[] imageBytes = File.ReadAllBytes(imgPath);

                _azureClient.UpdateUserPhoto(userId, imageBytes).GetAwaiter().GetResult();

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
