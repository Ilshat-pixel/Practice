class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(ProcessString("a"));
        Console.WriteLine(ProcessString("abcdef"));
        Console.WriteLine(ProcessString("abcde"));

        Console.Write("Введите строку: ");
        string userInput = Console.ReadLine();
        Console.WriteLine("Обработанная строка: " + ProcessString(userInput));
    }

    static string ProcessString(string input)
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

    static string ReverseString(string s)
    {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
}
