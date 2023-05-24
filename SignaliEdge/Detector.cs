using System;
using System.Collections.Generic;
using System.Runtime;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;

namespace SignaliEdge
{
    class Detector
    {
        public Detector() { }

        public HashSet<Point> currentListEnd = new HashSet<Point>();
        internal HashSet<Point> _HashdataCoordinates = new HashSet<Point>();
        internal List<Point> _coordinatesVectors = new List<Point>();
        internal byte directionContours = 0;
        internal Point currentData;
        internal Point start_checkPointItems;
        internal Point start_checkPoint;
        internal bool is_Close_Search = false;

        internal byte signX = 1;
        internal byte signY = 0;
        internal bool is_signX = true;
        internal bool is_signY = false;

        internal bool _maxPrecision;
        internal bool _is_checkAfterAbyss = false;
        public bool MaxPrecision { get; set; }

        //Словарь незамкнутых линий
        public Dictionary<int, List<Point>> incompleteLines = new Dictionary<int, List<Point>>();
        internal int _count_incompleteLines = 0;

        //Временный список
        internal HashSet<Point> currentListLive = new HashSet<Point>();
        internal List<Point> currentListDeath = new List<Point>();

        public Dictionary<int, DataPoints> currentListDictionary = new Dictionary<int, DataPoints>();
        private int DictionaryCounter = 0;

        //UpperTreshold = ut, LowerTreshold = lt
        internal double ut = 0.002;
        internal double lt = 0.001;
        public double LowerTreshold { get; set; }
        public double UpperTreshold { get; set; }

        internal double[,] xIzvod;
        internal double[,] yIzvod;
        internal double[,] magnitudaGradijenta;
        internal double[,] smerGradijenta;
        internal double[,] xMatrica = { { 1, 0, -1 }, { 2, 0, -2 }, { 1, 0, -1 } };
        internal double[,] yMatrica = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
        internal double[,] gaussMatrica = {
                                            {0.0121, 0.0261, 0.0337, 0.0261, 0.0121},
                                            {0.0261, 0.0561, 0.0724, 0.0561, 0.0261},
                                            {0.0337, 0.0724, 0.0935, 0.0724, 0.0337},
                                            {0.0261, 0.0561, 0.0724, 0.0561, 0.0261},
                                            {0.0121, 0.0261, 0.0337, 0.0261, 0.0121}
                                        };

        internal double[,] konvolucija(double[,] slikaUlaz, double[,] kernel, int poluprecnik)
        {
            if (slikaUlaz == null) return null;

            int yPoz = slikaUlaz.GetLength(1);
            int xPoz = slikaUlaz.GetLength(0);

            double[,] slikaIzlaz = new double[xPoz, yPoz];

            //for (int i = 0; i < xPoz; i++)
            Parallel.For(0, xPoz, i => {
                for (int j = 0; j < yPoz; j++)
                {
                    double novaVrednost = 0;
                    for (int innerI = i - poluprecnik; innerI < i + poluprecnik + 1; innerI++)
                        for (int innerJ = j - poluprecnik; innerJ < j + poluprecnik + 1; innerJ++)
                        {
                            int idxX = (innerI + xPoz) % xPoz;
                            int idxY = (innerJ + yPoz) % yPoz;

                            int kernx = innerI - (i - poluprecnik);
                            int kerny = innerJ - (j - poluprecnik);
                            novaVrednost += slikaUlaz[idxX, idxY] * kernel[kernx, kerny];
                        }

                    slikaIzlaz[i, j] = novaVrednost;
                }
            });


            return slikaIzlaz;
        }

        internal double[,] prosiriMatricu(double[,] matrica, int prosirenje)
        {
            if (matrica == null) return null;

            int x = matrica.GetLength(0), y = matrica.GetLength(1);

            double[,] vracaSe = new double[x + 2 * prosirenje, y + 2 * prosirenje];
            for (int i = -prosirenje; i < x + prosirenje - 1; i++)
                for (int j = -prosirenje; j < y + prosirenje - 1; j++)
                {
                    var ii = (i + x) % x;
                    int jj = (j + y) % y;
                    vracaSe[i + 2, j + 2] = matrica[ii, jj];
                }
            return vracaSe;
        }

