using System.Text;
using System.Text.RegularExpressions;
using LabsAPI.Model;

namespace LabsAPI.Handlers;

public interface IProcessHandler
{
    public Task<ResultModel> GetResultAsync(string inputString, bool isQuickSort);
}

public class ProcessHandler : IProcessHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiUrl;
    private readonly List<string> _blacklistedWords;
    private readonly Random _random = new();
    private static readonly HashSet<char> Vowels = new() { 'a', 'e', 'i', 'o', 'u', 'y' };

    public ProcessHandler(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _apiUrl = configuration.GetValue<string>("RandomNumberApi")!;
        _blacklistedWords = configuration.GetSection("BlacklistedWords").Get<List<string>>();
    }

    public async Task<ResultModel> GetResultAsync(string inputString, bool isQuickSort)
    {
        var validationError = ValidateInputWithRegex(inputString);

        if (!string.IsNullOrEmpty(validationError))
        {
            return new ResultModel { Error = validationError };
        }

        if (_blacklistedWords.Contains(inputString.ToLower()))
        {
            return new ResultModel
            {
                Error = $"Ошибка: строка '{inputString}' находится в чёрном списке"
            };
        }
        string processedString = ProcessValidString(inputString);
        int randomIndex = await GetRandomIndex(processedString.Length);
        string trimmedString = RemoveCharAtPosition(processedString, randomIndex);
        var charCounts = GetCharacterCounts(processedString);
        string maxVowelSubstring = FindMaxVowelSubstring(processedString);

        // В реальном приложении выбор сортировки можно передавать как параметр
        string sortedString = QuickSortString(processedString); // По умолчанию QuickSort

        return new ResultModel
        {
            ProcessedString = processedString,
            CharacterCounts = charCounts,
            MaxVowelSubstring = maxVowelSubstring,
            SortedString = sortedString,
            RemovedCharIndex = randomIndex,
            RemovedChar = processedString[randomIndex],
            TrimmedString = trimmedString
        };
    }

    private async Task<string> ProcessString(string input, bool isQuickSort)
    {
        string validationError = ValidateInputWithRegex(input);

        if (!string.IsNullOrEmpty(validationError))
        {
            return validationError;
        }

        // Обработка строки
        string processedString = ProcessValidString(input);

        // Получаем случайное число
        int randomIndex = await GetRandomIndex(processedString.Length);
        string trimmedString = RemoveCharAtPosition(processedString, randomIndex);

        // Подсчет символов
        var charCounts = GetCharacterCounts(processedString);

        string maxVowelSubstring = FindMaxVowelSubstring(processedString);

        string sortedString = isQuickSort
            ? QuickSortString(processedString)
            : TreeSortString(processedString);

        // Формирование результата
        return BuildResult(
            processedString,
            charCounts,
            maxVowelSubstring,
            sortedString,
            trimmedString,
            randomIndex
        );
    }

    private async Task<int> GetRandomIndex(int maxValue)
    {
        if (maxValue <= 0)
            return 0;

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            string apiUrl = $"{_apiUrl}?min=0&max={maxValue - 1}&count=1";
            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                if (int.TryParse(content.Trim('[', ']'), out int result))
                {
                    return result;
                }
            }
        }
        catch
        {
            // Fall through to local random if API fails
        }

        return _random.Next(0, maxValue);
    }

    static string RemoveCharAtPosition(string str, int index)
    {
        if (string.IsNullOrEmpty(str) || index < 0 || index >= str.Length)
            return str;

        return str.Remove(index, 1);
    }

    static string FindMaxVowelSubstring(string str)
    {
        int maxLength = 0;
        string result = "";
        List<int> vowelIndices = new List<int>();

        for (int i = 0; i < str.Length; i++)
        {
            if (Vowels.Contains(str[i]))
            {
                vowelIndices.Add(i);
            }
        }

        if (vowelIndices.Count < 2)
        {
            return "Нет подходящей подстроки (требуется минимум 2 гласные)";
        }

        for (int i = 0; i < vowelIndices.Count - 1; i++)
        {
            for (int j = i + 1; j < vowelIndices.Count; j++)
            {
                int start = vowelIndices[i];
                int end = vowelIndices[j];
                int length = end - start + 1;

                if (length > maxLength)
                {
                    maxLength = length;
                    result = str.Substring(start, length);
                }
            }
        }

        return result;
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

    static string BuildResult(
        string processedString,
        Dictionary<char, int> charCounts,
        string maxVowelSubstring,
        string sortedString,
        string trimmedString,
        int removedIndex
    )
    {
        var result = new StringBuilder();
        result.AppendLine($"Обработанная строка: {processedString}");
        result.AppendLine("Количество повторений каждого символа:");

        foreach (var pair in charCounts.OrderBy(p => p.Key))
        {
            result.AppendLine($"'{pair.Key}': {pair.Value} раз(а)");
        }

        result.AppendLine($"Самая длинная подстрока между гласными: {maxVowelSubstring}");
        result.AppendLine($"Отсортированная строка: {sortedString}");
        result.AppendLine(
            $"Удалён символ на позиции {removedIndex}: '{processedString[removedIndex]}'"
        );
        result.AppendLine($"Урезанная строка: {trimmedString}");
        return result.ToString();
    }

    static string QuickSortString(string s)
    {
        if (s.Length <= 1)
            return s;

        char[] chars = s.ToCharArray();
        QuickSort(chars, 0, chars.Length - 1);
        return new string(chars);
    }

    static void QuickSort(char[] arr, int left, int right)
    {
        if (left < right)
        {
            int pivot = Partition(arr, left, right);
            QuickSort(arr, left, pivot - 1);
            QuickSort(arr, pivot + 1, right);
        }
    }

    static int Partition(char[] arr, int left, int right)
    {
        char pivot = arr[right];
        int i = left - 1;

        for (int j = left; j < right; j++)
        {
            if (arr[j] <= pivot)
            {
                i++;
                Swap(ref arr[i], ref arr[j]);
            }
        }

        Swap(ref arr[i + 1], ref arr[right]);
        return i + 1;
    }

    static void Swap(ref char a, ref char b)
    {
        (a, b) = (b, a);
    }

    // Реализация TreeSort для строки
    static string TreeSortString(string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;

        var tree = new BinaryTree();
        foreach (char c in s)
        {
            tree.Insert(c);
        }

        return tree.InOrderTraversal();
    }

    public Task<ResultModel> GetResultAsync(string inputString)
    {
        throw new NotImplementedException();
    }
}

class BinaryTree
{
    private Node root;

    private class Node
    {
        public char Value;
        public Node Left,
            Right;

        public Node(char value)
        {
            Value = value;
            Left = Right = null;
        }
    }

    public void Insert(char value)
    {
        root = InsertRec(root, value);
    }

    private Node InsertRec(Node node, char value)
    {
        if (node == null)
            return new Node(value);

        if (value <= node.Value)
            node.Left = InsertRec(node.Left, value);
        else
            node.Right = InsertRec(node.Right, value);

        return node;
    }

    public string InOrderTraversal()
    {
        var result = new StringBuilder();
        InOrderRec(root, result);
        return result.ToString();
    }

    private void InOrderRec(Node node, StringBuilder result)
    {
        if (node != null)
        {
            InOrderRec(node.Left, result);
            result.Append(node.Value);
            InOrderRec(node.Right, result);
        }
    }
}
