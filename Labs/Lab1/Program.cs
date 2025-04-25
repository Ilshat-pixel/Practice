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

        if (!string.IsNullOrEmpty(validationError))
        {
            return validationError;
        }

        // Обработка строки
        string processedString = ProcessValidString(input);

        // Подсчет символов
        var charCounts = GetCharacterCounts(processedString);

        // Формирование результата
        return BuildResult(processedString, charCounts);
    }

    static string ProcessValidString(string input)
    {
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

    static Dictionary<char, int> GetCharacterCounts(string str)
    {
        return str.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
    }

    static string BuildResult(string processedString, Dictionary<char, int> charCounts)
    {
        var result = new StringBuilder();
        result.AppendLine($"Обработанная строка: {processedString}");
        result.AppendLine("Количество повторений каждого символа:");

        foreach (var pair in charCounts.OrderBy(p => p.Key))
        {
            result.AppendLine($"'{pair.Key}': {pair.Value} раз(а)");
        }

        return result.ToString();
    }
}
