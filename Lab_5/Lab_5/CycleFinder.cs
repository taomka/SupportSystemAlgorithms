namespace Lab_5
{
    class CycleFinder
    {
        /// <summary>
        /// Знаходить цикл базових комірок у заданій булевій матриці, який починається у вказаній позиції.
        /// </summary>
        /// <remarks>Виконує пошук у глибину двічі (надаючи перевагу одній орієнтації, а не іншій) та
        /// повертає перший знайдений цикл. Очікується, що isBasic буде не null, а початкові індекси будуть у межах масиву
        /// меж.</remarks>
        /// <param name="isBasic">2D логічний масив, де true вказує на базову комірку.</param>
        /// <param name="startI">Індекс рядка початкової комірки, що починається з нуля.</param>
        /// <param name="startJ">Індекс стовпчика початкової комірки, що починається з нуля.</param>
        /// <returns>Список точок, що утворюють цикл, або null, якщо цикл не знайдено.</returns>
        public static List<Point>? FindCycle(bool[,] isBasic, int startI, int startJ)
        {
            int m = isBasic.GetLength(0), n = isBasic.GetLength(1);
            List<Point> path = [new Point(startI, startJ)];
            if (DFS(startI, startJ, true, path, isBasic, startI, startJ, m, n)) return path;
            path.Clear(); path.Add(new Point(startI, startJ));
            if (DFS(startI, startJ, false, path, isBasic, startI, startJ, m, n)) return path;
            return null;
        }

        /// <summary>
        /// Виконує пошук у глибину, чергуючи горизонтальні та вертикальні рухи, щоб знайти замкнутий цикл основних
        /// комірок, який повертається до початку.
        /// </summary>
        /// <remarks>Пошук змінює напрямок на кожному кроці, пропускає вже відвідані комірки та розглядає початкову комірку
        /// як дійсну точку замикання лише тоді, коли довжина шляху становить щонайменше чотири.</remarks>
        /// <param name="currI">Поточний індекс рядка позиції пошуку.</param>
        /// <param name="currJ">Поточний індекс стовпчика позиції пошуку.</param>
        /// <param name="moveHoriz">Якщо значення true, пошук рухається далі по горизонталі; інакше рухається вертикально.</param>
        /// <param name="path">Поточний шлях відвіданих комірок; нові точки додаються перед рекурсією та видаляються під час зворотного відстеження.</param>
        /// <param name="isBasic">Матриця, що вказує, які комірки є базовими (придатними для включення в цикл).</param>
        /// <param name="sI">Індекс рядка початкової комірки.</param>
        /// <param name="sJ">Індекс стовпчика початкової комірки.</param>
        /// <param name="m">Кількість рядків у сітці.</param>
        /// <param name="n">Кількість стовпчиків у сітці.</param>
        /// <returns>Істина, якщо знайдено замкнутий цикл, який повертається до початкової комірки щонайменше з чотирма вузлами; інакше хибність.</returns>
        private static bool DFS(int currI, int currJ, bool moveHoriz, List<Point> path, bool[,] isBasic, int sI, int sJ, int m, int n)
        {
            if (moveHoriz)
            {
                for (int j = 0; j < n; j++)
                {
                    if (j == currJ) continue;
                    if (isBasic[currI, j] || (currI == sI && j == sJ))
                    {
                        if (currI == sI && j == sJ) 
                        { 
                            if (path.Count >= 4) 
                                return true; 
                            continue; 
                        }
                        if (path.Any(p => p.X == currI && p.Y == j)) continue;
                        path.Add(new Point(currI, j));
                        if (DFS(currI, j, !moveHoriz, path, isBasic, sI, sJ, m, n)) return true;
                        path.RemoveAt(path.Count - 1);
                    }
                }
            }
            else
            {
                for (int i = 0; i < m; i++)
                {
                    if (i == currI) continue;
                    if (isBasic[i, currJ] || (i == sI && currJ == sJ))
                    {
                        if (i == sI && currJ == sJ) 
                        { 
                            if (path.Count >= 4)
                                return true; 
                            continue; 
                        }
                        if (path.Any(p => p.X == i && p.Y == currJ)) continue;
                        path.Add(new Point(i, currJ));
                        if (DFS(i, currJ, !moveHoriz, path, isBasic, sI, sJ, m, n)) return true;
                        path.RemoveAt(path.Count - 1);
                    }
                }
            }
            return false;
        }
    }
}
