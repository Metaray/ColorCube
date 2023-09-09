using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ColorCube
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private Effect spatialColorEffect;
        private Matrix mWorld;
        private Matrix mView;
        private Matrix mProjection;

        private MouseState currentMouseState;
        private MouseState lastMouseState;
        private KeyboardState currentKeyboardState;
        private KeyboardState lastKeyboardState;
        private bool isMovingView;

        private float vAngle = 0;
        private float hAngle = MathF.PI;
        private BackgroundStyle backgroundSelect = BackgroundStyle.Black;
        private ColorsDisplayMode colorsDisplayMode = ColorsDisplayMode.RGB;

        private VertexPositionColor[] outlineVerts;
        private Color[] imageColors;
        private readonly ColorQuad baseQuad = new(2.0f);
        private VertexBuffer quadVertexBuffer;
        private IndexBuffer quadIndexBuffer;
        private VertexBuffer colorInstanceBuffer;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 900,
                PreferredBackBufferHeight = 800,
                //GraphicsProfile = GraphicsProfile.HiDef,
                //PreferMultiSampling = true,
                //SynchronizeWithVerticalRetrace = false,
            };

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60);

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += SizeChangedHandler;

            Window.FileDrop += OnFileDrop;
        }

        protected override void Initialize()
        {
            lastMouseState = currentMouseState = Mouse.GetState();
            lastKeyboardState = currentKeyboardState = Keyboard.GetState();

            quadVertexBuffer = new VertexBuffer(GraphicsDevice, baseQuad.VertexDeclaration, baseQuad.VertexData.Length, BufferUsage.WriteOnly);
            quadVertexBuffer.SetData(baseQuad.VertexData);
            quadIndexBuffer = new IndexBuffer(GraphicsDevice, baseQuad.IndexData[0].GetType(), baseQuad.IndexData.Length, BufferUsage.WriteOnly);
            quadIndexBuffer.SetData(baseQuad.IndexData);

            mProjection = CreateProjectionMatrix();

            CalculateVertexData();

            base.Initialize();
        }

        private Vector2 GetDrawBufferSize()
        {
            return new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
        }

        private Matrix CreateProjectionMatrix()
        {
            Vector2 screenSize = GetDrawBufferSize();
            float aspect = screenSize.X / screenSize.Y;

            //return Matrix.CreatePerspectiveFieldOfView(
            //    MathHelper.ToRadians(80f),
            //    aspect,
            //    1f, 550f
            //);

            const float orthoSize = 400;
            return Matrix.CreateOrthographic(
                orthoSize * aspect, orthoSize,
                0, 550
            );
        }

        private void SizeChangedHandler(object sender, EventArgs e)
        {
            mProjection = CreateProjectionMatrix();
        }

        private void OnFileDrop(object sender, FileDropEventArgs e)
        {
            if (e.Files.Length > 0)
            {
                ScheduleLoadImage(e.Files[0]);
            }
        }

        private void ScheduleLoadImage(string path)
        {
            Task.Run(() =>
            {
                try
                {
                    Trace.WriteLine("Getting Colors");
                    imageColors = ColorUtils.ImageUniqueColors(path);

                    Trace.WriteLine($"Image has {imageColors.Length} colors");
                    CalculateVertexData();
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            });
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            spatialColorEffect = Content.Load<Effect>("mainshader");

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length >= 2)
            {
                ScheduleLoadImage(args[1]);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            lastMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
            lastKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            if (!IsActive)
            {
                return;
            }

            const int DeadzoneSize = 5;
            var interactiveRectangle = new Rectangle(
                DeadzoneSize,
                DeadzoneSize,
                Window.ClientBounds.Width - DeadzoneSize * 2,
                Window.ClientBounds.Height - DeadzoneSize * 2
            );

            if (currentMouseState.LeftButton == ButtonState.Released)
            {
                isMovingView = false;
            }
            else if (lastMouseState.LeftButton == ButtonState.Released
                && interactiveRectangle.Contains(currentMouseState.Position)
                && interactiveRectangle.Contains(lastMouseState.Position))
            {
                isMovingView = true;
            }

            if (isMovingView)
            {
                const float SpeedRatio = MathF.PI / 300; // radians/pixel

                int dx = currentMouseState.X - lastMouseState.X;
                int dy = currentMouseState.Y - lastMouseState.Y;

                hAngle += MathF.Abs(vAngle) <= MathF.PI * 0.5f || MathF.Abs(vAngle) >= MathF.PI * 1.5f
                    ? dx * SpeedRatio 
                    : dx * -SpeedRatio;
                hAngle %= MathF.PI * 2;

                vAngle += dy * SpeedRatio;
                //vAngle %= MathF.PI * 2;
                vAngle = Math.Min(Math.Max(vAngle, -MathF.PI / 2), MathF.PI / 2);
            }

            if (lastMouseState.RightButton == ButtonState.Released
                && currentMouseState.RightButton == ButtonState.Pressed
                && interactiveRectangle.Contains(currentMouseState.Position))
            {
                backgroundSelect = backgroundSelect switch
                {
                    BackgroundStyle.Black => BackgroundStyle.White,
                    BackgroundStyle.White => BackgroundStyle.Black,
                    _ => throw new NotImplementedException(),
                };
            }

            if (currentKeyboardState.IsKeyDown(Keys.F1) && lastKeyboardState.IsKeyUp(Keys.F1))
            {
                SetColorDisplayMode(ColorsDisplayMode.RGB);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.F2) && lastKeyboardState.IsKeyUp(Keys.F2))
            {
                SetColorDisplayMode(ColorsDisplayMode.HSV);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!IsActive)
            {
                return;
            }

            GraphicsDevice.Clear(
                backgroundSelect switch
                {
                    BackgroundStyle.Black => Color.Black,
                    BackgroundStyle.White => Color.White,
                    _ => throw new NotImplementedException(),
                }
            );

            // Camera looks into -Z (XY aligned with right+down, Z points to viewer, left hand rule)
            // 1) Center cube at 0,0,0
            // 2) Rotate horizontally
            // 3) Tilt (rotate) vertically
            mWorld = Matrix.CreateTranslation(-127.5f, -127.5f, -127.5f);
            mWorld *= Matrix.CreateRotationY(hAngle) * Matrix.CreateRotationX(vAngle);

            // 3) Place camera behind object
            mView = Matrix.CreateTranslation(0, 0, -300);

            spatialColorEffect.Parameters["WorldViewProjection"].SetValue(mWorld * mView * mProjection);
            spatialColorEffect.Parameters["InvScreenSize"].SetValue(Vector2.One / GetDrawBufferSize());

            EffectTechnique coloredVertexesTech = spatialColorEffect.Techniques.First(t => t.Name == "ColoredVertexes");
            EffectTechnique colorParticlesTech = spatialColorEffect.Techniques.First(t => t.Name == "ColorParticles");

            if (outlineVerts != null)
            {
                foreach (EffectPass pass in coloredVertexesTech.Passes)
                {
                    pass.Apply();

                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, outlineVerts, 0, outlineVerts.Length / 2);
                }
            }

            if (colorInstanceBuffer != null)
            {
                foreach (EffectPass pass in colorParticlesTech.Passes)
                {
                    pass.Apply();

                    GraphicsDevice.SetVertexBuffers(
                        new VertexBufferBinding(quadVertexBuffer, 0, 0),
                        new VertexBufferBinding(colorInstanceBuffer, 0, 1)
                    );
                    GraphicsDevice.Indices = quadIndexBuffer;
                    GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, quadIndexBuffer.IndexCount / 3, colorInstanceBuffer.VertexCount);
                }
            }

            base.Draw(gameTime);
        }

        private void SetColorDisplayMode(ColorsDisplayMode newMode)
        {
            if (newMode != colorsDisplayMode)
            {
                colorsDisplayMode = newMode;
                CalculateVertexData();
            }
        }

        private void CalculateVertexData()
        {
            outlineVerts = colorsDisplayMode switch
            {
                ColorsDisplayMode.RGB => new PointCloudRgb().MakeOutline(),
                ColorsDisplayMode.HSV => new PointCloudHsv().MakeOutline(),
                _ => throw new NotImplementedException()
            };

            if (imageColors != null)
            {
                VertexPositionColor[] particlesVerts = colorsDisplayMode switch
                {
                    ColorsDisplayMode.RGB => new PointCloudRgb().ColorsToVertexData(imageColors),
                    ColorsDisplayMode.HSV => new PointCloudHsv().ColorsToVertexData(imageColors),
                    _ => throw new NotImplementedException()
                };

                if (colorInstanceBuffer == null || colorInstanceBuffer.VertexCount != particlesVerts.Length)
                {
                    colorInstanceBuffer = new VertexBuffer(
                        GraphicsDevice,
                        VertexPositionColor.VertexDeclaration,
                        particlesVerts.Length,
                        BufferUsage.WriteOnly
                    );
                }

                colorInstanceBuffer.SetData(particlesVerts);
            }
        }

        private enum ColorsDisplayMode
        {
            RGB,
            HSV,
        }

        private enum BackgroundStyle
        {
            Black,
            White,
        }
    }
}
