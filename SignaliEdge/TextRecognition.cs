using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;

using Emgu.CV;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

using IronOcr;
using IronSoftware.Drawing;

namespace SignaliEdge
{
    class TextRecognition
    {
        private Dictionary<int, TextDictionary> _textDictionary = new Dictionary<int, TextDictionary>();
        private Dictionary<Point, List<TextDictionary>> _textDictionaryCopy = new Dictionary<Point, List<TextDictionary>>();
        private Dictionary<int, ValuesDictionary> _BlocksDictionaryCopy = new Dictionary<int, ValuesDictionary>();
        private HashSet<int> _CheckingID = new HashSet<int>();

        //Settings Text
        IronTesseract Ocr = new IronTesseract();

        public Dictionary<int, TextDictionary> RecognizerText(Dictionary<int, ValuesDictionary> BlocksDictionary, Image<Bgr, byte> inputImage, string Path)
        {
            Ocr.Language = OcrLanguage.Russian;
            Ocr.AddSecondaryLanguage(OcrLanguage.EnglishBest);

            //перебирает все блоки 

            foreach(var elem in BlocksDictionary.Values.ToArray())
            {
                if (elem.FirstChild == null)
                    continue;

                ascent(elem);
                void ascent(ValuesDictionary item)
                {
                    foreach (var child in item.FirstChild.ToArray())
                    {
                        if(!_CheckingID.Contains(child))
                        {
                            if (BlocksDictionary[child].FirstChild.Count == 1)
                            {
                                ascent(BlocksDictionary[child]);
                                _CheckingID.Add(BlocksDictionary[child].ID);
                                DubleRecognize(inputImage, BlocksDictionary[child]);
                            } else if(BlocksDictionary[child].FirstChild.Count > 1)
                            {
                                for (int i = 0; i <= BlocksDictionary[child].FirstChild.Count; i++)
                                {
                                    ascent(BlocksDictionary[child]);
                                    _CheckingID.Add(BlocksDictionary[child].ID);
                                    DubleRecognize(inputImage, BlocksDictionary[child]);
                                }
                                _CheckingID.Add(BlocksDictionary[child].ID);
                                DubleRecognize(inputImage, BlocksDictionary[child]);
                            } else
                            {
                                _CheckingID.Add(BlocksDictionary[child].ID);
                                DubleRecognize(inputImage, BlocksDictionary[child]);

                            }
                        }
                    }
                    if(!_CheckingID.Contains(item.ID))
                    { 
                        _CheckingID.Add(item.ID);
                        DubleRecognize(inputImage, item);
                    }
                }
            }
            _BlocksDictionaryCopy.ToList().ForEach(x => BlocksDictionary.Add(x.Key, x.Value));
            return _textDictionary;
        }

        private void DubleRecognize(Image<Bgr, byte> inputImage, ValuesDictionary CurrentItem)
        {

            var ContentArea = new Rectangle() { X = CurrentItem.PointsArea[0].X, Y = CurrentItem.PointsArea[0].Y, Height = CurrentItem.height + 1, Width = CurrentItem.width + 1 };
            using (var ocrInput = new OcrInput())
            {
                ocrInput.DeNoise();
                ocrInput.Contrast();
                
                var Result = Ocr.Read(inputImage.Bitmap, ContentArea);
                foreach (var item in Result.Paragraphs)
                {
                    if (!_CheckingID.Contains(MyGlobals.g_counterKey))
                    {
                        _textDictionary.Add(MyGlobals.g_counterKey, new TextDictionary(new List<int>() { item.Location.X, item.Location.Y }, item.Width, item.Height, item.Text));
                        _BlocksDictionaryCopy.Add(MyGlobals.g_counterKey, new ValuesDictionary(false, new List<Point>() {
                            new Point(item.Location.X, item.Location.Y),
                            new Point(item.Location.X + item.Width, item.Location.Y),
                            new Point(item.Location.X + item.Width, item.Location.Y + item.Height),
                            new Point(item.Location.X, item.Location.Y + item.Height),

                        }, MyGlobals.g_counterKey, item.Width, item.Height, $"<p class=\"Text{MyGlobals.g_counterKey}\">{item.Text}</p>", new Dictionary<int, Blocks>() { }, new Dictionary<int, BlocksTextP>() { }, CurrentItem.ID));
                        CurrentItem.FirstChild.Add(MyGlobals.g_counterKey);
                        _CheckingID.Add(MyGlobals.g_counterKey);
                        _BlocksDictionaryCopy[MyGlobals.g_counterKey].FirstChild = new List<int>();
                        MyGlobals.g_counterKey++;
                    }
                }
                for (int h = CurrentItem.PointsArea[0].Y; h <= CurrentItem.PointsArea[2].Y; h++)
                {
                    for (int w = CurrentItem.PointsArea[0].X; w <= CurrentItem.PointsArea[1].X; w++)
                    {
                        try
                        {
                            inputImage[h, w] = new Bgr(255, 255, 255);
                        } catch
                        {

                        }
                        
                    }
                }
            }
        }
    }

    class linePointsText
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public linePointsText(int Height, int Width, int X, int Y)
        {
            this.Height = Height;
            this.Width = Width;
            this.X = X;
            this.Y = Y;
        }
        public linePointsText() { }
    }

    class CheckingWidthDistance 
    {
        public int WidthDistance { get; set; }
        public CheckingWidthDistance(int Width) { WidthDistance = Width; }
    }

    class CheckingHeightDistance
    {
        public int HeightDistance { get; set; }
        public CheckingHeightDistance(int Height) { HeightDistance = Height;}
    }
}
