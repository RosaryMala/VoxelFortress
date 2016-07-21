using System;
using System.IO;
using System.Text;
using System.Windows.Media;

namespace Voxel_Fortress
{
    class XRaw
    {
        UniqueList<Color> _palette = new UniqueList<Color>();

        int _width = 0;
        int _length = 0;
        int _height = 0;

        public int Width
        {
            get
            {
                return _width;
            }
        }

        public int Length
        {
            get
            {
                return _length;
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }
        }

        short[][,] _voxels;

        int CoordToIndex(int x, int y, int z)
        {
            return x + (y * _width) + z * (_width * _length);
        }

        bool IsValidCoords(int x, int y, int z)
        {
            return (
                x >= 0 &&
                y >= 0 &&
                z >= 0 &&
                x < _width &&
                y < _length &&
                z < _height
                );
        }

        public Color this[int x, int y, int z]
        {
            get
            {
                if (!IsValidCoords(x, y, z))
                    throw new IndexOutOfRangeException();
                return _palette[_voxels[z][x, y]];
            }
            set
            {
                if (IsValidCoords(x, y, z))
                    _voxels[z][x, y] = (short)_palette.IndexAdd(value);
            }
        }

        public void SetIndex(int x, int y, int z, short index)
        {
            if (IsValidCoords(x, y, z))
                _voxels[z][x, y] = index;
        }

        public void SetHeight(Color color, int x, int y, int z)
        {
            short index = (short)_palette.IndexAdd(color);
            int surface = z + 1;
            for (int zz = 0; zz < surface; zz++)
                SetIndex(x, y, zz, index);
            if (surface < 99)
            {
                short waterIndex = (short)_palette.IndexAdd(Color.FromArgb(128, 0, 128, 128));
                for (int zz = surface; zz < 99; zz++)
                    SetIndex(x, y, zz, waterIndex);
                surface = 99;
            }
            for (int zz = surface; zz < Height; zz++)
                ClearVoxel(x, y, zz);
        }

        public bool Resize(int width, int length, int height)
        {
            if (width == _width && length == _length && height == _height)
                return false; //no size change

            if (width > 2048 || length > 2048 || height > 2048)
                return false; //too big

            _voxels = new short[height][,];
            for (int z = 0; z < height; z++)
                    _voxels[z] = new short[width, length];

            _width = width;
            _length = length;
            _height = height;

            //Clear();

            return true;
        }

        public void Clear()
        {
            for (int z = 0; z < _height; z++)
                for (int x = 0; x < _width; x++)
                    for (int y = 0; y < _length; y++)
                    {
                        _voxels[z][x, y] = ~0;
                    }
        }

        public XRaw(int width, int length, int height)
        {
            Resize(width, length, height);
        }

        public void ClearVoxel(int x, int y, int z)
        {
            if (IsValidCoords(x, y, z))
                _voxels[z][x, y] = ~0;
        }

        public void SaveFile(string path)
        {
            BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create), Encoding.ASCII);

            //Beginning of Header
            writer.Write(Encoding.ASCII.GetBytes("XRAW")); //Magic number
            writer.Write((byte)0); //Unsigned Int channels
            writer.Write((byte)4); //RGBA
            writer.Write((byte)8); //Bits per channel
            writer.Write((byte)16); //Bits per index
            writer.Write(_width); //x
            writer.Write(_length); //y
            writer.Write(_height); //z
            writer.Write(_palette.Count); //number of palette colors

            //End of Header
            foreach (var layer in _voxels)
            {
                foreach (var index in layer)
                {
                    writer.Write((short)index);
                }
            }

            foreach (Color color in _palette)
            {
                writer.Write(color.R);
                writer.Write(color.G);
                writer.Write(color.B);
                writer.Write(color.A);
            }
            writer.Dispose();
        }
    }
}
