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
            UpdateUser(userCredentials.Id).GetAwaiter().GetResult();
            try
            {
                _azureClient.SetAccountEnabled(userCredentials.Id, false).GetAwaiter().GetResult();
                UserSharePoint userSharePoint = _azureClient.GetUserSharePoint(userCredentials.Id).GetAwaiter().GetResult();
                string userPasswordPolicies = _azureClient.GetUserPasswordPolicies(userCredentials.Id).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            try
            {
                _azureClient.DeleteUser(userCredentials.Id).GetAwaiter().GetResult();
                _azureClient.RestoreUser(userCredentials.Id).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            //ResetPassword();
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
                {"BusinessPhones", new List<string>{ "7201010" }},
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

        static async Task GetUserSharePoint(string userId)
        {
            Console.WriteLine();
            try
            {
                UserSharePoint userSharePoint = await _azureClient.GetUserSharePoint(userId);
                Console.WriteLine("GetUserSharePoint was success");
                Console.WriteLine(@$"AboutMe - {userSharePoint.AboutMe}");
                Console.WriteLine($"Birthday - {userSharePoint.Birthday}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine();
        }


        public static void ResetPassword()
        {
            try
            {
                _azureClient.ResetUserPassword("055c4599-9a4a-4678-b64f-0d5d9841d8a2").GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
