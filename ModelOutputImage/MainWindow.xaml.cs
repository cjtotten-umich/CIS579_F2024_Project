﻿using System;
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
using System.Text.RegularExpressions;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using System.Windows.Markup;
using System.Net.NetworkInformation;


namespace ModelOutputImage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //file/folder names for data
        private string imagesFolder;
        private string csvLabelsFile;

        //combined data tuple of annotations and images full path
        private List<Tuple<string, double, double, double, string>> annontationsImagePathAttached = new List<Tuple<string, double, double, double, string>>(); //item 1 = file name, item 2 = sign or no sign, item 3 = x location, item 4 = y location, item 5 = image folder location
        private int imageIndexer = 0;

        public MainWindow()
        {
            InitializeComponent();
        }
       
        /// <summary>
        /// On button press, browse for a folder that contains images for applying labels to
        /// </summary>
        private void BrowseImageFolder(object sender, RoutedEventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select the folder containing images that have annotations.";
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
        private void BrowseCSVLabelFile(object sender, RoutedEventArgs e)
        {
            if(imagesFolder == null || imagesFolder == string.Empty)
            {
                System.Windows.MessageBox.Show("Please select the image folder path first."); 
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "Open Annotations CSV File"
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
        private void LoadAllData(object sender, RoutedEventArgs e)
        {
            annontationsImagePathAttached = new List<Tuple<string, double, double, double, string>>(); //item 1 = file name, item 2 = sign or no sign, item 3 = x location, item 4 = y location, item 5 = image folder location
            imageIndexer = 0;

            //check that image folde has been selected
            if (imagesFolder == string.Empty || imagesFolder == null || Directory.Exists(imagesFolder) == false)
            {
                System.Windows.MessageBox.Show("Please select an image directory containing .png, .bmp, or .jpg files via \"Browse for Image Folder\" button.");
                return;
            }

            //check that csv has been selected
            if (csvLabelsFile == string.Empty || csvLabelsFile == null || File.Exists(csvLabelsFile) == false)
            {
                System.Windows.MessageBox.Show("Please select csv file containing image annotations via \"Browse for Labels CSV\" button.");
                return;
            }

            //try to load the data
            try
            {
                loadCSVDataWithPath();
            }
            catch (Exception ex) 
            {
                System.Windows.MessageBox.Show("Error loading image annotations. Please check .csv file and format. " + ex.Message);
            }

            imageIndexer = 0; //set to first element at start
            loadMainImage();

            System.Windows.MessageBox.Show("CSV and Image Data Loaded Properly. Matches found in data: " + annontationsImagePathAttached.Count.ToString());
        }

        /// <summary>
        /// Load csv data from the selected path and add the image path
        /// </summary>
        private void loadCSVDataWithPath()
        {
            try
            {
                using (var reader = new StreamReader(csvLabelsFile))
                {
                    // Skip header
                    string headerLine = reader.ReadLine();

                    //read all rows of the csv, add to the annotations with path tuple, the last item is the path for image folder
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        var values = line.Split(',');
                        Tuple<string, double, double, double, string> annotationWithPath = new Tuple<string, double, double, double, string>(values[0], Convert.ToDouble(values[1]), double.Parse(values[2]), double.Parse(values[3]), imagesFolder);
                        annontationsImagePathAttached.Add(annotationWithPath);
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
            //build fileName
            if(annontationsImagePathAttached.Count == 0)
            {
                System.Windows.MessageBox.Show("Data has not been loaded.");
                return;
            }

            //build fileName
            string fileName = annontationsImagePathAttached[imageIndexer].Item5 + "/" + annontationsImagePathAttached[imageIndexer].Item1;

            try
            {
                BitmapImage bitmap = new BitmapImage(new Uri(fileName));
                mainImage.Source = bitmap;
                currentImageConfidence.Text = "0";
                regionImage.Source = null;
                currentFileName.Text = fileName;
                loadAnnotations(bitmap);
            }
            catch(Exception ex) 
            {
                System.Windows.MessageBox.Show("Error loading bitmap and annotations: " + ex.Message);
            }
        }

        //
        private void loadAnnotations(BitmapImage image)
        {
            //if an annotation location is present, display the confidence and draw on the image for display
            if (annontationsImagePathAttached[imageIndexer].Item3 > 0.000001 || annontationsImagePathAttached[imageIndexer].Item4 > 0.000001) // do not draw when both items are 0
            {
                currentImageConfidence.Text = annontationsImagePathAttached[imageIndexer].Item2.ToString("0.00");
                drawDataOnImage(annontationsImagePathAttached[imageIndexer], image);
            }
            else
            {
                currentImageConfidence.Text = "N/A";
                emptyAnnotationText.Visibility = Visibility.Visible;
                regionImage.Visibility -= Visibility.Hidden; 
            }
        }

        //draw annotation data on the image and extract a zommed region
        private void drawDataOnImage(Tuple<string, double, double, double, string> imageData, BitmapImage bitmap)
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

        //extract a zoomed region where the annotation is present for better viewing and display on UI
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
            if(imageIndexer == (annontationsImagePathAttached.Count - 1) || annontationsImagePathAttached.Count == 0)
            {
                System.Windows.MessageBox.Show("Data and image collection end has been reached");
                return;
            }

            imageIndexer++;
            loadMainImage(); 
        }

        private void LoadPreviosImage(object sender, RoutedEventArgs e)
        {
            if (imageIndexer == 0)
            {
                System.Windows.MessageBox.Show("Data and image collection start has been reached");
                return;
            }

            imageIndexer--;        
            loadMainImage();
        }

        private void SearchAndLoadFileName(object sender, RoutedEventArgs e)
        {
            Tuple<string, double, double, double, string> match = annontationsImagePathAttached.FirstOrDefault(t => t.Item1 == fileNameToSearchFor.Text);
            imageIndexer = annontationsImagePathAttached.IndexOf(match);
            loadMainImage();                  
        }

        #region Logic for User Entering A Threshold Score - Ensure a usable real value is input
        private void NumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits, a single decimal point, and restrict leading zeros
            if (!IsInputValid(ScoreSort.Text, e.Text))
            {
                e.Handled = true;
            }
        }

        private void NumberTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Allow backspace and delete keys
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                return;
            }
        }

        private void NumberTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ScoreSort.Text))
            {
                if (double.TryParse(ScoreSort.Text, out double value))
                {
                    // Ensure the value is between 0 and 100
                    if (value < 0 || value > 100)
                    {
                        ScoreSort.Text = "";
                    }
                }
                else
                {
                    ScoreSort.Text = "";
                }

                // Move the caret to the end
                ScoreSort.CaretIndex = ScoreSort.Text.Length;
            }
        }

        private bool IsInputValid(string currentText, string newText)
        {
            // Regex to allow digits, a single decimal point, and avoid leading zeroes
            var regex = new Regex(@"^(0|[1-9]\d{0,2})(\.\d+)?$");
            var newTextWithCurrent = currentText + newText;

            // Check if the new text would still be valid
            return regex.IsMatch(newTextWithCurrent) &&
                   double.TryParse(newTextWithCurrent, out double result) &&
                   result >= 0 && result <= 100;
        }
        #endregion

        private void ScoreSort_LostFocus(object sender, RoutedEventArgs e)
        {
            //sort the annotations list
            List<Tuple<string, double, double, double, string>> filteredList = annontationsImagePathAttached.Where(t => t.Item2 > (Convert.ToDouble(ScoreSort.Text))/100).ToList();
            annontationsImagePathAttached = filteredList;
            imageIndexer = 0;
            loadMainImage(); 
        }

        private void SortAscending_Click(object sender, RoutedEventArgs e)
        {
            List<Tuple<string, double, double, double, string>> sortedList = annontationsImagePathAttached.OrderBy(t => t.Item2).ToList();
            annontationsImagePathAttached = sortedList;
            imageIndexer = 0;
            loadMainImage();
        }

        private void SortDescending_Click(object sender, RoutedEventArgs e)
        {
            List<Tuple<string, double, double, double, string>> sortedList = annontationsImagePathAttached.OrderByDescending(t => t.Item2).ToList();
            annontationsImagePathAttached = sortedList;
            imageIndexer = 0;
            loadMainImage();
        }
    }
}
