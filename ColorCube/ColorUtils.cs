using Microsoft.Xna.Framework;
using System;

namespace ColorCube
{
    public static class ColorUtils
    {
        public static Color[] ImageUniqueColors(string path)
        {
            var hasColor = new bool[256 * 256 * 256];
            int uniqueCount = 0;

            using (var bitmap = new System.Drawing.Bitmap(path))
            {
                var data = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb
                );

                unsafe
                {
                    var scan = (byte*)data.Scan0;

                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            var idx = x * 3 + data.Stride * y;
                            var clr = (scan[idx + 2] << 16) | (scan[idx + 1] << 8) | scan[idx + 0];
                            
                            if (!hasColor[clr])
                            {
                                uniqueCount++;
                                hasColor[clr] = true;
                            }
                        }
                    }
                }

                bitmap.UnlockBits(data);
            }

            var uniqColors = new Color[uniqueCount];
            for (int i = 0, j = 0; i < hasColor.Length; ++i)
            {
                if (hasColor[i])
                {
                    uniqColors[j].R = (byte)(i >> 16);
                    uniqColors[j].G = (byte)(i >> 8);
                    uniqColors[j].B = (byte)i;
                    j++;
                }
            }
            return uniqColors;
        }

        public static Vector3 RgbToHsv(Vector3 rgb)
        {
            var maxc = Math.Max(Math.Max(rgb.X, rgb.Y), rgb.Z);
            var minc = Math.Min(Math.Min(rgb.X, rgb.Y), rgb.Z);
            var v = maxc;
            if (minc == maxc)
            {
                return new Vector3(0, 0, v);
            }
            var s = (maxc - minc) / maxc;
            var rc = (maxc - rgb.X) / (maxc - minc);
            var gc = (maxc - rgb.Y) / (maxc - minc);
            var bc = (maxc - rgb.Z) / (maxc - minc);
            float h;
            if (rgb.X == maxc)
                h = bc - gc;
            else if (rgb.Y == maxc)
                h = 2.0f + rc - bc;
            else
                h = 4.0f + gc - rc;
            h = (h / 6.0f) % 1.0f;
            return new Vector3(h, s, v);
        }
    }
}
