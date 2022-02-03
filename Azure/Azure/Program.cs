using System;
using System.Collections.Generic;
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

            string userId = userCredentials.Id;

            UpdateAdditionalInfoUser("56f5423e-3eee-4dbf-a286-baf96e795e8a").GetAwaiter().GetResult();
            UpdateUser(userId).GetAwaiter().GetResult();

            try
            {
                _azureClient.SetAccountEnabled(userId, false).GetAwaiter().GetResult();
                UserSharePoint userSharePoint = GetUserAdditionalInfo(userId).GetAwaiter().GetResult();
                string userPasswordPolicies = _azureClient.GetUserPasswordPolicies(userId).GetAwaiter().GetResult();
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
    }
}
