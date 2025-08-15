using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace TerraMine
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // 3D компоненты
        private BasicEffect _basicEffect;
        private VertexBuffer _cubeVertexBuffer;
        private IndexBuffer _cubeIndexBuffer;
        private Matrix _world;
        private Matrix _view;
        private Matrix _projection;

        // Камера
        private Vector3 _cameraPosition;
        private Vector3 _cameraTarget;
        private Vector3 _cameraUp;
        private float _cameraYaw;
        private float _cameraPitch;
        private float _cameraSpeed;
        private float _mouseSensitivity;

        // Состояние мыши
        private MouseState _previousMouseState;
        private bool _firstMouse;
        private bool _mouseCaptured;
        private Point _windowCenter;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true; // Сначала мышь видна
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += (s, e) => UpdateWindowCenter();
            // Настройка графики
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
        }

        private void UpdateWindowCenter()
        {
            _windowCenter = new Point(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
        }

        private void CaptureMouse()
        {
            if (!_mouseCaptured)
            {
                _mouseCaptured = true;
                IsMouseVisible = false;
                UpdateWindowCenter();
                Mouse.SetPosition(Window.ClientBounds.Left + _windowCenter.X, Window.ClientBounds.Top + _windowCenter.Y);
                _firstMouse = true;
            }
        }

        protected override void Initialize()
        {
            // Инициализация камеры
            _cameraPosition = new Vector3(0, 0, 0);
            _cameraTarget = new Vector3(0, 0, 1);
            _cameraUp = Vector3.Up;
            _cameraYaw = 0f;
            _cameraPitch = 0f;
            _cameraSpeed = 5f;
            _mouseSensitivity = 0.002f;
            _firstMouse = true;
            _mouseCaptured = false;
            UpdateWindowCenter();
            // Инициализация матриц
            _world = Matrix.Identity;
            _view = Matrix.CreateLookAt(_cameraPosition, _cameraTarget, _cameraUp);
            _projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45f),
                GraphicsDevice.Viewport.AspectRatio,
                0.1f,
                1000f);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Создание BasicEffect для 3D рендеринга
            _basicEffect = new BasicEffect(GraphicsDevice);
            _basicEffect.World = _world;
            _basicEffect.View = _view;
            _basicEffect.Projection = _projection;
            _basicEffect.VertexColorEnabled = true;
            _basicEffect.LightingEnabled = false;

            // Создание куба
            CreateCube();
        }

        private void CreateCube()
        {
            // Вершины куба (8 вершин)
            VertexPositionColor[] vertices = new VertexPositionColor[8];

            // Передняя грань
            vertices[0] = new VertexPositionColor(new Vector3(-1, -1, 1), Color.Red);
            vertices[1] = new VertexPositionColor(new Vector3(1, -1, 1), Color.Green);
            vertices[2] = new VertexPositionColor(new Vector3(1, 1, 1), Color.Blue);
            vertices[3] = new VertexPositionColor(new Vector3(-1, 1, 1), Color.Yellow);

            // Задняя грань
            vertices[4] = new VertexPositionColor(new Vector3(-1, -1, -1), Color.Cyan);
            vertices[5] = new VertexPositionColor(new Vector3(1, -1, -1), Color.Magenta);
            vertices[6] = new VertexPositionColor(new Vector3(1, 1, -1), Color.White);
            vertices[7] = new VertexPositionColor(new Vector3(-1, 1, -1), Color.Orange);

            // Создание VertexBuffer
            _cubeVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);
            _cubeVertexBuffer.SetData(vertices);

            // Индексы для отрисовки граней куба (12 треугольников = 36 индексов)
            short[] indices = new short[36];

            // Передняя грань
            indices[0] = 0; indices[1] = 1; indices[2] = 2;
            indices[3] = 0; indices[4] = 2; indices[5] = 3;

            // Задняя грань
            indices[6] = 5; indices[7] = 4; indices[8] = 7;
            indices[9] = 5; indices[10] = 7; indices[11] = 6;

            // Левая грань
            indices[12] = 4; indices[13] = 0; indices[14] = 3;
            indices[15] = 4; indices[16] = 3; indices[17] = 7;

            // Правая грань
            indices[18] = 1; indices[19] = 5; indices[20] = 6;
            indices[21] = 1; indices[22] = 6; indices[23] = 2;

            // Верхняя грань
            indices[24] = 3; indices[25] = 2; indices[26] = 6;
            indices[27] = 3; indices[28] = 6; indices[29] = 7;

            // Нижняя грань
            indices[30] = 4; indices[31] = 5; indices[32] = 1;
            indices[33] = 4; indices[34] = 1; indices[35] = 0;

            // Создание IndexBuffer
            _cubeIndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            _cubeIndexBuffer.SetData(indices);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }
            if (!_mouseCaptured && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                CaptureMouse();
            }
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Обработка ввода клавиатуры (WASD)
            HandleKeyboardInput(deltaTime);
            // Обработка ввода мыши
            if (_mouseCaptured)
                HandleMouseInput();
            // Обновление матрицы вида
            UpdateViewMatrix();
            // Обновление BasicEffect
            _basicEffect.View = _view;
            // Куб всегда перед камерой
            UpdateCubeWorldMatrix();
            base.Update(gameTime);
        }

        private void HandleKeyboardInput(float deltaTime)
        {
            KeyboardState keyState = Keyboard.GetState();
            Vector3 forward = Vector3.Normalize(_cameraTarget - _cameraPosition);
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, _cameraUp));

            // Движение вперед (W)
            if (keyState.IsKeyDown(Keys.W))
            {
                _cameraPosition += forward * _cameraSpeed * deltaTime;
            }

            // Движение назад (S)
            if (keyState.IsKeyDown(Keys.S))
            {
                _cameraPosition -= forward * _cameraSpeed * deltaTime;
            }

            // Движение влево (A)
            if (keyState.IsKeyDown(Keys.A))
            {
                _cameraPosition -= right * _cameraSpeed * deltaTime;
            }

            // Движение вправо (D)
            if (keyState.IsKeyDown(Keys.D))
            {
                _cameraPosition += right * _cameraSpeed * deltaTime;
            }

            // Движение вверх (Space)
            if (keyState.IsKeyDown(Keys.Space))
            {
                _cameraPosition += _cameraUp * _cameraSpeed * deltaTime;
            }

            // Движение вниз (LeftShift)
            if (keyState.IsKeyDown(Keys.LeftShift))
            {
                _cameraPosition -= _cameraUp * _cameraSpeed * deltaTime;
            }
        }

        private void HandleMouseInput()
        {
            MouseState currentMouseState = Mouse.GetState();
            if (_firstMouse)
            {
                _previousMouseState = currentMouseState;
                _firstMouse = false;
                return;
            }
            float deltaX = currentMouseState.X - (_windowCenter.X + Window.ClientBounds.Left);
            float deltaY = currentMouseState.Y - (_windowCenter.Y + Window.ClientBounds.Top);
            _cameraYaw += deltaX * _mouseSensitivity;
            _cameraPitch -= deltaY * _mouseSensitivity;
            _cameraPitch = MathHelper.Clamp(_cameraPitch, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);
            // Сброс мыши в центр окна
            Mouse.SetPosition(Window.ClientBounds.Left + _windowCenter.X, Window.ClientBounds.Top + _windowCenter.Y);
            _previousMouseState = Mouse.GetState();
        }

        private void UpdateViewMatrix()
        {
            // Вычисление направления камеры на основе углов
            Vector3 direction = new Vector3(
                (float)(Math.Cos(_cameraPitch) * Math.Cos(_cameraYaw)),
                (float)Math.Sin(_cameraPitch),
                (float)(Math.Cos(_cameraPitch) * Math.Sin(_cameraYaw))
            );

            // Обновление цели камеры
            _cameraTarget = _cameraPosition + direction;

            // Создание матрицы вида
            _view = Matrix.CreateLookAt(_cameraPosition, _cameraTarget, _cameraUp);
        }

        private void UpdateCubeWorldMatrix()
        {
            // Куб на фиксированном расстоянии перед камерой (например, 5 единиц)
            Vector3 direction = Vector3.Normalize(_cameraTarget - _cameraPosition);
            Vector3 cubePos = _cameraPosition + direction * 5f;
            _world = Matrix.CreateTranslation(cubePos);
            _basicEffect.World = _world;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Настройка графического устройства для 3D
            GraphicsDevice.SetVertexBuffer(_cubeVertexBuffer);
            GraphicsDevice.Indices = _cubeIndexBuffer;

            // Отрисовка куба
            foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 12);
            }

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            _cubeVertexBuffer?.Dispose();
            _cubeIndexBuffer?.Dispose();
            _basicEffect?.Dispose();
            base.UnloadContent();
        }
    }
}
