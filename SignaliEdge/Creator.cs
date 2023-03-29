using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

using Emgu.CV;
using Emgu.CV.Structure;

namespace SignaliEdge
{
    class Creator
    {
        private int centerPointX = 0;
        private int centerPointY = 0;

        private double[] values = new double[1024];
        private int? ParentFirst = 0;

        private List<Point> coordinates;
        Dictionary<int, ValuesDictionary> BlocksDictionary = new Dictionary<int, ValuesDictionary>();
        private Dictionary<Point, CheckDataGray> _searchColor = new Dictionary<Point, CheckDataGray>();

        private List<Distance> distancies = new List<Distance>();

        public double[] getValues()
        {
            return values;
        }

        //Settings 
        private int _maxBorderThickness = 10;

        public Dictionary<int, ValuesDictionary> FilterDictionary(Dictionary<int, ValuesDictionary> BlocksDictionary, Bitmap inputImage)
        {

            foreach (var parent in BlocksDictionary.ToArray())
            {
                foreach (var child in BlocksDictionary.ToArray())
                {
                    if (parent.Value.width - child.Value.width <= _maxBorderThickness && parent.Value.PointsArea[0] <= child.Value.PointsArea[0] && parent.Value.PointsArea[2] >= child.Value.PointsArea[2] && parent.Key != child.Key)//проверка входит ли блок j в блок i по ширине
                    {
                        if (parent.Value.height - child.Value.height <= _maxBorderThickness && parent.Value.PointsArea[1] <= child.Value.PointsArea[1] && parent.Value.PointsArea[5] >= child.Value.PointsArea[5])//проверка входит ли блок j в блок i по высоте
                        {
                            BlocksDictionary.Remove(parent.Value.ID);
                            child.Value.PointsArea[0] -= 2;
                            child.Value.PointsArea[1] -= 2;
                            child.Value.PointsArea[2] += 2;
                            child.Value.PointsArea[3] -= 2;
                            child.Value.PointsArea[4] += 2;
                            child.Value.PointsArea[5] += 2;
                            child.Value.PointsArea[6] -= 2;
                            child.Value.PointsArea[7] += 2;

                            break;
                        }
                    }
                }
            }


            //searchBackground(inputImageinputImage);
            return BlocksDictionary;
        }

