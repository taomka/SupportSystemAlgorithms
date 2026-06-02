using System.Text;

namespace Lab_6
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            while (true)
            {
                Console.WriteLine("Задача про призначення");
                Console.WriteLine("1. Запустити на прикладі");
                Console.WriteLine("2. Ввести дані вручну");
                Console.WriteLine("ESC. Вийти");
                switch (Console.ReadKey())
                {
                    case var key when key.Key == ConsoleKey.D1:
                        RunPresetData();
                        break;
                    case var key when key.Key == ConsoleKey.D2:
                        RunManualInput();
                        break;
                    case var key when key.Key == ConsoleKey.Escape:
                        return;
                }
            }
        }

        static void RunPresetData()
        {
            double[,] C = {
                { 11, 5, 9, 10 },
                { 6, 9, 6, 5 },
                { 7, 11, 10, 8 },
                { 10, 5, 7, 9 }
            };

            HungarianSolver.Solve(C);
        }

        static void RunManualInput()
        {
            int n = ConsoleHelpers.ReadInt("Введіть розмірність матриці вартостей (N): ");

            double[,] C = new double[n, n];
            Console.WriteLine("\nВведіть матрицю вартостей (по рядках, через пробіл):");
            for (int i = 0; i < n; i++)
            {
                double[] row = ConsoleHelpers.ParseRow(n);
                for (int j = 0; j < n; j++) C[i, j] = row[j];
            }

            HungarianSolver.Solve(C);
        }
    }
}