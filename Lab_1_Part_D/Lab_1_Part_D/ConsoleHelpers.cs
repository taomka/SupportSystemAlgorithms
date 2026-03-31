namespace Lab_1_Part_D
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
                string? input = Console.ReadLine()?.Replace('.', ',');
                if (double.TryParse(input, out double result)) return result;
                Console.WriteLine("Помилка вводу. Введіть число.");
            }
        }

        public static void PrintTable(double[,] tab, List<string> rLabels, List<string> cLabels)
        {
            int rows = tab.GetLength(0);
            int cols = tab.GetLength(1);

            Console.WriteLine();
            Console.Write($"{"",4}");
            for (int j = 0; j < cols; j++) Console.Write($"{cLabels[j],8}");
            Console.WriteLine("\n" + new string('-', 4 + cols * 8));

            for (int i = 0; i < rows; i++)
            {
                Console.Write($"{rLabels[i],2} =");
                for (int j = 0; j < cols; j++)
                {
                    Console.Write($"{tab[i, j],8:F2}");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public static void PrintX(double[,] table, List<string> rowLabels, int originalN)
        {
            double[] X = new double[originalN];
            int rows = table.GetLength(0);
            int cols = table.GetLength(1);

            for (int i = 0; i < rows - 1; i++)
            {
                if (rowLabels[i].StartsWith("x"))
                {
                    int index = int.Parse(rowLabels[i].Substring(1)) - 1;
                    X[index] = Math.Round(table[i, cols - 1]);
                }
            }

            Console.Write("X = (");
            for (int j = 0; j < originalN; j++)
            {
                Console.Write($"{X[j]}");
                if (j < originalN - 1) Console.Write("; ");
            }
            Console.WriteLine(")");
        }
    }
}