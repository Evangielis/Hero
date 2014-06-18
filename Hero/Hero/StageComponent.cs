using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Archidamas;
using Archidamas.Extensions;


namespace Hero
{
    interface IActor
    {
        /// <summary>
        /// The currently active sprite sheet
        /// </summary>
        Texture2D Texture { get; set; }

        /// <summary>
        /// The current source rectangle from the sprite sheet
        /// </summary>
        Rectangle Source { get; set; }

        /// <summary>
        /// Vector points to the center of the sprite from its top-left corner.  This is done because the actual location
        /// of the actor is the exact center of the sprite.
        /// </summary>
        Vector2 Center { get; }
    }
    class Actor : IActor
    {
        Texture2D Texture { get; set; }
        Rectangle Source { get; set; }
        Vector2 Center { get; set; }

        Texture2D IActor.Texture
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
        Rectangle IActor.Source
        {
            get
            {
                return this.Source;
            }
            set
            {
                this.Source = value;
                this.Center = new Vector2(value.Width / 2, value.Height / 2);
            }
        }

        Vector2 IActor.Center
        {
            get { return this.Center; }
        }
    }

    interface IStageService
    {
        Vector2 PlayerLoc { get; }
        void TeleportPlayer(Vector2 loc);
    }

    class StageComponent : DrawableGameComponent, IStageService
    {
        const int DRAW_ORDER = 2;

        SpriteBatch SpriteBatch { get; set; }
        ICameraService CameraService { get; set; }
        IKeyService KeyService { get; set; }

        Dictionary<IActor, Vector2> _actorLocs;

        Vector2 PlayerLoc 
        {
            get
            {
                return this._actorLocs[this.Player];
            }
            set
            {
                this._actorLocs[this.Player] = value;
            }
        }
        IActor Player { get; set; }

        public StageComponent(Game game)
            : base(game)
        {
            game.Components.Add(this);
            game.Services.AddService(typeof(IStageService), this);
        }

        public override void Initialize()
        {
            this.DrawOrder = DRAW_ORDER;

            this.CameraService = (ICameraService)Game.Services.GetService(typeof(ICameraService));
            this.KeyService = (IKeyService)Game.Services.GetService(typeof(IKeyService));

            this._actorLocs = new Dictionary<IActor, Vector2>();

            this.Player = new Actor();
            this.PlayerLoc = Vector2.Zero;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            this.SpriteBatch = new SpriteBatch(Game.GraphicsDevice);

            this.Player.Texture = Game.Content.Load<Texture2D>("Avatar");
            this.Player.Source = new Rectangle(0, 0, 32, 32);

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            //Center the camera
            this.CameraService.SetCenter(this.PlayerLoc);

            //Calculate movement
            Vector2 proposed = Vector2.Zero;
            Dictionary<Keys, bool> ks = this.KeyService.KeyStatus;
            if (ks[Keys.W]) proposed += Vector2.UnitY * (-1);
            if (ks[Keys.S]) proposed += Vector2.UnitY * (1);
            if (ks[Keys.A]) proposed += Vector2.UnitX * (-1);
            if (ks[Keys.D]) proposed += Vector2.UnitX * (1);

            this.PlayerLoc += proposed;

            base.Update(gameTime);
        }

        #region Draw Methods
        public override void Draw(GameTime gameTime)
        {
            this.SpriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, this.CameraService.TranslationMatrix);

            foreach (IActor a in _actorLocs.Keys)
            {
                this.DrawActor(a);
            }

            this.SpriteBatch.End();

            base.Draw(gameTime);
        }
        void DrawActor(IActor actor)
        {
            this.DrawActor(actor.Texture, actor.Source, GraphicsExt.GetDrawLayer(DrawLayer.Actor), this._actorLocs[actor], actor.Center);
        }
        void DrawActor(Texture2D texture, Rectangle source, float layer, Vector2 loc, Vector2 center)
        {
            this.SpriteBatch.Draw(texture, loc, source, Color.White, 0, center, 1F ,SpriteEffects.None, layer);
        }
        #endregion

        #region IStageService Implementation
        Vector2 IStageService.PlayerLoc
        {
            get { return this.PlayerLoc; }
        }
        void IStageService.TeleportPlayer(Vector2 loc)
        {
            this.PlayerLoc = loc;
        }
        #endregion
    }
}
