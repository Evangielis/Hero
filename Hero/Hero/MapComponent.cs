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
    interface IMapService
    {
        Vector2 GetActualLoc(int x, int y);
        Vector2 GetActualLoc(Point p);
        Point GetGridRef(Vector2 v);

        bool IsWaterAt(int x, int y);
        bool IsWaterAt(Point p);
        
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
        //Dictionary<Feature, IMapFeature> _featureDict { get; set; }
        //Dictionary<Floor, IFloorTile> _floorDict { get; set; }
        ITileLibrary TileLibrary { get; set; }

        IMapFeature[,] _features;
        IFloorTile[,] _floors;

        SpriteBatch SpriteBatch { get; set; }
        ICameraService CameraService { get; set; }
        IStageService StageService { get; set; }
        /// <summary>
        /// This is a representation of the player loc using grid coordinates.
        /// </summary>
        #endregion

        public MapComponent(Game game)
            : base(game)
        {
            game.Components.Add(this);
            game.Services.AddService(typeof(IMapService), this);
        }

        public override void Initialize()
        {
            this.DrawOrder = DRAW_ORDER;

            this.TileLibrary = new TileLibrary(Game, TILE_SIZE);
            
            _features = new IMapFeature[MAX_MAP_SIZE, MAX_MAP_SIZE];
            _floors = new IFloorTile[MAX_MAP_SIZE, MAX_MAP_SIZE];

            this.CameraService = (ICameraService)Game.Services.GetService(typeof(ICameraService));
            this.StageService = (IStageService)Game.Services.GetService(typeof(IStageService));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            this.SpriteBatch = new SpriteBatch(Game.GraphicsDevice);

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        #region Draw Methods
        public override void Draw(GameTime gameTime)
        {
            //Optimize this
            Point p = this.GetGridRef(this.CameraService.Center);
            Rectangle drawBounds = new Rectangle(p.X, p.Y, DRAW_BUFFER * 4, DRAW_BUFFER * 2);
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
                    this._floors[i, j] = this.TileLibrary.GetFloorTile(Floor.Ocean);
                    this._features[i, j] = this.TileLibrary.GetFeature(Feature.None);
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
            this._floors[x, y] = this.TileLibrary.GetFloorTile(floor);
        }
        void IMapService.SetFeatureAt(int x, int y, Feature feature)
        {
            //Watch for meta features here.
            if (feature.Equals(Feature.StartPoint))
            {
                this.StageService.TeleportActor(this.StageService.GetActorByID(0), this.GetActualLoc(x, y));
                return;
            }

            this._features[x, y] = this.TileLibrary.GetFeature(feature);
        }
        bool IMapService.IsWaterAt(int x, int y)
        {
            return this.TileLibrary.IsWaterTerrain(this._floors[x, y].Type);
        }
        bool IMapService.IsWaterAt(Point p)
        {
            return this.TileLibrary.IsWaterTerrain(this._floors[p.X, p.Y].Type);
        }
        #endregion
    }

    #region MapTileLibrary stuff
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

    enum TerrainType
    {
        Grass,
        Saltwater,
        Sand
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
        TerrainType Type { get; set; }
    }
    class FloorTile : IFloorTile
    {
        Texture2D Texture { get; set; }
        Rectangle Source { get; set; }
        TerrainType Type { get; set; }

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
        TerrainType IFloorTile.Type
        {
            get
            {
                return this.Type;
            }
            set
            {
                this.Type = value;
            }
        }
    }
    interface ITileLibrary
    {
        IMapFeature GetFeature(Feature feature);
        IFloorTile GetFloorTile(Floor tile);
        bool IsWaterTerrain(TerrainType type);
    }
    class TileLibrary : ITileLibrary
    {
        int TILE_SIZE { get; set; }

        Game Game { get; set; }

        Dictionary<Feature, IMapFeature> _featureDict;
        Dictionary<Floor, IFloorTile> _floorDict;

        public TileLibrary(Game game, int tilesize)
        {
            this.Game = game;
            this.TILE_SIZE = tilesize;

            this.Initialize();
            this.LoadContent();
        }

        public void Initialize() 
        {
            _featureDict = new Dictionary<Feature, IMapFeature>();
            _floorDict = new Dictionary<Floor, IFloorTile>();
        }

        public void LoadContent()
        {
            //Features
            _featureDict.Add(Feature.None, null);

            //Floor tiles
            _floorDict.Add(Floor.None, null);
            _floorDict.Add(Floor.Grass, LoadFloorTile("Terrain", 0, 0, TerrainType.Grass));
            _floorDict.Add(Floor.Ocean, LoadFloorTile("Terrain", 1, 0, TerrainType.Saltwater));
        }
        IMapFeature LoadMapFeature(string spriteSheet, int sourceX, int sourceY)
        {
            Rectangle sourceRect = new Rectangle(sourceX * TILE_SIZE, sourceY * TILE_SIZE, TILE_SIZE, TILE_SIZE);
            IMapFeature feature = new MapFeature();
            feature.Texture = Game.Content.Load<Texture2D>(spriteSheet);
            feature.Source = sourceRect;
            return feature;
        }
        IFloorTile LoadFloorTile(string spriteSheet, int sourceX, int sourceY, TerrainType type)
        {
            Rectangle sourceRect = new Rectangle(sourceX * TILE_SIZE, sourceY * TILE_SIZE, TILE_SIZE, TILE_SIZE);
            IFloorTile tile = new FloorTile();
            tile.Texture = Game.Content.Load<Texture2D>(spriteSheet);
            tile.Source = sourceRect;
            tile.Type = type;
            return tile;
        }

        IMapFeature ITileLibrary.GetFeature(Feature feature)
        {
            return this._featureDict[feature];
        }
        IFloorTile ITileLibrary.GetFloorTile(Floor tile)
        {
            return this._floorDict[tile];
        }
        bool ITileLibrary.IsWaterTerrain(TerrainType type)
        {
            switch (type)
            {
                case TerrainType.Saltwater: return true;
                default: return false;
            }
        }
    }
    #endregion
}


