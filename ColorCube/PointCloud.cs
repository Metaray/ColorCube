using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ColorCube
{
    public abstract class PointCloud
    {
        public VertexPositionNormalTexture[] ColorsToVertexData(Color[] colors)
        {
            const float size = 2.0f;
            var bottomLeft = new Vector2(-size, -size);
            var upperLeft = new Vector2(-size, size);
            var bottomRight = new Vector2(size, -size);
            var upperRight = new Vector2(size, size);

            var positions = ColorsToPositions(colors);
            var verts = new VertexPositionNormalTexture[colors.Length * 6];

            for (int i = 0; i < colors.Length; i++)
            {
                var clr = colors[i].ToVector3();
                
                for (int j = 0; j < 6; ++j)
                {
                    verts[i * 6 + j].Position = positions[i];
                    verts[i * 6 + j].Normal = clr; // color data
                }

                verts[i * 6 + 0].TextureCoordinate = bottomLeft;
                verts[i * 6 + 1].TextureCoordinate = upperLeft;
                verts[i * 6 + 2].TextureCoordinate = bottomRight;
                verts[i * 6 + 3].TextureCoordinate = bottomRight;
                verts[i * 6 + 4].TextureCoordinate = upperLeft;
                verts[i * 6 + 5].TextureCoordinate = upperRight;
            }

            return verts;
        }

        public abstract Vector3[] ColorsToPositions(Color[] colors);

        public abstract VertexPositionNormalTexture[] MakeOutline();
    }
}
