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
        XRaw voxels;

        int[,] landElevations;
        int[,] waterElevations;

        public void LoadMap(object sender, DoWorkEventArgs e)
        {
            try
            {
                string path = (string)e.Argument;
                BitmapImage elevationMap = new BitmapImage(new Uri(path));

                int bytesPerPixel = (elevationMap.Format.BitsPerPixel + 7) / 8;

                byte[] colorArray = new byte[elevationMap.PixelWidth * elevationMap.PixelHeight * bytesPerPixel];

                elevationMap.CopyPixels(colorArray, bytesPerPixel * elevationMap.PixelWidth, 0);

                int voxelWidth = elevationMap.PixelWidth;
                int voxelLength = elevationMap.PixelHeight;

                if (voxelWidth > 2048)
                    voxelWidth = 2048;
                if (voxelLength > 2048)
                    voxelLength = 2048;

                voxels = new XRaw(voxelWidth, voxelLength, 280);

                for (int x = 0; x < voxelWidth; x++)
                {
                    for (int y = 0; y < voxelLength; y++)
                    {
                        int index = (x + y * elevationMap.PixelWidth) * bytesPerPixel;
                        if(colorArray[index + 2] == 0)
                            voxels.SetHeight(Color.FromRgb(colorArray[index + 2], colorArray[index + 1], colorArray[index + 0]), x, y, colorArray[index + 0]);
                        else
                            voxels.SetHeight(Color.FromRgb(colorArray[index + 2], colorArray[index + 1], colorArray[index + 0]), x, y, colorArray[index + 0] + 25);
                    }
                    (sender as BackgroundWorker).ReportProgress(x * 2048 / voxelWidth, string.Format("Generating Heightmap: {0} of {1}", x, voxelWidth));
                }
                string savePath = Path.ChangeExtension(path, "xraw");
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
