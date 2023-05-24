using System;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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

            data_copy = new double[_currentBitmap.Width, _currentBitmap.Height];

            byte* PtrFirstPixel = (byte*)bitmapData.Scan0.ToPointer();
            byte* pixel;


            List<ItemPixel> abs = new List<ItemPixel>();
            List<ItemReference> temporaryResult = new List<ItemReference>();
            List<List<ItemReference>> temporaryResultEnd = new List<List<ItemReference>>();
            List<string> arrayVisit = new List<string>();
            byte counter = 0;

            //Цикл поиска
            int stepConvolutionWidth = 6;
            int stepConvolutionHeight = 6;

            int mainHeight = bitmapData.Height;
            int mainWidth = bitmapData.Width;

            for (int i = 0; i < mainHeight; i += stepConvolutionHeight)
            {
                for (int j = 0; j < mainWidth; j += stepConvolutionWidth)
                {
                    arrayVisit.Clear();
                    abs.Clear();
                    temporaryResult.Clear();
                    counter = 0;
                    int height = (j + 6);
                    int width = (i + 6);

                    stepConvolutionWidth = bitmapData.Width - j < 6 ? bitmapData.Width - j : 6;
                    stepConvolutionHeight = bitmapData.Height - i < 6 ? bitmapData.Height - i : 6;

                    //Проход по циклу с шагом 6
                    for (int r = i; r < (i + stepConvolutionHeight); r++)
                    {
                        for (int c = j; c < (j + stepConvolutionWidth); c++)
                        {
                            pixel = PtrFirstPixel + r * bitmapData.Stride + c * bitsPerPixel / 8;

                            ItemPixel startPoint = new ItemPixel();
                            startPoint.id = $"{r}|{c}";
                            startPoint.color = pixel[0];
                            startPoint.children = new List<ItemPixel>();
                            startPoint.specialChildren = new List<string>();
                            startPoint.index = counter;
                            counter++;

                            for (int y = r - 1; y <= r + 1; ++y)
                            {
                                for (int x = c - 1; x <= c + 1; ++x)
                                {
                                    if (0 <= y && y < width && 0 <= x && x < height && (y != r || x != c))
                                    {
                                        byte* currentPixel = PtrFirstPixel + y * bitmapData.Stride + x * bitsPerPixel / 8;
                                        if (startPoint.color == currentPixel[0])
                                        {
                                            if (y < i || y >= (i + 6) || x < j || x >= (j + 6))
                                            {
                                                startPoint.specialChildren.Add($"{y}|{x}");
                                            }
                                            else
                                            {
                                                ItemPixel childremnItem = new ItemPixel();
                                                childremnItem.id = $"{y}|{x}";
                                                childremnItem.specialChildren = new List<string>();

                                                startPoint.children.Add(childremnItem);
                                            }
                                            //Можно улучшить код если не создавать новый ItemPixel а ссылаться на старые
                                        }
                                    }
                                }
                            }
                            abs.Add(startPoint);
                            _fullABS.Add(startPoint);
                        }
                    }

                    //BFS
                    HashSet<string> checkingVisited = new HashSet<string>();
                    for (int a = 0; a < abs.Count; a++)
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
                            temporaryResult.Add(new ItemReference(_all_reference_elems, resultStride, _reference_count));
                            _all_reference_elems.Clear();
                            _reference_count++;

                            queue = null;
                            resultStride = null;
                        }
                    }
                    temporaryResultEnd.Add(temporaryResult);
                }
            }
            _all_reference_elems = null;
            temporaryResult = null;
            GC.Collect();

            //Start clear
            for (int item = 0; item < temporaryResultEnd.Count; item++)
            {
                for (int i = 0; i < temporaryResultEnd[item].Count; i++)
                {
                    if (temporaryResultEnd[item].Count != 1)
                    {
                        if (temporaryResultEnd[item][i].list.Count < MyGlobals.g_list_count_more)
                        {
                            if (temporaryResultEnd[item][i].refs.Count > 0)
                            {
                                int counter_list_length = temporaryResultEnd[item][i].list.Count;
                                List<int> checking_length = new List<int>();

                                for (int j = 0; j < temporaryResultEnd[item][i].refs.Count; j++)
                                {
                                    int found = _fullABS.Find(elem => elem.id == temporaryResultEnd[item][i].refs[j]).index_reference;
                                    if (!checking_length.Contains(found))
                                    {
                                        for (int n = 0; n < temporaryResultEnd.Count; n++)
                                        {
                                            for (int m = 0; m < temporaryResultEnd[n].Count; m++)
                                            {
                                                if (temporaryResultEnd[n][m].index == found)
                                                {
                                                    checking_length.Add(found);
                                                    counter_list_length += temporaryResultEnd[n][m].list.Count;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (counter_list_length < MyGlobals.g_list_count_more)
                                {
                                    //Очистка
                                }
                            }
                        }
                    }
                }
            }

            _currentBitmap.UnlockBits(bitmapData);

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

            Console.WriteLine(_originalBitmap.Width + " " + _currentBitmap.Height);
            Console.WriteLine(data_copy);
            var normalizedMatrix = new double[_currentBitmap.Width, _originalBitmap.Height];

            byte* data;
            for (int i = 0; i < bData.Height; ++i)
            {
                for (int j = 0; j < bData.Width; ++j)
                {
                    data = scan0 + i * bData.Stride + j * bitsPerPixel / 8; // * bitsPerPixel / 8
                    normalizedMatrix[j, i] = (data[0] / 255d);
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
        //public Point position { get; set; }
        //public bool visited { get; set; }
        public int index { get; set; }
        public byte color { get; set; }
        public List<ItemPixel> children { get; set; }
        public int index_reference { get; set; }
        public List<string> specialChildren { get; set; }
        public string id { get; set; }

        public ItemPixel(string id, byte color, List<ItemPixel> children, int index, int index_reference, List<string> specialChildren)
        {
            //this.position = position;
            this.id = id;
            this.color = color;
            this.children = children;
            //this.visited = visited;
            this.specialChildren = specialChildren;
            this.index = index;
            this.index_reference = index_reference;
        }
    }
}