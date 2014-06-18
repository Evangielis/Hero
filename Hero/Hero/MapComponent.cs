using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Archidamas;
using Archidamas.Extensions;

namespace Hero
{
    enum Feature
    {
        None,
        StartPoint,
    };

    enum Floor
    {
        None,
        Ocean,
        Grass
    };

    interface IMapFeature
    {
        Texture2D Texture { get; set; }
        Rectangle Source { get; set; }
    }
    class MapFeature : IMapFeature
    {
        Texture2D Texture { get; set; }
        Rectangle Source { get; set; }

        Texture2D IMapFeature.Texture
        {
            get
            {
                return this.Texture;
            }
            set
            {
                this.Texture = value;
            }
        }

        Rectangle IMapFeature.Source
        {
            get
            {
                return this.Source;
            }
            set
            {
                this.Source = value;
            }
        }
    }
    interface IFloorTile
    {
        Texture2D Texture { get; set; }
        Rectangle Source { get; set; }
    }
    class FloorTile : IFloorTile
    {
        Texture2D Texture { get; set; }
        Rectangle Source { get; set; }

        Texture2D IFloorTile.Texture
        {
            get
            {
                return this.Texture;
            }
            set
            {
                this.Texture = value;
            }
        }
        Rectangle IFloorTile.Source
        {
            get
            {
                return this.Source;
            }
            set
            {
                this.Source = value;
            }
        }
    }

    interface IMapService
    {
        Vector2 GetActualLoc(int x, int y);
        Vector2 GetActualLoc(Point p);
        Point GetGridRef(Vector2 v);
        //Point[] GetGridRange(Rectangle bounds);
        
        void SetFloorAt(int x, int y, Floor floor);
        void SetFeatureAt(int x, int y, Feature feature);
    }

    class MapComponent : DrawableGameComponent, IMapService
    {
        const int DRAW_ORDER = 1;
        const int MAX_MAP_SIZE = 1000;
        const int TILE_SIZE = 32;
        const int DRAW_BUFFER = 10;

        #region Properties
        int MapSize { get; set; }

        //Dictionaries
        Dictionary<Feature, IMapFeature> _featureDict;
        Dictionary<Floor, IFloorTile> _floorDict;

        IMapFeature[,] _features;
        IFloorTile[,] _floors;

        SpriteBatch SpriteBatch { get; set; }
        ICameraService CameraService { get; set; }
        IStageService StageService { get; set; }
        /// <summary>
        /// This is a representation of the player loc using grid coordinates.
        /// </summary>
        Point PlayerGridLoc 
        { 
            get
            {
                return this.GetGridRef(this.StageService.PlayerLoc);
            }
        }
        #endregion

        public MapComponent(Game game)
            : base(game)
        {
            game.Components.Add(this);
        }

        public override void Initialize()
        {
            this.DrawOrder = DRAW_ORDER;

            _featureDict = new Dictionary<Feature, IMapFeature>();
            _floorDict = new Dictionary<Floor, IFloorTile>();

            _features = new IMapFeature[MAX_MAP_SIZE, MAX_MAP_SIZE];
            _floors = new IFloorTile[MAX_MAP_SIZE, MAX_MAP_SIZE];

            this.SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
            this.CameraService = (ICameraService)Game.Services.GetService(typeof(ICameraService));
            this.StageService = (IStageService)Game.Services.GetService(typeof(IStageService));

            base.Initialize();
        }

