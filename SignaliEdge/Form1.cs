using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.UI;

namespace SignaliEdge
{
    public partial class Form1 : Form
    {
        public static int key = 0;
        private Mat mainModelImage = null;
        private Image<Bgr, byte> inputImage;
        private Image<Bgr, byte> TakeImage;
        private Image<Gray, byte> GrayImage;
        private List<Point> dataCoordinates = new List<Point>();

        private List<Point> dataCoordinates2 = new List<Point>();
        Dictionary<int, ValuesDictionary> BlocksDictionary = new Dictionary<int, ValuesDictionary>();
        Dictionary<int, PropertyCSS> BlocksDictionaryCSS = new Dictionary<int, PropertyCSS>();
        Dictionary<int, TextDictionary> textDictionary = new Dictionary<int, TextDictionary>();

        readonly ImageHandler imageHandler = new ImageHandler();
        readonly Detector detector = new Detector();
        readonly Creator creator = new Creator();
        readonly ImageComparison compration = new ImageComparison();
        readonly HTMLDOM hTMLDOM = new HTMLDOM();
        readonly RenderStyles renderer = new RenderStyles();
        readonly TextRecognition textRecognition = new TextRecognition();
        readonly CreateHTML createHTML = new CreateHTML();

        public Form1()
        {
            InitializeComponent();
            fdIzborSlike = new OpenFileDialog {RestoreDirectory = true, FilterIndex = 1}; 
            //fdIzborSlike.Filter = "jpg Files (*.jpg)|*.jpg|gif Files (*.gif)|*.gif|png Files (*.png)|*.png |bmp Files (*.bmp)|*.bmp";

            double lower = 0, upper = 0;
            trbLower.Value = 0;
            trbUpper.Value = 1;
            lblLowerTreshold.Text = lower.ToString();
            lblUpperTreshold.Text = upper.ToString();
            detector.LowerTreshold = lower;
            detector.UpperTreshold = upper;
            detector.MaxPrecision = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void btnFileDialog_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK != fdIzborSlike.ShowDialog()) return;
            pbSlika.Image = null;
            pbSlikaOriginal.Image = null;
            mainModelImage = CvInvoke.Imread(fdIzborSlike.FileName, ImreadModes.Grayscale);

            if (imageHandler.CurrentBitmap != null) imageHandler.CurrentBitmap.Dispose();
            if (imageHandler.OriginalBitmap != null) imageHandler.OriginalBitmap.Dispose();


            imageHandler.StartBitmap = (Bitmap)Image.FromFile(fdIzborSlike.FileName);
            //imageHandler.SetNewImage();
            imageHandler.CurrentBitmap = imageHandler.StartBitmap;
            imageHandler.OriginalBitmap = imageHandler.StartBitmap;

            inputImage = new Image<Bgr, byte>(fdIzborSlike.FileName);
            imageHandler.BitmapPath = fdIzborSlike.FileName;
            GrayImage = inputImage.Convert<Gray, Byte>();


            pbSlikaOriginal.Image = inputImage.Bitmap; //imageHandler.OriginalBitmap -  resultImage.Bitmap
            lblImageResolution.Text = pbSlikaOriginal.Image.Width.ToString() + "x" + pbSlikaOriginal.Image.Height.ToString();
            lblImageSize.Text = Math.Round((new FileInfo(fdIzborSlike.FileName).Length/1000000.0), 2).ToString() + "MB";
        }

        private void trbLower_Scroll(object sender, EventArgs e)
        {
            var v = trbLower.Value;
            detector.LowerTreshold = v;
            lblLowerTreshold.Text = v.ToString();
        }

        private void trbUpper_Scroll(object sender, EventArgs e)
        {
            var v = trbUpper.Value;
            detector.UpperTreshold = v;
            lblUpperTreshold.Text = v.ToString();
        }

        private void btnDetect_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            lblLastDetection.Text = "Working...";

            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                //______________________________________________________________________ПРОВЕДИ РЕФАКТОРИНГ КОДА
                double[,] n, slika;
                 

                n = imageHandler.GetNormalizedMatrix();
                slika = detector.Detection(n, trbPrecision.Value);
                imageHandler.DenormalizeCurrent(slika, dataCoordinates);
                detector.ContoursList(dataCoordinates);
                BlocksDictionary = creator.shapeCenter(detector.currentListDictionary);
                BlocksDictionary = creator.FilterDictionary(BlocksDictionary, GrayImage.Bitmap);
                //hTMLDOM.HTMLCompletion(BlocksDictionary);
                //textDictionary = textRecognition.RecognizerText(BlocksDictionary, inputImage, fdIzborSlike.FileName);

