using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ColorCube
{
    public class PointCloudRgb : PointCloud
    {
        public override Vector3 ColorToPosition(Color color)
        {
            // RGB cube centered on (127.5, 127.5, 127.5)
            return color.ToVector3() * 255;
        }

        public override VertexPositionColor[] MakeOutline()
        {
            var outline = new VertexPositionColor[24];

            outline[0].Position = new Vector3(-1f, -1f, -1f);
            outline[1].Position = new Vector3(-1f, -1f, 256f);
            outline[2].Position = new Vector3(256f, -1f, -1f);
            outline[3].Position = new Vector3(256f, -1f, 256f);
            outline[4].Position = new Vector3(-1f, 256f, -1f);
            outline[5].Position = new Vector3(-1f, 256f, 256f);
            outline[6].Position = new Vector3(256f, 256f, -1f);
            outline[7].Position = new Vector3(256f, 256f, 256f);

            outline[0].Color = new Color(0, 0, 0);
            outline[1].Color = new Color(0, 0, 255);
            outline[2].Color = new Color(255, 0, 0);
            outline[3].Color = new Color(255, 0, 255);
            outline[4].Color = new Color(0, 255, 0);
            outline[5].Color = new Color(0, 255, 255);
            outline[6].Color = new Color(255, 255, 0);
            outline[7].Color = new Color(255, 255, 255);

            outline[8] = outline[0];
            outline[9] = outline[2];
            outline[10] = outline[1];
            outline[11] = outline[3];
            outline[12] = outline[4];
            outline[13] = outline[6];
            outline[14] = outline[5];
            outline[15] = outline[7];

            outline[16] = outline[0];
            outline[17] = outline[4];
            outline[18] = outline[1];
            outline[19] = outline[5];
            outline[20] = outline[2];
            outline[21] = outline[6];
            outline[22] = outline[3];
            outline[23] = outline[7];

            return outline;
        }
    }
}
