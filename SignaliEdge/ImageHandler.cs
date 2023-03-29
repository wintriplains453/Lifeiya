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
        private Bitmap _startBitmap;
        private bool isGrayscale = false;
        byte bitsPerPixel;
        private double[,] data_copy;

        private Dictionary<int, CheckData> Checking = new Dictionary<int, CheckData>();
        private int counterPixels = 0;

        /// <summary>
        /// Used to avoid transforming the image to grayscale more than once
        /// </summary>
        public bool IsGrayscale { 
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

        /// <summary>
        /// Change pixels in image
        /// </summary>
        public Bitmap StartBitmap 
        {
            get { return _startBitmap; }
            set { _startBitmap = value; isGrayscale = false; }
        }

        public byte GetBitsPerPixel(PixelFormat pf)
        {
            byte BitsPerPixel;
            switch(pf)       
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
            BitmapData bitmapData = _startBitmap.LockBits(new Rectangle(0, 0, _startBitmap.Width, _startBitmap.Height),
                ImageLockMode.ReadWrite, _startBitmap.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(_startBitmap.PixelFormat) / 8;//Размер пикселя
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;
            int counter = 0;
            List<int> arrSpecial = new List<int>();
            data_copy = new double[_startBitmap.Height, _startBitmap.Width];

            byte* PtrFirstPixel = (byte*)bitmapData.Scan0;
            
            //Цикл поиска
            for (int y = 0; y < bitmapData.Height - 1; y++)
            {
                byte* row = PtrFirstPixel + y * bitmapData.Stride;
                for (int x = 0; x < bitmapData.Width; x++)
                {
                    byte* pixel = row + x * bytesPerPixel;
                    if (!Checking.ContainsKey(pixel[0]))
                    {
                        Checking.Add(pixel[0], new CheckData(counterPixels));
                    }
                    else
                    {
                        foreach (var item in Checking)
                        {
                            if (item.Key == pixel[0])
                            {
                                item.Value.counterPixels += 1;
                            }
                        }

                    }
                }
            }
            var maxValueKey = Checking.OrderByDescending(x => x.Value.counterPixels).FirstOrDefault().Value.counterPixels;
            //Цикл порога
            foreach (var item in Checking.ToArray())
            {
                if (item.Value.counterPixels < 1)//порог
                {
                    Checking.Remove(item.Key);
                }
            }


            //Цикл замены
            for (int y = 0; y < bitmapData.Height; y++)
            {
                byte* row = PtrFirstPixel + y * bitmapData.Stride;
                for (int x = 0; x < bitmapData.Width; x++)
                {
                    byte* pixel = row + x * bytesPerPixel;
                    if (!Checking.ContainsKey(pixel[0]))//проверяет пиксели которых нет в массиве data_copy (эти пиксели нужно заметить теми что в массиве)
                    {
                        if (!arrSpecial.Contains(pixel[0]))
                        {
                            arrSpecial.Add(pixel[0]);
                            counter++;
                        }
                        int max = Checking.Max(v => v.Key);
                        byte temp = 0;
                        foreach (var item in Checking)//перебор всего массива макимальных значений
                        {
                            //Console.WriteLine(item.Key);
                            int current_color = Math.Abs(item.Key - pixel[0]);

                            if (current_color <= max)
                            {
                                //Console.WriteLine(item.Key);
                                temp = (byte)item.Key;
                                max = current_color;
                                //Console.WriteLine("r = " + y + " c = " + x + " result r + c = " + pixel[0] + "    max = " + max);
                            }
                        }
                        data_copy[y,x] = temp;
                    }
                    else
                    {
                        data_copy[y, x] = pixel[0];
                    }
                }
            }
            _startBitmap.UnlockBits(bitmapData);

            /*using (StreamWriter sw = new StreamWriter("C:\\Python\\test.txt", false, System.Text.Encoding.Default))
            {
                for (int i = 0; i < bitmapData.Height - 1; i++)
                {
                    for (int j = 0; j < bitmapData.Width - 1; j++)
                    {
                        if (data_copy[i, j] != 255)
                        {
                            sw.Write(data_copy[i, j]);
                            sw.Write(",");
                        }
                    }
                    sw.WriteLine();
                }
            }*/

            return StartBitmap;

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
        public unsafe double[,] GetNormalizedMatrix(int startY)
        {
            if (_originalBitmap == null) return null;

            BitmapData bData = _originalBitmap.LockBits(new Rectangle(0, startY, _originalBitmap.Width, MyGlobals.g_const_height_img), ImageLockMode.ReadWrite, _originalBitmap.PixelFormat);
            bitsPerPixel = GetBitsPerPixel(bData.PixelFormat);
            byte* scan0 = (byte*)bData.Scan0.ToPointer();

            var normalizedMatrix = new double[_originalBitmap.Width, MyGlobals.g_const_height_img];

            byte* data;
            for (int i = 0; i < bData.Height; ++i)
            {
                for (int j = 0; j < bData.Width; ++j)
                {
                    data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;

                    normalizedMatrix[j, i] = data[0] / 255d;
                    //data is a pointer to the first byte of the 3-byte color data
                }
            }

            _originalBitmap.UnlockBits(bData);

            return normalizedMatrix;
        }

        /// <summary>
        /// Fills the current bitmap with denormalized values
        /// from passed matrix.
        /// Passed matrix consists only of 0s and 1s.
        /// </summary>
        /// <param name="norm">Matrix with values between 0 and 1</param>
        public unsafe void DenormalizeCurrent(double[,] norm, List<Point> dataCoordinates, int startY)
        {
            List<Point> style_dataCoordinates = new List<Point>();

            if (norm == null) return;
            int n = norm.GetLength(0);
            int m = norm.GetLength(1);

            if (m != MyGlobals.g_const_height_img || n != _currentBitmap.Width)
            {
                throw new Exception("Sizes don't match.");
            }


            BitmapData bData = _currentBitmap.LockBits(new Rectangle(0, startY, _currentBitmap.Width, MyGlobals.g_const_height_img), ImageLockMode.ReadWrite, _currentBitmap.PixelFormat);
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

            //MyGlobals.g_dataCoordinate_style.Add(startY, style_dataCoordinates);
            _currentBitmap.UnlockBits(bData);
        }


        public void CleanUp()
        {
            _currentBitmap = null;
            _originalBitmap = null;
            _startBitmap = null;
            isGrayscale = false;
            bitsPerPixel = 0;
            GC.Collect();
        }

    }

    class CheckData {
        public int counterPixels {get; set;}

        public CheckData(int counterPixels)
        {
            this.counterPixels = counterPixels;
        }
    }
}