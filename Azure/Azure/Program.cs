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
            Dictionary<string, string> _idHashDictionary = new Dictionary<string, string>();

            UserCredentials userCredentials = CreateUser().GetAwaiter().GetResult();

            string userId = userCredentials?.Id ?? "8a7cca23-9049-495e-b1d3-e6b28877ece8";
            
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

            UpdateUserPhoto(userId, "C:\\Users\\d.bondarenko\\Documents\\GitHub\\ConsoleAzureClient\\Azure\\Azure\\Img\\IMG_3615.JPG");

            try
            {
                string photoHash = _azureClient.GetPhotoHash(userId).GetAwaiter().GetResult();
                _idHashDictionary.Add(userId, photoHash);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            UpdateUserPhoto(userId, "C:\\Users\\d.bondarenko\\Documents\\GitHub\\ConsoleAzureClient\\Azure\\Azure\\Img\\IMG_3614.JPG");

            try
            {
                string newPhotoHash = _azureClient.GetPhotoHash(userId).GetAwaiter().GetResult();

                if (_idHashDictionary.TryGetValue(userId, out string oldPhotoHash))
                {
                    if (!newPhotoHash.Equals(oldPhotoHash))
                    {

                        byte[] imageBytes = _azureClient.GetUserPhoto(userId).GetAwaiter().GetResult();

                        File.WriteAllBytes(@"C:\Users\d.bondarenko\Documents\GitHub\ConsoleAzureClient\Azure\Azure\Img\ImageFromAzureAD.JPG",
                            imageBytes); // Вызов этой функции нужен для того чтобы проверить что полученное изображение соответствует тому что находится на странице пользователя
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            UpdateAdditionalInfoUser(userId).GetAwaiter().GetResult();

            try
            {
                UserSharePoint userSharePoint = GetUserAdditionalInfo(userId).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async Task<UserCredentials> CreateUser()
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

        public static async Task UpdateUser(string userId)
        {
            Console.WriteLine();

            Dictionary<string, object> propertyValueDictionary = new Dictionary<string, object>
            {
                {"BusinessPhones", new List<string>{ "937-99-92" }},
                {"GivenName", "Some One"},
                {"PasswordPolicies", "DisableStrongPassword"},
            };
            try
            {
                await _azureClient.UpdateUser(userId, propertyValueDictionary);
                Console.WriteLine("UpdateUser was success");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine();
        }

        public static async Task UpdateAdditionalInfoUser(string userId)
        {
            Console.WriteLine();

            Dictionary<string, object> propertyValueDictionary = new Dictionary<string, object>
            {
                {"AboutMe", "Im Dev"},
                {"Skills", new List<string>{"C#", ".Net", "Drink coffee"}},
                {"Birthday", DateTimeOffset.Now.AddYears(-20)}
            };

            try
            {
                await _azureClient.UpdateUser(userId, propertyValueDictionary);
                Console.WriteLine("UpdateAdditionalInfoUser was success");
            }
            catch (Exception e) /// 404 Not found Issues: https://docs.microsoft.com/en-us/graph/known-issues#access-to-user-resources-is-delayed-after-creation
            {
                Console.WriteLine(e);
            }

            Console.WriteLine();
        }
        
        static async Task<UserSharePoint> GetUserAdditionalInfo(string userId)
        {
            Console.WriteLine();
            UserSharePoint userSharePoint = default;
            try
            {
                userSharePoint = await _azureClient.GetUserAdditionalInfo(userId);
                Console.WriteLine("GetUserAdditionalInfo was success");
                Console.WriteLine(@$"AboutMe - {userSharePoint.AboutMe}");
                Console.WriteLine($"Birthday - {userSharePoint.Birthday}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine();

            return userSharePoint;
        }
        
        public static void ResetPassword(string userId)
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

        public static void UpdateUserPhoto(string userId, string imgPath)
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
