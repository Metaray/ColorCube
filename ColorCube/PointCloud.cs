using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ColorCube
{
    public abstract class PointCloud
    {
        public static readonly VertexDeclaration ParticleInstanceDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );

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
