using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging; 
using static System.Net.Mime.MediaTypeNames;
using Pen = System.Drawing.Pen;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;


namespace ModelOutputImage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string imagesFolder;
        private string csvLabelsFile;
        private List<string> imageFileNames = new List<string>();
        private List<Tuple<string, double, double, double>> annontations = new List<Tuple<string, double, double, double>>(); //item 1 = file name, item 2 = sign or no sign, item 3 = x location, item 4 = y location
        private int imageIndexer = 0;

        public MainWindow()
        {
            InitializeComponent();
        }
       
        /// <summary>
        /// On button press, browse for a folder that contains images for applying labels to
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseImageFolder(object sender, RoutedEventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select a the Folder Containing Processed Images";
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    imagesFolder = folderDialog.SelectedPath;
                    currentPath.Text = imagesFolder;
                }
            }
        }

        /// <summary>
        /// On button press, browse for the csv file contianing annotations for the images selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseCSVLabelFile(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "Open Labels CSV File"
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                csvLabelsFile = openFileDialog.FileName;
                currentLabelFile.Text = csvLabelsFile;
            }
        }

        /// <summary>
        /// Load all image fileNames form the currently selected image folder path
        /// </summary>
        private void LoadImageFileNames(object sender, RoutedEventArgs e)
        {
            if(imagesFolder == string.Empty || imagesFolder == null ||Directory.Exists(imagesFolder) == false)
            {
                System.Windows.MessageBox.Show("Please select an image directory containing .png files via \"Browse for Image Folder\" button");
                return;
            }

            string[] files = Directory.GetFiles(imagesFolder, "*.*");
            imageFileNames.Clear(); // clear previous entires
            imageFileNames.AddRange(files);

            try
            {
                loadCSVData();
            }
            catch (Exception ex) 
            {
                System.Windows.MessageBox.Show("Error loading image annotations. Please check .csv file and format. " + ex.Message);
            }

            imageIndexer = 0;
            loadMainImage();
        }

        /// <summary>
        /// Load csv data from the selected path
        /// </summary>
        private void loadCSVData()
        {
            try
            {
                using (var reader = new StreamReader(csvLabelsFile))
                {
                    // Skip header
                    string headerLine = reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        var values = line.Split(',');
                        Tuple<string, double, double, double> annotation = new Tuple<string, double, double, double>(values[0], Convert.ToDouble(values[1]), double.Parse(values[2]), double.Parse(values[3]));
                        annontations.Add(annotation);
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void loadMainImage()
        {
            BitmapImage bitmap = new BitmapImage(new Uri(imageFileNames[imageIndexer]));
            mainImage.Source = bitmap;
            regionImage.Source = null; 
            currentFileName.Text = imageFileNames[imageIndexer];

            FileInfo fi = new FileInfo(imageFileNames[imageIndexer]);
            searchForAnnotations(fi.Name, bitmap);
        }

        private void searchForAnnotations(string fileName, BitmapImage image)
        {
            foreach(Tuple<string, double, double, double> annotation in annontations)
            {
                if(annotation.Item1 == fileName)
                {
                    if (annotation.Item3 > 0.000001 || annotation.Item4 > 0.000001) // do not draw when both items are 0
                    {
                        drawDataOnImage(annotation, image);
                    }
                    else
                    {
                        emptyAnnotationText.Visibility = Visibility.Visible;
                        regionImage.Visibility -= Visibility.Hidden; 
                    }
                }
            }
        }
        private void drawDataOnImage(Tuple<string, double, double, double> imageData, BitmapImage bitmap)
        {
            // Convert annotation points to points in image space
            int x = Convert.ToInt32(imageData.Item3 * 512);
            int y = Convert.ToInt32(imageData.Item4 * 512);

            // Convert BitmapImage to Bitmap
            Bitmap bmp;
            using (var stream = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
                bmp = new Bitmap(stream);
            }

            //Draw on image
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                using (Pen redPen = new Pen(Color.Cyan, 2))
                {
                    // Draw an "X" at the specified coordinates
                    graphics.DrawLine(redPen, x - 5, y - 5, x + 5, y + 5); // First diagonal
                    graphics.DrawLine(redPen, x + 5, y - 5, x - 5, y + 5); // Second diagonal
                }
            }

            // Convert Bitmap back to BitmapImage
            BitmapImage resultImage = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                bmp.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                resultImage.BeginInit();
                resultImage.StreamSource = stream;
                resultImage.CacheOption = BitmapCacheOption.OnLoad;
                resultImage.EndInit();
                resultImage.Freeze(); // Optional: make it cross-thread accessible
            }

            mainImage.Source = resultImage;
            extractRegionImage(bmp, x, y, 50, 50);
        }

        private void extractRegionImage(Bitmap sourceBitmap, int centerX, int centerY, int width, int height)
        {
            // Calculate the top-left corner of the rectangle to extract
            int x = centerX - width / 2;
            int y = centerY - height / 2;

            // Ensure the rectangle is within the bounds of the source bitmap
            if (x < 0)
            {
                x = 0;
            }
            if (y < 0)
            {
                y = 0;
            }
            if (x + width > sourceBitmap.Width)
            {
                width = sourceBitmap.Width - x;
            }
            if (y + height > sourceBitmap.Height)
            {
                height = sourceBitmap.Height - y;
            }

            // Create a rectangle to define the region to extract
            Rectangle region = new Rectangle(x, y, width, height);

            // Create a new bitmap to hold the extracted region
            Bitmap extractedBitmap = new Bitmap(width, height);

            // Draw the specified region from the source bitmap into the new bitmap
            using (Graphics g = Graphics.FromImage(extractedBitmap))
            {
                g.DrawImage(sourceBitmap, new Rectangle(0, 0, width, height), region, GraphicsUnit.Pixel);
            }

            // Convert Bitmap back to BitmapImage
            BitmapImage resultImage = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                extractedBitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                resultImage.BeginInit();
                resultImage.StreamSource = stream;
                resultImage.CacheOption = BitmapCacheOption.OnLoad;
                resultImage.EndInit();
                resultImage.Freeze(); // Optional: make it cross-thread accessible
            }

            regionImage.Source = resultImage;
            emptyAnnotationText.Visibility = Visibility.Hidden;
            regionImage.Visibility = Visibility.Visible;

        }

        private void LoadNextImage(object sender, RoutedEventArgs e)
        {
            imageIndexer++;
            loadMainImage(); 
        }

        private void LoadPreviosImage(object sender, RoutedEventArgs e)
        {
            if(imageIndexer >= 1)
            {  
                imageIndexer--;
            }
            loadMainImage();
        }

        private void SearchAndLoadFileName(object sender, RoutedEventArgs e)
        {
            foreach(string fileName in imageFileNames)
            {
                FileInfo fileInfo = new FileInfo(fileName);

                if(fileNameToSearchFor.Text == fileInfo.Name)
                {
                    imageIndexer = imageFileNames.IndexOf(fileInfo.FullName);
                    loadMainImage();
                }
            }
        }
    }
}
