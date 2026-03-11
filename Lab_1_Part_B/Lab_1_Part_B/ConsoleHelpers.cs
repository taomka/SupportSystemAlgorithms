namespace Lab_1_Part_B
{
    class ConsoleHelpers
    {
        public static int ReadInt(string prompt)
        {
            int result;
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine() ?? string.Empty;
                if (int.TryParse(input, out result) && result > 0) return result;
                Console.WriteLine("Помилка: введіть ціле додатне число.");
            }
        }

        public static double ReadDouble(string prompt)
        {
            double result;
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine() ?? string.Empty;
                if (!string.IsNullOrEmpty(input)) input = input.Replace('.', ',');
                if (double.TryParse(input, out result)) return result;
                Console.WriteLine("Помилка вводу. Будь ласка, введіть число.");
            }
        }

        public static void PrintTable(double[,] tab, string[] rLabels, string[] cLabels, int m, int n)
        {
            Console.WriteLine();
            Console.Write($"{"",6}");
            for (int j = 0; j <= n; j++) Console.Write($"{cLabels[j],8}");
            Console.WriteLine("\n-------------------------------------------------------------");
            for (int i = 0; i <= m; i++)
            {
                Console.Write($"{rLabels[i],4} =");
                for (int j = 0; j <= n; j++)
                {
                    Console.Write($"{tab[i, j],8:F2}");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public static void PrintX(double[,] table, string[] rowLabels, int m, int n)
        {
            double[] X = new double[n];
            for (int i = 0; i < m; i++)
            {
                if (rowLabels[i].StartsWith("x"))
                {
                    int index = int.Parse(rowLabels[i].Substring(1)) - 1;
                    X[index] = table[i, n];
                }
            }

            Console.Write("X = (");
            for (int j = 0; j < n; j++)
            {
                Console.Write($"{X[j]:F2}");
                if (j < n - 1) Console.Write("; ");
            }
            Console.WriteLine(")");
        }
    }
}
