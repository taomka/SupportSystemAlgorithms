namespace DZ
{
    public class SimplexTableau(double[,] initialMatrix, List<string> rLabels, List<string> cLabels)
    {
        public double[,] Matrix { get; private set; } = initialMatrix;
        public List<string> RLabels { get; private set; } = rLabels;
        public List<string> CLabels { get; private set; } = cLabels;

        public void PerformMJE(int r, int s)
        {
            int m = RLabels.Count;
            int cols = CLabels.Count;
            double pivot = Matrix[r, s];
            double[,] nextMatrix = new double[m + 1, cols + 1];

            for (int i = 0; i <= m; i++)
            {
                for (int j = 0; j <= cols; j++)
                {
                    if (i == r && j == s) nextMatrix[i, j] = 1.0 / pivot;
                    else if (i == r) nextMatrix[i, j] = Matrix[i, j] / pivot;
                    else if (j == s) nextMatrix[i, j] = -Matrix[i, j] / pivot;
                    else nextMatrix[i, j] = Matrix[i, j] - (Matrix[i, s] * Matrix[r, j] / pivot);
                }
            }

            (CLabels[s], RLabels[r]) = (RLabels[r], CLabels[s]);

            if (CLabels[s] == "0")
            {
                double[,] shrunkTable = new double[m + 1, cols];
                for (int i = 0; i <= m; i++)
                {
                    int c = 0;
                    for (int j = 0; j <= cols; j++)
                    {
                        if (j == s) continue;
                        shrunkTable[i, c++] = nextMatrix[i, j];
                    }
                }
                Matrix = shrunkTable;
                CLabels.RemoveAt(s);
            }
            else
            {
                Matrix = nextMatrix;
            }
        }

        public double[] ExtractSolution(int n)
        {
            double[] X = new double[n];
            for (int i = 0; i < n; i++)
            {
                string varName = $"x{i + 1}";
                int rIdx = RLabels.IndexOf(varName);
                X[i] = rIdx != -1 ? Matrix[rIdx, Matrix.GetLength(1) - 1] : 0.0;
            }
            return X;
        }
    }
}