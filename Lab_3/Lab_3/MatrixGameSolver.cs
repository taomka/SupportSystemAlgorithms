namespace Lab_3
{
    class MatrixGameSolver
    {
        // Метод пошуку сідлової точки
        public static bool FindSaddlePoint(double[,] A, out double[] P, out double[] Q, out double V)
        {
            int m = A.GetLength(0);
            int n = A.GetLength(1);
            P = new double[m];
            Q = new double[n];
            V = 0;

            Console.WriteLine("\nПошук сідлової точки:\n");

            double lowerValue = double.MinValue;
            int lowerRow = -1, lowerCol = -1;

            for (int i = 0; i < m; i++)
            {
                double rowMin = A[i, 0];
                int minCol = 0;
                for (int j = 1; j < n; j++)
                {
                    if (A[i, j] < rowMin)
                    {
                        rowMin = A[i, j];
                        minCol = j;
                    }
                }
                if (rowMin > lowerValue)
                {
                    lowerValue = rowMin;
                    lowerRow = i;
                    lowerCol = minCol;
                }
            }

            double upperValue = double.MaxValue;
            int upperRow = -1, upperCol = -1;

            for (int j = 0; j < n; j++)
            {
                double colMax = A[0, j];
                int maxRow = 0;
                for (int i = 1; i < m; i++)
                {
                    if (A[i, j] > colMax)
                    {
                        colMax = A[i, j];
                        maxRow = i;
                    }
                }
                if (colMax < upperValue)
                {
                    upperValue = colMax;
                    upperRow = maxRow;
                    upperCol = j;
                }
            }

            Console.WriteLine($"Знайдено нижню ціну гри: A[{lowerRow + 1}, {lowerCol + 1}] = {lowerValue}");
            Console.WriteLine($"Знайдено верхню ціну гри: A[{upperRow + 1}, {upperCol + 1}] = {upperValue}\n");

            if (Math.Abs(lowerValue - upperValue) < 1e-9)
            {
                Console.WriteLine("Сідлову точку знайдено!");
                Console.WriteLine($"Оптимальна стратегія 1-го гравця: рядок {lowerRow + 1}");
                Console.WriteLine($"Оптимальна стратегія 2-го гравця: стовпець {upperCol + 1}");
                Console.WriteLine($"Ціна гри: {lowerValue:F2}");

                // Якщо знайшли чисті стратегії, формуємо вектори
                P[lowerRow] = 1.0;
                Q[upperCol] = 1.0;
                V = lowerValue;
                return true;
            }
            Console.WriteLine("Сідлову точку не знайдено...\n");
            return false;
        }

        // Метод розв'язання гри симплекс-методом
        public static void SolveSimplex(double[,] A, out double[] P, out double[] Q, out double V)
        {
            int m = A.GetLength(0);
            int n = A.GetLength(1);

            Console.WriteLine("Розв’язання матричної гри симплекс-методом...\n");

            double shift = 0;
            double minA = A.Cast<double>().Min();
            if (minA <= 0)
                shift = Math.Abs(minA) + 1.0;

            PrintFormulation(A, m, n);

            double[,] table = new double[m + 1, n + 1];
            Label[] rowLabels = new Label[m];
            Label[] colLabels = new Label[n];

            InitializeSimplexTable(A, table, rowLabels, colLabels, m, n, shift);

            Console.WriteLine("Складено таку симплекс-таблицю:\n");
            ConsoleHelpers.PrintTable(table, rowLabels, colLabels);

            Console.WriteLine("Розв’язання пари двоїстих задач...\n");

            FindOptimalSolution(table, rowLabels, colLabels, m, n);

            Console.WriteLine("Знайдено оптимальні рішення двоїстих задач!\n");
            Console.WriteLine("Остаточна симплекс-таблиця:\n");
            ConsoleHelpers.PrintTable(table, rowLabels, colLabels);

            ExtractAndPrintResults(table, rowLabels, colLabels, m, n, shift, out P, out Q, out V);
        }

        // Формування початкової симплекс-таблиці
        private static void InitializeSimplexTable(double[,] A, double[,] table, Label[] rowLabels, Label[] colLabels, int m, int n, double shift)
        {
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++) 
                    table[i, j] = A[i, j] + shift;
                table[i, n] = 1.0;
                rowLabels[i] = new Label($"p{i + 1}", $"r{i + 1}");
            }
            for (int j = 0; j < n; j++)
            {
                table[m, j] = -1.0;
                colLabels[j] = new Label($"t{j + 1}", $"q{j + 1}");
            }
            table[m, n] = 0.0;
        }

        private static void FindOptimalSolution(double[,] table, Label[] rowLabels, Label[] colLabels, int rows, int cols)
        {
            while (true)
            {
                int s = -1;
                for (int j = 0; j < cols; j++)
                {
                    if (table[rows, j] < -1e-7)
                    { 
                        s = j;
                        break; 
                    }
                }
                if (s == -1) break;

                double minRatio = double.MaxValue;
                int elemR = -1;
                for (int i = 0; i < rows; i++)
                {
                    if (table[i, s] > 1e-7)
                    {
                        double ratio = table[i, cols] / table[i, s];
                        if (ratio >= -1e-7 && ratio < minRatio)
                        {
                            minRatio = ratio;
                            elemR = i;
                        }
                    }
                }

                if (elemR == -1) break;

                PerformMJE(table, elemR, s, rows, cols);

                // Заміна міток
                Label temp = rowLabels[elemR];
                rowLabels[elemR] = colLabels[s];
                colLabels[s] = temp;
            }
        }

        // Крок модифікованих жорданових виключень
        private static void PerformMJE(double[,] table, int r, int s, int rows, int cols)
        {
            double elem = table[r, s];
            double[,] nextTable = new double[rows + 1, cols + 1];

            for (int i = 0; i <= rows; i++)
            {
                for (int j = 0; j <= cols; j++)
                {
                    if (i == r && j == s) nextTable[i, j] = 1.0;
                    else if (i == r) nextTable[i, j] = table[i, j];
                    else if (j == s) nextTable[i, j] = -table[i, j];
                    else nextTable[i, j] = table[i, j] * elem - table[i, s] * table[r, j];
                }
            }

            for (int i = 0; i <= rows; i++)
                for (int j = 0; j <= cols; j++)
                    table[i, j] = nextTable[i, j] / elem;
        }

        // Виведення математичної постановки
        private static void PrintFormulation(double[,] A, int m, int n)
        {
            Console.WriteLine("Постановка прямої задачі:\n");
            Console.WriteLine($"Z = {string.Join(" + ", Enumerable.Range(1, n).Select(j => "q" + j))} -> max\n");
            Console.WriteLine("при обмеженнях:\n");
            for (int i = 0; i < m; i++)
                Console.WriteLine(ConsoleHelpers.FormatEquation(A, i, n, "q", "<= 1", true));
            Console.WriteLine($"{string.Join(", ", Enumerable.Range(1, n).Select(j => "q" + j))} >= 0\n");

            Console.WriteLine("Постановка двоїстої задачі:\n");
            Console.WriteLine($"W = {string.Join(" + ", Enumerable.Range(1, m).Select(i => "p" + i))} -> min\n");
            Console.WriteLine("при обмеженнях:\n");
            for (int j = 0; j < n; j++)
                Console.WriteLine(ConsoleHelpers.FormatEquation(A, j, m, "p", ">= 1", false));
            Console.WriteLine($"{string.Join(", ", Enumerable.Range(1, m).Select(i => "p" + i))} >= 0\n");
        }

        // Розрахунок та виведення фінальних результатів (P, Q, V)
        private static void ExtractAndPrintResults(double[,] table, Label[] rowLabels, Label[] colLabels, int m, int n, double shift, out double[] P, out double[] Q, out double V)
        {
            double Z_max = table[m, n];
            double V_shifted = 1.0 / Z_max;

            double[] pValues = new double[m];
            double[] qValues = new double[n];

            for (int i = 0; i < m; i++)
            {
                string targetP = "p" + (i + 1);
                for (int j = 0; j < n; j++)
                {
                    if (colLabels[j].left == targetP) pValues[i] = table[m, j];
                }
            }

            for (int j = 0; j < n; j++)
            {
                string targetQ = "q" + (j + 1);
                for (int i = 0; i < m; i++)
                {
                    if (rowLabels[i].right == targetQ) qValues[j] = table[i, n];
                }
            }

            Console.WriteLine($"Перший гравець: p: {string.Join("; ", pValues.Select(v => $"{v:F2}"))}");
            Console.WriteLine($"Другий гравець: q: {string.Join("; ", qValues.Select(v => $"{v:F2}"))}\n");

            Console.WriteLine($"Ціна гри: {V_shifted:F2}\n");

            Console.WriteLine("Розрахунок змішаних стратегій...\n");

            P = pValues.Select(v => Math.Abs(v * V_shifted)).ToArray();
            Q = qValues.Select(v => Math.Abs(v * V_shifted)).ToArray();
            V = V_shifted - shift;

            Console.WriteLine("Стратегії 1-го гравця:");
            Console.WriteLine($"{string.Join("; ", P.Select(v => $"{v:F2}"))}\n");

            Console.WriteLine("Стратегії 2-го гравця:");
            Console.WriteLine($"{string.Join("; ", Q.Select(v => $"{v:F2}"))}\n");

            Console.WriteLine($"Остаточна ціна гри: {V:F2}");
        }
    }
}
