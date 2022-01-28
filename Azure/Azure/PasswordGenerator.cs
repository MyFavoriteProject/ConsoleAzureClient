using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Azure
{
    public class PasswordGenerator
    {
        private readonly RNGCryptoServiceProvider _cryptoProvider;
        private readonly Random _random;
        private readonly int _minPasswordLength = 8;
        private readonly int _maxPasswordLength = 16;
        public const string CapitalLetters = "QWERTYUIOPASDFGHJKLZXCVBNM";
        public const string SmallLetters = "qwertyuiopasdfghjklzxcvbnm";
        public const string Digits = "0123456789";
        public const string SpecialCharacters = "!@#$%^&*()-_=+<,>.";

        public PasswordGenerator()
        {
            _cryptoProvider = new RNGCryptoServiceProvider();
            _random = new Random();
        }

        public string GetPassword()
        {
            int passwordLength = _random.Next(_minPasswordLength, _maxPasswordLength);

            string validChars = CapitalLetters + Digits + SmallLetters + SpecialCharacters;

            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < passwordLength; i++)
            {
                stringBuilder = stringBuilder.Append(GenerateChar(validChars));
            }

            return stringBuilder.ToString();
        }

        private char GenerateChar(string availableChars)
        {
            var byteArray = new byte[1];
            char generateChar;
            do
            {
                _cryptoProvider.GetBytes(byteArray);
                generateChar = (char)byteArray[0];

            } while (!availableChars.Any(c => c == generateChar));

            return generateChar;
        }
    }
}
