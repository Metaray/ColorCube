using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ColorCube
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private Effect spatialColorEffect;
        private Effect particlesEffect;
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
        private ProjectionType projectionType = ProjectionType.Orthographic;

        private VertexPositionColor[] outlineVerts;
        private Color[] imageColors;
        private readonly ColorQuad baseQuad = new(0.5f);
        private VertexBuffer quadVertexBuffer;
        private IndexBuffer quadIndexBuffer;
        private VertexBuffer colorInstanceBuffer;
        private int forceDrawFrames;

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

            quadIndexBuffer = new IndexBuffer(GraphicsDevice, baseQuad.IndexType, baseQuad.IndexData.Length, BufferUsage.WriteOnly);
            quadIndexBuffer.SetData(baseQuad.IndexData);

            UpdateViewMatrixes();

            CalculateVertexData();

            base.Initialize();
        }

        private Vector2 GetDrawBufferSize()
        {
            return new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        }

        private void UpdateViewMatrixes()
        {
            Vector2 screenSize = GetDrawBufferSize();
            float aspect = screenSize.X / screenSize.Y;

            switch (projectionType)
            {
                case ProjectionType.Orthographic:
                {
                    const float OrthoSize = 400;

                    mView = Matrix.Identity;
                    mProjection = Matrix.CreateOrthographic(
                        OrthoSize * aspect, OrthoSize,
                        -300, 300
                    );
                    break;
                }
                case ProjectionType.Perspective:
                {
                    float fov = MathHelper.ToRadians(30);
                    const float CloseSlice = 300;

                    float distance = 255.0f / 2 + (CloseSlice / 2) / MathF.Tan(fov / 2);
                    mView = Matrix.CreateTranslation(0, 0, -distance);
                    mProjection = Matrix.CreatePerspectiveFieldOfView(
                        fov,
                        aspect,
                        distance * 0.5f,
                        distance * 2.0f
                    );
                    break;
                }
            }

            ScheduleRedraw();
        }

        private void SizeChangedHandler(object sender, EventArgs e)
        {
            UpdateViewMatrixes();
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

            spatialColorEffect = Content.Load<Effect>("spatialColor");
            particlesEffect = Content.Load<Effect>("particles");

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

                ScheduleRedraw();
            }

            if (lastMouseState.RightButton == ButtonState.Released
                && currentMouseState.RightButton == ButtonState.Pressed
                && interactiveRectangle.Contains(currentMouseState.Position))
            {
                SetBackgroundStyle(backgroundSelect switch
                {
                    BackgroundStyle.Black => BackgroundStyle.White,
                    BackgroundStyle.White => BackgroundStyle.Black,
                    _ => throw new NotImplementedException(),
                });
            }

            if (KeyPressed(Keys.F1))
            {
                SetColorDisplayMode(ColorsDisplayMode.RGB);
            }
            else if (KeyPressed(Keys.F2))
            {
                SetColorDisplayMode(ColorsDisplayMode.HSV);
            }
            else if (KeyPressed(Keys.F3))
            {
                SetColorDisplayMode(ColorsDisplayMode.XYY);
            }

            if (KeyPressed(Keys.D1))
            {
                SetProjectionType(ProjectionType.Orthographic);
            }
            else if (KeyPressed(Keys.D2))
            {
                SetProjectionType(ProjectionType.Perspective);
            }

            base.Update(gameTime);

            bool KeyPressed(Keys key)
            {
                return currentKeyboardState.IsKeyDown(key) && lastKeyboardState.IsKeyUp(key);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (forceDrawFrames <= 0)
            {
                return;
            }

            forceDrawFrames--;
            //Trace.WriteLine($"GT: {gameTime.ElapsedGameTime}");

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
            mWorld = Matrix.CreateTranslation(-127.5f, -127.5f, -127.5f) * Matrix.CreateRotationY(hAngle) * Matrix.CreateRotationX(vAngle);

            spatialColorEffect.Parameters["WorldViewProjection"].SetValue(mWorld * mView * mProjection);

            //particlesEffect.Parameters["WorldViewProjection"].SetValue(mWorld * mView * mProjection);
            particlesEffect.Parameters["WorldView"].SetValue(mWorld * mView);
            particlesEffect.Parameters["Projection"].SetValue(mProjection);
            //particlesEffect.Parameters["InvScreenSize"].SetValue(Vector2.One / GetDrawBufferSize());

            if (outlineVerts != null)
            {
                foreach (EffectPass pass in spatialColorEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, outlineVerts, 0, outlineVerts.Length / 2);
                }
            }

            if (colorInstanceBuffer != null)
            {
                GraphicsDevice.SetVertexBuffers(
                    new VertexBufferBinding(quadVertexBuffer, 0, 0),
                    new VertexBufferBinding(colorInstanceBuffer, 0, 1)
                );
                GraphicsDevice.Indices = quadIndexBuffer;

                foreach (EffectPass pass in particlesEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    
                    GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, quadIndexBuffer.IndexCount / 3, colorInstanceBuffer.VertexCount);
                }
            }

            base.Draw(gameTime);
        }

        private void SetBackgroundStyle(BackgroundStyle newStyle)
        {
            if (backgroundSelect != newStyle)
            {
                backgroundSelect = newStyle;
                ScheduleRedraw();
            }
        }

        private void SetProjectionType(ProjectionType newType)
        {
            if (projectionType != newType)
            {
                projectionType = newType;
                UpdateViewMatrixes();
            }
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
            PointCloud pointCloud = colorsDisplayMode switch
            {
                ColorsDisplayMode.RGB => new PointCloudRgb(),
                ColorsDisplayMode.HSV => new PointCloudHsv(),
                ColorsDisplayMode.XYY => new PointCloudXyy(),
                _ => throw new NotImplementedException()
            };

            outlineVerts = pointCloud.MakeOutline();

            if (imageColors != null)
            {
                VertexPositionColor[] particlesVerts = pointCloud.ColorsToVertexData(imageColors);

                if (colorInstanceBuffer == null || colorInstanceBuffer.VertexCount != particlesVerts.Length)
                {
                    colorInstanceBuffer = new VertexBuffer(
                        GraphicsDevice,
                        PointCloud.ParticleInstanceDeclaration,
                        particlesVerts.Length,
                        BufferUsage.WriteOnly
                    );
                }

                colorInstanceBuffer.SetData(particlesVerts);
            }

            ScheduleRedraw();
        }

        private void ScheduleRedraw()
        {
            // Two for double buffering to work correctly
            forceDrawFrames = 2;
        }

        private enum ColorsDisplayMode
        {
            RGB,
            HSV,
            XYY,
        }

        private enum BackgroundStyle
        {
            Black,
            White,
        }

        private enum ProjectionType
        {
            Orthographic,
            Perspective,
        }
    }
}
