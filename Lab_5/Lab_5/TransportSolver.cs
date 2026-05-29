namespace Lab_5
{
    class TransportSolver
    {
        public static void Solve(double[,] C, double[] A, double[] B)
        {
            BalanceProblem(ref C, ref A, ref B);
            PrintInputData(C, A, B);

            int method = ConsoleHelpers.ReadInt("Оберіть метод побудови опорного плану (1 - Північно-західного кута, 2 - Мінімального елемента): ");

            BuildInitialPlan(method, C, A, B, out double[,] plan, out bool[,] isBasic);
            PrintPlan(plan, isBasic, C, "Опорний план перевезень:");

            OptimizePlan(ref plan, ref isBasic, C);

            int useSimplex = ConsoleHelpers.ReadInt("\nРозв'язати транспортну задачу симплекс-методом? (1 - так, 2 - ні): ");
            if (useSimplex == 1)
            {
                Console.WriteLine("\nПошук оптимального розв’язку задачі лінійного програмування:\n");
                SimplexMapper.SolveSimplex(C, A, B);
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }

        /// <summary>
        /// Балансує транспортну задачу, додавши фіктивний попит або пропозицію, коли загальний попит і пропозиція відрізняються.
        /// </summary>
        /// <remarks>Записує інформаційні повідомлення в консоль та обробляє підсумки рівними в межах
        /// допуску 1e-7. Додає стовпець з нульовою вартістю (фіктивне місце призначення), коли загальна пропозиція перевищує загальний попит, або
        /// рядок з нульовою вартістю (фіктивне джерело), ​​коли загальний попит перевищує загальну пропозицію.</remarks>
        /// <param name="C">Матриця вартості; може бути замінена матрицею зміненого розміру, яка містить фіктивний рядок або стовпець нулів.</param>
        /// <param name="A">Кількість поставок; може бути замінена масивом зміненого розміру, який включає фіктивну пропозицію для вирівнювання загальних сум.</param>
        /// <param name="B">Кількість заявок; може бути замінена масивом зміненого розміру, який включає фіктивну заявку для вирівнювання загальних сум.</param>
        private static void BalanceProblem(ref double[,] C, ref double[] A, ref double[] B)
        {
            double sumA = A.Sum();
            double sumB = B.Sum();
            int m = A.Length;
            int n = B.Length;

            if (Math.Abs(sumA - sumB) > 1e-7)
            {
                Console.WriteLine($"Задача відкрита (Сума запасів: {sumA}, Сума заявок: {sumB}).");
                if (sumA > sumB)
                {
                    Console.WriteLine("Додаємо фіктивний пункт призначення.");
                    double[] newB = new double[n + 1];
                    Array.Copy(B, newB, n);
                    newB[n] = sumA - sumB;

                    double[,] newC = new double[m, n + 1];
                    for (int i = 0; i < m; i++)
                    {
                        for (int j = 0; j < n; j++) newC[i, j] = C[i, j];
                        newC[i, n] = 0;
                    }
                    B = newB; C = newC;
                }
                else
                {
                    Console.WriteLine("Додаємо фіктивний пункт відправлення.");
                    double[] newA = new double[m + 1];
                    Array.Copy(A, newA, m);
                    newA[m] = sumB - sumA;

                    double[,] newC = new double[m + 1, n];
                    for (int i = 0; i < m; i++)
                    {
                        for (int j = 0; j < n; j++) newC[i, j] = C[i, j];
                    }
                    for (int j = 0; j < n; j++) newC[m, j] = 0;
                    A = newA; C = newC;
                }
                Console.WriteLine();
            }
        }

        private static void PrintInputData(double[,] C, double[] A, double[] B)
        {
            int m = A.Length;
            int n = B.Length;
            Console.WriteLine("Матриця вартостей:");
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++) Console.Write($"{C[i, j],4}");
                Console.WriteLine();
            }
            Console.WriteLine("Вектор запасів:\n  " + string.Join("  ", A));
            Console.WriteLine("Вектор заявок:\n  " + string.Join("  ", B) + "\n");
        }

        /// <summary>
        /// Будує початковий можливий план транспортування та позначає основні змінні, використовуючи заданий
        /// алгоритм початкового плану.
        /// </summary>
        /// <remarks>Записує прогрес розподілу та послідовність заповнених комірок у консоль. Клонує A
        /// та B для внутрішніх обчислень, створює до m + n - 1 базових змінних та не забезпечує примусове виконання або балансування
        /// загальної пропозиції та попиту.</remarks>
        /// <param name="method">Селектор алгоритму: 1 для правила північно-західного кута; будь-яке інше значення вибирає правило мінімального елемента.</param>
        /// <param name="C">Матриця вартості розміром m на n, що використовується вибором мінімальних елементів; значення не змінюються.</param>
        /// <param name="A">Вектор пропозиції довжиною m.</param>
        /// <param name="B">Вектор попиту довжиною n.</param>
        /// <param name="plan">Вихідний параметр, який отримує результуючий план m-by-n з розподіленими обсягами відвантаження.</param>
        /// <param name="isBasic">Вихідний параметр, який отримує булеву маску розміром mxn, де true вказує на базову змінну.</param>
        private static void BuildInitialPlan(int method, double[,] C, double[] A, double[] B, out double[,] plan, out bool[,] isBasic)
        {
            int m = A.Length;
            int n = B.Length;
            plan = new double[m, n];
            isBasic = new bool[m, n];

            double[] workA = (double[])A.Clone();
            double[] workB = (double[])B.Clone();
            List<string> seq = [];

            if (method == 1)
            {
                Console.WriteLine("\nПошук опорного плану перевезень методом північно-західного кута:\n");
                int i = 0, j = 0;
                while (i < m && j < n)
                {
                    double val = Math.Min(workA[i], workB[j]);
                    plan[i, j] = val;
                    isBasic[i, j] = true;
                    seq.Add($"(x{i + 1}{j + 1} = {val})");
                    workA[i] -= val;
                    workB[j] -= val;

                    if (workA[i] == 0 && i < m - 1) i++;
                    else j++;
                }
            }
            else
            {
                Console.WriteLine("\nПошук опорного плану перевезень методом мінімального елемента:\n");
                bool[] rowDone = new bool[m];
                bool[] colDone = new bool[n];
                for (int step = 0; step < m + n - 1; step++)
                {
                    double minC = double.MaxValue;
                    int bI = -1, bJ = -1;
                    for (int i = 0; i < m; i++)
                    {
                        if (rowDone[i]) continue;
                        for (int j = 0; j < n; j++)
                        {
                            if (colDone[j]) continue;
                            if (C[i, j] < minC) { minC = C[i, j]; bI = i; bJ = j; }
                        }
                    }
                    if (bI == -1) break;

                    double val = Math.Min(workA[bI], workB[bJ]);
                    plan[bI, bJ] = val;
                    isBasic[bI, bJ] = true;
                    seq.Add($"(x{bI + 1}{bJ + 1} = {val})");
                    workA[bI] -= val;
                    workB[bJ] -= val;

                    if (workA[bI] == 0 && !rowDone[bI]) rowDone[bI] = true;
                    else colDone[bJ] = true;
                }
            }

            Console.WriteLine("Послідовність заповнення таблиці:\n" + string.Join("->", seq) + "\n");
        }

        /// <summary>
        /// Оптимізує план перевезень за допомогою методу потенціалів (MODI), виконуючи ітеративні повороти, доки
        /// розв'язок не задовольнятиме умови оптимальності.
        /// </summary>
        /// <remarks>Обчислює потенціали рядків і стовпців, оцінює знижені витрати для визначення покращення
        /// небазисних комірок, вибирає вхідну комірку, будує цикл коригування, обчислює величину опорної точки (λ),
        /// оновлює план і базис вздовж циклу та повторює, доки всі знижені витрати не задовольнять оптимальність.
        /// Хід виконання та проміжні плани записуються в консоль.</remarks>
        /// <param name="plan">План перевезень як масив кількостей відвантажень розміром m на n; змінено на місці,
        /// щоб відобразити опорні точки та оновлені розподіли.</param>
        /// <param name="isBasic">Логічний масив розміром m на n, який вказує, які комірки є базовими (виділеними); оновлюється на місці, щоб позначити вхід та
        /// вихід з базових комірок під час змін.</param>
        /// <param name="C">Матриця витрат m на n, яка використовується для обчислення потенціалів рядків і стовпців і приведених (непрямих) витрат.</param>
        private static void OptimizePlan(ref double[,] plan, ref bool[,] isBasic, double[,] C)
        {
            int m = plan.GetLength(0);
            int n = plan.GetLength(1);

            while (true)
            {
                Console.WriteLine("Пошук оптимального плану перевезень методом потенціалів:\n");

                double?[] U = new double?[m];
                double?[] V = new double?[n];
                U[0] = 0;

                bool changed = true;
                while (changed)
                {
                    changed = false;
                    for (int i = 0; i < m; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            if (isBasic[i, j])
                            {
                                if (U[i].HasValue && !V[j].HasValue) { V[j] = C[i, j] - U[i]; changed = true; }
                                else if (V[j].HasValue && !U[i].HasValue) { U[i] = C[i, j] - V[j]; changed = true; }
                            }
                        }
                    }
                }

                Console.WriteLine("Потенціали постачальників:\n   " + string.Join("  ", U.Select(x => x.HasValue ? x.Value.ToString() : "x")));
                Console.WriteLine("Потенціали споживачів:\n   " + string.Join("  ", V.Select(x => x.HasValue ? x.Value.ToString() : "x")));

                double[,] Delta = new double[m, n];
                double maxDiff = 0;
                int badI = -1, badJ = -1;

                Console.WriteLine("Непрямі вартості:");
                List<string> badCells = [];

                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (isBasic[i, j]) Console.Write("   x");
                        else
                        {
                            Delta[i, j] = (U[i] ?? 0) + (V[j] ?? 0);
                            Console.Write($"{Delta[i, j],4}");
                            if (Delta[i, j] > C[i, j])
                            {
                                badCells.Add($"[{i + 1}, {j + 1}]");
                                double diff = Delta[i, j] - C[i, j];
                                if (diff > maxDiff) { maxDiff = diff; badI = i; badJ = j; }
                            }
                        }
                    }
                    Console.WriteLine();
                }

                if (badCells.Count == 0)
                {
                    Console.WriteLine("\nУмова оптимальності виконується.\n");
                    PrintPlan(plan, isBasic, C, "Знайдено оптимальний план перевезень:");
                    break;
                }

                Console.WriteLine("\nУмова оптимальності не виконується.");
                Console.WriteLine($"Знайдено «проблемні» клітини: {string.Join("; ", badCells)}\n");

                List<Point>? cycleNullable = CycleFinder.FindCycle(isBasic, badI, badJ);
                if (cycleNullable == null || cycleNullable.Count == 0) { Console.WriteLine("Не вдалося побудувати цикл для обраної клітини. Припинення оптимізації."); break; }

                List<Point> cycle = cycleNullable; // non-nullable
                Console.WriteLine("Побудовано цикл:");
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        int cIdx = cycle.FindIndex(p => p.X == i && p.Y == j);
                        if (cIdx == 0) Console.Write("   λ");
                        else if (cIdx % 2 == 1) Console.Write("   -");
                        else if (cIdx % 2 == 0 && cIdx > 0) Console.Write("   +");
                        else Console.Write("   x");
                    }
                    Console.WriteLine();
                }

                double lambda = double.MaxValue;
                Point? leaveNode = null;
                for (int k = 1; k < cycle.Count; k += 2)
                {
                    double v = plan[cycle[k].X, cycle[k].Y];
                    if (v < lambda) { lambda = v; leaveNode = cycle[k]; }
                }

                Console.WriteLine($"\nЗнайдено значення λ: {lambda}, економія: {maxDiff}\n");

                plan[badI, badJ] = lambda;
                isBasic[badI, badJ] = true;

                for (int k = 1; k < cycle.Count; k++)
                {
                    if (k % 2 == 1)
                    {
                        plan[cycle[k].X, cycle[k].Y] -= lambda;
                        if (cycle[k].X == leaveNode?.X && cycle[k].Y == leaveNode.Y) isBasic[cycle[k].X, cycle[k].Y] = false;
                    }
                    else
                    {
                        plan[cycle[k].X, cycle[k].Y] += lambda;
                    }
                }

                PrintPlan(plan, isBasic, C, "Новий план перевезень:");
            }
        }

        private static void PrintPlan(double[,] plan, bool[,] isBasic, double[,] C, string title)
        {
            Console.WriteLine(title);
            int m = plan.GetLength(0);
            int n = plan.GetLength(1);
            double sum = 0;
            List<string> terms = [];

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (isBasic[i, j])
                    {
                        Console.Write($"{plan[i, j],4}");
                        sum += plan[i, j] * C[i, j];
                        terms.Add($"{plan[i, j]} * {C[i, j]}");
                    }
                    else Console.Write("   x");
                }
                Console.WriteLine();
            }

            Console.WriteLine("\nВартість перевезень за " + (title.Contains("оптимальний") ? "оптимальним" : "опорним") + " планом:");
            Console.WriteLine($"S = {string.Join(" + ", terms)} = {sum:F2} грн\n");
        }
    }
}
