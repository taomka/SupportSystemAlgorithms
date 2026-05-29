namespace DZ
{
    public class MulticriteriaSolver
    {
        public static void SolveManual(int k, int n, int m, double[,] zCoeffs, bool[] isMax, double[,] constraints, string[] signs, double[] b)
        {
            double[][] optX = new double[k][];
            double[] optZ = new double[k];

            Console.WriteLine("Пошук оптимальних векторів:");

            // Розв'язання k задач ЛП ---
            for (int f = 0; f < k; f++)
            {
                double[] currentZCoeffs = GetRow(zCoeffs, f, n);
                var result = SimplexSolver.SolveSingle(n, m, currentZCoeffs, isMax[f], constraints, signs, b, f + 1);

                optX[f] = result.optX;
                optZ[f] = result.optZ;
            }

            // Теорія ігор ---
            double[] p = CalculateGameAndCompromise(k, n, optX, zCoeffs);

            Console.Write($"{"",3}");
            for (int i = 1; i <= n; i++) Console.Write($"{"x" + i,10}");
            Console.WriteLine($"{"|  Вага (p)",15}");

            string separator = new('-', 8 + n * 10 + 10);
            Console.WriteLine(separator);

            // Вивід базових оптимальних векторів та їхньої ваги
            for (int f = 0; f < k; f++)
            {
                Console.Write($"{"X*" + (f + 1) + ":"}");
                for (int i = 0; i < n; i++) Console.Write($"{optX[f][i],10:F2}");
                Console.WriteLine($"   | {p[f],9:F2}");
            }

            // Розрахунок компромісного розв'язку ---
            Console.WriteLine("\nКомпромісний розв’язок:\n");
            Console.Write($"{"",12}");
            for (int i = 1; i <= n; i++) Console.Write($"{"x" + i,10}");
            Console.WriteLine("\n" + new string('-', 12 + n * 11));

            double[] Xcomp = new double[n];
            Console.Write($"{"X*(компр):",-12}");
            for (int i = 0; i < n; i++)
            {
                for (int f = 0; f < k; f++) Xcomp[i] += p[f] * optX[f][i];
                Console.Write($"{Xcomp[i],10:F2}");
            }
            Console.WriteLine("\n");
        }

        /// <summary>
        /// Extracts a specific row from a 2D matrix.
        /// </summary>
        private static double[] GetRow(double[,] matrix, int rowNumber, int cols)
        {
            double[] row = new double[cols];
            for (int i = 0; i < cols; i++) row[i] = matrix[rowNumber, i];
            return row;
        }

        /// <summary>
        /// Calculates the non-optimality matrix and determines the game strategies to find compromise weights.
        /// </summary>
        private static double[] CalculateGameAndCompromise(int k, int n, double[][] optX, double[,] zCoeffs)
        {
            Console.WriteLine($"Отримали k={k} оптимальних вектори:\n");
            Console.Write($"{"",5}");
            for (int i = 1; i <= n; i++) Console.Write($"{"x" + i,10}");
            Console.WriteLine("\n" + new string('-', 5 + n * 10));

            for (int f = 0; f < k; f++)
            {
                Console.Write($"X{f + 1}*: ");
                for (int i = 0; i < n; i++) Console.Write($"{optX[f][i],10:F2}");
                Console.WriteLine();
            }

            Console.WriteLine("\nМатриця коефіцієнтів функцій мети:");
            Console.Write($"{"",4}");
            for (int i = 1; i <= n; i++) Console.Write($"{"x" + i,10}");
            Console.WriteLine("\n" + new string('-', 5 + n * 10));

            for (int f = 0; f < k; f++)
            {
                Console.Write($"C{f + 1}: ");
                for (int j = 0; j < zCoeffs.GetLength(1); j++) Console.Write($"{zCoeffs[f, j],10:F2}");
                Console.WriteLine();
            }

            // Матриця неоптимальності Q
            Console.WriteLine("\nПошук матриці неоптимальних розв’язків:");
            double[,] C = new double[k, k];
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < k; j++)
                {
                    C[i, j] = 0;
                    for (int v = 0; v < n; v++) C[i, j] += zCoeffs[j, v] * optX[i][v];
                }
            }

            double[,] Q = new double[k, k];
            for (int j = 0; j < k; j++)
            {
                double maxC = C[j, j];
                double denom = Math.Abs(maxC);
                if (denom < 1e-9) denom = 1.0;

                for (int i = 0; i < k; i++) Q[i, j] = Math.Abs(maxC - C[i, j]) / denom;
            }

            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < k; j++) Console.Write($"{Q[i, j],10:F2}");
                Console.WriteLine();
            }

            double maxQ = Q.Cast<double>().Max();
            Console.WriteLine($"\nmax = {maxQ:F2}\n");

            // Розв'язання матричної гри
            Console.WriteLine("Пошук розв’язків матричної гри\n");
            Console.WriteLine("Матриця A:\n");
            double[,] A = new double[k, k];
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < k; j++)
                {
                    A[i, j] = maxQ - Q[i, j];
                    Console.Write($"{A[i, j],10:F2}");
                }
                Console.WriteLine();
            }

            Console.WriteLine("\nПошук сідлової точки:\n");
            double lowerVal = double.MinValue, upperVal = double.MaxValue;
            for (int i = 0; i < k; i++) lowerVal = Math.Max(lowerVal, Enumerable.Range(0, k).Select(j => A[i, j]).Min());
            for (int j = 0; j < k; j++) upperVal = Math.Min(upperVal, Enumerable.Range(0, k).Select(i => A[i, j]).Max());

            Console.WriteLine($"Знайдено нижню ціну гри: A[...] = {lowerVal:F2}");
            Console.WriteLine($"Знайдено верхню ціну гри: A[...] = {upperVal:F2}");

            if (Math.Abs(lowerVal - upperVal) < 1e-9)
            {
                Console.WriteLine("\nСідлову точку знайдено! Гра має розв'язок у чистих стратегіях.");
            }
            else
            {
                Console.WriteLine("\nСідлову точку не знайдено!\n");
            }

            return SolveGameSimplex(A, k);
        }

        /// <summary>
        /// Solves the matrix game using the simplex method to obtain mixed strategies (weights).
        /// </summary>
        private static double[] SolveGameSimplex(double[,] A, int k)
        {
            Console.WriteLine("Розв’язання матричної гри симплекс-методом\n");
            Console.WriteLine("Постановка прямої задачі:\n");
            Console.WriteLine($"Z = {string.Join(" + ", Enumerable.Range(1, k).Select(j => "q" + j))} -> max\n");
            Console.WriteLine("при обмеженнях:\n");
            for (int i = 0; i < k; i++)
                Console.WriteLine(ConsoleHelpers.FormatEquation(A, i, k, "q", "<= 1", true));
            Console.WriteLine($"{string.Join(", ", Enumerable.Range(1, k).Select(j => "q" + j))} >= 0\n");

            Console.WriteLine("Постановка двоїстої задачі:\n");
            Console.WriteLine($"W = {string.Join(" + ", Enumerable.Range(1, k).Select(i => "p" + i))} -> min\n");
            Console.WriteLine("при обмеженнях:\n");
            for (int j = 0; j < k; j++)
                Console.WriteLine(ConsoleHelpers.FormatEquation(A, j, k, "p", ">= 1", false));
            Console.WriteLine($"{string.Join(", ", Enumerable.Range(1, k).Select(i => "p" + i))} >= 0\n");

            double[,] table = new double[k + 1, k + 1];
            Label[] rowLabels = new Label[k];
            Label[] colLabels = new Label[k];

            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < k; j++) table[i, j] = A[i, j];
                table[i, k] = 1.0;
                rowLabels[i] = new Label($"p{i + 1}", $"r{i + 1}");
            }
            for (int j = 0; j < k; j++)
            {
                table[k, j] = -1.0;
                colLabels[j] = new Label($"t{j + 1}", $"q{j + 1}");
            }
            table[k, k] = 0.0;

            Console.WriteLine("Складено симплекс-таблицю:\n");
            ConsoleHelpers.PrintGameTable(table, rowLabels, colLabels);

            Console.WriteLine("Розв’язання пари двоїстих задач:\n");

            while (true)
            {
                int s = -1;
                for (int j = 0; j < k; j++) if (table[k, j] < -1e-7) { s = j; break; }
                if (s == -1) break;

                int r = -1;
                double minRatio = double.MaxValue;
                for (int i = 0; i < k; i++)
                {
                    if (table[i, s] > 1e-7)
                    {
                        double ratio = table[i, k] / table[i, s];
                        if (ratio >= -1e-7 && ratio < minRatio) { minRatio = ratio; r = i; }
                    }
                }
                if (r == -1) break;

                double pivot = table[r, s];
                double[,] nextTable = new double[k + 1, k + 1];
                for (int i = 0; i <= k; i++)
                    for (int j = 0; j <= k; j++)
                    {
                        if (i == r && j == s) nextTable[i, j] = 1.0 / pivot;
                        else if (i == r) nextTable[i, j] = table[i, j] / pivot;
                        else if (j == s) nextTable[i, j] = -table[i, j] / pivot;
                        else nextTable[i, j] = table[i, j] - (table[i, s] * table[r, j] / pivot);
                    }
                table = nextTable;
                (colLabels[s], rowLabels[r]) = (rowLabels[r], colLabels[s]);
            }

            Console.WriteLine("Знайдено оптимальні рішення двоїстих задач!\n");
            Console.WriteLine("Остаточна симплекс-таблиця:\n");
            ConsoleHelpers.PrintGameTable(table, rowLabels, colLabels);

            double V = 1.0 / table[k, k];
            double[] p = new double[k];
            double[] q = new double[k];

            for (int i = 0; i < k; i++)
            {
                string targetP = "p" + (i + 1);
                for (int j = 0; j < k; j++) if (colLabels[j].left == targetP) p[i] = table[k, j] * V;

                string targetQ = "q" + (i + 1);
                for (int row = 0; row < k; row++) if (rowLabels[row].right == targetQ) q[i] = table[row, k] * V;
            }

            Console.WriteLine($"Перший гравець: p: {string.Join("; ", p.Select(v => $"{v:F2}"))}");
            Console.WriteLine($"Другий гравець: q: {string.Join("; ", q.Select(v => $"{v:F2}"))}");
            Console.WriteLine($"Ціна гри: {V:F2}\n");

            Console.WriteLine("Розрахунок змішаних стратегій:\n");
            Console.WriteLine($"Стратегії 1-го гравця:\n{string.Join("; ", p.Select(v => $"{v:F2}"))}\n");
            Console.WriteLine($"Стратегії 2-го гравця:\n{string.Join("; ", q.Select(v => $"{v:F2}"))}\n");
            Console.WriteLine($"Остаточна ціна гри: {V:F2}\n");
            Console.WriteLine($"Вагові коефіцієнти розв’язків: {string.Join("; ", p.Select(v => $"{v:F2}"))}\n");

            return p; // Повертаємо масив p для подальшого розрахунку компромісу
        }
    }
}