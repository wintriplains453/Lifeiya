using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SignaliEdge
{
    class Detector
    {
        public Detector() { }

        public List<string> currentListEnd = new List<string>();
        private byte directionContours = 0;
        private int[] currentData;
        private string start_checkPoint = "";
        private bool is_BlockingSearch = false;

        private byte signX = 1;
        private byte signY = 0;
        private bool is_signX = true;
        private bool is_signY = false;


        private bool _maxPrecision;
        public bool MaxPrecision { get; set; }

        //Временный список
        private List<string> currentListLive = new List<string>();

        //UpperTreshold = ut, LowerTreshold = lt
        private double ut, lt;
        public double LowerTreshold { get; set; }
        public double UpperTreshold { get; set; }

        private double[,] xIzvod;
        private double[,] yIzvod;
        private double[,] magnitudaGradijenta;
        private double[,] smerGradijenta;
        private double[,] xMatrica = { { 1, 0, -1 }, { 2, 0, -2 }, { 1, 0, -1 } };
        private double[,] yMatrica = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
        private double[,] gaussMatrica = {
                                            {0.0121, 0.0261, 0.0337, 0.0261, 0.0121},
                                            {0.0261, 0.0561, 0.0724, 0.0561, 0.0261},
                                            {0.0337, 0.0724, 0.0935, 0.0724, 0.0337},
                                            {0.0261, 0.0561, 0.0724, 0.0561, 0.0261},
                                            {0.0121, 0.0261, 0.0337, 0.0261, 0.0121}
                                        };

        private double[,] konvolucija(double[,] slikaUlaz, double[,] kernel, int poluprecnik)
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

        public double[,] prosiriMatricu(double[,] matrica, int prosirenje)
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

        public double[,] Detection(double[,] normSlika, int precision)
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
        public void ContoursList(List<string> dataCoordinates)
        {
            int length_dataCoordinates = dataCoordinates.Count;
            bool is_cyrcle_while = true;

            int[] itemData = dataCoordinates[0].Split(new char[] { '|' }).Select(x => int.Parse(x)).ToArray();//X , Y
            currentData = dataCoordinates[0].Split(new char[] { '|' }).Select(x => int.Parse(x)).ToArray();//X , Y

            for (int m = 0; m < length_dataCoordinates; m++)//Перебор всех точек
            {
                //Console.WriteLine(true);
                //if (!currentListEnd.Contains(dataCoordinates[m]))
                //{
                currentData = dataCoordinates[m].Split(new char[] { '|' }).Select(x => int.Parse(x)).ToArray();//X , Y
                is_cyrcle_while = true;
                //Console.WriteLine("start = " + dataCoordinates[m]);

                signX = 1;
                signY = 0;
                is_signX = true;
                is_signY = false;
                start_checkPoint = dataCoordinates[m];

                while (is_cyrcle_while)//Перебор всех точек
                {
                    if (!currentListEnd.Contains(currentData[0] + "|" + currentData[1]) && dataCoordinates.Contains(currentData[0] + "|" + currentData[1]))
                    {
                        currentListLive.Add(currentData[0] + "|" + currentData[1]);

                        //X и Y -1 или +1 (направления в signX и signY определяется в методе ChangeDirection)
                        currentData[0] = is_signX == true ? currentData[0] + signX : currentData[0] - signX;
                        currentData[1] = is_signY == true ? currentData[1] + signY : currentData[1] - signY;

                        if (!dataCoordinates.Contains(currentData[0] + "|" + currentData[1]))
                        {
                            currentData[0] = is_signX == true ? currentData[0] - signX : currentData[0] + signX;
                            currentData[1] = is_signY == true ? currentData[1] - signY : currentData[1] + signY;
                            ChangeDirection(dataCoordinates);
                        }
                        //Console.WriteLine("X = " + currentData[0] + " Y = " + currentData[1]);
                    }
                    else
                    {
                        ChangeDirection(dataCoordinates);

                        if (is_BlockingSearch)
                        {
                            is_BlockingSearch = false;
                            is_cyrcle_while = false;
                            break;
                        }
                    }
                    int length_currentListLive = currentListLive.Count;
                    for (int j = 0; j < length_currentListLive; j++)
                    {
                        currentListEnd.Add(currentListLive[j]);
                    }
                    currentListLive.Clear();
                    if (is_BlockingSearch)
                    {
                        is_BlockingSearch = false;
                        break;
                    }
                }
                //} else
                //{
                //int[] start_checkPoint_Array = start_checkPoint.Split(new char[] { '|' }).Select(x => int.Parse(x)).ToArray();//X , Y
                //int[] result_Point = new int[2];
                //result_Point[0] = start_checkPoint_Array[0] > currentData[0] ? start_checkPoint_Array[0] - currentData[0] : currentData[0] - start_checkPoint_Array[0];
                //result_Point[1] = start_checkPoint_Array[1] > currentData[1] ? start_checkPoint_Array[1] - currentData[1] : currentData[1] - start_checkPoint_Array[1];
                //if(result_Point[0] == 1 || result_Point[1] == 1)
                //{
                //    Console.WriteLine("Замкнулся");
                //    break;
                // }
                // }
            }
        }

        //signX signY - значения var = -1 , +1
        public void ChangeDirection(List<string> dataCoordinates)
        {
            //test
            //Console.WriteLine("X = " + currentData[0] + " Y = " + currentData[1]);
            bool _is_searching = false;

            string[] _dataPosition = new string[8];
            string[,] _dataPosition_direction = new string[8, 4];
            byte[] _dataPosition_directionPosition = new byte[8];

            switch (directionContours)
            {
                case 0:
                    _dataPosition[0] = (currentData[0] + 1) + "|" + currentData[1];
                    _dataPosition[1] = currentData[0] + "|" + (currentData[1] + 1);
                    _dataPosition[2] = (currentData[0] + 1) + "|" + (currentData[1] + 1);
                    _dataPosition[3] = currentData[0] + "|" + (currentData[1] - 1);
                    _dataPosition[4] = (currentData[0] + 1) + "|" + (currentData[1] - 1);
                    _dataPosition[5] = (currentData[0] + 1) + "|" + currentData[1];
                    _dataPosition[6] = (currentData[0] + 1) + "|" + currentData[1];
                    _dataPosition[7] = (currentData[0] + 1) + "|" + currentData[1];

                    _dataPosition_direction = new string[8, 4] {
                        { "1", "0", "true", "false" },
                        { "0", "1", "false", "true" },
                        { "1", "1", "true", "true" },
                        { "0", "1", "false", "false" },
                        { "1", "1", "true", "false" },
                        { "1", "0", "true", "false" },
                        { "1", "0", "true", "false" },
                        { "1", "0", "true", "false" },
                    };
                    _dataPosition_directionPosition = new byte[8] { 0, 2, 1, 6, 7, 0, 0, 0 };
                    break;
                case 1:
                    _dataPosition[0] = (currentData[0] + 1) + "|" + currentData[1];
                    _dataPosition[1] = currentData[0] + "|" + (currentData[1] + 1);
                    _dataPosition[2] = (currentData[0] + 1) + "|" + (currentData[1] + 1);
                    _dataPosition[3] = (currentData[0] + 1) + "|" + currentData[1];
                    _dataPosition[4] = (currentData[0] + 1) + "|" + currentData[1];
                    _dataPosition[5] = (currentData[0] + 1) + "|" + currentData[1];
                    _dataPosition[6] = (currentData[0] + 1) + "|" + currentData[1];
                    _dataPosition[7] = (currentData[0] + 1) + "|" + currentData[1];

                    _dataPosition_direction = new string[8, 4] {
                        { "1", "0", "true", "false" },
                        { "0", "1", "false", "true" },
                        { "1", "1", "true", "true" },
                        { "1", "0", "true", "false" },
                        { "1", "0", "true", "false" },
                        { "1", "0", "true", "false" },
                        { "1", "0", "true", "false" },
                        { "1", "0", "true", "false" },
                    };
                    _dataPosition_directionPosition = new byte[8] { 0, 2, 1, 0, 0, 0, 0, 0 };
                    break; 
                case 2:
                    _dataPosition[0] = currentData[0] + "|" + (currentData[1] + 1);
                    _dataPosition[1] = (currentData[0] - 1) + "|" + currentData[1];
                    _dataPosition[2] = (currentData[0] - 1) + "|" + (currentData[1] + 1);
                    _dataPosition[3] = (currentData[0] + 1) + "|" + currentData[1];
                    _dataPosition[4] = (currentData[0] + 1) + "|" + (currentData[1] + 1);
                    _dataPosition[5] = currentData[0] + "|" + (currentData[1] + 1);
                    _dataPosition[6] = currentData[0] + "|" + (currentData[1] + 1);
                    _dataPosition[7] = currentData[0] + "|" + (currentData[1] + 1);

                    _dataPosition_direction = new string[8, 4] {
                        { "0", "1", "false", "true" },
                        { "1", "0", "false", "false" },
                        { "1", "1", "false", "true" },
                        { "1", "0", "true", "false" },
                        { "1", "1", "true", "true" },
                        { "0", "1", "false", "true" },
                        { "0", "1", "false", "true" },
                        { "0", "1", "false", "true" },
                    };
                    _dataPosition_directionPosition = new byte[8] { 2, 4, 3, 0, 1, 2, 2, 2 };
                    break;
                case 3:
                    _dataPosition[0] = currentData[0] + "|" + (currentData[1] + 1);
                    _dataPosition[1] = (currentData[0] - 1) + "|" + currentData[1];
                    _dataPosition[2] = (currentData[0] - 1) + "|" + (currentData[1] + 1);
                    _dataPosition[3] = currentData[0] + "|" + (currentData[1] + 1);
                    _dataPosition[4] = currentData[0] + "|" + (currentData[1] + 1);
                    _dataPosition[5] = currentData[0] + "|" + (currentData[1] + 1);
                    _dataPosition[6] = currentData[0] + "|" + (currentData[1] + 1);
                    _dataPosition[7] = currentData[0] + "|" + (currentData[1] + 1);

                    _dataPosition_direction = new string[8, 4] {
                        { "0", "1", "false", "true" },
                        { "1", "0", "false", "false" },
                        { "1", "1", "false", "true" },
                        { "0", "1", "false", "true" },
                        { "0", "1", "false", "true" },
                        { "0", "1", "false", "true" },
                        { "0", "1", "false", "true" },
                        { "0", "1", "false", "true" },
                    };
                    _dataPosition_directionPosition = new byte[8] { 2, 4, 3, 2, 2, 2, 2, 2 };
                    break;
                case 4:
                    _dataPosition[0] = (currentData[0] - 1) + "|" + currentData[1];
                    _dataPosition[1] = currentData[0] + "|" + (currentData[1] - 1);
                    _dataPosition[2] = (currentData[0] - 1) + "|" + (currentData[1] - 1);
                    _dataPosition[3] = currentData[0] + "|" + (currentData[1] + 1);
                    _dataPosition[4] = (currentData[0] - 1) + "|" + (currentData[1] + 1);
                    _dataPosition[5] = (currentData[0] - 1) + "|" + currentData[1];
                    _dataPosition[6] = (currentData[0] - 1) + "|" + currentData[1];
                    _dataPosition[7] = (currentData[0] - 1) + "|" + currentData[1];

                    _dataPosition_direction = new string[8, 4] {
                        { "1", "0", "false", "false" },
                        { "0", "1", "false", "false" },
                        { "1", "1", "false", "false" },
                        { "0", "1", "false", "true" },
                        { "1", "1", "false", "true" },
                        { "1", "0", "false", "false" },
                        { "1", "0", "false", "false" },
                        { "1", "0", "false", "false" },
                    };
                    _dataPosition_directionPosition = new byte[8] { 4, 6, 5, 2, 3, 4, 4, 4 };
                    break;
                case 5:
                    _dataPosition[0] = (currentData[0] - 1) + "|" + currentData[1];
                    _dataPosition[1] = currentData[0] + "|" + (currentData[1] - 1);
                    _dataPosition[2] = (currentData[0] - 1) + "|" + (currentData[1] - 1);
                    _dataPosition[3] = (currentData[0] - 1) + "|" + currentData[1];
                    _dataPosition[4] = (currentData[0] - 1) + "|" + currentData[1];
                    _dataPosition[5] = (currentData[0] - 1) + "|" + currentData[1];
                    _dataPosition[6] = (currentData[0] - 1) + "|" + currentData[1];
                    _dataPosition[7] = (currentData[0] - 1) + "|" + currentData[1];

                    _dataPosition_direction = new string[8, 4] {
                        { "1", "0", "false", "false" },
                        { "0", "1", "false", "false" },
                        { "1", "1", "false", "false" },
                        { "1", "0", "false", "false" },
                        { "1", "0", "false", "false" },
                        { "1", "0", "false", "false" },
                        { "1", "0", "false", "false" },
                        { "1", "0", "false", "false" },
                    };
                    _dataPosition_directionPosition = new byte[8] { 4, 6, 5, 4, 4, 4, 4, 4 };
                    break;
                case 6:
                    _dataPosition[0] = currentData[0] + "|" + (currentData[1] - 1);
                    _dataPosition[1] = (currentData[0] + 1) + "|" + currentData[1];
                    _dataPosition[2] = (currentData[0] + 1) + "|" + (currentData[1] - 1);
                    _dataPosition[3] = (currentData[0] - 1) + "|" + currentData[1];
                    _dataPosition[4] = (currentData[0] - 1) + "|" + (currentData[1] - 1);
                    _dataPosition[5] = currentData[0] + "|" + (currentData[1] - 1);
                    _dataPosition[6] = currentData[0] + "|" + (currentData[1] - 1);
                    _dataPosition[7] = currentData[0] + "|" + (currentData[1] - 1);

                    _dataPosition_direction = new string[8, 4] {
                        { "0", "1", "false", "false" },
                        { "1", "0", "true", "false" },
                        { "1", "1", "true", "false" },
                        { "1", "0", "false", "false" },
                        { "1", "1", "false", "false" },
                        { "0", "1", "false", "false" },
                        { "0", "1", "false", "false" },
                        { "0", "1", "false", "false" },
                    };
                    _dataPosition_directionPosition = new byte[8] { 6, 0, 7, 4, 5, 6, 6, 6 };
                    break;
                case 7:
                    _dataPosition[0] = currentData[0] + "|" + (currentData[1] - 1);
                    _dataPosition[1] = (currentData[0] + 1) + "|" + currentData[1];
                    _dataPosition[2] = (currentData[0] + 1) + "|" + (currentData[1] - 1);
                    _dataPosition[3] = currentData[0] + "|" + (currentData[1] - 1);
                    _dataPosition[4] = currentData[0] + "|" + (currentData[1] - 1);
                    _dataPosition[5] = currentData[0] + "|" + (currentData[1] - 1);
                    _dataPosition[6] = currentData[0] + "|" + (currentData[1] - 1);
                    _dataPosition[7] = currentData[0] + "|" + (currentData[1] - 1);

                    _dataPosition_direction = new string[8, 4] {
                        { "0", "1", "false", "false" },
                        { "1", "0", "true", "false" },
                        { "1", "1", "true", "false" },
                        { "0", "1", "false", "false" },
                        { "0", "1", "false", "false" },
                        { "0", "1", "false", "false" },
                        { "0", "1", "false", "false" },
                        { "0", "1", "false", "false" },
                    };
                    _dataPosition_directionPosition = new byte[8] { 6, 0, 7, 6, 6, 6, 6, 6 };
                    break;
            }

            for (byte i = 0; i < _dataPosition.Length; i++)
            {
                if (dataCoordinates.Contains(_dataPosition[i]) && i != 7 && !currentListEnd.Contains(currentData[0] + "|" + currentData[1]))
                {
                    signX = Convert.ToByte(_dataPosition_direction[i, 0]);
                    signY = Convert.ToByte(_dataPosition_direction[i, 1]);

                    is_signX = Convert.ToBoolean(_dataPosition_direction[i, 2]);
                    is_signY = Convert.ToBoolean(_dataPosition_direction[i, 3]);

                    directionContours = _dataPosition_directionPosition[i];

                    currentData[0] = is_signX == true ? currentData[0] + signX : currentData[0] - signX;
                    currentData[1] = is_signY == true ? currentData[1] + signY : currentData[1] - signY;

                    if (currentListEnd.Contains(currentData[0] + "|" + currentData[1]))
                    {
                        currentData[0] = is_signX == true ? currentData[0] - signX : currentData[0] + signX;
                        currentData[1] = is_signY == true ? currentData[1] - signY : currentData[1] + signY;
                    }

                    if (start_checkPoint == currentData[0] + "|" + currentData[1])
                    {
                        Console.WriteLine("Замкнулся");
                        is_BlockingSearch = true;
                    }
                    _is_searching = true;
                    break;
                }
            }

            if (!_is_searching)
            {
                is_BlockingSearch = true;
            }
            //
        }

        internal void CleanUp()
        {
            xIzvod = null;
            yIzvod = null;
            magnitudaGradijenta = null;
            smerGradijenta = null;
            GC.Collect();
        }


    }
}
