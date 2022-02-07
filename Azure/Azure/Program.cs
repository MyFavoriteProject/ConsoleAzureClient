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
            UserCredential userCredential = CreateUser();

            /// Если необходимо обратится к созданому пользователю
            string userId = userCredential.Id; 

            UpdateUser(userId);

            try
            {
                _azureClient.SetAccountEnabledAsync(userId, false).GetAwaiter().GetResult();
                string userPasswordPolicies = _azureClient.GetUserPasswordPoliciesAsync(userId).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            try
            {
                /// Входными параметрами передаются значения для создания группы 
                string groupId = _azureClient.CreateGroupAsync("Library Assist", "library", true, false).GetAwaiter().GetResult();
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
            UpdateAdditionalInfoUser(userId); 

            try
            {
                UserAdditionalInfo userAdditionalInfo = GetUserAdditionalInfo(userId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static UserCredential CreateUser()
        {
            Console.WriteLine();

            UserCredential userCredential = default;

            try
            {
                /// Входными параметрами передаются значения для создания юзера
                userCredential = _azureClient.CreateUserAsync("First user", 
                    "FirstUser", 
                    "FirstUser@dimabondarenko888gmail.onmicrosoft.com").GetAwaiter().GetResult();
                
                Console.WriteLine("Created user:"); 
                Console.WriteLine($"User Id - {userCredential.Id}");
                Console.WriteLine($"User PrincipalName - {userCredential.UserPrincipalName}");
                Console.WriteLine($"User password - {userCredential.Password}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine();

            return userCredential;
        }

        /// Обновление только Azure AD свойств 
        static void UpdateUser(string userId) 
        {
            Console.WriteLine();

            /// Формирования словаря по имени свойства и значения этого свойства 
            Dictionary<string, object> propNameByValueDictionary = new Dictionary<string, object>
            {
                { "BusinessPhones", new List<string>{ "937-99-92" }},
                { "GivenName", "Some One" },
                { "PasswordPolicies", "DisableStrongPassword" },
            };

            try
            {
                _azureClient.UpdateUserAsync(userId, propNameByValueDictionary).GetAwaiter().GetResult();
                Console.WriteLine("UpdateUser was success");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine();
        }

        /// Обновление только SharePoint Online свойств 
        static void UpdateAdditionalInfoUser(string userId) 
        {
            Console.WriteLine();

            /// Формирования словаря по имени свойства и значения этого свойства 
            Dictionary<string, object> propNameByValueDictionary = new Dictionary<string, object>
            {
                { "AboutMe", "Im Dev" },
                { "Skills", new List<string>{"C#", ".Net", "Drink coffee"} },
                { "Birthday", DateTimeOffset.Now.AddYears(-20) },
            }; 

            try
            {
                _azureClient.UpdateUserAsync(userId, propNameByValueDictionary).GetAwaiter().GetResult();
                Console.WriteLine("UpdateAdditionalInfoUser was success");
            }
            catch (Exception e) /// If 404 Not found. Issues: https://docs.microsoft.com/en-us/graph/known-issues#access-to-user-resources-is-delayed-after-creation
            {
                Console.WriteLine(e);
            }

            Console.WriteLine();
        }
        
        static UserAdditionalInfo GetUserAdditionalInfo(string userId)
        {
            Console.WriteLine();
            UserAdditionalInfo userAdditionalInfo = default;
            try
            {
                userAdditionalInfo = _azureClient.GetUserAdditionalInfoAsync(userId).GetAwaiter().GetResult();
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