        #region Content Methods
        protected override void LoadContent()
        {
            //Features
            _featureDict.Add(Feature.None, null);

            //Floor tiles
            _floorDict.Add(Floor.None, null);
            _floorDict.Add(Floor.Grass, LoadFloorTile("Terrain", 0, 0));
            _floorDict.Add(Floor.Ocean, LoadFloorTile("Terrain", 1, 0));

            base.LoadContent();
        }
        IMapFeature LoadMapFeature(string spriteSheet, int sourceX, int sourceY)
        {
            Rectangle sourceRect = new Rectangle(sourceX * TILE_SIZE, sourceY * TILE_SIZE, TILE_SIZE, TILE_SIZE);
            IMapFeature feature = new MapFeature();
            feature.Texture = Game.Content.Load<Texture2D>(spriteSheet);
            feature.Source = sourceRect;
            return feature;
        }
        IFloorTile LoadFloorTile(string spriteSheet, int sourceX, int sourceY)
        {
            Rectangle sourceRect = new Rectangle(sourceX * TILE_SIZE, sourceY * TILE_SIZE, TILE_SIZE, TILE_SIZE);
            IFloorTile tile = new FloorTile();
            tile.Texture = Game.Content.Load<Texture2D>(spriteSheet);
            tile.Source = sourceRect;
            return tile;
        }
        #endregion

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        #region Draw Methods
        public override void Draw(GameTime gameTime)
        {
            //Optimize this
            Rectangle drawBounds = new Rectangle(PlayerGridLoc.X, PlayerGridLoc.Y, DRAW_BUFFER * 4, DRAW_BUFFER * 2);
            drawBounds.Offset(-2 * DRAW_BUFFER, -1 * DRAW_BUFFER);

            this.SpriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, this.CameraService.TranslationMatrix);
                for (int i = drawBounds.Left; i < drawBounds.Right; i++)
                {
                    for (int j = drawBounds.Top; j < drawBounds.Bottom; j++)
                    {
                        DrawFloor(this._floors[i, j], i, j);
                        DrawFeature(this._features[i, j], i, j);
                    }
                }
            this.SpriteBatch.End();
            base.Draw(gameTime);
        }
        void DrawFloor(IFloorTile f, int x, int y)
        {
            if (f != null) DrawTile(f.Texture, f.Source, GraphicsExt.GetDrawLayer(DrawLayer.Floor), x, y);
        }
        void DrawFeature(IMapFeature f, int x, int y)
        {
            if (f != null) DrawTile(f.Texture, f.Source, GraphicsExt.GetDrawLayer(DrawLayer.Feature), x, y);
        }
        void DrawTile(Texture2D texture, Rectangle source, float layer, int x, int y)
        {
            this.SpriteBatch.Draw(texture, new Rectangle(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE),
                source, Color.White, 0, Vector2.Zero, SpriteEffects.None, layer);
        }
        #endregion

        public void InitializeMap()
        {
            //Load features and floors
            for (int i = 0; i < MAX_MAP_SIZE; i++)
            {
                for (int j = 0; j < MAX_MAP_SIZE; j++)
                {
                    this._floors[i, j] = this._floorDict[Floor.Ocean];
                    this._features[i, j] = this._featureDict[Feature.None];
                }
            }
        }

        /// <summary>
        /// Gets the Vector2 pointing to the actual location of the center of a gridref tile.
        /// </summary>
        /// <param name="x">The gridref tile X</param>
        /// <param name="y">The gridref tile Y</param>
        /// <returns>A Vector2 pointing to the center of the desired gridref tile</returns>
        Vector2 GetActualLoc(int x, int y)
        {
            return new Vector2((x * TILE_SIZE) + TILE_SIZE/2, (y * TILE_SIZE) + TILE_SIZE/2);
        }
        Vector2 GetActualLoc(Point p)
        {
            return this.GetActualLoc(p.X, p.Y);
        }
        Point GetGridRef(Vector2 v)
        {
            int x = (int)v.X / TILE_SIZE;
            int y = (int)v.Y / TILE_SIZE;
            return new Point(x, y);
        }
        
        #region IMapService Implementation
        Vector2 IMapService.GetActualLoc(int x, int y)
        {
            return this.GetActualLoc(x, y);
        }
        Vector2 IMapService.GetActualLoc(Point p)
        {
            return this.GetActualLoc(p);
        }
        Point IMapService.GetGridRef(Vector2 v)
        {
            return this.GetGridRef(v);
        }
        void IMapService.SetFloorAt(int x, int y, Floor floor)
        {
            this._floors[x, y] = this._floorDict[floor];
        }
        void IMapService.SetFeatureAt(int x, int y, Feature feature)
        {
            //Watch for meta features here.
            if (feature.Equals(Feature.StartPoint))
            {
                this.StageService.TeleportPlayer(this.GetActualLoc(x, y));
                return;
            }

            this._features[x, y] = this._featureDict[feature];
        }
        #endregion
    }
}
