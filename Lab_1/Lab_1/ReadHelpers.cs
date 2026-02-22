namespace Lab_1
{
    class ReadHelpers
    {
        public static double ReadDouble(string prompt)
        {
            double result;
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine() ?? string.Empty;
                if (!string.IsNullOrEmpty(input))
                    input = input.Replace('.', ',');
                if (double.TryParse(input, out result))
                    return result;
                Console.WriteLine("Помилка вводу. Будь ласка, введіть число (наприклад: 5 або -2,5).");
            }
        }

        public static int ReadInt(string prompt)
        {
            int result;
            while (true)
            {
                Console.Write(prompt);
                string line = Console.ReadLine() ?? string.Empty;
                if (int.TryParse(line, out result) && result > 0)
                    return result;
                Console.WriteLine("Помилка: введіть ціле додатне число.");
            }
        }
    }
}
