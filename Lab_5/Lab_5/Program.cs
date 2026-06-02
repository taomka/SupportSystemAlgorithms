using System.Text;

namespace Lab_5
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            while (true)
            {
                Console.WriteLine("Транспортна задача");
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
                { 9, 8, 6, 9 },
                { 4, 7, 5, 10 },
                { 10, 8, 6, 8 }
            };
            double[] A = [70, 65, 55];
            double[] B = [85, 35, 30, 40];

            TransportSolver.Solve(C, A, B);
        }

        static void RunManualInput()
        {
            int m = ConsoleHelpers.ReadInt("Введіть кількість пунктів відправлення (m): ");
            int n = ConsoleHelpers.ReadInt("Введіть кількість пунктів призначення (n): ");

            double[,] C = new double[m, n];
            Console.WriteLine("\nВведіть матрицю вартостей (по рядках, через пробіл):");
            for (int i = 0; i < m; i++)
            {
                double[] row = ConsoleHelpers.ParseRow(n);
                for (int j = 0; j < n; j++) C[i, j] = row[j];
            }

            Console.WriteLine("\nВведіть вектор запасів (через пробіл):");
            double[] A = ConsoleHelpers.ParseRow(m);

            Console.WriteLine("\nВведіть вектор заявок (через пробіл):");
            double[] B = ConsoleHelpers.ParseRow(n);

            TransportSolver.Solve(C, A, B);
        }
    }
}