        internal double[,] Detection(double[,] normSlika, int precision)
        {

            if (normSlika == null) return null;
            int blurx, blury;

            double[,] blurovana;
            try
            {
                blurovana = konvolucija(normSlika, gaussMatrica, 2);

                blurx = blurovana.GetLength(0); blury = blurovana.GetLength(1);

                xIzvod = konvolucija(blurovana, xMatrica, 1);
                yIzvod = konvolucija(blurovana, yMatrica, 1);
            }
            catch (OutOfMemoryException)
            {
                throw;
            }

            int xIzvodx = xIzvod.GetLength(0), xIzvody = xIzvod.GetLength(1);
            magnitudaGradijenta = new double[xIzvodx, xIzvody];
            smerGradijenta = new double[xIzvodx, xIzvody];

            //Проходим по фильтрованому blur изображению и ищем направление контура
            for (int x = 0; x < blurx; x++)
            {
                for (int y = 0; y < blury; y++)
                {
                    magnitudaGradijenta[x, y] = Math.Sqrt(xIzvod[x, y] * xIzvod[x, y] + yIzvod[x, y] * yIzvod[x, y]);
                    double pom = Math.Atan2(xIzvod[x, y], yIzvod[x, y]);
                    if ((pom >= -Math.PI / 8 && pom < Math.PI / 8) || (pom <= -7 * Math.PI / 8 && pom > 7 * Math.PI / 8))
                        smerGradijenta[x, y] = 0;//0 радусов 
                    else if ((pom >= Math.PI / 8 && pom < 3 * Math.PI / 8) || (pom <= -5 * Math.PI / 8 && pom > -7 * Math.PI / 8))
                        smerGradijenta[x, y] = Math.PI / 4;//45 радусов
                    else if ((pom >= 3 * Math.PI / 8 && pom <= 5 * Math.PI / 8) || (-3 * Math.PI / 8 >= pom && pom > -5 * Math.PI / 8))
                        smerGradijenta[x, y] = Math.PI / 2;//90 градусов
                    else if ((pom < -Math.PI / 8 && pom >= -3 * Math.PI / 8) || (pom > 5 * Math.PI / 8 && pom <= 7 * Math.PI / 8))
                        smerGradijenta[x, y] = -Math.PI / 4;//135 радусов
                }
            }

            var max = maxi(magnitudaGradijenta);
            for (int i = 0; i < xIzvodx; i++)
            {
                for (int j = 0; j < xIzvody; j++)
                {
                    magnitudaGradijenta[i, j] /= max;
                }
            }

            if (ut == 0 && lt == 0) OdrediPragove(blurx, blury);

            for (int i = 0; i < xIzvodx; i++)
            {
                for (int j = 0; j < xIzvody; j++)
                {
                    magnitudaGradijenta[i, j] = magnitudaGradijenta[i, j] < lt ? 0 : magnitudaGradijenta[i, j];
                }
            }

            for (var x = 1; x < blurx - 1; x++)
            {
                for (var y = 1; y < blury - 1; y++)
                {
                    //Определяется следующий элемент по градусу входит ли в направление

                    //Градус 0
                    if (smerGradijenta[x, y] == 0 && (magnitudaGradijenta[x, y] <= magnitudaGradijenta[x - 1, y] || magnitudaGradijenta[x, y] <= magnitudaGradijenta[x + 1, y]))
                        magnitudaGradijenta[x, y] = 0;//Тoчка будeт cчитатьcя границeй, ecли eё интeнcивнocть бoльшe чeм у тoчки вышe и нижe 
                    //Градус 90
                    else if (smerGradijenta[x, y] == Math.PI / 2 && (magnitudaGradijenta[x, y] <= magnitudaGradijenta[x, y - 1] || magnitudaGradijenta[x, y + 1] >= magnitudaGradijenta[x, y]))
                        magnitudaGradijenta[x, y] = 0;//Тoчка будeт cчитатьcя границeй, ecли eё интeнcивнocть бoльшe чeм у тoчки cлeва и cправа
                    //Градус 45
                    else if (smerGradijenta[x, y] == Math.PI / 4 && (magnitudaGradijenta[x, y] <= magnitudaGradijenta[x - 1, y + 1] || magnitudaGradijenta[x, y] <= magnitudaGradijenta[x + 1, y - 1]))
                        magnitudaGradijenta[x, y] = 0;//Тoчка будeт cчитатьcя границeй, ecли eё интeнcивнocть бoльшe чeм у тoчeк нахoдящихcя в вeрхнeм правoм и нижнeм лeвoм углу
                    //Градус 135
                    else if (smerGradijenta[x, y] == -Math.PI / 4 && (magnitudaGradijenta[x, y] <= magnitudaGradijenta[x - 1, y - 1] || magnitudaGradijenta[x, y] <= magnitudaGradijenta[x + 1, y + 1]))
                        magnitudaGradijenta[x, y] = 0;//Тoчка будeт cчитатьcя границeй, ecли eё интeнcивнocть бoльшe чeм у тoчeк нахoдящихcя в вeрхнeм лeвoм и нижнeм правoм углу
                }
            }

            //Еще одно определение следующих элемент по градусу в направление
            for (var x = 2; x < blurx - 2; x++)
            {
                for (var y = 2; y < blury - 2; y++)
                {
                    if (smerGradijenta[x, y] == 0)
                        if (magnitudaGradijenta[x - 2, y] > magnitudaGradijenta[x, y] || magnitudaGradijenta[x + 2, y] > magnitudaGradijenta[x, y])
                            magnitudaGradijenta[x, y] = 0;
                    if (smerGradijenta[x, y] == Math.PI / 2)
                        if (magnitudaGradijenta[x, y - 2] > magnitudaGradijenta[x, y] || magnitudaGradijenta[x, y + 2] > magnitudaGradijenta[x, y])
                            magnitudaGradijenta[x, y] = 0;
                    if (smerGradijenta[x, y] == Math.PI / 4)
                        if (magnitudaGradijenta[x - 2, y + 2] > magnitudaGradijenta[x, y] || magnitudaGradijenta[x + 2, y - 2] > magnitudaGradijenta[x, y])
                            magnitudaGradijenta[x, y] = 0;
                    if (smerGradijenta[x, y] == -Math.PI / 4)
                        if (magnitudaGradijenta[x + 2, y + 2] > magnitudaGradijenta[x, y] || magnitudaGradijenta[x - 2, y - 2] > magnitudaGradijenta[x, y])
                            magnitudaGradijenta[x, y] = 0;
                }
            }

            for (var x = 0; x < blurx; x++)
            {
                for (var y = 0; y < blury; y++)
                {
                    if (magnitudaGradijenta[x, y] > ut)
                        magnitudaGradijenta[x, y] = 1;
                }
            }

            //histerezis pocetak

            int pomH = 0;
            int pomStaro = -1;
            int prolaz = 0;

            bool nastavi = true;
            while (nastavi)
            {
                prolaz = prolaz + 1;
                pomStaro = pomH;
                for (int x = 1; x < xIzvodx - 1; x++)
                {
                    for (int y = 1; y < xIzvody - 1; y++)
                    {
                        if (magnitudaGradijenta[x, y] <= ut && magnitudaGradijenta[x, y] >= lt)
                        {
                            double pom1 = magnitudaGradijenta[x - 1, y - 1];
                            double pom2 = magnitudaGradijenta[x, y - 1];
                            double pom3 = magnitudaGradijenta[x + 1, y - 1];
                            double pom4 = magnitudaGradijenta[x - 1, y];
                            double pom5 = magnitudaGradijenta[x + 1, y];
                            double pom6 = magnitudaGradijenta[x - 1, y + 1];
                            double pom7 = magnitudaGradijenta[x, y + 1];
                            double pom8 = magnitudaGradijenta[x + 1, y + 1];

                            if (pom1 == 1 || pom2 == 1 || pom3 == 1 || pom4 == 1 || pom5 == 1 || pom6 == 1 || pom7 == 1 || pom8 == 1)
                            {
                                magnitudaGradijenta[x, y] = 1;
                                pomH = pomH + 1;

                            }
                        }
                    }
                }

                if (_maxPrecision)
                {
                    nastavi = pomH != pomStaro;
                }
                else
                {
                    nastavi = prolaz <= precision;
                }
            }

            for (int i = 0; i < xIzvodx; i++)
            {
                for (int j = 0; j < xIzvody; j++)
                {
                    if (magnitudaGradijenta[i, j] <= ut)
                        magnitudaGradijenta[i, j] = 0;
                }
            }

            //histerezis kraj
            return magnitudaGradijenta;
        }

