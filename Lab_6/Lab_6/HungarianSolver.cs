namespace Lab_6
{
    class HungarianSolver
    {
        public static void Solve(double[,] origC)
        {
            int n = origC.GetLength(0);
            double[,] C = (double[,])origC.Clone();

            Console.WriteLine("Матриця вартостей:");
            PrintMatrix(C);

            // 1. Віднімання мінімумів по рядках
            Console.WriteLine("\nПошук мінімальних елементів у кожному рядку та віднімання його від кожного елемента в рядку:");
            for (int i = 0; i < n; i++)
            {
                double min = double.MaxValue;
                for (int j = 0; j < n; j++) min = Math.Min(min, C[i, j]);
                Console.WriteLine($"В рядку {i + 1} знайдено 'min': {min}");
                for (int j = 0; j < n; j++) C[i, j] -= min;
            }
            Console.WriteLine("\nМатриця вартостей після віднімання мінімальних елементів у рядках:");
            PrintMatrix(C);

            // 2. Віднімання мінімумів по стовпцях
            Console.WriteLine("\nПошук мінімальних елементів у кожному стовпці та віднімання його від кожного елемента в стовпці:");
            for (int j = 0; j < n; j++)
            {
                double min = double.MaxValue;
                for (int i = 0; i < n; i++) min = Math.Min(min, C[i, j]);
                Console.WriteLine($"В стовпці {j + 1} знайдено 'min': {min}");
                for (int i = 0; i < n; i++) C[i, j] -= min;
            }
            Console.WriteLine("\nМатриця вартостей після віднімання мінімальних елементів у стовпцях:");
            PrintMatrix(C);

            Console.WriteLine("\nПошук матриці оптимальних призначень:");

            int[] rowMatch;
            int[] colMatch;

            while (true)
            {
                Console.WriteLine("\nВикреслення всіх нулів:\n");

                int assignments = FindMaxMatching(C, out rowMatch, out colMatch, n);
                GetCover(C, rowMatch, colMatch, n, out bool[] rowCover, out bool[] colCover);

                Console.WriteLine("Матриця вартостей після викреслення рядків і стовбців з нулями:");
                PrintCoveredMatrix(C, rowCover, colCover, rowMatch, n);

                Console.WriteLine($"\nКількість призначень на роботу: {assignments}, всього робіт: {n}\n");

                if (assignments == n)
                {
                    Console.WriteLine("Матрицю оптимальних призначень знайдено!\n");
                    break;
                }
                else
                {
                    Console.WriteLine("Матрицю оптимальних призначень не знайдено...\n");

                    double minUncovered = double.MaxValue;
                    for (int i = 0; i < n; i++)
                        for (int j = 0; j < n; j++)
                            if (!rowCover[i] && !colCover[j])
                                minUncovered = Math.Min(minUncovered, C[i, j]);

                    Console.WriteLine($"Серед невикреслених елементів знайдено 'min': {minUncovered}\n");

                    for (int i = 0; i < n; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            if (!rowCover[i] && !colCover[j]) C[i, j] -= minUncovered;
                            else if (rowCover[i] && colCover[j]) C[i, j] += minUncovered;
                        }
                    }
                    Console.WriteLine("Матриця вартостей після додавання/віднімання 'min' до/від відповідних елементів:");
                    PrintMatrix(C);
                }
            }

            Console.WriteLine("Побудова матриці призначень:\n");
            Console.WriteLine("Матриця вартостей, в якій відмічено призначення на роботу:");
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (rowMatch[i] == j) Console.Write("  [0]");
                    else Console.Write($"{C[i, j],5}");
                }
                Console.WriteLine();
            }

            Console.WriteLine("\nМатриця призначень:");
            double sum = 0;
            List<string> terms = [];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (rowMatch[i] == j)
                    {
                        Console.Write("    1");
                        sum += origC[i, j];
                        terms.Add(origC[i, j].ToString());
                    }
                    else Console.Write("    0");
                }
                Console.WriteLine();
            }

            Console.WriteLine("\nЗагальна вартість робіт:\n");
            Console.WriteLine($"S = {string.Join(" + ", terms)} = {sum}\n");

            int useSimplex = ConsoleHelpers.ReadInt("Розв'язати задачу про призначення симплекс-методом? (1 - так, 0 - ні): ");
            if (useSimplex == 1)
            {
                SimplexMapper.SolveSimplex(origC);
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }

        private static void PrintMatrix(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++) Console.Write($"{matrix[i, j],5}");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Допоміжний метод для виведення матриці вартостей з позначенням викреслених рядків і стовпців, а також знайдених призначень.
        /// </summary>
        private static void PrintCoveredMatrix(double[,] matrix, bool[] rowCover, bool[] colCover, int[] rowMatch, int n)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (rowMatch[i] == j) Console.Write("  [0]");
                    else if (rowCover[i] && colCover[j]) Console.Write("    +");
                    else if (rowCover[i]) Console.Write("    -");
                    else if (colCover[j]) Console.Write("    |");
                    else Console.Write($"{matrix[i, j],5}");
                }
                Console.WriteLine();
            }
        }

        // --- Математичне ядро для пошуку ліній ---

        /// <summary>
        /// Допоміжний метод для пошуку максимального парування в двочастковому графі, представленому булевою матрицею.
        /// </summary>
        /// <param name="bpGraph">Булева матриця, що представляє двочастковий граф, де true вказує на наявність ребра між вершинами</param>
        /// <param name="matchR">Масив, що містить відповідність стовпців рядкам</param>
        /// <param name="seen">Масив, що містить інформацію про відвідані вершини</param>
        /// <param name="n">Розмір матриці</param>
        /// <returns>True, якщо знайдено парування, інакше false</returns>
        private static bool BPM(int u, bool[,] bpGraph, int[] matchR, bool[] seen, int n)
        {
            for (int v = 0; v < n; v++)
            {
                if (bpGraph[u, v] && !seen[v])
                {
                    seen[v] = true;
                    if (matchR[v] < 0 || BPM(matchR[v], bpGraph, matchR, seen, n))
                    {
                        matchR[v] = u;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Допоміжний метод для пошуку максимального парування в двочастковому графі, представленому матрицею вартостей, де нульові елементи вказують на можливі призначення.
        /// </summary>
        /// <param name="matrix">Матриця вартостей</param>
        /// <param name="rowMatch">Масив, що містить відповідність рядків стовпцям</param>
        /// <param name="colMatch">Масив, що містить відповідність стовпців рядкам</param>
        /// <param name="n">Розмір матриці</param>
        /// <returns>Кількість знайдених парувань</returns>
        private static int FindMaxMatching(double[,] matrix, out int[] rowMatch, out int[] colMatch, int n)
        {
            bool[,] bpGraph = new bool[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    if (Math.Abs(matrix[i, j]) < 1e-9) bpGraph[i, j] = true;

            int[] matchR = new int[n];
            for (int i = 0; i < n; i++) matchR[i] = -1;

            int result = 0;
            for (int u = 0; u < n; u++)
            {
                bool[] seen = new bool[n];
                if (BPM(u, bpGraph, matchR, seen, n)) result++;
            }

            rowMatch = new int[n];
            colMatch = new int[n];
            for (int i = 0; i < n; i++) rowMatch[i] = -1;
            for (int i = 0; i < n; i++) colMatch[i] = matchR[i];
            for (int j = 0; j < n; j++)
                if (colMatch[j] != -1) rowMatch[colMatch[j]] = j;

            return result;
        }

        /// <summary>
        /// Допоміжний метод для визначення, які рядки і стовпці потрібно викреслити, щоб покрити всі нулі в матриці вартостей, враховуючи знайдені призначення.
        /// </summary>
        /// <param name="matrix">Матриця вартостей</param>
        /// <param name="rowMatch">Масив, що містить відповідність рядків стовпцям</param>
        /// <param name="colMatch">Масив, що містить відповідність стовпців рядкам</param>
        /// <param name="n">Розмір матриці</param>
        /// <param name="rowCover">Масив, що містить інформацію про викреслені рядки</param>
        /// <param name="colCover">Масив, що містить інформацію про викреслені стовпці</param>
        private static void GetCover(double[,] matrix, int[] rowMatch, int[] colMatch, int n, out bool[] rowCover, out bool[] colCover)
        {
            rowCover = new bool[n];
            colCover = new bool[n];
            bool[] markedRows = new bool[n];
            bool[] markedCols = new bool[n];

            Queue<int> q = new();
            for (int i = 0; i < n; i++)
            {
                if (rowMatch[i] == -1)
                {
                    markedRows[i] = true;
                    q.Enqueue(i);
                }
            }

            while (q.Count > 0)
            {
                int r = q.Dequeue();
                for (int j = 0; j < n; j++)
                {
                    if (Math.Abs(matrix[r, j]) < 1e-9 && !markedCols[j])
                    {
                        markedCols[j] = true;
                        int assignedRow = colMatch[j];
                        if (assignedRow != -1 && !markedRows[assignedRow])
                        {
                            markedRows[assignedRow] = true;
                            q.Enqueue(assignedRow);
                        }
                    }
                }
            }

            for (int i = 0; i < n; i++) rowCover[i] = !markedRows[i];
            for (int j = 0; j < n; j++) colCover[j] = markedCols[j];
        }
    }
}
