﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using MonoGame.Extended.ViewportAdapters;
using System.Collections;
using System.Collections.Generic;

namespace Platformer
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Player player = new Player(); //Create an instance of our player class

        public List<Enemy> enemies = new List<Enemy>();
        public Chest goal = null;

        Camera2D camera = null; //Creates an instance of a 2D camera
        TiledMap map = null; //Creates an instance of a Tiled map
        TiledMapRenderer mapRenderer = null; // Creates an instance of what makes a Tiled map

        TiledMapTileLayer collisionLayer;
        public ArrayList allCollisionTiles = new ArrayList();
        public Sprite[,] levelGrid;

        public int tileHeight = 0;
        public int levelTileWidth = 0;
        public int levelTileHeight = 0;

        public Vector2 gravity = new Vector2(0, 1500);

        Song gameMusic;

        SpriteFont arialFont;
        int score = 0;
        int lives = 3;
        Texture2D heart = null;
        public Rectangle myMap;
        
        //public Rectangle canvas = new Rectangle();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferHeight = 900;
            graphics.PreferredBackBufferWidth = 1600;

            Content.RootDirectory = "Content";


        }

        protected override void Initialize()
        {
            //canvas.X = 0;
            //canvas.Y = 0;
            //canvas.Width = graphics.GraphicsDevice.Viewport.Width;
            //canvas.Height = graphics.GraphicsDevice.Viewport.Height;
            myMap.X = 0;
            myMap.Y = 0;
            myMap.Width = 6400; // each block is 64 by 64 px
            myMap.Height = 6400;


            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            player.Load(Content, this); // Call the 'Load' function in the Player class.
            //'this' basically means "pass all information in our class through as a variable"


            arialFont = Content.Load<SpriteFont>("Arial");
            heart = Content.Load<Texture2D>("heart");

            BoxingViewportAdapter viewportAdapter = new BoxingViewportAdapter(Window,
                                                        GraphicsDevice,
                                                        graphics.GraphicsDevice.Viewport.Width,
                                                        graphics.GraphicsDevice.Viewport.Height);

            camera = new Camera2D(viewportAdapter);
            camera.Position = new Vector2(0, graphics.GraphicsDevice.Viewport.Height);

            map = Content.Load<TiledMap>("Level1");
            mapRenderer = new TiledMapRenderer(GraphicsDevice);

            gameMusic = Content.Load<Song>("Background");
            MediaPlayer.Play(gameMusic);

            SetUpTiles();
            LoadObjects();
        }

        protected override void UnloadContent()
        {
        }


        public void SetUpTiles()
        {
            tileHeight = map.TileHeight;
            levelTileHeight = map.Height;
            levelTileWidth = map.Width;
            levelGrid = new Sprite[levelTileWidth, levelTileHeight];
            foreach (TiledMapTileLayer layer in map.TileLayers)
            {
                if (layer.Name == "Collision")
                {
                    collisionLayer = layer;
                }
            }

            int columns = 0;
            int rows = 0;
            int loopCount = 0;
            while (loopCount < collisionLayer.Tiles.Count)
            {
                if (collisionLayer.Tiles[loopCount].GlobalIdentifier != 0)
                {
                    Sprite tileSprite = new Sprite();
                    tileSprite.position.X = columns * tileHeight;
                    tileSprite.position.Y = rows * tileHeight;
                    tileSprite.width = tileHeight;
                    tileSprite.height = tileHeight;
                    tileSprite.UpdateHitBox();
                    allCollisionTiles.Add(tileSprite);
                    levelGrid[columns, rows] = tileSprite;
                }

                columns++;

                if (columns == levelTileWidth)
                {
                    columns = 0;
                    rows++;
                }

                loopCount++;
            }
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            player.Update(deltaTime); // Call the 'Update' from our Player class.

            foreach (Enemy enemy in enemies)
            {
                enemy.Update(deltaTime);
            }

            //This makes the camera follow the player's position, it also should center the player to the centr of the camera
            camera.Position = player.playerSprite.position - new Vector2(graphics.GraphicsDevice.Viewport.Width / 2,
                                                                            graphics.GraphicsDevice.Viewport.Height / 2);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clears anything previously drawn to the screen
            GraphicsDevice.Clear(Color.Gray);

            var viewMatrix = camera.GetViewMatrix();
            var projectionMatrix = Matrix.CreateOrthographicOffCenter(0,
                                                                        GraphicsDevice.Viewport.Width,
                                                                        GraphicsDevice.Viewport.Height,
                                                                        0, 0.0f, -1.0f);

            //Begin drawing
            spriteBatch.Begin(transformMatrix: viewMatrix);


            mapRenderer.Draw(map, ref viewMatrix, ref projectionMatrix);
            // Call the 'Draw' function from our Player class
            player.Draw(spriteBatch);
            goal.Draw(spriteBatch);



            foreach (Enemy enemy in enemies)
            {
                enemy.Draw(spriteBatch);
            }
            
            
            //Finish drawing
            spriteBatch.End();

            spriteBatch.Begin();
            spriteBatch.DrawString(arialFont, "Score : " + score.ToString(), new Vector2(20, 20), Color.Orange);


            int loopCount = 0;
            while(loopCount < lives)
            {
                spriteBatch.Draw(heart, new Vector2(GraphicsDevice.Viewport.Width - 80 - loopCount * 20, 20), Color.White);
                loopCount++;
            }
            spriteBatch.End();


            base.Draw(gameTime);
        }

        void LoadObjects()
        {
            foreach (TiledMapObjectLayer layer in map.ObjectLayers)
            {
                if (layer.Name == "Enemies")
                {
                    foreach (TiledMapObject thing in layer.Objects)
                    {
                        Enemy enemy = new Enemy();
                        Vector2 tiles = new Vector2((int)(thing.Position.X / tileHeight),
                            (int)(thing.Position.Y / tileHeight));
                        enemy.enemySprite.position = tiles * tileHeight;
                        enemy.Load(Content, this);
                        enemies.Add(enemy);
                    }
                }
                if (layer.Name == "Goal")
                {
                    TiledMapObject thing = layer.Objects[0];
                    if (thing != null)
                    {
                        Chest chest = new Chest();
                        chest.chestSprite.position = new Vector2(thing.Position.X, thing.Position.Y);
                        chest.Load(Content, this);
                        goal = chest;
                    }
                }
            }
        }



        //public float WhereOnMyMap(float maxSize, float positionOfTiles)
        //{
        //    float tileSize = 64;
        //    float position;
        //    position = positionOfTiles * tileSize;

        //    return position;
        //}
        //public float MyMapX(float maxSize, float amountOfTiles, float tileSize)
        //{
        //    return 0.0f;
        //}
    }
    /*public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Player player = new Player(); //Create an instance of our player class

        Camera2D camera = null; //Creates an instance of a 2D camera
        TiledMap map = null; //Creates an instance of a Tiled map
        TiledMapRenderer mapRenderer = null; // Creates an instance of what makes a Tiled map

        TiledMapTileLayer collisionLayer;
        public ArrayList allCollisionTiles = new ArrayList();
        public Sprite[,] levelGrid;

        public int tileHeight = 0;
        public int levelTileWidth = 0;
        public int levelTileHeight = 0;


        public Rectangle myMap;

        //public Rectangle canvas = new Rectangle();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferHeight = 900;
            graphics.PreferredBackBufferWidth = 1600;

            Content.RootDirectory = "Content";


        }

        protected override void Initialize()
        {
            //canvas.X = 0;
            //canvas.Y = 0;
            //canvas.Width = graphics.GraphicsDevice.Viewport.Width;
            //canvas.Height = graphics.GraphicsDevice.Viewport.Height;
            myMap.X = 0;
            myMap.Y = 0;
            myMap.Width = 6400; // each block is 64 by 64 px
            myMap.Height = 6400;


            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            player.Load(Content, this); // Call the 'Load' function in the Player class.
            //'this' basically means "pass all information in our class through as a variable"


            BoxingViewportAdapter viewportAdapter = new BoxingViewportAdapter(Window,
                                                        GraphicsDevice,
                                                        graphics.GraphicsDevice.Viewport.Width,
                                                        graphics.GraphicsDevice.Viewport.Height);

            camera = new Camera2D(viewportAdapter);
            camera.Position = new Vector2(0, graphics.GraphicsDevice.Viewport.Height);

            map = Content.Load<TiledMap>("Level2");
            mapRenderer = new TiledMapRenderer(GraphicsDevice);

            SetUpTiles();
        }

        protected override void UnloadContent()
        {
        }


        public void SetUpTiles()
        {
            tileHeight = map.TileHeight;
            levelTileHeight = map.Height;
            levelTileWidth = map.Width;
            levelGrid = new Sprite[levelTileWidth, levelTileHeight];
            foreach (TiledMapTileLayer layer in map.TileLayers)
            {
                if (layer.Name == "Collision")
                {
                    collisionLayer = layer;
                }
            }

            int columns = 0;
            int rows = 0;
            int loopCount = 0;
            while (loopCount < collisionLayer.Tiles.Count)
            {
                if (collisionLayer.Tiles[loopCount].GlobalIdentifier != 0)
                {
                    Sprite tileSprite = new Sprite();
                    tileSprite.position.X = columns * tileHeight;
                    tileSprite.position.Y = rows * tileHeight;
                    tileSprite.width = tileHeight;
                    tileSprite.height = tileHeight;
                    tileSprite.UpdateHitBox();
                    allCollisionTiles.Add(tileSprite);
                    levelGrid[columns, rows] = tileSprite;
                }

                columns++;

                if (columns == levelTileWidth)
                {
                    columns = 0;
                    rows++;
                }

                loopCount++;
            }
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            player.Update(deltaTime); // Call the 'Update' from our Player class.

            //This makes the camera follow the player's position, it also should center the player to the centr of the camera
            camera.Position = player.playerSprite.position - new Vector2(graphics.GraphicsDevice.Viewport.Width / 2,
                                                                            graphics.GraphicsDevice.Viewport.Height / 2);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clears anything previously drawn to the screen
            GraphicsDevice.Clear(Color.Gray);

            var viewMatrix = camera.GetViewMatrix();
            var projectionMatrix = Matrix.CreateOrthographicOffCenter(0,
                                                                        GraphicsDevice.Viewport.Width,
                                                                        GraphicsDevice.Viewport.Height,
                                                                        0, 0.0f, -1.0f);

            //Begin drawing
            spriteBatch.Begin(transformMatrix: viewMatrix);


            mapRenderer.Draw(map, ref viewMatrix, ref projectionMatrix);
            // Call the 'Draw' function from our Player class
            player.Draw(spriteBatch);
            //Finish drawing
            spriteBatch.End();

            base.Draw(gameTime);
        }

        //public float WhereOnMyMap(float maxSize, float positionOfTiles)
        //{
        //    float tileSize = 64;
        //    float position;
        //    position = positionOfTiles * tileSize;

        //    return position;
        //}
        //public float MyMapX(float maxSize, float amountOfTiles, float tileSize)
        //{
        //    return 0.0f;
        //}
    }*/
}
    /*public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Player player = new Player();

        Camera2D camera = null;
        TiledMap map = null;
        TiledMapRenderer mapRenderer = null;

        TiledMapTileLayer collisionLayer;
        public ArrayList allCollisionTiles = new ArrayList();
        public Sprite[,] levelGrid;

        public int tileHeight = 0;
        public int levelTileWidth = 0;
        public int levelTileHeight = 0;

        public Rectangle myMap;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferHeight = 900;
            graphics.PreferredBackBufferWidth = 1600;

            Content.RootDirectory = "Content";

        }

        
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            myMap.X = 0;
            myMap.Y = 0;
            myMap.Width = 6400;
            myMap.Height = 6400;


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            player.Load(Content, this);

            BoxingViewportAdapter viewportAdapter = new BoxingViewportAdapter(Window,
                GraphicsDevice,
                graphics.GraphicsDevice.Viewport.Width,
                graphics.GraphicsDevice.Viewport.Height);

            camera = new Camera2D(viewportAdapter);
            camera.Position = new Vector2(0, graphics.GraphicsDevice.Viewport.Height);

            map = Content.Load<TiledMap>("Level1");
            mapRenderer = new TiledMapRenderer(GraphicsDevice);

            SetUpTiles();

            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public void SetUpTiles()
        {
            tileHeight = map.Height;
            levelTileHeight = map.Height;
            levelTileWidth = map.Width;
            levelGrid = new Sprite[levelTileWidth, levelTileHeight];
            foreach(TiledMapTileLayer layer in map.TileLayers)
            {
                if (layer.Name == "Collision")
                {
                    collisionLayer = layer;
                }
            }

            int columns = 0;
            int rows = 0;
            int loopCount = 0;
            while (loopCount < collisionLayer.Tiles.Count)
            {
                if (collisionLayer.Tiles[loopCount].GlobalIdentifier != 0)
                {
                    Sprite tileSprite = new Sprite();
                    tileSprite.position.X = columns * tileHeight;
                    tileSprite.position.Y = rows * tileHeight;
                    tileSprite.width = tileHeight;
                    tileSprite.height = tileHeight;
                    tileSprite.UpdateHitBox();
                    allCollisionTiles.Add(tileSprite);
                    levelGrid[columns, rows] = tileSprite;
                }

                columns++;

                if (columns == levelTileWidth)
                {
                    columns = 0;
                    rows++;
                }

                loopCount++;

            }
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            player.Update(deltaTime);

            camera.Position = player.playerSprite.position - new Vector2(graphics.GraphicsDevice.Viewport.Width / 2,
                graphics.GraphicsDevice.Viewport.Height / 2);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            var viewMatrix = camera.GetViewMatrix();
            var projectionMatrix = Matrix.CreateOrthographicOffCenter(0,
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height,
                0, 0.0f, -1.0f);



            spriteBatch.Begin(transformMatrix: viewMatrix);

            mapRenderer.Draw(map, ref viewMatrix, ref projectionMatrix);


            player.Draw(spriteBatch);

            spriteBatch.End();
            

            base.Draw(gameTime);
        }
    }
}
*/