        private void OdrediPragove(int dimx, int dimy)
        {
            //automatsko odredjivanje
            double suma = 0;
            double broj = 0;

            for (var x = 1; x < dimx - 1; x++)
                for (var y = 1; y < dimy - 1; y++)
                {
                    if (magnitudaGradijenta[x, y] != 0)
                    {
                        suma += magnitudaGradijenta[x, y];
                        broj++;
                    }
                }
            ut = suma / broj;
            lt = 0.4 * ut;

        }

        private double maxi(double[,] mat)
        {
            double m = -1;

            foreach (var el in mat)
            {
                m = el > m ? el : m;
            }

            return m;
        }

        //dataCoordinates[x,y]
        internal void ContoursList(List<Point> dataCoordinates)
        {
            //dataCoordinates = dataCoordinates.OrderBy(x => x.Y).ThenBy(y => y.X).ToList();
            _HashdataCoordinates.UnionWith(dataCoordinates);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int length_dataCoordinates = dataCoordinates.Count;
            currentData = dataCoordinates[0];

            for (int m = 0; m < length_dataCoordinates; m++)//Перебор всех точек
            {
                if (!currentListEnd.Contains(dataCoordinates[m]))
                {
                    currentData = dataCoordinates[m];
                    signX = 1;
                    signY = 0;
                    is_signX = true;
                    is_signY = false;
                    start_checkPoint = dataCoordinates[m];
                    start_checkPointItems = dataCoordinates[m];
                    directionContours = 0;
                    is_Close_Search = false;
                    _is_checkAfterAbyss = false;
                    currentListDeath.Clear();
                    _coordinatesVectors.Clear();

                    while (true)//Перебор всех точек в точках
                    {
                        //Console.WriteLine(" currentData = " + currentData.X + " " + currentData.Y);
                        currentListDeath.Add(currentData);
                        if (!currentListEnd.Contains(currentData) && _HashdataCoordinates.Contains(currentData))
                        {
                            currentListLive.Add(currentData);

                            //X и Y -1 или +1 (направления в signX и signY определяется в методе ChangeDirection)
                            currentData.X = is_signX == true ? currentData.X + signX : currentData.X - signX;
                            currentData.Y = is_signY == true ? currentData.Y + signY : currentData.Y - signY;

                            if (!_HashdataCoordinates.Contains(currentData) || _is_checkAfterAbyss)
                            {
                                currentData.X = is_signX == true ? currentData.X - signX : currentData.X + signX;
                                currentData.Y = is_signY == true ? currentData.Y - signY : currentData.Y + signY;
                                //Для одноразовых точек (шумы)
                                if (start_checkPoint == currentData)
                                {
                                    break;
                                }
                                else
                                {
                                    ChangeDirection();
                                }
                            }
                        }
                        else
                        {
                            //Для замкнутых линий
                            if (start_checkPoint == currentData && currentListDeath.Count > 50)
                            {
                                currentListDictionary.Add(DictionaryCounter, new DataPoints(new List<Point>(currentListDeath)));
                                DictionaryCounter++;

                                //Console.WriteLine("Замкнулся " + " 0X = " + start_checkPointItems.X + " 1X = " + currentData.X + " 0Y = " + start_checkPointItems.Y + " 1Y = " + currentData.Y);
                                is_Close_Search = true;
                            }
                            ChangeDirection();
                        }

                        int length_currentListLive = currentListLive.Count;
                        for (int j = 0; j < length_currentListLive; j++)
                        {
                            currentListEnd.Add(currentListLive.ToList()[j]);
                        }
                        currentListLive.Clear();

                        if (is_Close_Search)
                        {
                            break;
                        }
                    }

                    //ПРОВЕРКА EQUAL (ЕСТЬ ЛИ ТАКОЙ СПИСОК currentListDeath В ОДНОМ ИЗ ЗНАЧЕНИЕ СЛОВАРЯ currentListDictionary)
                    bool is_have_dictionary = false;
                    foreach (int key in currentListDictionary.Keys)
                    {
                        if (currentListDictionary[key].data.SequenceEqual(currentListDeath))
                        {
                            is_have_dictionary = true;
                            break;
                        }
                    }
                    if (currentListDeath.Count > MyGlobals.g_length_Line_Rectangle / 2 && !is_have_dictionary)
                    {
                        //Линии
                        incompleteLines.Add(_count_incompleteLines, new List<Point>(currentListDeath));
                        _count_incompleteLines++;

                    }
                    //END COMMENT
                }
            }


            stopwatch.Stop();
            Console.WriteLine("Time result = " + stopwatch.Elapsed.TotalSeconds);
        }

