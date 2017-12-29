using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using Voxel_Fortress;

namespace Voxel_Fortress_CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Elevation map argument missing");
                return;
            }
            string heightMapFileName = args[0];
            if (!File.Exists(heightMapFileName))
            {
                Console.WriteLine("Elevation map file not found: {0}", heightMapFileName);
                return;
            }
            heightMapFileName = Path.GetFullPath(heightMapFileName);
            Console.WriteLine("Elevation map: {0}", heightMapFileName);
            
            if (args.Length < 2)
            {
                Console.WriteLine("Color map argument missing");
                return;
            }
            string colorMapFileName = args[1];
            if (!File.Exists(colorMapFileName))
            {
                Console.WriteLine("Color map file not found: {0}", colorMapFileName);
                return;
            }
            colorMapFileName = Path.GetFullPath(colorMapFileName);
            Console.WriteLine("Color map: {0}", colorMapFileName);

            MapMaker mapMaker = new MapMaker();
            MapMaker.Images mapImages = new MapMaker.Images();

            {
                mapImages.imageFilename = heightMapFileName;
                BitmapImage elevationMap = new BitmapImage(new Uri(heightMapFileName));
                mapImages.elevationMap = MapMaker.ConvertBitmapImage(elevationMap);

                mapImages.imageFilename = colorMapFileName;
                BitmapImage colorMap = new BitmapImage(new Uri(colorMapFileName));
                mapImages.colorMap = MapMaker.ConvertBitmapImage(colorMap);
            }

            exportMap(mapMaker, mapImages);
        }

        private static void exportMap(MapMaker mapMaker, MapMaker.Images mapImages)
        {
            ManualResetEvent done = new ManualResetEvent(false);

            BackgroundWorker mapExportWorker = new BackgroundWorker();
            mapExportWorker.WorkerReportsProgress = true;
            mapExportWorker.DoWork += mapMaker.LoadMap;
            mapExportWorker.ProgressChanged += (s, e) => {
                Console.WriteLine("{0}\t{1}", e.ProgressPercentage, e.UserState as string);
            };
            mapExportWorker.RunWorkerCompleted += (s, e) => {
                Console.WriteLine("Completed.");
                done.Set();
            };
            mapExportWorker.RunWorkerAsync(mapImages);

            done.WaitOne();
        }
    }
}
