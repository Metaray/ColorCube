using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ColorCube
{
    public class PointCloudHsv : PointCloud
    {
        private const float HsvRadius = 160.0f;

        public override VertexPositionNormalTexture[] ColorsToVertexes(Color[] colors)
        {
            var verts = PrepareColorsVertexes(colors);
            for (int i = 0; i < colors.Length; i++)
            {
                // pos = HSV cylinder centered on (127.5, 127.5, 127.5)
                var hsv = ColorUtils.RgbToHsv(colors[i].ToVector3());
                var pos = new Vector3(
                    MathF.Cos(hsv.X * MathF.PI * 2) * hsv.Y * HsvRadius + 127.5f,
                    hsv.Z * 255,
                    MathF.Sin(hsv.X * MathF.PI * 2) * hsv.Y * HsvRadius + 127.5f
                );
                for (int j = 0; j < 6; ++j)
                {
                    verts[i * 6 + j].Position = pos;
                }
            }
            return verts;
        }

        public override VertexPositionNormalTexture[] MakeOutline()
        {
            const int quality = 100;
            var outline = new VertexPositionNormalTexture[quality * 2 * 2];

            for (int i = 0; i < outline.Length; ++i)
            {
                outline[i].TextureCoordinate = new Vector2(0);
            }

            for (int idx = 0; idx < quality; ++idx)
            {
                int i1 = idx * 2 + 1, i2 = (idx * 2 + 2) % (quality * 2);
                int i3 = i1 + quality * 2, i4 = i2 + quality * 2;

                var h = (float)(idx + 1) / quality;
                int i = (int)(h * 6);
                var f = (h * 6.0f) - i;
                var q = 1.0f - f;
                var color = (i % 6) switch
                {
                    0 => new Vector3(1, f, 0),
                    1 => new Vector3(q, 1, 0),
                    2 => new Vector3(0, 1, f),
                    3 => new Vector3(0, q, 1),
                    4 => new Vector3(f, 0, 1),
                    5 => new Vector3(1, 0, q),
                };

                outline[i1].Normal = outline[i2].Normal = outline[i3].Normal = outline[i4].Normal = color;
                var x = MathF.Cos(h * MathF.PI * 2) * HsvRadius + 127.5f;
                var y = MathF.Sin(h * MathF.PI * 2) * HsvRadius + 127.5f;
                outline[i1].Position = outline[i2].Position = new Vector3(x, -1, y);
                outline[i3].Position = outline[i4].Position = new Vector3(x, 256, y);
            }

            return outline;
        }
    }
}