        //Поиск центра масс
        public Dictionary<int, ValuesDictionary> shapeCenter(Dictionary<int, DataPoints> PointMasses)
        {
            for (int itemMesses = 0; itemMesses < PointMasses.Count; itemMesses++)
            {
                Corners corners = new Corners();
                coordinates = new List<Point>();

                for (int i = 0; i < PointMasses[itemMesses].data.Count; i++)
                {
                    centerPointX += PointMasses[itemMesses].data[i].X;
                    centerPointY += PointMasses[itemMesses].data[i].Y;
                }

                centerPointX /= PointMasses[itemMesses].data.Count;
                centerPointY /= PointMasses[itemMesses].data.Count;

                /*for (int i = 0; i < PointMasses[itemMesses].data.Count; i++)
                {
                    Point point = PointMasses[itemMesses].data[i];

                    //Под вопросом
                    Distance distance = new Distance();

                    int deltaX = point.X - centerPointX;
                    int deltaY = point.Y - centerPointY;

                    distance.distance = Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2);
                    distance.angle = Math.Atan2(deltaX, deltaY);
                    distance.index = i;
                    distancies.Add(distance);
                    //Конец вопросного блока

                    if(point.X == centerPointX)
                    {

                        SearchCorners(point.X, point.Y, PointMasses[itemMesses].data, 'x', corners);
                    } else if(point.Y == centerPointY)
                    {
                        SearchCorners(point.X, point.Y, PointMasses[itemMesses].data, 'y', corners);
                    }
                }*/

                /*for (int i = 0; i < PointMasses[0].data.Count; i++)
                {
                    Console.WriteLine("X = " + PointMasses[0].data[i].X + " Y = " + PointMasses[0].data[i].Y + " ____ " + " X = " + ordered[i].X + " Y = " + ordered[i].Y); 
                }*/
                int x0, y0, x1, y1;
                int factorLine = 0;
                int counttest = 0;

                HashSet<Point> _HashdataCoordinates = new HashSet<Point>();
                List<Point> _ListdataCoordinates = new List<Point>();
                for (int i = 0; i < PointMasses[itemMesses].data.Count; i++)
                {
                    counttest++;
                    if (!_HashdataCoordinates.Contains(PointMasses[itemMesses].data[i]))
                    {
                        _ListdataCoordinates.Clear();
                        _HashdataCoordinates.Add(PointMasses[itemMesses].data[i]);
                        factorLine = 0;

                        x0 = PointMasses[itemMesses].data[i].X;
                        y0 = PointMasses[itemMesses].data[i].Y;

                        if (i + 1 == (PointMasses[itemMesses].data.Count)) { break; }

                        x1 = PointMasses[itemMesses].data[i+1].X;
                        y1 = PointMasses[itemMesses].data[i+1].Y;

                        for (int j = 0; j < PointMasses[itemMesses].data.Count; j++)
                        {
                            int currentX = PointMasses[itemMesses].data[j].X;
                            int currentY = PointMasses[itemMesses].data[j].Y;

                            if (((x1 - x0) * (currentY - y0) - (y1 - y0) * (currentX - x0)) == 0)
                            {
                                factorLine++;
                                _ListdataCoordinates.Add(PointMasses[itemMesses].data[j]);
                            }
                        }

                        if (factorLine > 50)
                        {
                            coordinates.Add(new Point(_ListdataCoordinates[0].X, _ListdataCoordinates[0].Y));
                            coordinates.Add(new Point(_ListdataCoordinates[_ListdataCoordinates.Count - 1].X, _ListdataCoordinates[_ListdataCoordinates.Count - 1].Y));

                            //Console.WriteLine("Первая точка = " + _ListdataCoordinates[0] + " последняя точка = " + _ListdataCoordinates[_ListdataCoordinates.Count - 1]);
                            for(int j = 0; j < _ListdataCoordinates.Count; j++)
                            {
                                _HashdataCoordinates.Add(_ListdataCoordinates[j]);
                            }
                        }
                    }
                }

                corners.coordinates = coordinates;






                if (corners.coordinates.Count != 8)
                {
                    Console.WriteLine("BREAK");
                    continue;
                }


                Point dataLineFirst = corners.coordinates[1];
                Point dataLineSecond = corners.coordinates[7];
                Point dataLineThird = corners.coordinates[3];
                Point dataLineFourth = corners.coordinates[5];

                int[] dataLineCenter = new int[2] { dataLineFirst.X, dataLineSecond.Y };
                double d = 0;

                Point coord = new Point(dataLineSecond.X, dataLineSecond.Y);
                int width = dataLineThird.X - dataLineSecond.X;
                int height = dataLineFourth.Y - dataLineFirst.Y;
                int count = 0;

                //Console.WriteLine("Радиус по ширине = " + (dataLineFirst.X - dataLineSecond.X));
                //Console.WriteLine("Радиус по высоте = " + (dataLineSecond.Y - dataLineFirst.Y));

                /*for (int i = PointMasses[itemMesses].data.IndexOf(dataLineSecond); i < PointMasses[itemMesses].data.Count; i++)
                {
                    if(PointMasses[itemMesses].data[i] != corners.coordinates[1])
                    {
                        d = Math.Sqrt(Math.Pow(dataLineCenter[0] - coord.X, 2) + Math.Pow(dataLineCenter[1] - coord.Y, 2));
                        coord = PointMasses[itemMesses].data[i];
                        count++;
                    } else
                    {
                        break;
                    }
                }*/
                //Console.WriteLine(d-3);
                //Console.WriteLine(count);

                // BlocksDictionary - все блоки найденые системой
                BlocksDictionary.Add(MyGlobals.g_counterKey, new ValuesDictionary(false, new List<int>() {
                    dataLineSecond.X, dataLineFirst.Y,
                    dataLineThird.X, dataLineFirst.Y,
                    dataLineThird.X,  dataLineFourth.Y,
                    dataLineSecond.X, dataLineFourth.Y,
                    
                }, MyGlobals.g_counterKey, width, height, "", new Dictionary<int, Blocks>() { }, new Dictionary<int, BlocksTextP>() { }, ParentFirst));
                MyGlobals.g_counterKey++;
            }

            //Создание первого элемента body
            BlocksDictionary.Add(MyGlobals.g_counterKey, new ValuesDictionary(false, new List<int>() {
                0, 0,
                MyGlobals.g_inputImage.Width - 1, 0,
                MyGlobals.g_inputImage.Width - 1,  MyGlobals.g_inputImage.Height - 1,
                0, MyGlobals.g_inputImage.Height - 1,
            }, MyGlobals.g_counterKey, MyGlobals.g_inputImage.Width - 1, MyGlobals.g_inputImage.Height - 1, "", new Dictionary<int, Blocks>() { }, new Dictionary<int, BlocksTextP>() { }, null));

            MyGlobals.g_counterKey++;
            return BlocksDictionary;

        }




