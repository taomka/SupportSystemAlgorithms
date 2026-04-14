using System.Text;

namespace Lab_2
{
    class SimplexAlgorithm
    {
        private const int MAX_ITERATIONS = 100;

        public static void SolveDual(double[,] table, List<string> pRows, List<string> dRows, List<string> pCols, List<string> dCols, int originalN, int originalM)
        {
            Console.WriteLine("\nВхідна симплекс-таблиця для пари взаємно двоїстих задач:");
            ConsoleHelpers.PrintDualTable(table, pRows, dRows, pCols, dCols);
            PrintDualFormulation(table, originalN, originalM);

            Console.WriteLine("\nПошук опорного розв'язку:\n");
            if (!FindReferenceSolution(ref table, pRows, dRows, pCols, dCols)) return;
            Console.WriteLine("Знайдено опорний розв'язок:\n");
            Console.WriteLine("Розв’язки прямої задачі:\n");
            ConsoleHelpers.PrintSolution(table, pRows, originalN, "X", "x", true);
            Console.WriteLine("\nРозв’язки двоїстої задачі:\n");
            ConsoleHelpers.PrintSolution(table, dCols, originalM, "U", "u", false);

            Console.WriteLine("\nПошук оптимального розв’язку:\n");
            if (!FindOptimalSolution(ref table, pRows, dRows, pCols, dCols)) return;
            Console.WriteLine("Знайдено оптимальний розв’язок:\n");
            Console.WriteLine("Розв’язки прямої задачі:\n");
            ConsoleHelpers.PrintSolution(table, pRows, originalN, "X", "x", true);
            Console.WriteLine("\nРозв’язки двоїстої задачі:\n");
            ConsoleHelpers.PrintSolution(table, dCols, originalM, "U", "u", false);

            int rows = table.GetLength(0);
            int cols = table.GetLength(1);
            Console.WriteLine($"\nMax (Z) = {table[rows - 1, cols - 1]:F2}");
            Console.WriteLine($"\nMin (W) = {table[rows - 1, cols - 1]:F2}");
        }

        private static void PrintDualFormulation(double[,] table, int n, int m)
        {
            Console.WriteLine("Постановка двоїстої задачі:\n");
            StringBuilder wFunc = new("W = ");
            for (int i = 0; i < m; i++)
            {
                string formattedB = table[i, n] < 0 ? $"({table[i, n]:F2})" : $"{table[i, n]:F2}";
                wFunc.Append($"{formattedB} * u{i + 1}");
                if (i < m - 1) wFunc.Append(" + ");
            }
            wFunc.Append(" -> min\n");
            Console.WriteLine(wFunc.ToString());
            Console.WriteLine("при обмеженнях:\n");

            for (int j = 0; j < n; j++)
            {
                StringBuilder eq = new($"v{j + 1} = ");
                for (int i = 0; i < m; i++)
                {
                    string formattedA = table[i, j] < 0 ? $"({table[i, j]:F2})" : $"{table[i, j]:F2}";
                    eq.Append($"{formattedA} * u{i + 1} + ");
                }
                string formattedC = table[m, j] < 0 ? $"({table[m, j]:F2})" : $"{table[m, j]:F2}";
                eq.Append($"{formattedC} >= 0");
                Console.WriteLine(eq.ToString());
            }
        }

        private static bool FindReferenceSolution(ref double[,] table, List<string> pRows, List<string> dRows, List<string> pCols, List<string> dCols)
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

                int rows = table.GetLength(0);
                int cols = table.GetLength(1);
                int r = -1;
                for (int i = 0; i < rows - 1; i++)
                {
                    if (table[i, cols - 1] < -1e-5)
                    {
                        r = i;
                        break;
                    }
                }

                if (r == -1)
                    return true;

                int s = -1;
                for (int j = 0; j < cols - 1; j++)
                {
                    if (table[r, j] < -1e-5)
                    {
                        s = j;
                        break;
                    }
                }

                if (s == -1)
                {
                    Console.WriteLine("Система суперечлива");
                    return false;
                }

                double minRatio = double.MaxValue;
                int elemR = -1;
                for (int i = 0; i < rows - 1; i++)
                {
                    if (Math.Abs(table[i, s]) > 1e-5)
                    {
                        double ratio = table[i, cols - 1] / table[i, s];
                        if (ratio >= -1e-5 && ratio < minRatio - 1e-5)
                        {
                            minRatio = ratio;
                            elemR = i;
                        }
                    }
                }

                if (elemR == -1)
                    return false;

                Console.WriteLine($"Розв’язувальний рядок:   {pRows[elemR],4}");
                Console.WriteLine($"Розв’язувальний стовпець: -{pCols[s],2}");
                PerformMJE(ref table, pRows, dRows, pCols, dCols, elemR, s);
                ConsoleHelpers.PrintDualTable(table, pRows, dRows, pCols, dCols);
            }
        }

        private static bool FindOptimalSolution(ref double[,] table, List<string> pRows, List<string> dRows, List<string> pCols, List<string> dCols)
        {
            int iterations = 0;
            while (true)
            {
                if (iterations++ > MAX_ITERATIONS)
                {
                    Console.WriteLine("\nПомилка: Алгоритм зациклився під час пошуку оптимального розв'язку.");
                    Console.WriteLine("Система обмежень є суперечливою.");
                    return false;
                }

                int rows = table.GetLength(0);
                int cols = table.GetLength(1);

                int s = -1;
                for (int j = 0; j < cols - 1; j++)
                {
                    if (table[rows - 1, j] < -1e-5)
                    {
                        s = j;
                        break;
                    }
                }
                if (s == -1)
                    return true;

                double minRatio = double.MaxValue;
                int elemR = -1;
                for (int i = 0; i < rows - 1; i++)
                {
                    if (table[i, s] > 1e-5)
                    {
                        double ratio = table[i, cols - 1] / table[i, s];
                        if (ratio >= -1e-5 && ratio < minRatio)
                        {
                            minRatio = ratio;
                            elemR = i;
                        }
                    }
                }

                if (elemR == -1)
                {
                    Console.WriteLine("Функція мети не обмежена");
                    return false;
                }

                Console.WriteLine($"Розв’язувальний рядок:   {pRows[elemR],4}");
                Console.WriteLine($"Розв’язувальний стовпець: -{pCols[s],2}");
                PerformMJE(ref table, pRows, dRows, pCols, dCols, elemR, s);
                ConsoleHelpers.PrintDualTable(table, pRows, dRows, pCols, dCols);
            }
        }

        private static void PerformMJE(ref double[,] table, List<string> pRows, List<string> dRows, List<string> pCols, List<string> dCols, int r, int s)
        {
            int rows = table.GetLength(0);
            int cols = table.GetLength(1);
            double elem = table[r, s];
            double[,] nextTable = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (i == r && j == s) nextTable[i, j] = 1.0;
                    else if (i == r) nextTable[i, j] = table[i, j];
                    else if (j == s) nextTable[i, j] = -table[i, j];
                    else nextTable[i, j] = table[i, j] * elem - table[i, s] * table[r, j];
                }
            }

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    table[i, j] = nextTable[i, j] / elem;

            // Swap міток для прямої задачі
            string tempP = pRows[r];
            pRows[r] = pCols[s];
            pCols[s] = tempP;

            // Swap міток для двоїстої задачі
            string tempD = dRows[r];
            dRows[r] = dCols[s];
            dCols[s] = tempD;
        }
    }
}
