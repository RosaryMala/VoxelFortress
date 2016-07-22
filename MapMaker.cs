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
            public Color[,] colorMap;
            public string imageFilename;
        }

        const int maxSize = 2048;

        XRaw voxels;

        int[,] landElevations = null;

        bool IsWaterMap(Color[,] elevationMap)
        {
            foreach (var item in elevationMap)
            {
                if (item.R == 0 && item.G > 0 && item.B > 0)
                    return true;
            }
            return false;
        }

        int[,] LoadElevations( Color[,] elevationMap, object sender)
        {
            int[,] elevations = new int[elevationMap.GetLength(0), elevationMap.GetLength(1)];

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

            return elevations;
        }

        int[,] LoadWaterElevations(Color[,] elevationMap, object sender)
        {
            int[,] elevations = new int[elevationMap.GetLength(0), elevationMap.GetLength(1)];

            for (int x = 0; x < elevationMap.GetLength(0); x++)
            {
                for (int y = 0; y < elevationMap.GetLength(1); y++)
                {
                    if (elevationMap[x, y].R > 0)
                    {
                        elevations[x, y] = elevationMap[x, y].B + 25; //This means it's not water.
                        continue;
                    }
                    if (elevationMap[x, y].G > 0)
                    {
                        elevations[x, y] = elevationMap[x, y].B; //Rivers
                        continue;
                    }
                    elevations[x, y] = elevationMap[x, y].B + 25;
                }
                (sender as BackgroundWorker).ReportProgress(x * 2048 / elevationMap.GetLength(0), string.Format("Loading Water: {0} of {1}", x, elevationMap.GetLength(0)));
            }
            return elevations;
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

                if (images.elevationMap != null)
                {
                    if (IsWaterMap(images.elevationMap))
                        landElevations = LoadWaterElevations(images.elevationMap, sender);
                    else
                        landElevations = LoadElevations(images.elevationMap, sender);
                }
                else if (images.colorMap != null)
                    landElevations = new int[images.colorMap.GetLength(0), images.colorMap.GetLength(1)];
                else return; //nothing to work with.

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
                        int zMin = landElevations[x, y];
                        if (x > 0)
                            zMin = Math.Min(zMin, landElevations[x - 1, y]);
                        if (y > 0)
                            zMin = Math.Min(zMin, landElevations[x, y - 1]);
                        if (x < voxelWidth - 1)
                            zMin = Math.Min(zMin, landElevations[x + 1, y]);
                        if (y < voxelLength - 1)
                            zMin = Math.Min(zMin, landElevations[x, y + 1]);
                        if (images.colorMap != null)
                            voxels.SetColumn(images.colorMap[x, y], x, y, zMin, landElevations[x, y]);
                        else
                            voxels.SetColumn(Colors.White, x, y, zMin, landElevations[x, y]);
                    }
                    (sender as BackgroundWorker).ReportProgress(x * 2048 / voxelWidth, string.Format("Generating Voxels: {0} of {1}", x, voxelWidth));
                }
                string savePath = Path.ChangeExtension(images.imageFilename, "xraw");
                voxels.SaveFile(savePath, sender);
                (sender as BackgroundWorker).ReportProgress(2048, "Finished!");
            }
            catch (Exception exception)
            {
                (sender as BackgroundWorker).ReportProgress(0, exception.Message);
            }
        }
    }
}
