namespace Lab_4
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

        public static double[] ParseRow(int expectedCount)
        {
            while (true)
            {
                try
                {
                    string? input = Console.ReadLine()?.Trim();
                    if (string.IsNullOrEmpty(input)) continue;

                    string[] parts = input.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length != expectedCount)
                    {
                        Console.WriteLine($"Помилка: очікувалось {expectedCount} чисел, отримано {parts.Length}. Спробуйте ще раз:");
                        continue;
                    }

                    double[] row = new double[expectedCount];
                    for (int j = 0; j < expectedCount; j++)
                    {
                        row[j] = double.Parse(parts[j].Replace('.', ','));
                    }
                    return row;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Помилка: введено не числове значення. Спробуйте ще раз:");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка при вводі: {ex.Message}. Спробуйте ще раз:");
                }
            }
        }
    }
}
