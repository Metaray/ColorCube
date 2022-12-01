using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ColorCube
{
    public class PointCloudRgb : PointCloud
    {
        public override Vector3[] ColorsToPositions(Color[] colors)
        {
            var verts = new Vector3[colors.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                // RGB cube centered on (127.5, 127.5, 127.5)
                verts[i] = colors[i].ToVector3() * 255;
            }

            return verts;
        }

        public override VertexPositionNormalTexture[] MakeOutline()
        {
            var outline = new VertexPositionNormalTexture[24];
            outline[0].Position = outline[0].Normal = new Vector3(-1f, -1f, -1f);
            outline[1].Position = outline[1].Normal = new Vector3(-1f, -1f, 256f);
            outline[2].Position = outline[2].Normal = new Vector3(256f, -1f, -1f);
            outline[3].Position = outline[3].Normal = new Vector3(256f, -1f, 256f);
            outline[4].Position = outline[4].Normal = new Vector3(-1f, 256f, -1f);
            outline[5].Position = outline[5].Normal = new Vector3(-1f, 256f, 256f);
            outline[6].Position = outline[6].Normal = new Vector3(256f, 256f, -1f);
            outline[7].Position = outline[7].Normal = new Vector3(256f, 256f, 256f);

            for (int i = 0; i < 8; ++i)
            {
                outline[i].TextureCoordinate = new Vector2(0);
                outline[i].Normal /= 255;
            }

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
