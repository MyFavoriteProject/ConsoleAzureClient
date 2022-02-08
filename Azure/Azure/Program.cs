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
                    ForceChangePasswordNextSignIn = true
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
