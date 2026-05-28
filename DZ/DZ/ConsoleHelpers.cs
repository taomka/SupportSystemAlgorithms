namespace DZ
{
    class ConsoleHelpers
    {
        public static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int result) && result > 0) return result;
                Console.WriteLine("Помилка: введіть ціле додатне число.");
            }
        }

        public static double ReadDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (double.TryParse(Console.ReadLine()?.Replace('.', ','), out double result)) return result;
                Console.WriteLine("Помилка: введіть коректне число.");
            }
        }

        public static void PrintLPTable(double[,] tab, List<string> rLabels, List<string> cLabels, string zName)
        {
            int m = tab.GetLength(0) - 1;
            int cols = tab.GetLength(1) - 1;

            Console.Write($"{"",6}");
            for (int j = 0; j < cols; j++) Console.Write($"{"-" + cLabels[j],10}");
            Console.WriteLine($"{"1",10}");
            Console.WriteLine(new string('-', 6 + (cols + 1) * 10));

            for (int i = 0; i < m; i++)
            {
                Console.Write($"{rLabels[i],-4} =");
                for (int j = 0; j <= cols; j++) Console.Write($"{tab[i, j],10:F2}");
                Console.WriteLine();
            }

            Console.Write($"{zName,-4} =");
            for (int j = 0; j <= cols; j++) Console.Write($"{tab[m, j],10:F2}");
            Console.WriteLine("\n");
        }
        public static void PrintGameTable(double[,] tab, Label[] rLabels, Label[] cLabels)
        {
            int k = tab.GetLength(0) - 1;

            Console.Write($"{"",9}");
            for (int j = 0; j < k; j++) Console.Write($"{cLabels[j].left + ", -" + cLabels[j].right,10}");
            Console.WriteLine($"{"W, 1",8}");
            Console.WriteLine(new string('-', 7 + (k + 1) * 10));

            for (int i = 0; i < k; i++)
            {
                Console.Write($"{rLabels[i].left,-2} {rLabels[i].right,2} =");
                for (int j = 0; j <= k; j++) Console.Write($"{tab[i, j],10:F2}");
                Console.WriteLine();
            }

            Console.Write($"{"1",-2} {"Z",2} =");
            for (int j = 0; j <= k; j++) Console.Write($"{tab[k, j],10:F2}");
            Console.WriteLine("\n");
        }

        public static string FormatEquation(double[,] A, int index, int length, string varPrefix, string end, bool isRow)
        {
            List<string> terms = [];
            for (int k = 0; k < length; k++)
            {
                double val = isRow ? A[index, k] : A[k, index];
                if (Math.Abs(val) > 1e-9)
                {
                    if (Math.Abs(val - 1.0) < 1e-9) terms.Add($"{varPrefix}{k + 1}");
                    else terms.Add($"{val:F2} * {varPrefix}{k + 1}");
                }
            }
            return string.Join(" + ", terms) + $" {end}";
        }
    }
}
