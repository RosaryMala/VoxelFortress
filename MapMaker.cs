using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Voxel_Fortress
{
    class MapMaker
    {
        public class Images
        {
            public Color[,] elevationMap;
            public Color[,] waterMap;
            public Color[,] colorMap;
            public string imageFilename;
        }

        const int maxSize = 1040;

        XRaw voxels;

        int[,] landElevations;
        int[,] waterElevations;

        void LoadElevations(out int[,] elevations, Color[,] elevationMap, object sender)
        {
            elevations = new int[elevationMap.GetLength(0), elevationMap.GetLength(1)];

            for (int x = 0; x < elevationMap.GetLength(0); x++)
            {
                for (int y = 0; y < elevationMap.GetLength(1); y++)
                {
                    if (elevationMap[x, y].R == 0)
                        elevations[x, y] = elevationMap[x, y].B;
                    else
                        elevations[x, y] = elevationMap[x, y].B + 25;
                }
                (sender as BackgroundWorker).ReportProgress(x * 2048 / elevationMap.GetLength(0), string.Format("Loading Heightmap: {0} of {1}", x, elevationMap.GetLength(0)));
            }
        }

        public static Color[,] ConvertBitmapImage(BitmapImage image)
        {
            int bytesPerPixel = (image.Format.BitsPerPixel + 7) / 8;

            byte[] colorArray = new byte[image.PixelWidth * image.PixelHeight * bytesPerPixel];

            image.CopyPixels(colorArray, bytesPerPixel * image.PixelWidth, 0);

            Color[,] pixelArray = new Color[image.PixelWidth, image.PixelHeight];

            for (int x = 0; x < image.PixelWidth; x++)
                for (int y = 0; y < image.PixelHeight; y++)
                {
                    int index = (x + y * image.PixelWidth) * bytesPerPixel;
                    pixelArray[x, y] = Color.FromRgb(colorArray[index + 2], colorArray[index + 1], colorArray[index + 0]);
                }

            return pixelArray;
        }

        public void LoadMap(object sender, DoWorkEventArgs e)
        {
            try
            {
                Images images = (Images)e.Argument;

                LoadElevations(out landElevations, images.elevationMap, sender);

                int voxelWidth = landElevations.GetLength(0);
                int voxelLength = landElevations.GetLength(1);

                if (voxelWidth > maxSize)
                    voxelWidth = maxSize;
                if (voxelLength > maxSize)
                    voxelLength = maxSize;

                voxels = new XRaw(voxelWidth, voxelLength, 280);

                for (int x = 0; x < voxelWidth; x++)
                {
                    for (int y = 0; y < voxelLength; y++)
                    {
                        voxels.SetHeight(images.colorMap[x, y], x, y, landElevations[x, y]);
                    }
                    (sender as BackgroundWorker).ReportProgress(x * 2048 / voxelWidth, string.Format("Generating Voxels: {0} of {1}", x, voxelWidth));
                }
                string savePath = Path.ChangeExtension(images.imageFilename, "xraw");
                (sender as BackgroundWorker).ReportProgress(2048, "Saving " + savePath);
                voxels.SaveFile(savePath);
                (sender as BackgroundWorker).ReportProgress(2048, "Finished!");
            }
            catch (Exception exception)
            {
                (sender as BackgroundWorker).ReportProgress(0, exception.Message);
            }
        }
    }
}
