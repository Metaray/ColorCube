using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ColorCube
{
    public class PointCloudHsv : PointCloud
    {
        private const float HsvRadius = 160.0f;

        private const int Quality = 100;

        public override Vector3 ColorToPosition(Color color)
        {
            // HSV cylinder centered on (127.5, 127.5, 127.5)
            var hsv = ColorUtils.RgbToHsv(color.ToVector3());

            return new Vector3(
                MathF.Cos(hsv.X * MathF.PI * 2) * hsv.Y * HsvRadius + 127.5f,
                hsv.Z * 255,
                MathF.Sin(hsv.X * MathF.PI * 2) * hsv.Y * HsvRadius + 127.5f
            );
        }

        public override VertexPositionColor[] MakeOutline()
        {
            var outline = new VertexPositionColor[Quality * 2 * 2];

            for (var idx = 0; idx < Quality; ++idx)
            {
                var i1 = idx * 2 + 1;
                var i2 = (i1 + 1) % (Quality * 2);
                var i3 = i1 + Quality * 2;
                var i4 = i2 + Quality * 2;

                var h = (float)(idx + 0.5f) / Quality;
                var color = new Color(ColorUtils.HsvToRgb(new Vector3(h, 1, 1)));

                outline[i1].Color = outline[i2].Color = outline[i3].Color = outline[i4].Color = color;
                var x = MathF.Cos(h * MathF.PI * 2) * HsvRadius + 127.5f;
                var y = MathF.Sin(h * MathF.PI * 2) * HsvRadius + 127.5f;
                outline[i1].Position = outline[i2].Position = new Vector3(x, -1, y);
                outline[i3].Position = outline[i4].Position = new Vector3(x, 256, y);
            }

            return outline;
        }
    }
}