        private void SearchCorners(int x, int y, List<Point> PointMasses, char depend, Corners corners)
        {
            bool is_while = true;
            int copy = depend == 'x' ? x : y;

            while (is_while)
            {
                copy += 1;
                if(!PointMasses.Contains(depend == 'x' ? new Point(copy , y) : new Point(x , copy)))
                {
                    copy -= 1;
                    if (PointMasses.IndexOf(depend == 'x' ? new Point(copy , y) : new Point(x , copy)) != -1)
                    {
                        coordinates.Add(depend == 'x' ? new Point(copy , y) : new Point(x , copy));
                    }
                    
                    corners.coordinates = coordinates;
                    copy = depend == 'x' ? x : y;
                    while (is_while)
                    {
                        copy -= 1;
                        if(!PointMasses.Contains(depend == 'x' ? new Point(copy , y) : new Point(x , copy)))
                        {
                            is_while = false;
                            copy += 1;
                            if (PointMasses.IndexOf(depend == 'x' ? new Point(copy , y) : new Point(x , copy)) != -1)
                            {
                                coordinates.Add(depend == 'x' ? new Point(copy , y) : new Point(x , copy));
                            }
                        }
                    }
                }
            }
        }
        private unsafe void searchBackground(Bitmap GrayImage)
        {
            HashSet<byte> ListColors = new HashSet<byte>();
            foreach (var child in BlocksDictionary.ToArray())
            {
                BitmapData bitmapData = GrayImage.LockBits(new Rectangle(child.Value.PointsArea[0], child.Value.PointsArea[1], child.Value.PointsArea[2], child.Value.PointsArea[5]), 
                    ImageLockMode.ReadWrite, GrayImage.PixelFormat);

                int bytesPerPixel = Bitmap.GetPixelFormatSize(GrayImage.PixelFormat) / 8;//Размер пикселя
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;


                //Цикл поиска
                /*for (int y = 0; y < bitmapData.Height - 1; y++)
                {
                    byte* row = PtrFirstPixel + y * bitmapData.Stride;
                    for (int x = 0; x < bitmapData.Width; x++)
                    {
                        byte* pixel = row + x * bytesPerPixel;

                        if(_searchColor.Count == 0)
                        {
                            //_searchColor.Add(new Point(x, y), new CheckDataGray(1, pixel[0]));
                            ListColors.Add(pixel[0]);
                        } else
                        {
                            foreach (var item in _searchColor.ToArray())
                            {
                                if (!ListColors.Contains(pixel[0]) )
                                {
                                    //_searchColor.Add(new Point(x, y), new CheckDataGray(1, pixel[0]));
                                    ListColors.Add(pixel[0]);
                                }
                                else
                                {
                                    item.Value.counterPixels += 1;
                                }
                            }
                        }

                    }
                }*/

                GrayImage.UnlockBits(bitmapData);
            }

            //HERE
            foreach(var item in _searchColor)
            {
                Console.WriteLine(item.Key + " " + " color = " + item.Value.color + " " + item.Value.counterPixels);
                Console.WriteLine();
            }
        }

    }
    class Distance
    {
        public int index { get; set; }
        public double angle { get; set; }
        public double distance { get; set; }
        public double normalized { get; set; }
        public double position { get; set; }
    }

    class Corners 
    {
        public List<Point> coordinates { get; set; }
    }

    class CheckDataGray
    {
        public int counterPixels { get; set; }
        public int color { get; set; }
        public List<Point> pointsPixels { get; set; }

        public CheckDataGray(int counterPixels, int color, List<Point> pointsPixels)
        {
            this.counterPixels = counterPixels;
            this.color = color;
            this.pointsPixels = pointsPixels;
        }
    }


    class DistanceeComparer : IComparer<Distance>
    {
        public int Compare(Distance x, Distance y)
        {
            if (x.position < y.position)
                return -1;
            else if (x.position > y.position)
                return 1;
            else return 0;
        }
    }
}
