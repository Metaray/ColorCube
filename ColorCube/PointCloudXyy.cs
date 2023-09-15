using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ColorCube
{
    public class PointCloudXyy : PointCloud
    {
        private static readonly Matrix Transform;

        static PointCloudXyy()
        {
            //Vector3 w = ColorToXyy(Color.White);
            Vector3 w = (ColorToXyy(Color.Red) + ColorToXyy(Color.Green) + ColorToXyy(Color.Blue)) / 3;

            Transform =
                Matrix.CreateTranslation(-w.X, 0, -w.Z)
                * Matrix.CreateScale(2.0f, 1, 1.8f)
                * Matrix.CreateTranslation(0.5f, 0, 0.5f)
                * Matrix.CreateScale(255);
        }

        public override Vector3 ColorToPosition(Color color)
        {
            return Vector3.Transform(ColorToXyy(color), Transform);
        }

        public override VertexPositionColor[] MakeOutline()
        {
            Vector3 w = ColorToXyy(Color.White);
            w.Y = 0;

            var outline = new VertexPositionColor[18];

            outline[0].Color = Color.Red;
            outline[0].Position = ColorToXyy(outline[0].Color);
            outline[0].Position.Y = 0;
            outline[0].Position = MoveAway(outline[0].Position, w, 0.01f);

            outline[2].Color = Color.Green;
            outline[2].Position = ColorToXyy(outline[2].Color);
            outline[2].Position.Y = 0;
            outline[2].Position = MoveAway(outline[2].Position, w, 0.01f);

            outline[4].Color = Color.Blue;
            outline[4].Position = ColorToXyy(outline[4].Color);
            outline[4].Position.Y = 0;
            outline[4].Position = MoveAway(outline[4].Position, w, 0.01f);

            outline[1] = outline[2];
            outline[3] = outline[4];
            outline[5] = outline[0];

            for (var i = 0; i < 6; ++i)
            {
                outline[i + 6] = outline[i];
                outline[i + 6].Position.Y = 1;
            }

            outline[12] = outline[1];
            outline[13] = outline[7];
            outline[14] = outline[3];
            outline[15] = outline[9];
            outline[16] = outline[5];
            outline[17] = outline[11];

            for (var i = 0; i < outline.Length; i++)
            {
                outline[i].Position = Vector3.Transform(outline[i].Position, Transform);
            }

            return outline;
        }

        private static Vector3 ColorToXyy(Color color)
        {
            Vector3 xyz = ColorUtils.SrgbToXyz(ColorUtils.RgbToLinear(color.ToVector3()));
            Vector2 chromaXY = ColorUtils.XyzToChromaXy(xyz);
            return new Vector3(chromaXY.X, xyz.Y, chromaXY.Y);
        }

        private static Vector3 MoveAway(Vector3 point, Vector3 origin, float distance)
        {
            Vector3 direction = Vector3.Normalize(point - origin);
            return point + direction * distance;
        }
    }
}
