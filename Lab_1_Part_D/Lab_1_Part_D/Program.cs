using System.Text;

namespace Lab_1_Part_D
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Знаходження цілочислового розв'язку (Метод Гоморі)\n");

            int n = ConsoleHelpers.ReadInt("Введіть кількість змінних (n): ");
            int m = ConsoleHelpers.ReadInt("Введіть кількість обмежень (m): ");

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
            List<string> rowLabels = [];
            List<string> colLabels = [];

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

                if (sign == "<=")
                {
                    table[i, n] = b;
                    rowLabels.Add("y" + yCounter++);
                }
                else if (sign == ">=")
                {
                    for (int j = 0; j < n; j++) table[i, j] = -table[i, j];
                    table[i, n] = -b;
                    rowLabels.Add("y" + yCounter++);
                }
                else if (sign == "=")
                {
                    if (b < 0)
                    {
                        for (int j = 0; j < n; j++) table[i, j] = -table[i, j];
                        table[i, n] = -b;
                    }
                    else table[i, n] = b;

                    rowLabels.Add("0");
                }
            }

            // Заповнення Z-рядка в таблиці (-Cj)
            for (int j = 0; j < n; j++) table[m, j] = -C[j];
            table[m, n] = 0;
            rowLabels.Add("Z");

            for (int j = 0; j < n; j++) colLabels.Add("-x" + (j + 1));
            colLabels.Add("1");

            Console.WriteLine("Згенерований протокол обчислення:\n");
            SimplexAlgorithm.SolveGomory(table, rowLabels, colLabels, n, isMax);
        }
    }
}