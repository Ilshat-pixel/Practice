using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(ProcessString("a"));
        Console.WriteLine(ProcessString("abcdef"));
        Console.WriteLine(ProcessString("abcde"));

        Console.Write("Введите строку: ");
        string userInput = Console.ReadLine();
        Console.WriteLine(ProcessString(userInput));
    }

    static string ProcessString(string input)
    {
        string validationError = ValidateInputWithRegex(input);

        if (validationError != null)
        {
            return validationError;
        }

        int length = input.Length;

        if (length % 2 == 0)
        {
            int half = length / 2;
            string firstPart = ReverseString(input.Substring(0, half));
            string secondPart = ReverseString(input.Substring(half));
            return firstPart + secondPart;
        }
        else
        {
            string reversed = ReverseString(input);
            return reversed + input;
        }
    }

    static string ValidateInputWithRegex(string input)
    {
        // Регулярное выражение для проверки на строчные английские буквы
        var regex = new Regex(@"^[a-z]+$");

        if (!regex.IsMatch(input))
        {
            // Находим все недопустимые символы
            var invalidChars = input
                .Where(c => !char.IsLower(c) || c < 'a' || c > 'z')
                .Distinct()
                .ToArray();

            if (invalidChars.Length > 0)
            {
                var errorMessage = new StringBuilder(
                    "Ошибка: в строке содержатся недопустимые символы: "
                );
                foreach (var c in invalidChars)
                {
                    errorMessage.Append($"'{c}' ");
                }
                return errorMessage.ToString();
            }
        }

        return string.Empty;
    }

    static string ReverseString(string s)
    {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
}
