using System;
using System.Text;

namespace Azure
{
    public class PasswordGenerator
    {
        private readonly Random _random;

        /// произвольная длинна пароля 
        private const int PasswordLength = 10;
        /// Валидные символы для пароля
        private const string ValidChars = "QWERTYUIOPASDFGHJKLZXCVBNM" + "qwertyuiopasdfghjklzxcvbnm" + 
                                          "0123456789" + "!@#$%^&*()-_=+<,>."; 

        public PasswordGenerator()
        {
            _random = new Random();
        }

        public string GetPassword()
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < PasswordLength; i++)
            {
                /// Генерирует индекс валидного символа
                int charIndex = _random.Next(0, ValidChars.Length - 1);

                /// Записать символ по индексу 
                stringBuilder.Append(ValidChars[charIndex]); 
            }

            return stringBuilder.ToString();
        }
    }
}
