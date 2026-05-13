namespace Lab_3
{
    partial class ConsoleHelpers
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

        public static double[] ParseMatrixRow(int expectedCount)
        {
            while (true)
            {
                try
                {
                    string? input = Console.ReadLine()?.Trim();

                    if (string.IsNullOrEmpty(input))
                    {
                        Console.WriteLine($"Помилка: рядок не може бути порожнім. Введіть {expectedCount} чисел:");
                        continue;
                    }

                    string[] parts = input.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length != expectedCount)
                    {
                        Console.WriteLine($"Помилка: очікувалось {expectedCount} чисел, отримано {parts.Length}. Спробуйте ще раз:");
                        continue;
                    }

                    double[] row = new double[expectedCount];
                    for (int j = 0; j < expectedCount; j++)
                    {
                        row[j] = double.Parse(parts[j].Replace('.', ','));
                    }
                    return row;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Помилка: введено не числове значення. Спробуйте ще раз:");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка при вводі: {ex.Message}. Спробуйте ще раз:");
                }
            }
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

        public static void PrintTable(double[,] tab, Label[] rLabels, Label[] cLabels)
        {
            int m = tab.GetLength(0) - 1;
            int n = tab.GetLength(1) - 1;

            Console.Write($"{"",9}");
            for (int j = 0; j < n; j++)
                Console.Write($"{cLabels[j].left + ", -" + cLabels[j].right,10}");
            Console.WriteLine($"{"W, 1",8}");

            Console.WriteLine(new string('-', 10 + (n + 1) * 10));

            for (int i = 0; i < m; i++)
            {
                Console.Write($"{rLabels[i].left,-2} {rLabels[i].right,2} =");
                for (int j = 0; j <= n; j++) Console.Write($"{tab[i, j],10:F2}");
                Console.WriteLine();
            }

            Console.Write($"{"1",-2} {"Z",2} =");
            for (int j = 0; j <= n; j++) Console.Write($"{tab[m, j],10:F2}");
            Console.WriteLine("\n");
        }
    }
}