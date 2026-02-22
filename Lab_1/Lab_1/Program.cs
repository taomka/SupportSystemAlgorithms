using System.Text;

namespace Lab_1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            // Введення розмірності
            int n = ReadHelpers.ReadInt("Введіть кількість рядків матриці (n): ");
            int m = ReadHelpers.ReadInt("Введіть кількість стовпців матриці (m): ");

            double[,] matrixA = new double[n, m];

            // Введення матриці A
            Console.WriteLine("\n--- Введення матриці A ---");
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    matrixA[i, j] = ReadHelpers.ReadDouble($"A[{i + 1}, {j + 1}]: ");
                }
            }

            // Перевірка на квадратність
            if (n == m)
            {
                Console.WriteLine("Матриця є квадратною. Переходимо до розв'язання СЛАР.\n");

                double[] vectorB = new double[n];
                Console.WriteLine("--- Введення вектора вільних членів B ---");
                for (int i = 0; i < n; i++)
                {
                    vectorB[i] = ReadHelpers.ReadDouble($"B[{i + 1}]: ");
                }
                RunProtocolExecution(matrixA, vectorB);
            }
            else
            {
                Console.WriteLine($"Матриця є прямокутною ({n}x{m}). Пошук оберненої матриці неможливий.");
                Console.WriteLine("Виконується лише обчислення рангу...\n");

                int rank = CalculateRank(matrixA);
                Console.WriteLine($"Ранг вхідної матриці A: r = {rank}");
            }
        }

        static void RunProtocolExecution(double[,] matrixA, double[] vectorB)
        {
            int rank = CalculateRank(matrixA);
            Console.WriteLine($"Ранг вхідної матриці A: r = {rank}\n");
            Console.WriteLine("Знаходження розв’язків СЛАР 1-м методом (за допомогою оберненої матриці):");
            double[,]? inverseMatrix = FindInverseMatrix(matrixA);

            if (inverseMatrix != null)
            {
                CalculateAndPrintRoots(inverseMatrix, vectorB);
            }
        }

        static int CalculateRank(double[,] inputMatrix)
        {
            int rows = inputMatrix.GetLength(0);
            int cols = inputMatrix.GetLength(1);
            double[,] matrix = (double[,])inputMatrix.Clone();
            int rank = 0;
            int limit = Math.Min(rows, cols);

            for (int k = 0; k < limit; k++)
            {
                if (Math.Abs(matrix[k, k]) > 1e-9)
                {
                    PerformJordanStep(matrix, rows, cols, k, k);
                    rank++;
                }
            }
            return rank;
        }

        static double[,]? FindInverseMatrix(double[,] inputMatrix)
        {
            int n = inputMatrix.GetLength(0);
            double[,] matrix = (double[,])inputMatrix.Clone();

            Console.WriteLine("\nЗнаходження оберненої матриці:\n");
            Console.WriteLine("Вхідна матриця:");
            PrintMatrix(matrix);

            Console.WriteLine("\nПротокол обчислення:");

            for (int k = 0; k < n; k++)
            {
                Console.WriteLine($"\nКрок #{k + 1}\n");
                double elem = matrix[k, k];
                Console.WriteLine($"Розв’язувальний елемент: A[{k + 1}, {k + 1}] = {elem:F2}\n");

                if (Math.Abs(elem) < 1e-9)
                {
                    Console.WriteLine("Помилка: Розв'язувальний елемент дорівнює нулю. Оберненої матриці не існує.");
                    return null;
                }
                PerformJordanStep(matrix, n, n, k, k);

                Console.WriteLine("Матриця після виконання ЗЖВ:");
                PrintMatrix(matrix);
            }

            Console.WriteLine("\nОбернена матриця:\n");
            PrintMatrix(matrix);

            return matrix;
        }

        static void CalculateAndPrintRoots(double[,] inverseMatrix, double[] vectorB)
        {
            int n = inverseMatrix.GetLength(0);

            Console.WriteLine("\nВхідна матриця B:");
            for (int i = 0; i < n; i++)
            {
                Console.WriteLine($" {vectorB[i]:F2}");
            }

            Console.WriteLine("\nОбчислення розв’язків:\n");
            double[] resultX = new double[n];

            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                Console.Write($"X[{i + 1}] = ");

                for (int j = 0; j < n; j++)
                {
                    double valMatrix = inverseMatrix[i, j];
                    double valB = vectorB[j];
                    sum += valMatrix * valB;

                    Console.Write($"{valB:F2} * ({valMatrix:F2})");
                    if (j < n - 1) Console.Write(" + ");
                }

                resultX[i] = sum;
                Console.WriteLine($"\t = {sum:F2}");
            }
        }

        static void PerformJordanStep(double[,] matrix, int rows, int cols, int r, int s)
        {
            double elem = matrix[r, s];
            double[,] tempMatrix = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (i == r && j == s) tempMatrix[i, j] = 1.0;
                    else if (i == r) tempMatrix[i, j] = -matrix[i, j];
                    else if (j == s) tempMatrix[i, j] = matrix[i, j];
                    else tempMatrix[i, j] = (matrix[i, j] * elem) - (matrix[i, s] * matrix[r, j]);
                }
            }

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    matrix[i, j] = tempMatrix[i, j] / elem;
        }

        static void PrintMatrix(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write($"{matrix[i, j],8:F2} ");
                }
                Console.WriteLine();
            }
        }
    }
}