                //Renders
                createHTML.CreateDOM(BlocksDictionary);
                renderer.RenderCSS(BlocksDictionary);
                try
                {
                    foreach (var elem in BlocksDictionary.Values)
                    {
                        Point[] rect = new Point[]
                        {
                            new Point(elem.PointsArea[0], elem.PointsArea[1]),
                            new Point(elem.PointsArea[2], elem.PointsArea[3]),
                            new Point(elem.PointsArea[4], elem.PointsArea[5]),
                            new Point(elem.PointsArea[6], elem.PointsArea[7]),

                        };
                        using (VectorOfPoint vp = new VectorOfPoint(rect))
                        {
                            CvInvoke.Polylines(inputImage, vp, true, new MCvScalar(201, 25, 101, 255), 20);
                        }

                    }
                    foreach (var elem in textDictionary.Values)
                    {
                        Point[] Text = new Point[]
                        {
                                new Point(elem.TextPoints[0], elem.TextPoints[1]),
                                new Point(elem.TextPoints[0] + elem.width, elem.TextPoints[1]),
                                new Point(elem.TextPoints[0] + elem.width, elem.TextPoints[1] + elem.height),
                                new Point(elem.TextPoints[0], elem.TextPoints[1] + elem.height),

                        };
                        using (VectorOfPoint vm = new VectorOfPoint(Text))
                        {
                            CvInvoke.Polylines(inputImage, vm, true, new MCvScalar(0, 136, 248, 255), 15);
                        }
                    }
                    /*foreach (var item in dataCoordinates2)
                    {
                        inputImage[item.Y, item.X] = new Bgr(0, 0, 255);
                    }*/
                } catch
                {

                }
                n = null;
                slika = null;
                //Y ___ X
                
                sw.Stop();
                string elapsed = sw.Elapsed.ToString();
                lblLastDetection.Text = elapsed.Substring(0, 11);
                Console.WriteLine("Done after: " + sw.Elapsed);

                detector.CleanUp();
                GC.Collect();

                //pbSlika.Image = imageHandler.CurrentBitmap;
                //pbSlika.Image = inputImage.Bitmap;
            }
            catch (OutOfMemoryException)
            {
                pbSlika.Image = null; pbSlika.Dispose();
                pbSlikaOriginal.Image = null; pbSlikaOriginal.Dispose();
                imageHandler.CleanUp();
                detector.CleanUp();
                lblLastDetection.Text = "0";
                MessageBox.Show("The image you choose is too big. Please choose a smaller image and try again.");
            }

            Cursor = Cursors.Default;
        }

        private void cbCalcTresholds_CheckedChanged(object sender, EventArgs e)
        {
            detector.LowerTreshold = 0;
            detector.UpperTreshold = 0;

            trbUpper.Value = 0;
            trbLower.Value = 0;

            lblLowerTreshold.Text = "0.0";
            lblUpperTreshold.Text = "0.0";

            trbLower.Enabled = !trbLower.Enabled;
            trbUpper.Enabled = !trbUpper.Enabled;
        }

        private void cbMaxPrecision_CheckedChanged(object sender, EventArgs e)
        {
            trbPrecision.Value = 1;
            lblPrecision.Text = "1";
            trbPrecision.Enabled = !trbPrecision.Enabled;
            detector.MaxPrecision = !detector.MaxPrecision;
        }

        private void trbPrecision_Scroll(object sender, EventArgs e)
        {
            lblPrecision.Text = trbPrecision.Value.ToString();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK != fdIzborSlike.ShowDialog()) return;
            TakeImage = new Image<Bgr, byte>(fdIzborSlike.FileName);

            var resultImage = compration.FASTDetector(TakeImage);

            pictureBox1.Image = resultImage.Bitmap;

            long score;
            long matchTime;

            using (Mat observedImage = CvInvoke.Imread(fdIzborSlike.FileName, ImreadModes.Grayscale))
            {
                Mat result = compration.Draw(observedImage, mainModelImage, out matchTime, out score);
                pbSlikaOriginal.Image = result.Bitmap; //imageHandler.OriginalBitmap -  resultImage.Bitmap
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Tesseract tesseract = new Tesseract(@"C:\123\data", "eng", OcrEngineMode.TesseractLstmCombined);
            tesseract.SetImage(inputImage);
            tesseract.Recognize();
            Console.WriteLine(tesseract.GetUTF8Text());
            tesseract.Dispose();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            if (checkBox.Checked == true)
            {
                pbSlika.Image = imageHandler.CurrentBitmap;
            } else
            {
                pbSlika.Image = inputImage.Bitmap;
            }
        }
    }

    public static class MyGlobals
    {
        public static int g_counterKey = 0;

        //Settings
        public static int g_length_Line_Rectangle = 50;
        public static int g_distance_Abyss = 3;
    }
}
