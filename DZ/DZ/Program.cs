using System.Text;

namespace DZ
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            while (true)
            {
                Console.WriteLine("Багатокритеріальна оптимізація");
                Console.WriteLine("1. Заготовлений Варіант 12");
                Console.WriteLine("2. Ручне введення задачі");
                Console.WriteLine("0. Вихід");
                Console.Write("\nОберіть дію (0-2): ");

                string? choice = Console.ReadLine()?.Trim();
                switch (choice)
                {
                    case "1":
                        PresetData();
                        break;
                    case "2":
                        ManualInput();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Помилка вводу. Натисніть будь-яку клавішу...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        static void ManualInput()
        {
            Console.WriteLine("Ручне введення задачі:\n");

            int k = ConsoleHelpers.ReadInt("Введіть кількість функцій мети (k): ");
            int n = ConsoleHelpers.ReadInt("Введіть кількість змінних (n): ");
            int m = ConsoleHelpers.ReadInt("Введіть кількість обмежень (m): ");

            double[,] zCoeffs = new double[k, n];
            bool[] isMax = new bool[k];

            for (int f = 0; f < k; f++)
            {
                Console.WriteLine($"\nКоефіцієнти цільової функції Z{f + 1}:");
                for (int j = 0; j < n; j++)
                {
                    zCoeffs[f, j] = ConsoleHelpers.ReadDouble($"Коефіцієнт при x{j + 1}: ");
                }

                while (true)
                {
                    Console.Write("Шукаємо максимум чи мінімум? (введіть 'max' або 'min'): ");
                    string? minMax = Console.ReadLine()?.Trim().ToLower();
                    if (minMax == "max" || minMax == "min")
                    {
                        isMax[f] = (minMax == "max");
                        break;
                    }
                    Console.WriteLine("Помилка! Введіть 'max' або 'min'.");
                }
            }

            Console.WriteLine("\n--- Введення системи обмежень ---");
            double[,] constraints = new double[m, n];
            string[] signs = new string[m];
            double[] b = new double[m];

            for (int i = 0; i < m; i++)
            {
                Console.WriteLine($"\nОбмеження {i + 1}:");
                for (int j = 0; j < n; j++)
                {
                    constraints[i, j] = ConsoleHelpers.ReadDouble($"Коефіцієнт при x{j + 1}: ");
                }

                while (true)
                {
                    Console.Write("Знак ('<=', '>=', '='): ");
                    string? sign = Console.ReadLine()?.Trim();
                    if (sign == "<=" || sign == ">=" || sign == "=")
                    {
                        signs[i] = sign;
                        break;
                    }
                    Console.WriteLine("Помилка! Введіть '<=', '>=' або '='.");
                }

                b[i] = ConsoleHelpers.ReadDouble("Вільний член (b): ");
            }

            Console.WriteLine("Згенерований протокол обчислення:\n");
            MulticriteriaSolver.SolveManual(k, n, m, zCoeffs, isMax, constraints, signs, b);
            Console.WriteLine("Натисніть будь-яку клавішу для повернення в меню...");
            Console.ReadKey();
        }

        static void PresetData()
        {
            int k = 3, n = 6, m = 3;

            double[,] zCoeffs = {
                { 2, 1, -3, 0, -4, -1 },
                { 1, 2, 2, -1, 0, 0 },
                { -2, 0, 3, -1, 1, 0 }
            };
            bool[] isMax = [true, true, true];

            double[,] constraints = {
                { 1, 3, -1, 0, 2, 0 },
                { 0, -4, 3, 0, 8, 1 },
                { 0, -2, 4, 1, 0, 0 }
            };
            string[] signs = ["=", "=", "=" ];
            double[] b = [ 7, 10, 12 ];

            MulticriteriaSolver.SolveManual(k, n, m, zCoeffs, isMax, constraints, signs, b);
            Console.WriteLine("Натисніть будь-яку клавішу для повернення в меню...");
            Console.ReadKey();
        }
    }
}