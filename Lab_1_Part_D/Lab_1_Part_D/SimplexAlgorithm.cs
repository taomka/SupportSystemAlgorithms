using System.Text;

namespace Lab_1_Part_D
{
    class SimplexAlgorithm
    {
        private const int MAX_ITERATIONS = 100;

        public static void SolveGomory(double[,] initialTable, List<string> rowLabels, List<string> colLabels, int originalN, bool isMax)
        {
            double[,] table = initialTable;
            int sCounter = 1; // лічильник для додаткових змінних s

            Console.WriteLine("Вхідна симплекс-таблиця:");
            ConsoleHelpers.PrintTable(table, rowLabels, colLabels);

            while (true)
            {
                Console.WriteLine("Пошук опорного розв’язку:\n");
                if (!FindReferenceSolution(ref table, rowLabels, colLabels)) return;
                Console.WriteLine("Знайдено опорний розв’язок:\n");
                ConsoleHelpers.PrintX(table, rowLabels, originalN);
                Console.WriteLine("Пошук оптимального розв’язку:\n");
                if (!FindOptimalSolution(ref table, rowLabels, colLabels)) return;
                Console.WriteLine("Знайдено оптимальний розв’язок:\n");
                ConsoleHelpers.PrintX(table, rowLabels, originalN);

                int maxFracRow = -1;
                double maxFrac = -1.0;
                int rows = table.GetLength(0);
                int cols = table.GetLength(1);

                for (int i = 0; i < rows - 1; i++)
                {
                    if (rowLabels[i].StartsWith("x")) // Гоморі застосовуємо до змінних x
                    {
                        double b_i = Math.Round(table[i, cols - 1], 5);
                        double frac = b_i - Math.Floor(b_i);

                        // Якщо дробова частина суттєва (більше похибки)
                        if (frac > 1e-4 && frac < 0.9999)
                        {
                            if (frac > maxFrac)
                            {
                                maxFrac = frac;
                                maxFracRow = i;
                            }
                        }
                    }
                }

                // Якщо всі x цілі числа
                if (maxFracRow == -1)
                {
                    if (isMax)
                        Console.WriteLine($"\nMax (Z) = {Math.Round(table[rows - 1, cols - 1], 2):F2}");
                    else
                        Console.WriteLine($"\nMin (Z) = {-Math.Round(table[rows - 1, cols - 1], 2):F2}");
                    return;
                }

                // Побудова додаткового обмеження
                Console.WriteLine($"Знайдено розв’язок, у якому змінні мають дробову частину, максимальна дробова частина у змінної: {rowLabels[maxFracRow]} = {table[maxFracRow, cols - 1]:F2}\n");
                Console.WriteLine("Складено додаткове обмеження:\n");
                string newS = "s" + sCounter++;
                StringBuilder equation = new();
                equation.Append($"{newS} = ");

                double[,] newTable = new double[rows + 1, cols];
                List<string> newRowLabels = new(rowLabels);

                for (int i = 0; i < rows - 1; i++)
                {
                    for (int j = 0; j < cols; j++) newTable[i, j] = table[i, j];
                }

                for (int j = 0; j < cols; j++) newTable[rows, j] = table[rows - 1, j];
                newRowLabels.Insert(rows - 1, newS);

                // Розрахунок дробових частин для нового рядка
                for (int j = 0; j < cols - 1; j++)
                {
                    double a_ij = Math.Round(table[maxFracRow, j], 5);
                    double fracA = a_ij - Math.Floor(a_ij);
                    newTable[rows - 1, j] = -fracA;

                    string varName = colLabels[j].TrimStart('-');
                    equation.Append($"{fracA:F2} * {varName} + ");
                }
                
                double b_r = Math.Round(table[maxFracRow, cols - 1], 5);
                double fracB = b_r - Math.Floor(b_r);
                newTable[rows - 1, cols - 1] = -fracB;

                equation.Append($"({-fracB:F2}) >= 0");
                Console.WriteLine(equation.ToString() + "\n");

                table = newTable;
                rowLabels = newRowLabels;

                Console.WriteLine("Симплекс-таблиця з новим обмеженням:");
                ConsoleHelpers.PrintTable(table, rowLabels, colLabels);
            }
        }

        private static bool FindReferenceSolution(ref double[,] table, List<string> rowLabels, List<string> colLabels)
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
                    if (table[i, cols - 1] < -1e-5) { r = i; break; }

                if (r == -1) return true;

                int s = -1;
                for (int j = 0; j < cols - 1; j++)
                    if (table[r, j] < -1e-5) { s = j; break; }

                if (s == -1)
                {
                    Console.WriteLine("Система обмежень є суперечливою");
                    return false;
                }

                double minRatio = double.MaxValue;
                int elemR = -1;
                for (int i = 0; i < rows - 1; i++)
                {
                    if (Math.Abs(table[i, s]) > 1e-5)
                    {
                        double ratio = table[i, cols - 1] / table[i, s];
                        if (ratio >= -1e-5)
                        {
                            if (ratio < minRatio - 1e-5)
                            {
                                minRatio = ratio;
                                elemR = i;
                            }
                        }
                    }
                }

                if (elemR == -1) return false;

                Console.WriteLine($"Розв’язувальний рядок: {rowLabels[elemR],2}\nРозв’язувальний стовпець: {colLabels[s],3}");
                PerformMJE(ref table, rowLabels, colLabels, elemR, s);
                ConsoleHelpers.PrintTable(table, rowLabels, colLabels);
            }
        }

        private static bool FindOptimalSolution(ref double[,] table, List<string> rowLabels, List<string> colLabels)
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

                int rows = table.GetLength(0);
                int cols = table.GetLength(1);

                int s = -1;
                for (int j = 0; j < cols - 1; j++)
                    if (table[rows - 1, j] < -1e-5) { s = j; break; }

                if (s == -1) return true;

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
                    Console.WriteLine("Функція мети не обмежена зверху");
                    return false;
                }

                Console.WriteLine($"Розв’язувальний рядок: {rowLabels[elemR],2}\nРозв’язувальний стовпець: {colLabels[s],3}");
                PerformMJE(ref table, rowLabels, colLabels, elemR, s);
                ConsoleHelpers.PrintTable(table, rowLabels, colLabels);
            }
        }

        private static void PerformMJE(ref double[,] table, List<string> rowLabels, List<string> colLabels, int r, int s)
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

            string temp = rowLabels[r];
            rowLabels[r] = colLabels[s].TrimStart('-');
            colLabels[s] = "-" + temp;
        }
    }
}