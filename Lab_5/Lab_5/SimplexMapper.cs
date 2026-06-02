namespace Lab_5
{
    class SimplexMapper
    {
        public static void SolveSimplex(double[,] C, double[] A, double[] B)
        {
            int m = A.Length;
            int n = B.Length;
            int vars = m * n;

            Console.WriteLine("Постановка задачі:");
            List<string> zTerms = [];
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    if (C[i, j] > 0) zTerms.Add($"{C[i, j]}x{i * n + j + 1}");

            Console.WriteLine($"Z = {string.Join(" + ", zTerms)} -> min\n");
            Console.WriteLine("Перехід до задачі максимізації функції мети Z':");
            Console.WriteLine($"Z' = - {string.Join(" - ", zTerms)} -> max\n");
            Console.WriteLine("Обмеження:");

            double[,] table = new double[m + n + 1, vars + 1];
            List<string> rLabels = [];
            List<string> cLabels = [.. Enumerable.Range(1, vars).Select(i => $"x{i}")];

            for (int i = 0; i < m; i++)
            {
                List<string> eq = [];
                for (int j = 0; j < n; j++)
                {
                    table[i, i * n + j] = 1.0;
                    eq.Add($"- x{i * n + j + 1}");
                }
                table[i, vars] = A[i];
                rLabels.Add($"y{i + 1}");
                Console.WriteLine($"{string.Join(" ", eq)} + {A[i]} >= 0");
            }

            for (int j = 0; j < n; j++)
            {
                List<string> eq = [];
                for (int i = 0; i < m; i++)
                {
                    table[m + j, i * n + j] = -1.0;
                    eq.Add($"x{i * n + j + 1}");
                }
                table[m + j, vars] = -B[j];
                rLabels.Add($"y{m + j + 1}");
                Console.WriteLine($"{string.Join(" + ", eq)} - {B[j]} >= 0");
            }
            Console.WriteLine($"x[j]>=0, j=1,{vars}\n");

            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    table[m + n, i * n + j] = C[i, j];
            table[m + n, vars] = 0;

            Console.WriteLine("Вхідна симплекс-таблиця:\n");
            PrintTable(table, rLabels, cLabels);

            // Фаза 1: Двоїстий симплекс-метод
            while (true)
            {
                int r = -1;
                double minB = -1e-7;
                for (int i = 0; i < m + n; i++)
                {
                    if (table[i, vars] < minB) { minB = table[i, vars]; r = i; }
                }
                if (r == -1) break;

                int s = -1;
                double minRatio = double.MaxValue;
                for (int j = 0; j < vars; j++)
                {
                    if (table[r, j] < -1e-7)
                    {
                        double ratio = Math.Abs(table[m + n, j] / table[r, j]);
                        if (ratio < minRatio) { minRatio = ratio; s = j; }
                    }
                }

                if (s == -1) { Console.WriteLine("Немає розв'язку!"); return; }
                PerformMJE(ref table, rLabels, cLabels, r, s);
            }

            // вивід опорного розв'язку після Фази 1
            Console.WriteLine("Знайдено опорний розв’язок:\n\nСимплекс-таблиця:\n");
            PrintTable(table, rLabels, cLabels);
            PrintXVector(table, rLabels, vars);
            PrintTransportPlanFromSimplex(table, rLabels, m, n, C, "Знайдено опорний план перевезень:");

            // Фаза 2: Прямий симплекс-метод
            bool phase2DidWork = false;
            while (true)
            {
                int s = -1;
                for (int j = 0; j < cLabels.Count; j++) if (table[m + n, j] < -1e-7) { s = j; break; }
                if (s == -1) break;

                int r = -1;
                double minRatio = double.MaxValue;
                for (int i = 0; i < m + n; i++)
                {
                    if (table[i, s] > 1e-7)
                    {
                        double ratio = table[i, vars] / table[i, s];
                        if (ratio >= -1e-7 && ratio < minRatio) { minRatio = ratio; r = i; }
                    }
                }
                if (r == -1) break;

                PerformMJE(ref table, rLabels, cLabels, r, s);
                phase2DidWork = true;
            }

            // ВИВІД ОПТИМАЛЬНОГО РОЗВ'ЯЗКУ
            if (phase2DidWork)
            {
                Console.WriteLine("Після оптимізації знайдено оптимальний розв’язок:\n\nСимплекс-таблиця:\n");
                PrintTable(table, rLabels, cLabels);
                PrintXVector(table, rLabels, vars);
            }
            else
            {
                Console.WriteLine("Опорний розв'язок вже є оптимальним (Фаза 2 не потребувала кроків).\n");
            }

            PrintTransportPlanFromSimplex(table, rLabels, m, n, C, "Знайдено оптимальний план перевезень:");
            Console.WriteLine($"Min (Z) = {Math.Abs(table[m + n, vars]):F2} грн\n");
        }

        /// <summary>
        /// Допоміжний метод для виведення матриці перевезень прямо із симплекс-таблиці
        /// </summary>
        /// <param name="table">Масив, що представляє симплекс-таблицю</param>
        /// <param name="rLabels">Список назв рядків</param>
        /// <param name="m">Кількість постачальників</param>
        /// <param name="n">Кількість споживачів</param>
        /// <param name="C">Матриця вартостей перевезень</param>
        /// <param name="title">Заголовок для виведення</param>
        static void PrintTransportPlanFromSimplex(double[,] table, List<string> rLabels, int m, int n, double[,] C, string title)
        {
            Console.WriteLine(title);
            double sum = 0;
            List<string> terms = [];

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    // відновлення індексу змінної X, яка відповідає цій клітинці
                    int varIndex = i * n + j + 1;
                    int rowIdx = rLabels.IndexOf($"x{varIndex}");

                    // пошук її значення у базисі
                    double val = rowIdx != -1 ? table[rowIdx, m * n] : 0.0;

                    if (val > 1e-7)
                    {
                        Console.Write($"{val,4} ");
                        sum += val * C[i, j];
                        terms.Add($"{val} * {C[i, j]}");
                    }
                    else
                    {
                        Console.Write($"{"x",4} ");
                    }
                }
                Console.WriteLine();
            }

            string planType = title.Contains("оптимальний") ? "оптимальним" : "опорним";
            Console.WriteLine($"\nВартість перевезень за {planType} планом:");
            Console.WriteLine($"S = {string.Join(" + ", terms)} = {sum}\n");
        }

        /// <summary>
        /// Допоміжний метод для виведення вектора X
        /// </summary>
        /// <param name="table">Масив, що представляє симплекс-таблицю</param>
        /// <param name="rLabels">Список назв рядків</param>
        /// <param name="vars">Кількість змінних</param>
        static void PrintXVector(double[,] table, List<string> rLabels, int vars)
        {
            double[] X = new double[vars];
            for (int i = 0; i < vars; i++)
            {
                int idx = rLabels.IndexOf($"x{i + 1}");
                X[i] = idx != -1 ? table[idx, vars] : 0.0;
            }
            Console.WriteLine($"X = ({string.Join("; ", X.Select(v => $"{v:F2}"))})\n");
        }

        static void PerformMJE(ref double[,] table, List<string> rLabels, List<string> cLabels, int r, int s)
        {
            int rows = rLabels.Count, cols = cLabels.Count;
            double pivot = table[r, s];
            double[,] next = new double[rows + 1, cols + 1];

            for (int i = 0; i <= rows; i++)
                for (int j = 0; j <= cols; j++)
                {
                    if (i == r && j == s) next[i, j] = 1.0 / pivot;
                    else if (i == r) next[i, j] = table[i, j] / pivot;
                    else if (j == s) next[i, j] = -table[i, j] / pivot;
                    else next[i, j] = table[i, j] - (table[i, s] * table[r, j] / pivot);
                }
            table = next;
            (cLabels[s], rLabels[r]) = (rLabels[r], cLabels[s]);
        }

        /// <summary>
        /// Метод для виведення симплекс-таблиці у зручному форматі. Виводить назви рядків та стовпців, а також значення у клітинках з двома знаками після коми.
        /// </summary>
        /// <param name="tab">Масив, що представляє симплекс-таблицю</param>
        /// <param name="rL">Список назв рядків</param>
        /// <param name="cL">Список назв стовпців</param>
        static void PrintTable(double[,] tab, List<string> rL, List<string> cL)
        {
            Console.Write($"{"",6}");
            for (int j = 0; j < cL.Count; j++) Console.Write($"{"-" + cL[j],8}");
            Console.WriteLine($"{"1",8}");
            Console.WriteLine(new string('-', 6 + (cL.Count + 1) * 8));

            for (int i = 0; i < rL.Count; i++)
            {
                Console.Write($"{rL[i],-4} =");
                for (int j = 0; j <= cL.Count; j++) Console.Write($"{tab[i, j],8:F2}");
                Console.WriteLine();
            }
            Console.Write($"{"Z",-4} =");
            for (int j = 0; j <= cL.Count; j++) Console.Write($"{tab[rL.Count, j],8:F2}");
            Console.WriteLine("\n");
        }
    }
}