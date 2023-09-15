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

        public static Vector3 HsvToRgb(Vector3 hsv)
        {
            float h = hsv.X, s = hsv.Y, v = hsv.Z;

            if (s == 0.0)
            {
                return new Vector3(v, v, v);
            }

            h %= 1.0f;
            if (h < 0)
            {
                h += 1;
            }

            var i = (int)(h * 6);
            var f = (h * 6) - i;
            var p = v * (1 - s);
            var q = v * (1 - s * f);
            var t = v * (1 - s * (1 - f));

            return (i % 6) switch
            {
                0 => new Vector3(v, t, p),
                1 => new Vector3(q, v, p),
                2 => new Vector3(p, v, t),
                3 => new Vector3(p, q, v),
                4 => new Vector3(t, p, v),
                _ => new Vector3(v, p, q),
            };
        }

        public static float RgbToLinear(float value)
        {
            if (value <= 0.04045f)
            {
                return value / 12.92f;
            }
            else
            {
                return MathF.Pow((value + 0.055f) / 1.055f, 2.4f);
            }
        }

        public static Vector3 RgbToLinear(Vector3 color)
        {
            return new Vector3(RgbToLinear(color.X), RgbToLinear(color.Y), RgbToLinear(color.Z));
        }

        public static Vector3 SrgbToXyz(Vector3 srgb)
        {
            return new Vector3(
                srgb.X * 0.664511f + srgb.Y * 0.154324f + srgb.Z * 0.162028f,
                srgb.X * 0.283881f + srgb.Y * 0.668433f + srgb.Z * 0.047685f,
                srgb.X * 0.000088f + srgb.Y * 0.072310f + srgb.Z * 0.986039f
            );
        }

        public static Vector2 XyzToChromaXy(Vector3 xyz)
        {
            float sum = xyz.X + xyz.Y + xyz.Z;
            
            if (sum < 1.0e-6f)
            {
                return new Vector2(0.32272672086556803f, 0.32902290955907926f);
            }
            
            return new Vector2(xyz.X, xyz.Y) / sum;
        }
    }
}
