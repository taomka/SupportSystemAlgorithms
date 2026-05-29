namespace DZ
{
    public class SimplexSolver
    {
        public static (double[] optX, double optZ) SolveSingle(int n, int m, double[] zCoeff, bool isMax, double[,] constraints, string[] signs, double[] b, int functionIndex)
        {
            Console.WriteLine($"\nПошук оптимального розв’язку задачі лінійного програмування для Z{functionIndex}:\n");

            List<string> cLabels = [.. Enumerable.Range(1, n).Select(i => $"x{i}")];
            List<string> rLabels = [];
            double[,] table = new double[m + 1, n + 1];

            // Побудова таблиці
            for (int i = 0; i < m; i++)
            {
                double bVal = b[i];
                double[] rowC = new double[n];
                for (int j = 0; j < n; j++) rowC[j] = constraints[i, j];

                if (bVal < 0)
                {
                    for (int j = 0; j < n; j++) rowC[j] = -rowC[j];
                    bVal = -bVal;
                    if (signs[i] == "<=") signs[i] = ">=";
                    else if (signs[i] == ">=") signs[i] = "<=";
                }

                if (signs[i] == "<=")
                {
                    rLabels.Add($"y{i + 1}");
                    for (int j = 0; j < n; j++) table[i, j] = rowC[j];
                    table[i, cLabels.Count] = bVal;
                }
                else if (signs[i] == "=")
                {
                    rLabels.Add("0");
                    for (int j = 0; j < n; j++) table[i, j] = rowC[j];
                    table[i, cLabels.Count] = bVal;
                }
                else // ">="
                {
                    rLabels.Add($"y{i + 1}");
                    for (int j = 0; j < n; j++) table[i, j] = -rowC[j];
                    table[i, cLabels.Count] = -bVal;
                }
            }

            for (int j = 0; j < n; j++) table[m, j] = isMax ? -zCoeff[j] : zCoeff[j];
            table[m, n] = 0;

            var tableau = new SimplexTableau(table, rLabels, cLabels);

            Console.WriteLine("Вхідна симплекс-таблиця:");
            ConsoleHelpers.PrintLPTable(tableau.Matrix, tableau.RLabels, tableau.CLabels, $"Z{functionIndex}");

            // Фаза 1: Скорочення таблиці
            while (tableau.RLabels.Contains("0"))
            {
                int r = tableau.RLabels.IndexOf("0");
                int s = -1;

                for (int j = 0; j < tableau.CLabels.Count; j++)
                {
                    if (Math.Abs(tableau.Matrix[r, j]) > 1e-7)
                    {
                        double ratio = tableau.Matrix[r, tableau.CLabels.Count] / tableau.Matrix[r, j];
                        if (ratio < 0) continue;
                        bool valid = true;
                        for (int i = 0; i < tableau.RLabels.Count; i++)
                        {
                            if (i != r && tableau.Matrix[i, j] > 1e-7)
                            {
                                if (tableau.Matrix[i, tableau.CLabels.Count] / tableau.Matrix[i, j] < ratio - 1e-7) { valid = false; break; }
                            }
                        }
                        if (valid) { s = j; break; }
                    }
                }

                if (s == -1) // Fallback 
                    for (int j = 0; j < tableau.CLabels.Count; j++) if (Math.Abs(tableau.Matrix[r, j]) > 1e-7) { s = j; break; }

                tableau.PerformMJE(r, s);

                Console.WriteLine("Знайдено опорний розв’язок:\nСимплекс-таблиця:");
                ConsoleHelpers.PrintLPTable(tableau.Matrix, tableau.RLabels, tableau.CLabels, $"Z{functionIndex}");
                double[] tempX = tableau.ExtractSolution(n);
                Console.WriteLine($"X = ({string.Join("; ", tempX.Select(v => $"{v:F2}"))})\n");
            }

            // Фаза 2: Оптимізація
            while (true)
            {
                int s = -1;
                for (int j = 0; j < tableau.CLabels.Count; j++) if (tableau.Matrix[tableau.RLabels.Count, j] < -1e-7) { s = j; break; }
                if (s == -1) break;

                int r = -1;
                double minRatio = double.MaxValue;
                for (int i = 0; i < tableau.RLabels.Count; i++)
                {
                    if (tableau.Matrix[i, s] > 1e-7)
                    {
                        double ratio = tableau.Matrix[i, tableau.CLabels.Count] / tableau.Matrix[i, s];
                        if (ratio >= -1e-7 && ratio < minRatio) { minRatio = ratio; r = i; }
                    }
                }
                if (r == -1) { Console.WriteLine("Функція мети не обмежена!\n"); break; }

                tableau.PerformMJE(r, s);

                Console.WriteLine("Знайдено опорний розв’язок:\nСимплекс-таблиця:");
                ConsoleHelpers.PrintLPTable(tableau.Matrix, tableau.RLabels, tableau.CLabels, $"Z{functionIndex}");
                double[] tempX = tableau.ExtractSolution(n);
                Console.WriteLine($"X = ({string.Join("; ", tempX.Select(v => $"{v:F2}"))})\n");
            }

            Console.WriteLine("Знайдено оптимальний розв’язок:\nСимплекс-таблиця:");
            ConsoleHelpers.PrintLPTable(tableau.Matrix, tableau.RLabels, tableau.CLabels, $"Z{functionIndex}");

            double[] optX = tableau.ExtractSolution(n);
            double optZ = isMax ? tableau.Matrix[tableau.RLabels.Count, tableau.CLabels.Count] : -tableau.Matrix[tableau.RLabels.Count, tableau.CLabels.Count];
            string funcType = isMax ? "Max" : "Min";

            Console.WriteLine($"X{functionIndex}* = ({string.Join("; ", optX.Select(v => $"{v:F2}"))})");
            Console.WriteLine($"{funcType} (Z{functionIndex}) = {optZ:F2}\n");

            return (optX, optZ);
        }
    }
}