        //signX signY - значения var = -x,y , +x,y
        internal void ChangeDirection()
        {
            int length_distance_Abyss = currentListDeath.Count < 5 ? 1 : MyGlobals.g_distance_Abyss;
            byte signXCoopy = signX;
            byte signYCopy = signY;

            for (int y = 1; y <= length_distance_Abyss; y++)
            {
                for (int x = 1; x <= length_distance_Abyss; x++)
                {
                    Point[] _dataPosition = new Point[8];
                    string[,] _dataPosition_direction = new string[8, 4];
                    byte[] _dataPosition_directionPosition = new byte[8];

                    switch (directionContours)
                    {
                        case 0:
                            _dataPosition[0] = new Point(currentData.X + x, currentData.Y);
                            _dataPosition[1] = new Point(currentData.X + x, currentData.Y - y);
                            _dataPosition[2] = new Point(currentData.X + x, currentData.Y + y);
                            _dataPosition[3] = new Point(currentData.X, currentData.Y + y);
                            _dataPosition[4] = new Point(currentData.X - x, currentData.Y + y);
                            _dataPosition[5] = new Point(currentData.X - x, currentData.Y - y);
                            _dataPosition[6] = new Point(currentData.X, currentData.Y - y);
                            _dataPosition[7] = new Point(currentData.X + x, currentData.Y);

                            _dataPosition_direction = new string[8, 4] {
                            { "1", "0", "true", "false" },
                            { "1", "1", "true", "false" },
                            { "1", "1", "true", "true" },
                            { "0", "1", "false", "true" },
                            { "1", "1", "false", "true" },
                            { "1", "1", "false", "false" },
                            { "0", "1", "false", "false" },
                            { "1", "0", "true", "false" },
                        };
                            _dataPosition_directionPosition = new byte[8] { 0, 7, 1, 2, 3, 5, 6, 0 };
                            break;
                        case 1:
                            _dataPosition[0] = new Point(currentData.X + x, currentData.Y + y);
                            _dataPosition[1] = new Point(currentData.X + x, currentData.Y);
                            _dataPosition[2] = new Point(currentData.X, currentData.Y + y);
                            _dataPosition[3] = new Point(currentData.X - x, currentData.Y + y);
                            _dataPosition[4] = new Point(currentData.X - x, currentData.Y);
                            _dataPosition[5] = new Point(currentData.X, currentData.Y - y);
                            _dataPosition[6] = new Point(currentData.X + x, currentData.Y - y);
                            _dataPosition[7] = new Point(currentData.X + x, currentData.Y + y);

                            _dataPosition_direction = new string[8, 4] {
                            { "1", "1", "true", "true" },
                            { "1", "0", "true", "false" },
                            { "0", "1", "false", "true" },
                            { "1", "1", "false", "true" },
                            { "1", "0", "false", "false" },
                            { "0", "1", "false", "false" },
                            { "1", "1", "true", "false" },
                            { "1", "1", "true", "true" },
                        };
                            _dataPosition_directionPosition = new byte[8] { 1, 0, 2, 3, 4, 6, 7, 1 };
                            break;
                        case 2:
                            _dataPosition[0] = new Point(currentData.X, currentData.Y + y);
                            _dataPosition[1] = new Point(currentData.X + x, currentData.Y + y);
                            _dataPosition[2] = new Point(currentData.X - x, currentData.Y + y);
                            _dataPosition[3] = new Point(currentData.X - x, currentData.Y);
                            _dataPosition[4] = new Point(currentData.X - x, currentData.Y - y);
                            _dataPosition[5] = new Point(currentData.X + x, currentData.Y - y);
                            _dataPosition[6] = new Point(currentData.X + x, currentData.Y);
                            _dataPosition[7] = new Point(currentData.X, currentData.Y + y);

                            _dataPosition_direction = new string[8, 4] {
                            { "0", "1", "false", "true" },
                            { "1", "1", "true", "true" },
                            { "1", "1", "false", "true" },
                            { "1", "0", "false", "false" },
                            { "1", "1", "false", "false" },
                            { "1", "1", "true", "false" },
                            { "1", "0", "true", "false" },
                             { "0", "1", "false", "true" },
                        };
                            _dataPosition_directionPosition = new byte[8] { 2, 1, 3, 4, 5, 7, 0, 2 };
                            break;
                        case 3:
                            _dataPosition[0] = new Point(currentData.X - x, currentData.Y + y);
                            _dataPosition[1] = new Point(currentData.X, currentData.Y + y);
                            _dataPosition[2] = new Point(currentData.X - x, currentData.Y);
                            _dataPosition[3] = new Point(currentData.X - x, currentData.Y - y);
                            _dataPosition[4] = new Point(currentData.X, currentData.Y - y);
                            _dataPosition[5] = new Point(currentData.X + x, currentData.Y);
                            _dataPosition[6] = new Point(currentData.X + x, currentData.Y + y);
                            _dataPosition[7] = new Point(currentData.X - x, currentData.Y + y);

                            _dataPosition_direction = new string[8, 4] {
                            { "1", "1", "false", "true" },
                            { "0", "1", "false", "true" },
                            { "1", "0", "false", "false" },
                            { "1", "1", "false", "false" },
                            { "0", "1", "false", "false" },
                            { "1", "0", "true", "false" },
                            { "1", "1", "true", "true" },
                            { "1", "1", "false", "true" },
                        };
                            _dataPosition_directionPosition = new byte[8] { 3, 2, 4, 5, 6, 0, 1, 3 };
                            break;
                        case 4:
                            _dataPosition[0] = new Point(currentData.X - x, currentData.Y);
                            _dataPosition[1] = new Point(currentData.X - x, currentData.Y + y);
                            _dataPosition[2] = new Point(currentData.X - x, currentData.Y - y);
                            _dataPosition[3] = new Point(currentData.X, currentData.Y - y);
                            _dataPosition[4] = new Point(currentData.X + x, currentData.Y - y);
                            _dataPosition[5] = new Point(currentData.X + x, currentData.Y + y);
                            _dataPosition[6] = new Point(currentData.X, currentData.Y + y);
                            _dataPosition[7] = new Point(currentData.X - x, currentData.Y);

                            _dataPosition_direction = new string[8, 4] {
                            { "1", "0", "false", "false" },
                            { "1", "1", "false", "true" },
                            { "1", "1", "false", "false" },
                            { "0", "1", "false", "false" },
                            { "1", "1", "true", "false" },
                            { "1", "1", "true", "true" },
                            { "0", "1", "false", "true" },
                            { "1", "0", "false", "false" },
                        };
                            _dataPosition_directionPosition = new byte[8] { 4, 3, 5, 6, 7, 1, 2, 4 };
                            break;
                        case 5:
                            _dataPosition[0] = new Point(currentData.X - x, currentData.Y - y);
                            _dataPosition[1] = new Point(currentData.X - x, currentData.Y);
                            _dataPosition[2] = new Point(currentData.X, currentData.Y - y);
                            _dataPosition[3] = new Point(currentData.X + x, currentData.Y - y);
                            _dataPosition[4] = new Point(currentData.X + x, currentData.Y);
                            _dataPosition[5] = new Point(currentData.X, currentData.Y + y);
                            _dataPosition[6] = new Point(currentData.X - x, currentData.Y + y);
                            _dataPosition[7] = new Point(currentData.X - x, currentData.Y - y);

                            _dataPosition_direction = new string[8, 4] {
                            { "1", "1", "false", "false" },
                            { "1", "0", "false", "false" },
                            { "0", "1", "false", "false" },
                            { "1", "1", "true", "false" },
                            { "1", "0", "true", "false" },
                            { "0", "1", "false", "true" },
                            { "1", "1", "false", "true" },
                            { "1", "1", "false", "false" },
                        };
                            _dataPosition_directionPosition = new byte[8] { 5, 4, 6, 7, 0, 2, 3, 5 };
                            break;
                        case 6:
                            _dataPosition[0] = new Point(currentData.X, currentData.Y - y);
                            _dataPosition[1] = new Point(currentData.X - x, currentData.Y - y);
                            _dataPosition[2] = new Point(currentData.X + x, currentData.Y - y);
                            _dataPosition[3] = new Point(currentData.X + x, currentData.Y);
                            _dataPosition[4] = new Point(currentData.X + x, currentData.Y + y);
                            _dataPosition[5] = new Point(currentData.X - x, currentData.Y + y);
                            _dataPosition[6] = new Point(currentData.X - x, currentData.Y);
                            _dataPosition[7] = new Point(currentData.X, currentData.Y - y);

                            _dataPosition_direction = new string[8, 4] {
                            { "0", "1", "false", "false" },
                            { "1", "1", "false", "false" },
                            { "1", "1", "true", "false" },
                            { "1", "0", "true", "false" },
                            { "1", "1", "true", "true" },
                            { "1", "1", "false", "true" },
                            { "1", "0", "false", "false" },
                            { "0", "1", "false", "false" },
                        };
                            _dataPosition_directionPosition = new byte[8] { 6, 5, 7, 0, 1, 3, 4, 6 };
                            break;
                        case 7:
                            _dataPosition[0] = new Point(currentData.X + x, currentData.Y - y);
                            _dataPosition[1] = new Point(currentData.X, currentData.Y - y);
                            _dataPosition[2] = new Point(currentData.X + x, currentData.Y);
                            _dataPosition[3] = new Point(currentData.X + x, currentData.Y + y);
                            _dataPosition[4] = new Point(currentData.X, currentData.Y + y);
                            _dataPosition[5] = new Point(currentData.X - x, currentData.Y);
                            _dataPosition[6] = new Point(currentData.X - x, currentData.Y - y);
                            _dataPosition[7] = new Point(currentData.X + x, currentData.Y - y);

                            _dataPosition_direction = new string[8, 4] {
                            { "1", "1", "true", "false" },
                            { "0", "1", "false", "false" },
                            { "1", "0", "true", "false" },
                            { "1", "1", "true", "true" },
                            { "0", "1", "false", "true" },
                            { "1", "0", "false", "false" },
                            { "1", "1", "false", "false" },
                            { "1", "1", "true", "false" },
                        };
                            _dataPosition_directionPosition = new byte[8] { 7, 6, 0, 1, 2, 4, 5, 7 };
                            break;
                    }

                    for (byte i = 0; i < _dataPosition.Length; i++)
                    {
                        if (_is_checkAfterAbyss)
                        {
                            if (i == 0)
                                continue;
                        }
                        if (_HashdataCoordinates.Contains(_dataPosition[i]) && !currentListEnd.Contains(currentData))
                        {
                            _coordinatesVectors.Add(_dataPosition[i]);
                            signX = Convert.ToByte(_dataPosition_direction[i, 0]);
                            signY = Convert.ToByte(_dataPosition_direction[i, 1]);

                            signX *= (byte)x;
                            signY *= (byte)y;

                            is_signX = Convert.ToBoolean(_dataPosition_direction[i, 2]);
                            is_signY = Convert.ToBoolean(_dataPosition_direction[i, 3]);

                            directionContours = _dataPosition_directionPosition[i];

                            currentData.X = is_signX == true ? currentData.X + signX : currentData.X - signX;
                            currentData.Y = is_signY == true ? currentData.Y + signY : currentData.Y - signY;

                            if (currentListEnd.Contains(currentData))
                            {
                                currentData.X = is_signX == true ? currentData.X - signX : currentData.X + signX;
                                currentData.Y = is_signY == true ? currentData.Y - signY : currentData.Y + signY;
                            }
                            //Для замкнутых линий
                            if ((start_checkPointItems.X - 1 == currentData.X || start_checkPointItems.X + 1 == currentData.X) && (start_checkPointItems.Y - 1 == currentData.Y || start_checkPointItems.Y + 1 == currentData.Y) && currentListDeath.Count > 50)
                            {
                                currentListDictionary.Add(DictionaryCounter, new DataPoints(new List<Point>(currentListDeath)));
                                DictionaryCounter++;
                                //Console.WriteLine("Замкнулся " + " 0X = " + start_checkPointItems.X + " 1X = " + currentData.X + " 0Y = " + start_checkPointItems.Y + " 1Y = " + currentData.Y);
                                is_Close_Search = true;
                                return;
                            }
                            if (x > 1 || y > 1)
                            {
                                signX = signXCoopy;
                                signY = signYCopy;
                                _is_checkAfterAbyss = true;
                            } else
                            {
                                _is_checkAfterAbyss = false;
                            }
                            return;
                        }
                        //СЛЕДУЩАЯ ТОЧКА ОТКЛЮЧАЕТСЯ ПО I == 0
                    }
                }
            }
            /*foreach(var vector in _coordinatesVectors.ToArray())!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!Под вопросом нужности
            {
                currentData.X = vector.X;
                currentData.Y = vector.Y;
                _coordinatesVectors.Remove(vector);
                ChangeDirection();
            }*/
            is_Close_Search = true;
        }

        

