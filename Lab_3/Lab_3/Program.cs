using System.Text;

namespace Lab_3
{
    class Program
    {
        static void Main()
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
                while (true)
                {
                    Console.WriteLine("=== Меню: Розв'язання та моделювання матричних ігор ===");
                    Console.WriteLine("1. Моделювання матриці А1 (Варіант 12)");
                    Console.WriteLine("2. Ввести власну матрицю");
                    Console.WriteLine("0. Вихід");
                    Console.Write("\nОберіть дію (0-2): ");

                    string? choice = Console.ReadLine()?.Trim();

                    switch (choice)
                    {
                        case "1":
                            RunPresetSimulation();
                            break;
                        case "2":
                            RunUniversalSolverAndSimulation();
                            break;
                        case "0":
                            return;
                        default:
                            Console.WriteLine("Помилка вводу. Натисніть будь-яку клавішу для продовження...");
                            Console.ReadKey();
                            break;
                    }
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void RunPresetSimulation()
        {
            Console.WriteLine("=== Дія 1: Моделювання із заготовками ===\n");

            double[,] A = {
                { -2, -1, -2 },
                {  4, -2,  1 },
                {  1,  3, -5 }
            };

            double[] P = { 0.0, 8.0 / 11.0, 3.0 / 11.0 };
            double[] Q = { 0.0, 6.0 / 11.0, 5.0 / 11.0 };
            double V = -7.0 / 11.0;

            Console.WriteLine("Матриця А1:");
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++) Console.Write($"{A[i, j],4} ");
                Console.WriteLine();
            }
            Console.WriteLine($"\nТеоретичні стратегії гравця А: P = ({P[0]:F3}; {P[1]:F3}; {P[2]:F3})");
            Console.WriteLine($"Теоретичні стратегії гравця B: Q = ({Q[0]:F3}; {Q[1]:F3}; {Q[2]:F3})");
            Console.WriteLine($"Теоретична ціна гри: V = {V:F4}\n");

            RunSimulation(A, P, Q, V);
        }

        static void RunUniversalSolverAndSimulation()
        {
            Console.WriteLine("=== Дія 2: Ручний ввід матриці ===\n");

            int m = ConsoleHelpers.ReadInt("Введіть кількість рядків матриці (m): ");
            int n = ConsoleHelpers.ReadInt("Введіть кількість стовпців матриці (n): ");

            double[,] A = new double[m, n];
            Console.WriteLine($"Введіть матрицю по рядкам ({n} чисел через пробіл у кожному рядку):");

            for (int i = 0; i < m; i++)
            {
                double[] row = ConsoleHelpers.ParseMatrixRow(n);
                for (int j = 0; j < n; j++) A[i, j] = row[j];
            }

            Console.WriteLine("\nМатриця А:");
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++) Console.Write($"{A[i, j],4} ");
                Console.WriteLine();
            }

            if (MatrixGameSolver.FindSaddlePoint(A, out double[] P, out double[] Q, out double V))
            {
                Console.WriteLine("\nСідлову точку знайдено! Гра має розв'язок у чистих стратегіях.");
                Console.WriteLine("Моделювання Монте-Карло для чистих стратегій не є обов'язковим, але його можна провести.");
            }
            else
            {
                // Якщо сідлової точки немає, розв'язуємо симплекс-методом
                MatrixGameSolver.SolveSimplex(A, out P, out Q, out V);
            }

            Console.Write("\nПровести імітаційне моделювання (Монте-Карло) для знайдених стратегій? (y/n): ");
            if (Console.ReadLine()?.Trim().ToLower() == "y")
            {
                RunSimulation(A, P, Q, V);
            }
            else
            {
                Console.WriteLine("\nНатисніть будь-яку клавішу для повернення в меню...");
                Console.ReadKey();
            }
        }

        static void RunSimulation(double[,] A, double[] P, double[] Q, double theoreticalValue)
        {
            int m = A.GetLength(0);
            int n = A.GetLength(1);

            int N = ConsoleHelpers.ReadInt("Введіть кількість партій для моделювання (N): ");

            Console.Write("Експортувати протокол моделювання у файл Excel (.csv)? (y/n): ");
            bool exportToExcel = Console.ReadLine()?.Trim().ToLower() == "y";

            Random rnd = new Random();
            double totalWinA = 0;
            int[] countA = new int[m];
            int[] countB = new int[n];

            StreamWriter? csvWriter = null;
            if (exportToExcel)
            {
                // UTF8Encoding(true) додає BOM (щоб Excel правильно бачив кирилицю)
                csvWriter = new StreamWriter("Simulation_Protocol.csv", false, new UTF8Encoding(true));
                csvWriter.WriteLine("Номер партії;Випадкове число гравця А;Стратегія гравця А;Випадкове число гравця В;Стратегія гравця В;Виграш А;Накопичений виграш А;Середній виграш А (ціна гри)");
            }

            Console.WriteLine("\nТаблиця 1.1 – Протокол моделювання матричної гри:");
            Console.WriteLine(new string('-', 105));
            Console.WriteLine($"{"Номер",-5} | {"Випадкове",-10} | {"Стратегія",-9} | {"Випадкове",-10} | {"Стратегія",-9} | {"Виграш",-8} | {"Накопичений",-11} | {"Середній виграш",-15}");
            Console.WriteLine(new string('-', 105));

            for (int i = 1; i <= N; i++)
            {
                double rA = rnd.NextDouble();
                int stratA = GetStrategy(rA, P);
                countA[stratA - 1]++;

                double rB = rnd.NextDouble();
                int stratB = GetStrategy(rB, Q);
                countB[stratB - 1]++;

                double winA = A[stratA - 1, stratB - 1];
                totalWinA += winA;
                double avgWin = totalWinA / i;

                Console.WriteLine($"{i,5} | {rA,10:F4} | {stratA,9} | {rB,10:F4} | {stratB,9} | {winA,8:F2} | {totalWinA,11:F2} | {avgWin,15:F4}");

                if (exportToExcel)
                {
                    csvWriter?.WriteLine($"{i};{rA:F4};{stratA};{rB:F4};{stratB};{winA:F2};{totalWinA:F2};{avgWin:F4}");
                }
            }
            Console.WriteLine(new string('-', 105));

            if (exportToExcel)
            {
                csvWriter?.Close();
                Console.WriteLine($"\n[УСПІХ] Протокол збережено: {Path.GetFullPath("Simulation_Protocol.csv")}");
            }

            Console.WriteLine("\n=== Аналіз результатів моделювання ===");
            var expP = countA.Select(c => c / (double)N).ToArray();
            var expQ = countB.Select(c => c / (double)N).ToArray();

            Console.WriteLine($"Експериментальні стратегії P* = ({string.Join("; ", expP.Select(v => $"{v:F3}"))})");
            Console.WriteLine($"Експериментальні стратегії Q* = ({string.Join("; ", expQ.Select(v => $"{v:F3}"))})");
            Console.WriteLine($"Експериментальна ціна гри: {totalWinA / N:F4}");
            Console.WriteLine($"Теоретична ціна гри: {theoreticalValue:F4}");

            Console.WriteLine("\nНатисніть будь-яку клавішу для повернення в меню...");
            Console.ReadKey();
        }

        static int GetStrategy(double r, double[] probabilities)
        {
            double sum = 0;
            for (int i = 0; i < probabilities.Length; i++)
            {
                sum += probabilities[i];
                if (r <= sum) return i + 1;
            }
            return probabilities.Length;
        }
    }
}