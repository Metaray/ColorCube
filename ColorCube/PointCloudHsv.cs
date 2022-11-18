using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ColorCube
{
    public class PointCloudHsv : PointCloud
    {
        private const float HsvRadius = 160.0f;

        private const int Quality = 100;

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
            var outline = new VertexPositionNormalTexture[Quality * 2 * 2];

            for (var i = 0; i < outline.Length; ++i)
            {
                outline[i].TextureCoordinate = new Vector2(0);
            }

            for (var idx = 0; idx < Quality; ++idx)
            {
                var i1 = idx * 2 + 1;
                var i2 = (i1 + 1) % (Quality * 2);
                var i3 = i1 + Quality * 2;
                var i4 = i2 + Quality * 2;

                var h = (float)(idx + 0.5f) / Quality;
                var color = ColorUtils.HsvToRgb(new Vector3(h, 1, 1));

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
