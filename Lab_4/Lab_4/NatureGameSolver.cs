namespace Lab_4
{
    class NatureGameSolver
    {
        public static void GenerateProtocol(double[,] U, double alpha, double[] p)
        {
            int m = U.GetLength(0);
            int n = U.GetLength(1);
            int[] stratCounts = new int[m];

            Console.WriteLine("Згенерований протокол обчислення:\n");

            Console.WriteLine("Матриця корисності результатів U:\n");
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++) Console.Write($"{U[i, j],3} ");
                Console.WriteLine();
            }

            // Збереження мінімумів та максимумів рядків для Гурвіца
            double[] rowMins = new double[m];
            double[] rowMaxs = new double[m];

            SolveWald(U, m, n, rowMins, stratCounts);
            SolveOptimism(U, m, n, rowMaxs, stratCounts);
            SolveHurwicz(m, alpha, rowMins, rowMaxs, stratCounts);
            SolveSavage(U, m, n, stratCounts);
            SolveBayes(U, m, n, p, stratCounts);
            SolveLaplace(U, m, n, stratCounts);

            PrintFinalConclusion(m, stratCounts);
        }

        private static void SolveWald(double[,] U, int m, int n, double[] rowMins, int[] stratCounts)
        {
            Console.WriteLine("\nКритерій Вальда:\n");
            for (int i = 0; i < m; i++)
            {
                rowMins[i] = U[i, 0];
                for (int j = 1; j < n; j++) if (U[i, j] < rowMins[i]) rowMins[i] = U[i, j];
                Console.WriteLine($"min в рядку {i + 1}: {rowMins[i]}");
            }
            double waldMax = rowMins.Max();
            Console.WriteLine($"\nМаксимальний елемент: {waldMax}");

            List<int> opt = [];
            for (int i = 0; i < m; i++) if (Math.Abs(rowMins[i] - waldMax) < 1e-9) { opt.Add(i + 1); stratCounts[i]++; }
            Console.WriteLine($"Оптимальні стратегії: {string.Join(" або ", opt.Select(x => "A" + x))}");
        }

        private static void SolveOptimism(double[,] U, int m, int n, double[] rowMaxs, int[] stratCounts)
        {
            Console.WriteLine("\nКритерій максимаксу:\n");
            for (int i = 0; i < m; i++)
            {
                rowMaxs[i] = U[i, 0];
                for (int j = 1; j < n; j++) if (U[i, j] > rowMaxs[i]) rowMaxs[i] = U[i, j];
                Console.WriteLine($"max в рядку {i + 1}: {rowMaxs[i]}");
            }
            double maxMax = rowMaxs.Max();
            Console.WriteLine($"\nМаксимальний елемент: {maxMax}");

            List<int> opt = [];
            for (int i = 0; i < m; i++) if (Math.Abs(rowMaxs[i] - maxMax) < 1e-9) { opt.Add(i + 1); stratCounts[i]++; }
            Console.WriteLine($"Оптимальні стратегії: {string.Join(" або ", opt.Select(x => "A" + x))}");
        }

        private static void SolveHurwicz(int m, double alpha, double[] rowMins, double[] rowMaxs, int[] stratCounts)
        {
            Console.WriteLine("\nКритерій Гурвіца:\n");
            Console.WriteLine($"Коефіцієнт y = {alpha:0.##}\n");
            for (int i = 0; i < m; i++) Console.WriteLine($"min в рядку {i + 1}: {rowMins[i]}");
            for (int i = 0; i < m; i++) Console.WriteLine($"max в рядку {i + 1}: {rowMaxs[i]}");
            Console.WriteLine();

            double[] vals = new double[m];
            for (int i = 0; i < m; i++)
            {
                vals[i] = alpha * rowMins[i] + (1 - alpha) * rowMaxs[i];
                Console.WriteLine($"s{i + 1} = {alpha:0.##} * {rowMins[i]} + (1 - {alpha:0.##}) * {rowMaxs[i]} = {vals[i]:0.##}");
            }
            double maxVal = vals.Max();
            Console.WriteLine($"\nМаксимальний елемент: {maxVal:0.##}");

            List<int> opt = [];
            for (int i = 0; i < m; i++) if (Math.Abs(vals[i] - maxVal) < 1e-9) { opt.Add(i + 1); stratCounts[i]++; }
            Console.WriteLine($"Оптимальні стратегії: {string.Join(" або ", opt.Select(x => "A" + x))}");
        }

        private static void SolveSavage(double[,] U, int m, int n, int[] stratCounts)
        {
            Console.WriteLine("\nКритерій Севіджа:\n");
            Console.WriteLine("Матриця ризиків:\n");
            double[,] R = new double[m, n];
            for (int j = 0; j < n; j++)
            {
                double colMax = U[0, j];
                for (int i = 1; i < m; i++) if (U[i, j] > colMax) colMax = U[i, j];
                for (int i = 0; i < m; i++) R[i, j] = colMax - U[i, j];
            }
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++) Console.Write($"{R[i, j],3} ");
                Console.WriteLine();
            }
            Console.WriteLine();

            double[] riskMaxs = new double[m];
            for (int i = 0; i < m; i++)
            {
                riskMaxs[i] = R[i, 0];
                for (int j = 1; j < n; j++) if (R[i, j] > riskMaxs[i]) riskMaxs[i] = R[i, j];
                Console.WriteLine($"max в рядку {i + 1}: {riskMaxs[i]}");
            }
            double savageMin = riskMaxs.Min();
            Console.WriteLine($"\nМінімальний елемент: {savageMin}");

            List<int> opt = [];
            for (int i = 0; i < m; i++) if (Math.Abs(riskMaxs[i] - savageMin) < 1e-9) { opt.Add(i + 1); stratCounts[i]++; }
            Console.WriteLine($"Оптимальні стратегії: {string.Join(" або ", opt.Select(x => "A" + x))}");
        }

        private static void SolveBayes(double[,] U, int m, int n, double[] p, int[] stratCounts)
        {
            Console.WriteLine("\nКритерій Байєса:\n");
            Console.WriteLine($"Ймовірності застосування природою своїх стратегій: {string.Join("; ", p.Select((v, i) => $"p{i + 1} = {v:0.##}"))};\n");
            double[] vals = new double[m];
            for (int i = 0; i < m; i++)
            {
                List<string> terms = [];
                double sum = 0;
                for (int j = 0; j < n; j++)
                {
                    terms.Add($"{U[i, j]} * {p[j]:0.##}");
                    sum += U[i, j] * p[j];
                }
                vals[i] = sum;
                Console.WriteLine($"s{i + 1} = {string.Join(" + ", terms)} = {sum:F2}");
            }
            double maxVal = vals.Max();
            Console.WriteLine($"\nМаксимальний елемент: {maxVal:F2}");

            List<int> opt = [];
            for (int i = 0; i < m; i++) if (Math.Abs(vals[i] - maxVal) < 1e-9) { opt.Add(i + 1); stratCounts[i]++; }
            Console.WriteLine($"Оптимальні стратегії: {string.Join(" або ", opt.Select(x => "A" + x))}");
        }

        private static void SolveLaplace(double[,] U, int m, int n, int[] stratCounts)
        {
            Console.WriteLine("\nКритерій Лапласа:\n");
            double[] vals = new double[m];
            double laplaceP = 1.0 / n;
            for (int i = 0; i < m; i++)
            {
                List<string> terms = [];
                double sum = 0;
                for (int j = 0; j < n; j++)
                {
                    terms.Add($"{U[i, j]} * {laplaceP:0.##}");
                    sum += U[i, j] * laplaceP;
                }
                vals[i] = sum;
                Console.WriteLine($"s{i + 1} = {string.Join(" + ", terms)} = {sum:F2}");
            }
            double maxVal = vals.Max();
            Console.WriteLine($"\nМаксимальний елемент: {maxVal:F2}");

            List<int> opt = [];
            for (int i = 0; i < m; i++) if (Math.Abs(vals[i] - maxVal) < 1e-9) { opt.Add(i + 1); stratCounts[i]++; }
            Console.WriteLine($"Оптимальні стратегії: {string.Join(" або ", opt.Select(x => "A" + x))}");
        }

        private static void PrintFinalConclusion(int m, int[] stratCounts)
        {
            int maxCount = stratCounts.Max();
            var bestStrats = Enumerable.Range(0, m).Where(i => stratCounts[i] == maxCount).Select(i => $"A{i + 1}");
            Console.WriteLine($"\nНайчастіше були оптимальними стратегії: {string.Join(" або ", bestStrats)}");
        }
    }
}
