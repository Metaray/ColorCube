using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ColorCube
{
    public abstract class PointCloud
    {
        public VertexPositionColor[] ColorsToVertexData(Color[] colors)
        {
            var verts = new VertexPositionColor[colors.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                verts[i].Position = ColorToPosition(colors[i]);
                verts[i].Color = colors[i];
            }

            return verts;
        }

        public abstract Vector3 ColorToPosition(Color color);

        public abstract VertexPositionColor[] MakeOutline();
    }
}
