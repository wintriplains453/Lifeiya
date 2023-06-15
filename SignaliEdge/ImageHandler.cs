using System;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

namespace SignaliEdge
{
    public class ImageHandler
    {
        private Bitmap _currentBitmap;
        private Bitmap _originalBitmap;

        private List<ItemPixel> _fullABS = new List<ItemPixel>();
        private List<string> _all_reference_elems = new List<string>();
        private int _reference_count = 0;

        private bool isGrayscale = false;
        byte bitsPerPixel;
        private double[,] data_copy;

        /// <summary>
        /// Used to avoid transforming the image to grayscale more than once
        /// </summary>
        public bool IsGrayscale
        {
            get { return isGrayscale; }
            private set { isGrayscale = value; }
        }

        /// <summary>
        /// Bitmap used to store the original version of image,
        /// without any transformations.
        /// </summary>
        public Bitmap OriginalBitmap
        {
            set { _originalBitmap = value; }
            get { return _originalBitmap; }
        }

        /// <summary>
        /// Bitmap used to store modified version of the OriginalBitmap.
        /// This bitmap is initially same as OriginalBitmap, but after
        /// processing holds the bitmap with found edges.
        /// </summary>
        public Bitmap CurrentBitmap
        {
            get { return _currentBitmap; }
            set { _currentBitmap = value; isGrayscale = false; }
        }

        public byte GetBitsPerPixel(PixelFormat pf)
        {
            byte BitsPerPixel;
            switch (pf)
            {
                case PixelFormat.Format8bppIndexed:
                    BitsPerPixel = 8;
                    break;
                case PixelFormat.Format24bppRgb:
                    BitsPerPixel = 24;
                    break;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    BitsPerPixel = 32;
                    break;
                default:
                    BitsPerPixel = 0;
                    break;
            }
            return BitsPerPixel;
        }

        public unsafe Bitmap SetNewImage()
        {
            BitmapData bitmapData = _currentBitmap.LockBits(new Rectangle(0, 0, _currentBitmap.Width, _currentBitmap.Height),
                ImageLockMode.ReadWrite, _currentBitmap.PixelFormat);
            data_copy = new double[bitmapData.Width, bitmapData.Height];

            bitsPerPixel = GetBitsPerPixel(bitmapData.PixelFormat);
            byte* scanZero = (byte*)bitmapData.Scan0.ToPointer();
            byte* pixel;

            int currentElemY = 0;
            int currentElemX = 0;

            List<ItemPixel> abs = new List<ItemPixel>();
            List<ItemReference> temporaryResult = new List<ItemReference>();
            List<List<ItemReference>> temporaryResultEnd = new List<List<ItemReference>>();
            HashSet<string> arrayVisit = new HashSet<string>();
            HashSet<string> checkingVisited = new HashSet<string>();
            Dictionary<byte, int> colorsLayout = new Dictionary<byte, int>();


            byte counter = 0;

            //Цикл поиска
            int stepConvolutionWidth = 6;
            int stepConvolutionHeight = 6;

            int mainHeight = bitmapData.Height;
            int mainWidth = bitmapData.Width;

            int colorCounter = 0;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < mainHeight; i++)
            {
                for (int j = 0; j < mainWidth; j++)
                {
                    pixel = scanZero + i * bitmapData.Stride + j * bitsPerPixel / 8;
                    var gray = (byte)(.299 * pixel[2] + .587 * pixel[1] + .114 * pixel[0]);

                    if (!colorsLayout.ContainsKey(gray)) {
                        colorsLayout.Add(gray, 1);
                        colorCounter++;
                    } else
                    {
                        colorsLayout[gray]++;
                        colorCounter++;
                    }
                }
            }

            foreach(var color in colorsLayout.ToArray())
            {
                if((double)color.Value/(double)colorCounter*100d < 5)
                {
                    colorsLayout.Remove(color.Key);
                }
            }


