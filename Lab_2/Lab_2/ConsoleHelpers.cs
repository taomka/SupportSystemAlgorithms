namespace Lab_2
{
    class ConsoleHelpers
    {
        public static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int result) && result > 0) return result;
                Console.WriteLine("Помилка: введіть ціле додатне число.");
            }
        }

        public static double ReadDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine()?.Replace('.', ',');
                if (double.TryParse(input, out double result)) return result;
                Console.WriteLine("Помилка вводу. Введіть число.");
            }
        }

        //public static void PrintTable(double[,] tab, List<string> rLabels, List<string> cLabels)
        //{
        //    int rows = tab.GetLength(0);
        //    int cols = tab.GetLength(1);

        //    Console.WriteLine();
        //    Console.Write($"{"",4}");
        //    for (int j = 0; j < cols; j++) Console.Write($"{cLabels[j],8}");
        //    Console.WriteLine("\n" + new string('-', 4 + cols * 8));

        //    for (int i = 0; i < rows; i++)
        //    {
        //        Console.Write($"{rLabels[i],2} =");
        //        for (int j = 0; j < cols; j++)
        //        {
        //            Console.Write($"{tab[i, j],8:F2}");
        //        }
        //        Console.WriteLine();
        //    }
        //    Console.WriteLine();
        //}

        //public static void PrintX(double[,] table, List<string> rowLabels, int originalN)
        //{
        //    double[] X = new double[originalN];
        //    int rows = table.GetLength(0);
        //    int cols = table.GetLength(1);

        //    for (int i = 0; i < rows - 1; i++)
        //    {
        //        if (rowLabels[i].StartsWith("x"))
        //        {
        //            int index = int.Parse(rowLabels[i].Substring(1)) - 1;
        //            X[index] = Math.Round(table[i, cols - 1]);
        //        }
        //    }

        //    Console.Write("X = (");
        //    for (int j = 0; j < originalN; j++)
        //    {
        //        Console.Write($"{X[j]}");
        //        if (j < originalN - 1) Console.Write("; ");
        //    }
        //    Console.WriteLine(")");
        //}

        public static void PrintDualTable(double[,] tab, List<string> pRows, List<string> dRows, List<string> pCols, List<string> dCols)
        {
            int rows = tab.GetLength(0);
            int cols = tab.GetLength(1);

            Console.WriteLine();
            Console.Write($"{"",8}");
            for (int j = 0; j < cols - 1; j++)
                Console.Write($"{dCols[j] + ", -" + pCols[j],10}");
            Console.WriteLine($"{"W, 1",9}");

            Console.WriteLine(new string('-', 12 + cols * 9));

            for (int i = 0; i < rows - 1; i++)
            {
                Console.Write($"{dRows[i],-2} {pRows[i],2} =");
                for (int j = 0; j < cols; j++)
                {
                    if (Math.Abs(tab[i, j]) < 1e-12)
                    {
                        tab[i, j] = 0.0;
                        Console.Write($"{tab[i, j],10:F2}");
                    }
                    else
                    {
                        Console.Write($"{tab[i, j],10:F2}");
                    }
                }
                Console.WriteLine();
            }

            Console.Write($"{dRows[rows - 1],-2}  {pRows[rows - 1],1} =");
            for (int j = 0; j < cols; j++)
            {
                if (Math.Abs(tab[rows - 1, j]) < 1e-12) 
                {
                    tab[rows - 1, j] = 0.0;
                    Console.Write($"{tab[rows - 1, j],10:F2}"); 
                }
                else
                {
                    Console.Write($"{tab[rows - 1, j],10:F2}");
                }
            }
            Console.WriteLine("\n");
        }

        public static void PrintSolution(double[,] table, List<string> labels, int count, string vectorName, string varPrefix, bool isPrimal)
        {
            double[] res = new double[count];
            int rows = table.GetLength(0);
            int cols = table.GetLength(1);
            for (int idx = 0; idx < count; idx++)
            {
                string target = varPrefix + (idx + 1);
                res[idx] = 0;
                if (isPrimal)
                {
                    //шукаємо зліва, значення в останньому стовпці
                    for (int i = 0; i < rows - 1; i++)
                    {
                        if (labels[i] == target)
                        {
                            res[idx] = table[i, cols - 1];
                            break;
                        }
                    }
                }
                else
                {
                    //шукаємо зверху, значення в останньому (Z) рядку
                    for (int j = 0; j < cols - 1; j++)
                    {
                        if (labels[j] == target)
                        {
                            res[idx] = table[rows - 1, j];
                            break;
                        }
                    }
                }
            }

            Console.Write($"{vectorName} = (");
            for (int i = 0; i < count; i++)
            {
                Console.Write($"{res[i]:F2}");
                if (i < count - 1)
                    Console.Write("; ");
            }
            Console.WriteLine(")");
        }
    }
}