using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Voxel_Fortress
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MapMaker mapMaker = new MapMaker();

        MapMaker.Images mapImages = new MapMaker.Images();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void heightMapButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog heightMapDialog = new OpenFileDialog();

            heightMapDialog.Filter = "Image Files (*.png, *.bmp, *.jpg)|*.png;*.bmp;*.jpg;*.jpeg|All Files (*.*)|*.*";

            if (heightMapDialog.ShowDialog() == true)
            {
                mapImages.imageFilename = heightMapDialog.FileName;
                BitmapImage elevationMap = new BitmapImage(new Uri(heightMapDialog.FileName));

                mapImages.elevationMap = MapMaker.ConvertBitmapImage(elevationMap);

                elevationMapImage.Source = elevationMap;
                exportButton.IsEnabled = true;
            }
        }

        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            exportProgress.Value = 0;

            BackgroundWorker mapExportWorker = new BackgroundWorker();
            mapExportWorker.WorkerReportsProgress = true;
            mapExportWorker.DoWork += mapMaker.LoadMap;
            mapExportWorker.ProgressChanged += MapExportWorker_ProgressChanged;
            mapExportWorker.RunWorkerCompleted += MapExportWorker_RunWorkerCompleted;
            mapExportWorker.RunWorkerAsync(mapImages);

            (sender as Button).IsEnabled = false;
        }

        private void MapExportWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            exportButton.IsEnabled = true;
        }

        private void MapExportWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            exportProgress.Value = e.ProgressPercentage;
            exportLabel.Text = e.UserState as string;
        }

        private void waterMapButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog heightMapDialog = new OpenFileDialog();

            heightMapDialog.Filter = "Image Files (*.png, *.bmp, *.jpg)|*.png;*.bmp;*.jpg;*.jpeg|All Files (*.*)|*.*";

            if (heightMapDialog.ShowDialog() == true)
            {
                //imageFileName = heightMapDialog.FileName;
                BitmapImage waterMap = new BitmapImage(new Uri(heightMapDialog.FileName));

                mapImages.waterMap = MapMaker.ConvertBitmapImage(waterMap);

                waterMapImage.Source = waterMap;
                //exportButton.IsEnabled = true;
            }
        }

        private void colorMapButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog heightMapDialog = new OpenFileDialog();

            heightMapDialog.Filter = "Image Files (*.png, *.bmp, *.jpg)|*.png;*.bmp;*.jpg;*.jpeg|All Files (*.*)|*.*";

            if (heightMapDialog.ShowDialog() == true)
            {
                //imageFileName = heightMapDialog.FileName;
                BitmapImage colorMap = new BitmapImage(new Uri(heightMapDialog.FileName));

                mapImages.colorMap = MapMaker.ConvertBitmapImage(colorMap);

                colorMapImage.Source = colorMap;
                //exportButton.IsEnabled = true;
            }
        }
    }
}