            for (int i = 0; i < mainHeight - 6; i += stepConvolutionHeight)
            {
                for (int j = 0; j < mainWidth - 6; j += stepConvolutionWidth)
                {

                    arrayVisit.Clear();
                    abs = new List<ItemPixel>();
                    temporaryResult = new List<ItemReference>();
                    counter = 0;

                    /*if((bitmapData.Width - j) < 7)
                    {
                        stepConvolutionWidth = bitmapData.Width % 6 == 0 ? 6 : 6 - (bitmapData.Width % 6);
                    } if((bitmapData.Height - i) < 7)
                    {
                        stepConvolutionHeight = bitmapData.Height % 6 == 0 ? 6 : 6 - (bitmapData.Height % 6);
                    }*/

                    List<int> checkingColors = new List<int>();
                    bool is_allowance = true;

                    for (int y = i; y <= i + stepConvolutionHeight; y++)
                    {
                        for (int x = j; x <= j + stepConvolutionWidth; x++)
                        {
                            pixel = scanZero + y * bitmapData.Stride + x * bitsPerPixel / 8;

                            var gray = (byte)(.299 * pixel[2] + .587 * pixel[1] + .114 * pixel[0]);
                            //Console.WriteLine(pixel[0] + " i = " + y + " j = " + x);
                            
                            if (!checkingColors.Contains(gray))
                            {
                                checkingColors.Add(gray);
                                if(checkingColors.Count > 1)
                                {
                                    is_allowance = false;
                                    break;
                                }
                            }
                        }
                        if(is_allowance == false)
                        {
                            break;
                        }
                    }
                    

                    if (is_allowance == false)
                    {
                        //Console.WriteLine("i = " + i + " j = " + j);
                        for (int y = i - stepConvolutionHeight; y <= i + stepConvolutionHeight; y += stepConvolutionHeight)
                        {
                            for (int x = j - stepConvolutionWidth; x <= j + stepConvolutionWidth; x += stepConvolutionWidth)
                            {
                                if (0 <= y && y < mainHeight && 0 <= x && x < mainWidth)
                                {
                                    //Console.WriteLine("y !!= " + y + " x !!= " + x + " mainHeight = " + mainHeight);
                                    if (y == i && x == j)
                                    {
                                        currentElemY = y;
                                        currentElemX = x;

                                    }

                                    repeatCircle(y, x);
                                }
                            }
                        }

                        void repeatCircle(int y, int x)
                        {
                            abs.Clear();
                            temporaryResult = new List<ItemReference>();
                            counter = 0;
                            //Проход по циклу с шагом 6 
                            for (int r = y; r < (y + stepConvolutionHeight); r++)
                            {
                                for (int c = x; c < (x + stepConvolutionWidth); c++)
                                {
                                    if (0 <= r && r < mainHeight && 0 <= c && c < mainWidth)
                                    {
                                        pixel = scanZero + r * bitmapData.Stride + c * bitsPerPixel / 8;

                                        ItemPixel startPoint = new ItemPixel();
                                        startPoint.id = $"{r}|{c}";
                                        startPoint.color = pixel[0];
                                        startPoint.children = new List<ItemPixel>();
                                        startPoint.specialChildren = new List<string>();
                                        startPoint.index = counter;
                                        startPoint.position = new Point(c, r);
                                        counter++;

                                        //var gray = (byte)(.299 * pixel[2] + .587 * pixel[1] + .114 * pixel[0]);
                                        //data_copy[c, r] = gray;
                                        //Console.WriteLine(r + " __ " + c + " | " + data_copy[r, c]);

                                        //if (pixel[0] != 255 && pixel[1] != 255 && pixel[2] != 255)
                                        //{
                                        //Console.WriteLine(r + " __ " + c + " | " + data_copy[r, c]);
                                        //}


                                        for (int yt = r - 1; yt <= r + 1; yt++)
                                        {
                                            for (int xt = c - 1; xt <= c + 1; xt++)
                                            {
                                                if (0 <= yt && yt < bitmapData.Height && 0 <= xt && xt < bitmapData.Width && (yt != r || xt != c))
                                                {
                                                    pixel = scanZero + yt * bitmapData.Stride + xt * bitsPerPixel / 8;

                                                    if (startPoint.color == pixel[0])
                                                    {
                                                        if (yt < y || yt >= (y + MyGlobals.g_list_count_more) || xt < x || xt >= (x + MyGlobals.g_list_count_more))
                                                        {
                                                            startPoint.specialChildren.Add($"{yt}|{xt}");
                                                        }
                                                        else
                                                        {
                                                            ItemPixel childremnItem = new ItemPixel();
                                                            childremnItem.id = $"{yt}|{xt}";
                                                            childremnItem.specialChildren = new List<string>();

                                                            startPoint.children.Add(childremnItem);
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                        abs.Add(startPoint);
                                        _fullABS.Add(startPoint);
                                    }
                                }
                            }
                            //BFS                            
                            for (int a = 0; a < abs.Count; a++)//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! если abs 36 то нет нужды в переборе и чистке, можно жлбавить поле к общему списку
                            {
                                if (!arrayVisit.Contains(abs[a].id))
                                {
                                    arrayVisit.Add(abs[a].id);
                                    List<ItemPixel> resultStride = new List<ItemPixel>();
                                    List<ItemPixel> queue = new List<ItemPixel>();

                                    ItemPixel s = abs[a];
                                    queue.Add(s);
                                    resultStride.Add(s);
                                    s.index_reference = _reference_count;

                                    for (int spc = 0; spc < s.specialChildren.Count; spc++)
                                    {
                                        if (!_all_reference_elems.Contains(s.specialChildren[spc]))
                                        {
                                            _all_reference_elems.Add(s.specialChildren[spc]);
                                        }
                                    }

                                    checkingVisited.Add(s.id);
                                    while (queue.Count > 0)
                                    {
                                        int removeIndex = queue[0].index;
                                        queue.RemoveAt(0);

                                        foreach (var neighbor in abs[removeIndex].children)
                                        {
                                            ItemPixel currentPixel = abs.Find(item => item.id == neighbor.id);
                                            if (currentPixel.specialChildren != null)
                                            {
                                                if (!checkingVisited.Contains(currentPixel.id))
                                                {
                                                    for (int spc = 0; spc < currentPixel.specialChildren.Count; spc++)
                                                    {
                                                        if (!_all_reference_elems.Contains(currentPixel.specialChildren[spc]))
                                                        {
                                                            _all_reference_elems.Add(currentPixel.specialChildren[spc]);
                                                        }
                                                    }
                                                    //Console.WriteLine(currentPixel.position);
                                                    queue.Add(currentPixel);
                                                    currentPixel.index_reference = _reference_count;
                                                    arrayVisit.Add(neighbor.id);
                                                    checkingVisited.Add(currentPixel.id);
                                                    resultStride.Add(currentPixel);
                                                }
                                            }

                                        }
                                    }
                                    temporaryResult.Add(new ItemReference(_all_reference_elems, resultStride, _reference_count));
                                    _all_reference_elems.Clear();
                                    _reference_count++;

                                    //queue = null;
                                    //resultStride = null;
                                }
                            }
                            if (temporaryResult.Count > 0)
                            {
                                temporaryResultEnd.Add(temporaryResult);
                            }
                        }

                        //CLEAR
                        //Clear отменится если 36 элементов одинаковые 
                        bool is_flag = false;
                        for (int item = 0; item < temporaryResultEnd.Count; item++)//Иногда count 36 
                        {
                            if ($"{currentElemY}|{currentElemX}" == temporaryResultEnd[item][0].list[0].id)
                            {
                                //Console.WriteLine("current = " + currentElemY + " _ " + currentElemX);
                                for (int ic = 0; ic < temporaryResultEnd[item].Count; ic++)
                                {
                                    if (temporaryResultEnd[item].Count > 1)
                                    {
                                        Console.WriteLine(temporaryResultEnd);
                                        if (temporaryResultEnd[item][ic].list.Count < MyGlobals.g_list_count_more)
                                        {

                                            bool is_cleaned = true;
                                            if (temporaryResultEnd[item][ic].refs.Count > 0)
                                            {
                                                ItemPixel elemArr = temporaryResultEnd[item][ic].list[0];

                                                int counterLength = temporaryResultEnd[item][ic].list.Count;
                                                List<int> checkingLength = new List<int>();


                                                for (int jc = 0; jc < temporaryResultEnd[item][ic].refs.Count; jc++)
                                                {
                                                    // console.log(temporaryResultEnd[item][i].refs[j])
                                                    int finding = _fullABS.Find(elem => elem.id == temporaryResultEnd[item][ic].refs[jc]).index_reference;
                                                   
                                                    bool neewerFlag = false;
                                                    if (!checkingLength.Contains(finding))
                                                    {
                                                        for (int n = 0; n < temporaryResultEnd.Count; n++)
                                                        {
                                                            for (int m = 0; m < temporaryResultEnd[n].Count; m++)
                                                            {
                                                                if (temporaryResultEnd[n][m].index == finding)
                                                                {
                                                                    checkingLength.Add(temporaryResultEnd[n][m].index);

                                                                    counterLength += temporaryResultEnd[n][m].list.Count;
                                                                    neewerFlag = true;

                                                                    break;
                                                                }
                                                            }
                                                            if (neewerFlag)
                                                            {
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (counterLength > 6)
                                                {
                                                    is_cleaned = false;
                                                    break;
                                                }
                                                // console.log(elemArr.id + " " + counterLength)
                                            }

                                            if (is_cleaned)
                                            {
                                                for (int jc = 0; jc < temporaryResultEnd[item][ic].list.Count; jc++)
                                                {
                                                    ItemPixel elem = temporaryResultEnd[item][ic].list[jc];
                                                    int posY = Convert.ToInt32(elem.id.Split('|')[0]);
                                                    int posX = Convert.ToInt32(elem.id.Split('|')[1]);
                                                    List<byte> repeatableValueList = new List<byte>();
                                                    //Console.WriteLine(elem.color + " " + elem.id);

                                                    /*for (int y = posY - 1; y <= posY + 1; ++y)//На какой пиксель из 8 соседних заменить текущий
                                                    {
                                                        for (int x = posX - 1; x <= posX + 1; ++x)
                                                        {
                                                            pixel = scanZero + y * bitmapData.Stride + x * bitsPerPixel / 8;

                                                            if (0 <= y && y < bitmapData.Height && 0 <= x && x < bitmapData.Width && (y != posY || x != posX))
                                                            {
                                                                //8 соседей
                                                                if (pixel[0] != elem.color)
                                                                {
                                                                    if(!repeatableValueList.Contains(pixel[0]))
                                                                    {
                                                                        repeatableValueList.Add(pixel[0]);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }*/
                                                    //Console.WriteLine(repeatableValueList.Count);
                                                    /*if(repeatableValueList.Count > 1)
                                                    {
                                                        data_copy[posX, posY] = repeatableValueList.OrderBy(x => Math.Abs(x - data_copy[posX, posY])).First();
                                                    } 
                                                    else if(repeatableValueList.Count == 1) 
                                                    {
                                                        data_copy[posX, posY] = repeatableValueList[0];
                                                    }
                                                    repeatableValueList.Clear();*/

                                                    foreach(var newColor in colorsLayout)
                                                    {
                                                        repeatableValueList.Add(newColor.Key);
                                                    }
                                                    data_copy[posX, posY] = repeatableValueList.OrderBy(x => Math.Abs(x - elem.color)).First();
                                                    repeatableValueList.Clear();
                                                }
                                            }
                                        }
                                    }
                                }
                                is_flag = true;
                                break;
                            }
                            if (is_flag)
                            {
                                break;
                            }
                            
                        }
                        temporaryResultEnd.Clear();
                        _fullABS.Clear();
                    }
                }
            }
            _all_reference_elems = null;
            _fullABS = null;
            temporaryResult = null;
            //arrayVisit.Clear();
            Console.WriteLine("___");
            GC.Collect();


            _currentBitmap.UnlockBits(bitmapData);

            BitmapData bData = _currentBitmap.LockBits(new Rectangle(0, 0, _currentBitmap.Width, _currentBitmap.Height), ImageLockMode.ReadWrite, _currentBitmap.PixelFormat);
            bitsPerPixel = GetBitsPerPixel(bData.PixelFormat);
            byte* scan0 = (byte*)bData.Scan0.ToPointer();

            byte* data;
            for (int i = 0; i < bData.Height; ++i)
            {
                for (int j = 0; j < bData.Width; ++j)
                {
                    data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;

                    if (bitsPerPixel >= 24 && data_copy[j, i] != 0)
                    {
                        data[0] = (byte)data_copy[j, i];
                        data[1] = (byte)data_copy[j, i]; 
                        data[2] = (byte)data_copy[j, i]; 
                    }

                }
            }

            _currentBitmap.UnlockBits(bData);

            sw.Stop();
            string elapsed = sw.Elapsed.ToString();
            Console.WriteLine("Done after: " + sw.Elapsed);
            return _currentBitmap;
        }

        /// <summary>
        /// Makes the current bitmap grayscale
        /// </summary>
        public unsafe void SetGrayscale()
        {
            if (CurrentBitmap == null || isGrayscale) return;

            BitmapData bData = _currentBitmap.LockBits(new Rectangle(0, 0, _currentBitmap.Width, _currentBitmap.Height), ImageLockMode.ReadWrite, _currentBitmap.PixelFormat);
            bitsPerPixel = GetBitsPerPixel(bData.PixelFormat);
            byte* scan0 = (byte*)bData.Scan0.ToPointer();

            //data_copy = new double[_currentBitmap.Width, _currentBitmap.Height];

            byte* data;
            for (int i = 0; i < bData.Height; ++i)
            {
                for (int j = 0; j < bData.Width; ++j)
                {
                    data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;

                    if (bitsPerPixel >= 24)
                    {
                        var gray = (byte)(.299 * data[2] + .587 * data[1] + .114 * data[0]);

                        data[0] = gray;
                        data[1] = gray;
                        data[2] = gray;
                        //data is a pointer to the first byte of the 3-byte color data    
                    }

                }
            }

            _currentBitmap.UnlockBits(bData);
            isGrayscale = true;
        }

        /// <summary>
        /// Returns the normalized version of original bitmap
        /// </summary>
        /// <returns>Matrix of doubles between 0s and 1s</returns>
        public unsafe double[,] GetNormalizedMatrix()
        {
            if (_currentBitmap == null) return null;

            BitmapData bData = _currentBitmap.LockBits(new Rectangle(0, 0, _currentBitmap.Width, _currentBitmap.Height), ImageLockMode.ReadWrite, _currentBitmap.PixelFormat);
            bitsPerPixel = GetBitsPerPixel(bData.PixelFormat);
            byte* scan0 = (byte*)bData.Scan0.ToPointer();

            var normalizedMatrix = new double[_currentBitmap.Width, _currentBitmap.Height];//new double[_currentBitmap.Width, _currentBitmap.Height];

            byte* data;
            for (int i = 0; i < bData.Height; ++i)
            {
                for (int j = 0; j < bData.Width; ++j)
                {
                    data = scan0 + i * bData.Stride + j * bitsPerPixel / 8; // * bitsPerPixel / 8
                    normalizedMatrix[j, i] = data[0] / 255d;
                    
                    //data is a pointer to the first byte of the 3-byte color data
                }
            }

            _currentBitmap.UnlockBits(bData);

            return normalizedMatrix;
        }

        /// <summary>
        /// Fills the current bitmap with denormalized values
        /// from passed matrix.
        /// Passed matrix consists only of 0s and 1s.
        /// </summary>
        /// <param name="norm">Matrix with values between 0 and 1</param>
        public unsafe void DenormalizeCurrent(double[,] norm, List<Point> dataCoordinates)
        {
            List<Point> style_dataCoordinates = new List<Point>();

            if (norm == null) return;
            int n = norm.GetLength(0);
            int m = norm.GetLength(1);

            if (m != _currentBitmap.Height || n != _currentBitmap.Width)
            {
                throw new Exception("Sizes don't match.");
            }


            BitmapData bData = _currentBitmap.LockBits(new Rectangle(0, 0, _currentBitmap.Width, _currentBitmap.Height), ImageLockMode.ReadWrite, _currentBitmap.PixelFormat);
            bitsPerPixel = GetBitsPerPixel(bData.PixelFormat);
            byte* scan0 = (byte*)bData.Scan0.ToPointer();

            byte* data;
            for (int i = 0; i < bData.Height; ++i)
            {
                for (int j = 0; j < bData.Width; ++j)
                {
                    data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;

                    byte newCol = norm[j, i] == 0 ? (byte)0 : (byte)255;
                    if (norm[j, i] != 0 && i != bData.Height - 1 && i != 0)
                    {
                        dataCoordinates.Add(new Point(j, i + MyGlobals.keycount));
                        style_dataCoordinates.Add(new Point(j, i));
                    }
                    if (bitsPerPixel >= 24)
                    {
                        data[0] = newCol;
                        data[1] = newCol;
                        data[2] = newCol;
                        //data is a pointer to the first byte of the 3-byte color data
                    }
                    else
                    {
                        data[0] = newCol;
                    }
                }
            }

            _currentBitmap.UnlockBits(bData);
        }


        public void CleanUp()
        {
            _currentBitmap = null;
            _originalBitmap = null;
            isGrayscale = false;
            bitsPerPixel = 0;
            GC.Collect();
        }

    }
    class ItemReference
    {
        public List<string> refs { get; set; }
        public List<ItemPixel> list { get; set; }
        public int index { get; set; }

        public ItemReference(List<string> refs, List<ItemPixel> list, int index)
        {
            this.refs = refs;
            this.list = list;
            this.index = index;
        }
    }


    struct ItemPixel
    {
        public Point position { get; set; }
        //public bool visited { get; set; }
        public int index { get; set; }
        public byte color { get; set; }
        public List<ItemPixel> children { get; set; }
        public int index_reference { get; set; }
        public List<string> specialChildren { get; set; }
        public string id { get; set; }

        public ItemPixel(string id, byte color, List<ItemPixel> children, List<string> specialChildren, int index_reference, int index, Point position)
        {
            this.id = id;
            this.specialChildren = specialChildren;
            this.color = color;
            this.children = children;
            this.index_reference = index_reference;
            this.index = index;
            this.position = position;
        }
    }
}