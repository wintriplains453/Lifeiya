using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace SignaliEdge
{
    class GlobalMethods
    {
        private int centerPointY = 0;
        private int centerPointX = 0;

        internal Point SearchCenter(List<Point> listData) 
        {
            for (int i = 0; i < listData.Count; i++)
            {
                centerPointX += listData[i].X;
                centerPointY += listData[i].Y;
            }

            centerPointX /= listData.Count;
            centerPointY /= listData.Count;

            return new Point(centerPointX, centerPointY);
        }
    }
}
