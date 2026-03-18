namespace Lab_1_Part_C
{
    public class SimplexAlgorithm
    {
        private const int MAX_ITERATIONS = 100;

        public static void Solve(double[,] table, string[] rowLabels, List<string> colLabels, int originalN, bool isMax)
        {
            Console.WriteLine("Вхідна симплекс-таблиця:");
            ConsoleHelpers.PrintTable(table, rowLabels, colLabels);

            // Видалення нуль-рядків
            if (!EliminateZeroRows(ref table, rowLabels, colLabels)) return;

            // Пошук опорного розв'язку
            Console.WriteLine("Пошук опорного розв’язку:\n");
            if (!FindReferenceSolution(ref table, rowLabels, colLabels)) return;

            Console.WriteLine("Знайдено опорний розв’язок:\n");
            ConsoleHelpers.PrintX(table, rowLabels, originalN);
            Console.WriteLine();

            // Пошук оптимального розв'язку
            Console.WriteLine("Пошук оптимального розв’язку:\n");
            if (!FindOptimalSolution(ref table, rowLabels, colLabels)) return;

            Console.WriteLine("Знайдено оптимальний розв’язок:\n");
            ConsoleHelpers.PrintX(table, rowLabels, originalN);

            int m = table.GetLength(0) - 1;
            int n = table.GetLength(1) - 1;

            if (isMax) Console.WriteLine($"\nMax (Z) = {table[m, n]:F2}");
            else Console.WriteLine($"\nMin (Z) = {-table[m, n]:F2}");
        }

        private static bool EliminateZeroRows(ref double[,] table, string[] rowLabels, List<string> colLabels)
        {
            bool hasZeroRows = false;
            foreach (var lbl in rowLabels) if (lbl == "0") hasZeroRows = true;

            if (!hasZeroRows) return true;

            Console.WriteLine("Видалення нуль-рядків:\n");
            int iterations = 0;

            while (true)
            {
                if (iterations++ > MAX_ITERATIONS)
                {
                    Console.WriteLine("Помилка: Зациклення при видаленні нуль-рядків.");
                    return false;
                }

                int rows = table.GetLength(0);
                int cols = table.GetLength(1);
                // Пошук 0-рядка
                int r = -1;
                for (int i = 0; i < rows - 1; i++)
                {
                    if (rowLabels[i] == "0") { r = i; break; }
                }

                if (r == -1) break; // Усі нуль-рядки видалено

                // Пошук додатного елемента в 0-рядку (визначає розв'язувальний стовпець)
                int s = -1;
                for (int j = 0; j < cols - 1; j++)
                {
                    if (table[r, j] > 1e-9) { s = j; break; }
                }

                if (s == -1)
                {
                    Console.WriteLine("Система обмежень є суперечливою");
                    return false;
                }

                // Розрахунок мінімального відношення для знаходження розв'язувального рядка
                double minRatio = double.MaxValue;
                int elemR = -1;
                for (int i = 0; i < rows - 1; i++)
                {
                    if (table[i, s] > 1e-9)
                    {
                        double ratio = table[i, cols - 1] / table[i, s];
                        if (ratio >= -1e-9)
                        {
                            if (ratio < minRatio - 1e-9)
                            {
                                minRatio = ratio;
                                elemR = i;
                            }
                            else if (Math.Abs(ratio - minRatio) <= 1e-9 && rowLabels[i] == "0")
                            {
                                // Пріоритет віддаємо 0-рядку при однакових відношеннях
                                elemR = i;
                            }
                        }
                    }
                }

                if (elemR == -1)
                {
                    Console.WriteLine("Система обмежень є суперечливою (неможливо знайти розв'язувальний рядок)");
                    return false;
                }

                Console.WriteLine($"Розв’язувальний рядок:   {rowLabels[elemR],4}");
                Console.WriteLine($"Розв’язувальний стовпець: {colLabels[s],3}\n");
                PerformMJE(ref table, rowLabels, colLabels, elemR, s);
                ConsoleHelpers.PrintTable(table, rowLabels, colLabels);
            }
            Console.WriteLine("Всі нуль-рядки видалено.\n");
            return true;
        }

        private static bool FindReferenceSolution(ref double[,] table, string[] rowLabels, List<string> colLabels)
        {
            int iterations = 0;
            while (true)
            {
                if (iterations++ > MAX_ITERATIONS)
                {
                    Console.WriteLine("\nПомилка: Зациклення під час пошуку опорного розв'язку.");
                    return false;
                }

                int rows = table.GetLength(0);
                int cols = table.GetLength(1);
                int r = -1;
                for (int i = 0; i < rows - 1; i++)
                    if (table[i, cols - 1] < -1e-9) { r = i; break; }

                if (r == -1) return true;

                int s = -1;
                for (int j = 0; j < cols - 1; j++)
                    if (table[r, j] < -1e-9) { s = j; break; }

                if (s == -1)
                {
                    Console.WriteLine("Система обмежень є суперечливою");
                    return false;
                }

                double minRatio = double.MaxValue;
                int elemR = -1;
                for (int i = 0; i < rows - 1; i++)
                {
                    if (Math.Abs(table[i, s]) > 1e-9)
                    {
                        double ratio = table[i, cols - 1] / table[i, s];
                        if (ratio >= -1e-9)
                        {
                            if (ratio < minRatio - 1e-9)
                            {
                                minRatio = ratio;
                                elemR = i;
                            }
                            else if (Math.Abs(ratio - minRatio) <= 1e-9 && table[i, cols - 1] < -1e-9)
                            {
                                elemR = i;
                            }
                        }
                    }
                }

                if (elemR == -1)
                {
                    Console.WriteLine("Система обмежень є суперечливою");
                    return false;
                }

                Console.WriteLine($"Розв’язувальний рядок:   {rowLabels[elemR],4}");
                Console.WriteLine($"Розв’язувальний стовпець: {colLabels[s],3}\n");
                PerformMJE(ref table, rowLabels, colLabels, elemR, s);
                ConsoleHelpers.PrintTable(table, rowLabels, colLabels);
            }
        }

        private static bool FindOptimalSolution(ref double[,] table, string[] rowLabels, List<string> colLabels)
        {
            int iterations = 0;
            while (true)
            {
                if (iterations++ > MAX_ITERATIONS)
                {
                    Console.WriteLine("\nПомилка: Зациклення під час пошуку оптимального розв'язку.");
                    return false;
                }

                int rows = table.GetLength(0);
                int cols = table.GetLength(1);

                int s = -1;
                for (int j = 0; j < cols - 1; j++)
                    if (table[rows - 1, j] < -1e-9) { s = j; break; }

                if (s == -1) return true;

                double minRatio = double.MaxValue;
                int elemR = -1;
                for (int i = 0; i < rows - 1; i++)
                {
                    if (table[i, s] > 1e-9)
                    {
                        double ratio = table[i, cols - 1] / table[i, s];
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

                Console.WriteLine($"Розв’язувальний рядок:   {rowLabels[elemR],4}");
                Console.WriteLine($"Розв’язувальний стовпець: {colLabels[s],3}\n");
                PerformMJE(ref table, rowLabels, colLabels, elemR, s);
                ConsoleHelpers.PrintTable(table, rowLabels, colLabels);
            }
        }

        private static void PerformMJE(ref double[,] table, string[] rowLabels, List<string> colLabels, int r, int s)
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

            // Якщо ми перемістили змінну "0" у стовпець, викреслюємо (видаляємо) цей 0-стовпець
            if (temp == "0")            
                table = RemoveColumn(table, colLabels, s);
        }

        private static double[,] RemoveColumn(double[,] oldTable, List<string> colLabels, int colToRemove)
        {
            int rows = oldTable.GetLength(0);
            int cols = oldTable.GetLength(1);
            double[,] newTable = new double[rows, cols - 1];

            for (int i = 0; i < rows; i++)
            {
                int newJ = 0;
                for (int j = 0; j < cols; j++)
                {
                    if (j == colToRemove) continue;
                    newTable[i, newJ] = oldTable[i, j];
                    newJ++;
                }
            }
            colLabels.RemoveAt(colToRemove);
            return newTable;
        }
    }
}
