using System.Text;

namespace Lab_4
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("=== Розв'язання ігор з природою ===\n");

            int m = ConsoleHelpers.ReadInt("Введіть кількість стратегій гравця (m): ");
            int n = ConsoleHelpers.ReadInt("Введіть кількість станів природи (n): ");

            double[,] U = new double[m, n];
            Console.WriteLine($"\nВведіть матрицю корисності результатів U по рядкам ({n} чисел через пробіл):");
            for (int i = 0; i < m; i++)
            {
                double[] row = ConsoleHelpers.ParseRow(n);
                for (int j = 0; j < n; j++) U[i, j] = row[j];
            }

            Console.Write("\nВведіть коефіцієнт y (песимізму) для критерію Гурвіца (наприклад, 0,3): ");
            if (!double.TryParse(Console.ReadLine()?.Replace('.', ','), out double alpha)) alpha = 0.3;

            Console.WriteLine($"\nВведіть {n} ймовірностей станів природи для критерію Байєса (через пробіл, сума = 1):");
            double[] p = ConsoleHelpers.ParseRow(n);
            NatureGameSolver.GenerateProtocol(U, alpha, p);
        }
    }
}