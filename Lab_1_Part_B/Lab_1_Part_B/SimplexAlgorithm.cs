namespace Lab_1_Part_B
{
    class SimplexAlgorithm
    {
        private const int MAX_ITERATIONS = 100;
        public static void Solve(double[,] table, string[] rowLabels, string[] colLabels, int m, int n, bool isMax)
        {
            Console.WriteLine("Вхідна симплекс-таблиця:");
            ConsoleHelpers.PrintTable(table, rowLabels, colLabels, m, n);

            // Пошук опорного розв'язку
            Console.WriteLine("Пошук опорного розв’язку:\n");
            if (!FindReferenceSolution(table, rowLabels, colLabels, m, n))
            {
                return;
            }

            Console.WriteLine("Знайдено опорний розв’язок:");
            ConsoleHelpers.PrintX(table, rowLabels, m, n);
            Console.WriteLine();

            // Пошук оптимального розв'язку
            Console.WriteLine("Пошук оптимального розв’язку:\n");
            if (!FindOptimalSolution(table, rowLabels, colLabels, m, n))
            {
                return;
            }

            Console.WriteLine("Знайдено оптимальний розв’язок:\n");
            ConsoleHelpers.PrintX(table, rowLabels, m, n);

            if (isMax)
                Console.WriteLine($"\nMax (Z) = {table[m, n]:F2}");
            else
                Console.WriteLine($"\nMin (Z) = {-table[m, n]:F2}");
        }

        private static bool FindReferenceSolution(double[,] table, string[] rowLabels, string[] colLabels, int m, int n)
        {
            int iterations = 0;
            while (true)
            {
                if (iterations++ > MAX_ITERATIONS)
                {
                    Console.WriteLine("\nПомилка: Алгоритм зациклився під час пошуку опорного розв'язку.");
                    Console.WriteLine("Система обмежень є суперечливою.");
                    return false;
                }

                int r = -1;
                for (int i = 0; i < m; i++)
                    if (table[i, n] < -1e-9) { r = i; break; }

                if (r == -1) return true; // Опорний розв'язок знайдено

                int s = -1;
                for (int j = 0; j < n; j++)
                    if (table[r, j] < -1e-9) { s = j; break; }

                if (s == -1)
                {
                    Console.WriteLine("Система обмежень є суперечливою");
                    return false;
                }

                double minRatio = double.MaxValue;
                int elemR = -1;
                for (int i = 0; i < m; i++)
                {
                    if (Math.Abs(table[i, s]) > 1e-9)
                    {
                        double ratio = table[i, n] / table[i, s];
                        if (ratio >= -1e-9)
                        {
                            if (ratio < minRatio - 1e-9)
                            {
                                minRatio = ratio;
                                elemR = i;
                            }
                            else if (Math.Abs(ratio - minRatio) <= 1e-9 && table[i, n] < -1e-9)
                            {
                                elemR = i;
                            }
                        }
                    }
                }
                if (elemR == -1)
                {
                    Console.WriteLine("\nСистема обмежень є суперечливою (неможливо знайти розв'язувальний рядок).");
                    return false;
                }
                Console.WriteLine($"Розв’язувальний рядок: {rowLabels[elemR],5}");
                Console.WriteLine($"Розв’язувальний стовпець: {colLabels[s],4}");
                PerformMJE(table, rowLabels, colLabels, elemR, s, m, n);
                ConsoleHelpers.PrintTable(table, rowLabels, colLabels, m, n);
            }
        }

        private static bool FindOptimalSolution(double[,] table, string[] rowLabels, string[] colLabels, int m, int n)
        {
            int iterations = 0;
            while (true)
            {
                if (iterations++ > MAX_ITERATIONS)
                {
                    Console.WriteLine("\nПомилка: Алгоритм зациклився під час пошуку оптимального розв'язку.");
                    Console.WriteLine("Задача не може досягти оптимуму.");
                    return false;
                }
                int s = -1;
                for (int j = 0; j < n; j++)
                    if (table[m, j] < -1e-9) { s = j; break; }

                if (s == -1) return true; // Оптимальний розв'язок знайдено

                double minRatio = double.MaxValue;
                int elemR = -1;
                for (int i = 0; i < m; i++)
                {
                    if (table[i, s] > 1e-9)
                    {
                        double ratio = table[i, n] / table[i, s];
                        if (ratio >= -1e-9 && ratio < minRatio)
                        {
                            minRatio = ratio;
                            elemR = i;
                        }
                    }
                }

                if (elemR == -1)
                {
                    Console.WriteLine("Функція мети не обмежена зверху");
                    return false;
                }
                Console.WriteLine($"Розв’язувальний рядок: {rowLabels[elemR],5}");
                Console.WriteLine($"Розв’язувальний стовпець: {colLabels[s],4}");
                PerformMJE(table, rowLabels, colLabels, elemR, s, m, n);
                ConsoleHelpers.PrintTable(table, rowLabels, colLabels, m, n);
            }
        }

        private static void PerformMJE(double[,] table, string[] rowLabels, string[] colLabels, int r, int s, int m, int n)
        {
            double elem = table[r, s];
            double[,] tempTable = new double[m + 1, n + 1];

            for (int i = 0; i <= m; i++)
            {
                for (int j = 0; j <= n; j++)
                {
                    if (i == r && j == s) tempTable[i, j] = 1.0;
                    else if (i == r) tempTable[i, j] = table[i, j];
                    else if (j == s) tempTable[i, j] = -table[i, j];
                    else tempTable[i, j] = table[i, j] * elem - table[i, s] * table[r, j];
                }
            }

            for (int i = 0; i <= m; i++)
                for (int j = 0; j <= n; j++)
                    table[i, j] = tempTable[i, j] / elem;

            string temp = rowLabels[r];
            rowLabels[r] = colLabels[s].TrimStart('-');
            colLabels[s] = "-" + temp;
        }
    }
}