        internal void BlocksDictionaryAdding(int largest__oordinate, KeyValuePair<int, ValuesDictionary> item, bool is_more, Dictionary<int, ValuesDictionary> BlocksDictionary)
        {
            int heightY = is_more ? item.Value.PointsArea[0].Y - largest__oordinate: largest__oordinate - item.Value.PointsArea[0].Y;

            BlocksDictionary.Add(MyGlobals.g_counterKey, new ValuesDictionary(false, new List<Point>() {
                new Point(item.Value.PointsArea[0].X, !is_more ? item.Value.PointsArea[0].Y : largest__oordinate),
                new Point(item.Value.PointsArea[1].X, !is_more ? item.Value.PointsArea[1].Y : largest__oordinate),
                new Point(item.Value.PointsArea[2].X, is_more ? item.Value.PointsArea[2].Y : largest__oordinate),
                new Point(item.Value.PointsArea[3].X, is_more ? item.Value.PointsArea[3].Y : largest__oordinate),

            }, MyGlobals.g_counterKey, item.Value.width, heightY, "", new Dictionary<int, Blocks>() { }, new Dictionary<int, BlocksTextP>() { }, item.Value.ParentFirst));
            MyGlobals.test.Add(MyGlobals.g_counterKey, new ValuesDictionary(false, new List<Point>() {
                new Point(item.Value.PointsArea[0].X, !is_more ? item.Value.PointsArea[0].Y : largest__oordinate),
                new Point(item.Value.PointsArea[1].X, !is_more ? item.Value.PointsArea[1].Y : largest__oordinate),
                new Point(item.Value.PointsArea[2].X, is_more ? item.Value.PointsArea[2].Y : largest__oordinate),
                new Point(item.Value.PointsArea[3].X, is_more ? item.Value.PointsArea[3].Y : largest__oordinate),

            }, MyGlobals.g_counterKey, item.Value.width, heightY, "", new Dictionary<int, Blocks>() { }, new Dictionary<int, BlocksTextP>() { }, item.Value.ParentFirst));
            MyGlobals.g_counterKey++;


        }

        internal void CleanUp()
        {
            xIzvod = null;
            yIzvod = null;
            magnitudaGradijenta = null;
            smerGradijenta = null;
            _HashdataCoordinates = null;
            currentListEnd = null;
            currentListDictionary = null;
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        }

    }

    class DataPoints
    {
        internal List<Point> data { get; set; }

        internal DataPoints(List<Point> data)
        {
            this.data = data;
        }
    }

    class SemiLines 
    {
        internal List<Point> data { get; set; }
        internal char prevails { get; set; }

        internal SemiLines(List<Point> data, char prevails)
        {
            this.data = data;
            this.prevails = prevails;
        }
    }

    class Distance
    {
        public int index { get; set; }
        public double angle { get; set; }
        //public double distance { get; set; }
        //public double normalized { get; set; }
        public Point position { get; set; }
    }
}