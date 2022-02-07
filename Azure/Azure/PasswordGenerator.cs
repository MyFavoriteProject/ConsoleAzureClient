using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Azure
{
    public class PasswordGenerator
    {
        private readonly Random _random;
        private const int PasswordLength = 10; /// произвольная длинна пароля 
        private const string ValidChars = "QWERTYUIOPASDFGHJKLZXCVBNM" + "qwertyuiopasdfghjklzxcvbnm" + "0123456789" +
                                          "!@#$%^&*()-_=+<,>."; /// Валидные символы для пароля

        public PasswordGenerator()
        {
            _random = new Random();
        }

        public string GetPassword()
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < PasswordLength; i++)
            {
                int charIndex = _random.Next(0, ValidChars.Length - 1);
                stringBuilder.Append(ValidChars[charIndex]);
            }

            return stringBuilder.ToString();
        }
    }
}
