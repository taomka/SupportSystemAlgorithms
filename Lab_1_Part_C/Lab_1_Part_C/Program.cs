using System.Text;

namespace Lab_1_Part_C
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Розв'язання задачі лінійного програмування зі змішаною системою обмежень \n");
            int n = ConsoleHelpers.ReadInt("Введіть кількість змінних (n): ");
            int m = ConsoleHelpers.ReadInt("Введіть кількість обмежень (m): ");

            // Введення функції Z
            double[] C = new double[n];
            Console.WriteLine("\n--- Коефіцієнти цільової функції Z ---");
            for (int j = 0; j < n; j++)
            {
                C[j] = ConsoleHelpers.ReadDouble($"Коефіцієнт при x{j + 1}: ");
            }

            Console.Write("Шукаємо максимум чи мінімум? (введіть 'max' або 'min'): ");
            bool isMax = Console.ReadLine()?.Trim().ToLower() != "min";

            if (!isMax)
            {
                for (int j = 0; j < n; j++) C[j] = -C[j];
                Console.WriteLine("Оскільки шукаємо min, переходимо до максимізації Z' = -Z.");
            }

            double[,] table = new double[m + 1, n + 1];
            string[] rowLabels = new string[m + 1];
            // Лічильник для додаткових змінних y
            int yCounter = 1;
            Console.WriteLine("\n--- Введення системи обмежень ---");
            for (int i = 0; i < m; i++)
            {
                Console.WriteLine($"\nОбмеження {i + 1}:");
                for (int j = 0; j < n; j++)
                {
                    table[i, j] = ConsoleHelpers.ReadDouble($"  Коефіцієнт при x{j + 1}: ");
                }

                string? sign;
                while (true)
                {
                    Console.Write("  Знак ('<=', '>=', '='): ");
                    sign = Console.ReadLine()?.Trim();
                    if (sign == "<=" || sign == ">=" || sign == "=") break;
                    Console.WriteLine("  Помилка! Введіть '<=', '>=' або '='.");
                }

                double b = ConsoleHelpers.ReadDouble("  Вільний член (b): ");

                // Формування рядка залежно від знаку
                if (sign == "<=")
                {
                    table[i, n] = b;
                    rowLabels[i] = "y" + yCounter++;
                }
                else if (sign == ">=")
                {
                    for (int j = 0; j < n; j++) table[i, j] = -table[i, j];
                    table[i, n] = -b;
                    rowLabels[i] = "y" + yCounter++;
                }
                else if (sign == "=")
                {
                    // У 0-рядках вільний член має бути додатним
                    if (b < 0)
                    {
                        for (int j = 0; j < n; j++) table[i, j] = -table[i, j];
                        table[i, n] = -b;
                    }
                    else
                    {
                        table[i, n] = b;
                    }
                    rowLabels[i] = "0";
                }
            }

            // Заповнення Z-рядка в таблиці (-Cj)
            for (int j = 0; j < n; j++) table[m, j] = -C[j];
            table[m, n] = 0;
            rowLabels[m] = "Z";

            // Динамічний список для міток стовпців
            List<string> colLabels = [];
            for (int j = 0; j < n; j++) colLabels.Add("-x" + (j + 1));
            colLabels.Add("1");

            Console.WriteLine("Згенерований протокол обчислення:\n");
            SimplexAlgorithm.Solve(table, rowLabels, colLabels, n, isMax);
        }
    }
}