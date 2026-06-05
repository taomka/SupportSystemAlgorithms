namespace Lab_6
{
    class ConsoleHelpers
    {
        /// <summary>
        /// Метод для безпечного зчитування цілого числа, що перевіряє правильність введення та гарантує, що число є додатним. 
        /// Якщо введення некоректне, користувач буде повторно запрошений до введення, доки не буде отримано правильне значення.
        /// </summary>
        /// <param name="prompt">Повідомлення для користувача</param>
        /// <returns></returns>
        public static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int r) && r > 0) return r;
                Console.WriteLine("Помилка вводу.");
            }
        }

        /// <summary>
        /// Розбирає рядок введення консолі в масив чисел типу double.
        /// </summary>
        /// <remarks>Повторює читання рядків, доки не буде введено рядок, що містить рівно кількість чисел, які можна розібрати.
        /// Розбиває вхідні дані на пробіли та табуляцію, замінює '.' на ',' перед розбором, щоб врахувати десяткові 
        /// роздільники, та записує повідомлення про помилку, якщо розбір не вдається або вказано неправильну кількість значень.
        ///</remarks>
        /// <param name="count">Точна кількість числових значень, необхідних у вхідних даних.</param>
        /// <returns>Масив чисел типу double, проаналізований з вхідного рядка.</returns>
        public static double[] ParseRow(int count)
        {
            while (true)
            {
                string input = Console.ReadLine()?.Trim() ?? "";
                string[] parts = input.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == count)
                {
                    try { return [.. parts.Select(p => double.Parse(p.Replace('.', ',')))]; } catch { }
                }
                Console.WriteLine($"Помилка. Введіть рівно {count} чисел.");
            }
        }
    }
}
