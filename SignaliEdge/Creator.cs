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
        internal int centerPointX = 0;
        internal int centerPointY = 0;

        internal Point _sideLeftStart;
        internal Point _sideLeftEnd;

        internal Point _sideRightStart;
        internal Point _sideRightEnd;

        internal Point _sideTopStart;
        internal Point _sideTopEnd;

        internal Point _sideBottomStart;
        internal Point _sideBottomEnd;

        internal double[] values = new double[1024];
        internal int? ParentFirst = 0;

        internal List<Point> coordinates;
        Dictionary<int, ValuesDictionary> BlocksDictionary = new Dictionary<int, ValuesDictionary>();
        internal Dictionary<Point, CheckDataGray> _searchColor = new Dictionary<Point, CheckDataGray>();

        internal List<Distance> distancies = new List<Distance>();
        GlobalMethods globalMethods = new GlobalMethods();


        public double[] getValues()
        {
            return values;
        }

        //Settings 

        public Dictionary<int, ValuesDictionary> FilterDictionary(Dictionary<int, ValuesDictionary> BlocksDictionary, Bitmap inputImage)
        {

            foreach (var parent in BlocksDictionary.ToArray())
            {
                foreach (var child in BlocksDictionary.ToArray())
                {
                    if (parent.Value.width - child.Value.width <= MyGlobals.g_boder_length && parent.Value.PointsArea[0].X <= child.Value.PointsArea[0].X && parent.Value.PointsArea[1].X >= child.Value.PointsArea[1].X && parent.Key != child.Key)//проверка входит ли блок j в блок i по ширине
                    {
                        if (parent.Value.height - child.Value.height <= MyGlobals.g_boder_length && parent.Value.PointsArea[0].Y <= child.Value.PointsArea[0].Y && parent.Value.PointsArea[2].Y >= child.Value.PointsArea[3].Y)//проверка входит ли блок j в блок i по высоте
                        {
                            BlocksDictionary.Remove(parent.Value.ID);
                            child.Value.PointsArea[0] = new Point(child.Value.PointsArea[0].X - 2, child.Value.PointsArea[0].Y - 2);
                            child.Value.PointsArea[1] = new Point(child.Value.PointsArea[1].X + 2, child.Value.PointsArea[1].Y - 2);
                            child.Value.PointsArea[2] = new Point(child.Value.PointsArea[2].X + 2, child.Value.PointsArea[2].Y + 2);
                            child.Value.PointsArea[3] = new Point(child.Value.PointsArea[3].X - 2, child.Value.PointsArea[3].Y + 2);

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
                _sideLeftStart = new Point(MyGlobals.g_inputImage.Width, MyGlobals.g_inputImage.Width);
                _sideTopStart = new Point(MyGlobals.g_inputImage.Height, MyGlobals.g_inputImage.Height);
                _sideRightStart = new Point();
                _sideBottomStart = new Point();

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

                HashSet<Point> _HashdataCoordinates = new HashSet<Point>();
                List<Point> _ListdataCoordinates = new List<Point>();
                for (int i = 0; i < PointMasses[itemMesses].data.Count - 1; i++)
                {
                    if (!_HashdataCoordinates.Contains(PointMasses[itemMesses].data[i]))
                    {
                        _ListdataCoordinates.Clear();
                        _HashdataCoordinates.Add(PointMasses[itemMesses].data[i]);
                        factorLine = 0;

                        x0 = PointMasses[itemMesses].data[i].X;
                        y0 = PointMasses[itemMesses].data[i].Y;

                        //if (i + 1 == (PointMasses[itemMesses].data.Count)) { break; }

                        x1 = PointMasses[itemMesses].data[i+1].X;
                        y1 = PointMasses[itemMesses].data[i+1].Y;

                        for (int j = 0; j < PointMasses[itemMesses].data.Count - 1; j++)
                        {
                            int currentX = PointMasses[itemMesses].data[j].X;
                            int currentY = PointMasses[itemMesses].data[j].Y;

                            if (((x1 - x0) * (currentY - y0) - (y1 - y0) * (currentX - x0)) == 0)
                            {
                                factorLine++;

                                _ListdataCoordinates.Add(PointMasses[itemMesses].data[j]);
                                _HashdataCoordinates.Add(PointMasses[itemMesses].data[j]);
                            }
                        }

                        //ВАРИАНТ С ДИАГОНАЛЯМИ НЕ ПОДХОДЯЩИЙ!!!!!!!!!!!!
                        if (factorLine > 50)
                        {
                            //Console.WriteLine(_ListdataCoordinates[0]);
                            if(_ListdataCoordinates[0].Y == _ListdataCoordinates[_ListdataCoordinates.Count - 1].Y)
                            {
                                if (_ListdataCoordinates[0].Y < centerPointY)
                                {
                                    if(_ListdataCoordinates[0].Y < _sideTopStart.Y)
                                    {
                                        //Console.WriteLine("Первая точка top = " + _ListdataCoordinates[0] + " последняя точка = " + _ListdataCoordinates[_ListdataCoordinates.Count - 1]);
                                        _sideTopStart = new Point(_ListdataCoordinates[0].X, _ListdataCoordinates[0].Y);
                                        _sideTopEnd = new Point(_ListdataCoordinates[_ListdataCoordinates.Count - 1].X, _ListdataCoordinates[_ListdataCoordinates.Count - 1].Y);
                                    }
                                   
                                } else
                                {
                                    if (_ListdataCoordinates[0].Y > _sideBottomStart.Y)
                                    {
                                        //Console.WriteLine("Первая точка bottom = " + _ListdataCoordinates[0] + " последняя точка = " + _ListdataCoordinates[_ListdataCoordinates.Count - 1]);
                                        _sideBottomStart = new Point(_ListdataCoordinates[0].X, _ListdataCoordinates[0].Y);
                                        _sideBottomEnd = new Point(_ListdataCoordinates[_ListdataCoordinates.Count - 1].X, _ListdataCoordinates[_ListdataCoordinates.Count - 1].Y);
                                    }
                                }
                            } else if(_ListdataCoordinates[0].X == _ListdataCoordinates[_ListdataCoordinates.Count - 1].X)
                            {
                                if (_ListdataCoordinates[0].X < centerPointX)
                                {
                                    if (_ListdataCoordinates[0].X < _sideLeftStart.X)
                                    {
                                        //Console.WriteLine("Первая точка left = " + _ListdataCoordinates[0] + " последняя точка = " + _ListdataCoordinates[_ListdataCoordinates.Count - 1]);
                                        _sideLeftStart = new Point(_ListdataCoordinates[0].X, _ListdataCoordinates[0].Y);
                                        _sideLeftEnd = new Point(_ListdataCoordinates[_ListdataCoordinates.Count - 1].X, _ListdataCoordinates[_ListdataCoordinates.Count - 1].Y);
                                    }
                                }
                                else
                                {
                                    if (_ListdataCoordinates[0].X > _sideRightStart.X)
                                    {
                                        //Console.WriteLine("Первая точка right = " + _ListdataCoordinates[0] + " последняя точка = " + _ListdataCoordinates[_ListdataCoordinates.Count - 1]);
                                        _sideRightStart = new Point(_ListdataCoordinates[0].X, _ListdataCoordinates[0].Y);
                                        _sideRightEnd = new Point(_ListdataCoordinates[_ListdataCoordinates.Count - 1].X, _ListdataCoordinates[_ListdataCoordinates.Count - 1].Y);
                                    }
                                }
                            }
                        }
                    }
                }
                //corners.coordinates = coordinates;

                /*if (corners.coordinates.Count != 8)
                {
                    Console.WriteLine("BREAK");
                    continue;
                }*/


                Point dataLineFirst = _sideTopEnd;
                Point dataLineSecond = _sideLeftEnd;
                Point dataLineThird = _sideRightEnd;
                Point dataLineFourth = _sideBottomEnd;

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

                //Console.WriteLine("Первая точка = " + _sideTopStart.X + " " + _sideTopStart.Y + " последняя точка = " + _sideTopEnd.X + " " + _sideTopEnd.Y);
                // BlocksDictionary - все блоки найденые системой
                BlocksDictionary.Add(MyGlobals.g_counterKey, new ValuesDictionary(false, new List<Point>() {
                    new Point(dataLineSecond.X, dataLineFirst.Y),
                    new Point(dataLineThird.X, dataLineFirst.Y),
                    new Point(dataLineThird.X,  dataLineFourth.Y),
                    new Point(dataLineSecond.X, dataLineFourth.Y),

                    /*dataLineSecond.X, dataLineFirst.Y,
                    dataLineThird.X, dataLineFirst.Y,
                    dataLineThird.X,  dataLineFourth.Y,
                    dataLineSecond.X, dataLineFourth.Y,*/

                }, MyGlobals.g_counterKey, width, height, "", new Dictionary<int, Blocks>() { }, new Dictionary<int, BlocksTextP>() { }, ParentFirst));
                MyGlobals.g_counterKey++;
            }

            //Создание первого элемента body
            BlocksDictionary.Add(MyGlobals.g_counterKey, new ValuesDictionary(false, new List<Point>() {
                new Point(0, 0),
                new Point(MyGlobals.g_inputImage.Width - 1, 0),
                new Point(MyGlobals.g_inputImage.Width - 1,  MyGlobals.g_inputImage.Height - 1),
                new Point(0, MyGlobals.g_inputImage.Height - 1),
            }, MyGlobals.g_counterKey, MyGlobals.g_inputImage.Width - 1, MyGlobals.g_inputImage.Height - 1, "", new Dictionary<int, Blocks>() { }, new Dictionary<int, BlocksTextP>() { }, null));

            MyGlobals.g_counterKey++;
            return BlocksDictionary;

        }

        internal void FilterIncompleteLines(Dictionary<int, ValuesDictionary> BlocksDictionary, Dictionary<int, List<Point>> _incompleteLines)//__________________________________переделать строку /*&^*/ отдельно для X и Y
        {
            if (_incompleteLines.Count == 0)
                return;

            int x0, y0, x1, y1;

            HashSet<Point> _HashdataCoordinates = new HashSet<Point>();
            List<Point> _ListdataCoordinates = new List<Point>();

            int factorLine = 0;
            int semiFigureCount = 0;

            foreach (var item in _incompleteLines.Values)
            {
                //try
                //{
                int countLines = 0;
                List<OpenLines> endAngles = new List<OpenLines>();
                Dictionary<int, SemiLines> _ListdataCoordinatesTriple = new Dictionary<int, SemiLines>();

                for (int i = 0; i < item.Count - 1; i++)
                {
                    if (!_HashdataCoordinates.Contains(item[i]))
                    {
                        _HashdataCoordinates.Add(item[i]);
                        _ListdataCoordinates.Clear();
                        factorLine = 0;

                        x0 = item[i].X;
                        y0 = item[i].Y;

                        x1 = item[i + 1].X;
                        y1 = item[i + 1].Y;

                        for (int j = 0; j < item.Count - 1; j++)
                        {
                            int currentX = item[j].X;
                            int currentY = item[j].Y;

                            if (((x1 - x0) * (currentY - y0) - (y1 - y0) * (currentX - x0)) == 0)
                            {
                                factorLine++;
                                _ListdataCoordinates.Add(item[j]);
                                _HashdataCoordinates.Add(item[j]);
                            }
                        }

                        if (factorLine > MyGlobals.g_length_Line_Rectangle * 2)
                        {
                            countLines++;
                            var largest__coordinate = _ListdataCoordinates.GroupBy(x => x.Y).Select(x => new { key = x.Key, value = x.Where(xv => xv.Y == x.Key).Count() }).OrderByDescending(x => x.value).First();
                            char plain = largest__coordinate.value > 1 ? 'Y' : 'X';//! опасное место, возможна путаница между X и Y 

                            semiFigureCount++;
                            //Console.WriteLine("! " + factorLine + " start " + _ListdataCoordinates[0] + " end " + _ListdataCoordinates[_ListdataCoordinates.Count - 1]);

                            if(endAngles.Count == 0)
                            {
                                endAngles.Add(new OpenLines(_ListdataCoordinates[0], _ListdataCoordinates[_ListdataCoordinates.Count - 1], plain));
                            }
                            else
                            {
                                Point newPoint = new Point();
                                if (endAngles[endAngles.Count - 1].plain == 'Y')
                                {
                                    newPoint.Y = endAngles[endAngles.Count - 1].endPoint.Y;
                                }
                                else
                                {
                                    newPoint.X = endAngles[endAngles.Count - 1].endPoint.X;
                                }
                                if (plain == 'Y') {
                                    newPoint.Y = _ListdataCoordinates[0].Y;
                                } else
                                {
                                    newPoint.X = _ListdataCoordinates[0].X;
                                }

                                endAngles[endAngles.Count - 1].endPoint = newPoint;
                                endAngles.Add(new OpenLines(newPoint, _ListdataCoordinates[_ListdataCoordinates.Count - 1], plain));
                            }
                        }
                    }
                }
                if (countLines > 3)
                {
                    countLines = 0;
                    Point centerPoints = globalMethods.SearchCenter(item);

                    double distance = 0;
                    double minimalDistance = MyGlobals.g_inputImage.Width + MyGlobals.g_inputImage.Height;
                    Point startResult = new Point();
                    Point endResult = new Point();

                    Point BlockItemCheck = new Point();

                    /*for(int i = 1; i < endAngles.Count - 1; i++)
                    {
                        endAngles.Insert(i,
                            new Point(
                                (endAngles[i].X + endAngles[i + 1].X) / 2,
                                (endAngles[i].Y + endAngles[i + 1].Y) / 2
                            )
                        );
                        endAngles.Remove(endAngles[i]);
                        endAngles.Remove(endAngles[i+1]);
                    }*/

                    //Перемещение всех точек класса OpenLines в список типа Point
                    List<Point> resultingAngles = new List<Point>();
                    foreach (var elem in endAngles)
                    {
                        if (!resultingAngles.Contains(elem.startPoint))
                        {
                            resultingAngles.Add(elem.startPoint);

                        }
                        if (!resultingAngles.Contains(elem.endPoint))
                        {
                            resultingAngles.Add(elem.endPoint);
                        }
                    }

                    //Сортировка точек по часовой стрелке
                    List<Distance> endAnglesSort = new List<Distance>();

                    for (int i = 0; i < resultingAngles.Count; i++)
                    {
                        Distance distanceClass = new Distance();

                        distanceClass.position = resultingAngles[i];
                        distanceClass.angle = Math.Atan2(resultingAngles[i].Y - centerPoints.Y, resultingAngles[i].X - centerPoints.X) * 180 / Math.PI;

                        endAnglesSort.Add(distanceClass);
                    }

                    endAnglesSort.Sort((a, b) => (int)a.angle - (int)b.angle);

                    foreach (var itemBlock in BlocksDictionary.Values)
                    {
                        for (int elem = 0; elem < itemBlock.PointsArea.Count; elem++)
                        {
                            BlockItemCheck = itemBlock.PointsArea[elem];
                            distance = Math.Pow(item[0].X - BlockItemCheck.X, 2) + Math.Pow(item[0].Y - BlockItemCheck.Y, 2);
                            if (minimalDistance > distance)
                            {
                                minimalDistance = distance;
                                startResult = new Point(BlockItemCheck.X, BlockItemCheck.Y);
                                //поиск точек по соседям

                                //ВЕСЬ КОД ЭТОГО МЕТОДА СТОИТ ПЕРЕДЕЛАТЬ!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                                switch (elem)
                                {
                                    case 0:
                                        ChooseResult(itemBlock.PointsArea[0], itemBlock.PointsArea[1], itemBlock.PointsArea[3]);
                                        break;
                                    case 1:
                                        ChooseResult(itemBlock.PointsArea[1], itemBlock.PointsArea[0], itemBlock.PointsArea[2]);
                                        break;
                                    case 2:
                                        ChooseResult(itemBlock.PointsArea[2], itemBlock.PointsArea[3], itemBlock.PointsArea[1]);
                                        break;
                                    case 3:
                                        ChooseResult(itemBlock.PointsArea[3], itemBlock.PointsArea[2], itemBlock.PointsArea[0]);
                                        break;
                                }
                                //resultingAngles[0] = endResult;
                                //resultingAngles[resultingAngles.Count - 1] = startResult;
                            }
                        }
                    }
                    /*List<Point> checkingRepete = new List<Point>();
                    foreach(var parentElem in endAngles.ToArray())
                    {
                        if (!checkingRepete.Contains(parentElem))
                        {
                            checkingRepete.Add(parentElem);
                            foreach (var childElem in endAngles.ToArray())
                            {
                                if(!checkingRepete.Contains(childElem))
                                {
                                    if (parentElem.X - childElem.X < 10 && parentElem.Y - childElem.Y < 10)
                                    {
                                        endAngles.Add(new Point((parentElem.X + childElem.X) / 2, (parentElem.Y + childElem.Y) / 2));

                                        endAngles.Remove(parentElem);
                                        endAngles.Remove(childElem);

                                        checkingRepete.Add(childElem);
                                    }
                                }
                            }
                        }
                    }*/

                    endAngles.Clear();
                    foreach (var elem in endAnglesSort)
                    {
                        Console.WriteLine(elem.position);
                        resultingAngles.Add(elem.position);
                    }

                    BlocksDictionary.Add(MyGlobals.g_counterKey, new ValuesDictionary(false, new List<Point>() {
                            resultingAngles[0],
                            resultingAngles[1],
                            resultingAngles[2],
                            resultingAngles[3],
                        }, MyGlobals.g_counterKey, (endResult.X - startResult.X), (_ListdataCoordinates[1].Y - endResult.Y), "", new Dictionary<int, Blocks>() { }, new Dictionary<int, BlocksTextP>() { }, null));
                    MyGlobals.g_counterKey++;

                    void ChooseResult(Point start, Point endX, Point endY)
                    {
                        if (start.X < centerPoints.X && centerPoints.X > endX.X)
                        {
                            endResult = endX;
                        }
                        else if (start.Y < centerPoints.Y && centerPoints.Y > endY.Y)
                        {
                            endResult = endY;
                        }
                    }

                    //Console.WriteLine(startResult + (" X = " + item[0].X + " Y = " + item[0].Y) + (" X end = " + item[item.Count - 1].X + " Y end = " + item[item.Count - 1].Y));
                }
                //} catch
                //{

                // }
            }
        }

        private unsafe void searchBackground(Bitmap GrayImage)
        {
            HashSet<byte> ListColors = new HashSet<byte>();
            foreach (var child in BlocksDictionary.ToArray())
            {
                BitmapData bitmapData = GrayImage.LockBits(new Rectangle(child.Value.PointsArea[0].X, child.Value.PointsArea[0].Y, child.Value.PointsArea[1].X, child.Value.PointsArea[2].Y), 
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

    class Corners 
    {
        public List<Point> coordinates { get; set; }
    }

    class OpenLines {
        public Point startPoint { get; set; }
        public Point endPoint { get; set; }
        public char plain { get; set; }

        public OpenLines(Point startPoint, Point endPoint, char plain)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
            this.plain = plain;
        }
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
}
