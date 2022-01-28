using System;

namespace Azure
{
    class Program
    {
        private static readonly AzureClient _azureClient = new AzureClient();
        static void Main(string[] args)
        {
            CreateUser();
            ResetPassword();
        }

        public static void CreateUser()
        {
            try
            {
                _azureClient.CreateUser("First user", "FirstUser", "FirstUser@dimabondarenko888gmail.onmicrosoft.com").GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
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
