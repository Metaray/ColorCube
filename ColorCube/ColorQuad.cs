using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ColorCube
{
    internal class ColorQuad
    {
        public ColorQuad(float size)
        {
            VertexData = new VertexPosition[4];
            VertexData[0].Position = new Vector3(-size, -size, 0);
            VertexData[1].Position = new Vector3(-size, size, 0);
            VertexData[2].Position = new Vector3(size, size, 0);
            VertexData[3].Position = new Vector3(size, -size, 0);

            IndexData = new short[6]
            {
                0, 1, 3,
                1, 2, 3,
            };
        }

        public VertexDeclaration VertexDeclaration => VertexPosition.VertexDeclaration;

        public VertexPosition[] VertexData { get; }

        public short[] IndexData { get; }
    }
}
