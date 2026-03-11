using System.Text;

namespace Lab_2_Part_B
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Розв'язання задачі лінійного програмування (Метод МЖВ): \n");
            int n = ConsoleHelpers.ReadInt("Введіть кількість змінних (n): ");
            int m = ConsoleHelpers.ReadInt("Введіть кількість обмежень (m): ");

            // Введення функції Z
            double[] C = new double[n];
            Console.WriteLine("\nКоефіцієнти цільової функції Z:");
            for (int j = 0; j < n; j++)
            {
                C[j] = ConsoleHelpers.ReadDouble($"Коефіцієнт при x{j + 1}: ");
            }

            Console.Write("Шукаємо максимум чи мінімум? (введіть 'max' або 'min'): ");
            bool isMax = Console.ReadLine()?.Trim().ToLower() != "min";

            if (!isMax)
            {
                for (int j = 0; j < n; j++) C[j] = -C[j];
                Console.WriteLine("Оскільки шукаємо min, переходимо до максимізації Z' = -Z");
            }

            // Введення системи обмежень
            double[,] table = new double[m + 1, n + 1];
            Console.WriteLine("\nВведення системи обмежень: ");
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
                    Console.Write("  Знак ('<=' або '>='): ");
                    sign = Console.ReadLine()?.Trim();
                    if (sign == "<=" || sign == ">=") break;
                    Console.WriteLine("  Помилка! Введіть '<=' або '>='");
                }

                double b = ConsoleHelpers.ReadDouble("  Вільний член (b): ");

                // Якщо знак >=, множимо всі коефіцієнти нерівності на -1 для зведення до вигляду <=
                if (sign == ">=")
                {
                    for (int j = 0; j < n; j++) table[i, j] = -table[i, j];
                    table[i, n] = -b;
                }
                else
                {
                    table[i, n] = b;
                }
            }

            // Заповнення Z-рядка в таблиці (-Cj)
            for (int j = 0; j < n; j++) table[m, j] = -C[j];
            table[m, n] = 0;

            // Формування заголовків
            string[] rowLabels = new string[m + 1];
            for (int i = 0; i < m; i++) rowLabels[i] = "y" + (i + 1);
            rowLabels[m] = "Z";

            string[] colLabels = new string[n + 1];
            for (int j = 0; j < n; j++) colLabels[j] = "-x" + (j + 1);
            colLabels[n] = "1";

            Console.WriteLine("Згенерований протокол обчислення:\n");
            SimplexAlgorithm.Solve(table, rowLabels, colLabels, m, n, isMax);
        }
    }
}