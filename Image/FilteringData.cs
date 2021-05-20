using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photomask.Image
{
    public enum FilteringMode
    {
        None,
        Linear,
        Median
    }

    public class FilteringData : ICloneable
    {
        public FilteringMode mode { get; set; } = FilteringMode.None;
        public double[,] kernel { get; set; } = new double[0, 0];
        public int median_radius { get; set; } = 5;
        public int mode_view
        {
            get => (int)mode;
            set => mode = (FilteringMode)value;
        }

        public string kernel_view { get; set; } = "";

        /*
        public string kernel_view
        {
            get => matrixToString(kernel);
        }

        
        public string matrixToString(double[,] matrix)
        {
            string res = "";
            for(int i = 0; i < matrix.GetLength(0); i++)
            {
                for(int j = 0; j < matrix.GetLength(1); j++)
                {
                    res += string.Format("{0:0.00000}", Math.Round(matrix[i, j], 5)) + " ";
                    //res += matrix[i, j].ToString() + " ";
                }
                if (i != matrix.GetLength(0) - 1) res += Environment.NewLine;
            }
            return res;
        }
        */

        public object Clone()
        {
            FilteringData data = new FilteringData();
            data.mode = mode;
            data.kernel = kernel.Clone() as double[,];
            data.median_radius = median_radius;
            data.kernel_view = kernel_view;
            return data;
        }
    }
}
