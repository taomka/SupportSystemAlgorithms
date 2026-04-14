using System.Text;

namespace Lab_2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Розв'язання пари двоїстих задач (Z, W):\n");

            int n = ConsoleHelpers.ReadInt("Введіть кількість змінних (n): ");
            int m = ConsoleHelpers.ReadInt("Введіть кількість обмежень (m): ");
            double[] C = new double[n];
            Console.WriteLine("\nКоефіцієнти цільової функції Z:");
            for (int j = 0; j < n; j++)
                C[j] = ConsoleHelpers.ReadDouble($"Коефіцієнт при x{j + 1}: ");

            Console.Write("Шукаємо максимум чи мінімум? (введіть 'max' або 'min'):");
            bool isMax = Console.ReadLine()?.Trim().ToLower() != "min";
            if (!isMax)
                for (int j = 0; j < n; j++) C[j] = -C[j];

            double[,] table = new double[m + 1, n + 1];
            List<string> primalRows = [];
            List<string> dualRows = [];
            List<string> primalCols = [];
            List<string> dualCols = [];

            int yCounter = 1;

            Console.WriteLine("\n--- Введення системи обмежень ---");
            for (int i = 0; i < m; i++)
            {
                Console.WriteLine($"\nОбмеження {i + 1}:");
                for (int j = 0; j < n; j++)
                {
                    table[i, j] = ConsoleHelpers.ReadDouble($" Коефіцієнт при x{j + 1}: ");
                }
                string? sign;
                while (true)
                {
                    Console.Write(" Знак ('<=', '>=', '='): ");
                    sign = Console.ReadLine()?.Trim();
                    if (sign == "<=" || sign == ">=" || sign == "=") break;
                    Console.WriteLine(" Помилка! Введіть '<=', '>=' або '='.");
                }

                double b = ConsoleHelpers.ReadDouble(" Вільний член (b): ");
                if (sign == "<=")
                    table[i, n] = b;
                else if (sign == ">=")
                {
                    for (int j = 0; j < n; j++) table[i, j] = -table[i, j];
                    table[i, n] = -b;
                }
                else if (sign == "=")
                {
                    if (b < 0)
                    {
                        for (int j = 0; j < n; j++) table[i, j] = -table[i, j];
                        table[i, n] = -b;
                    }
                    else table[i, n] = b;
                }
                primalRows.Add("y" + yCounter);
                dualRows.Add("u" + yCounter);
                yCounter++;
            }

            for (int j = 0; j < n; j++) table[m, j] = -C[j];
            table[m, n] = 0;

            primalRows.Add("Z");
            dualRows.Add("1");

            for (int j = 0; j < n; j++)
            {
                primalCols.Add("x" + (j + 1));
                dualCols.Add("v" + (j + 1));
            }
            primalCols.Add("1");
            dualCols.Add("W");

            SimplexAlgorithm.SolveDual(table, primalRows, dualRows, primalCols, dualCols, n, m);
        }
    }
}