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
    enum ActorType
    {
        Player
    };
    enum Gait
    {
        Walking,
        Running,
        Jumping,
        Swimming,
        Riding,
        Crawling,
        Flying
    };

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

        Gait Locomotion { get; set; }
        Vector2 Facing { get; set; }
        
        /// <summary>
        /// The physical radius of the actor is its distance from the center for purposes of collision
        /// </summary>
        float Radius { get; set; }
    }
    class StageActor : IActor
    {
        Texture2D Texture { get; set; }
        Rectangle Source { get; set; }
        Vector2 Center { get; set; }
        Gait Movement { get; set; }
        Vector2 Facing { get; set; }
        float Radius { get; set; }

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


        Gait IActor.Locomotion
        {
            get
            {
                return this.Movement;
            }
            set
            {
                this.Movement = value;
            }
        }
        Vector2 IActor.Facing
        {
            get
            {
                return this.Facing;
            }
            set
            {
                this.Facing = value;
            }
        }

        float IActor.Radius
        {
            get
            {
                return this.Radius;
            }
            set
            {
                this.Radius = value;
            }
        }
    }

    interface IStageService
    {
        IActor GetActorByID(int id);
        Vector2 GetActorLoc(IActor actor);
        int AddNewActor(ActorType type);
        void TeleportActor(IActor actor, Vector2 loc);
        void MoveActor(IActor actor, Vector2 movement);
    }

    class StageComponent : DrawableGameComponent, IStageService
    {
        const int DRAW_ORDER = 2;
        const int PLAYER_INDEX = 0;
        const float DEFAULT_RADIUS = 10;

        SpriteBatch SpriteBatch { get; set; }
        ICameraService CameraService { get; set; }
        IKeyService KeyService { get; set; }

        //Actor information storage
        List<IActor> _allActors;
        Dictionary<IActor, Vector2> _actorLocs;


        //Player links
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
        IActor Player 
        {
            get { return this._allActors[PLAYER_INDEX]; }
            set { this._allActors[PLAYER_INDEX] = value; }
        }

        //Command queue storage


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

            this._allActors = new List<IActor>(1);
            this._actorLocs = new Dictionary<IActor, Vector2>();
            //this._allActors.Add(null);

            base.Initialize();
        }


        protected override void LoadContent()
        {
            this.SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
            base.LoadContent();
        }
        int AddNewActor(string spriteSheet)
        {
            IActor a = new StageActor();
            a.Texture = Game.Content.Load<Texture2D>(spriteSheet);
            a.Source = new Rectangle(64, 0, 32, 32);
            a.Locomotion = Gait.Walking;
            a.Radius = DEFAULT_RADIUS;
            this._allActors.Add(a);
            this._actorLocs.Add(a, Vector2.Zero);
            return this._allActors.Count - 1;
        }

        public override void Update(GameTime gameTime)
        {
            //Center the camera
            this.CameraService.SetCenter(this.PlayerLoc);

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
        int IStageService.AddNewActor(ActorType type)
        {
            switch (type)
            {
                case ActorType.Player: return this.AddNewActor("Avatar");
                default: return -1;
            }
        }
        IActor IStageService.GetActorByID(int id)
        {
            return this._allActors[id];
        }
        Vector2 IStageService.GetActorLoc(IActor actor)
        {
            return this._actorLocs[actor];
        }
        void IStageService.TeleportActor(IActor actor, Vector2 loc)
        {
            this._actorLocs[actor] = loc;
        }
        void IStageService.MoveActor(IActor actor, Vector2 movement)
        {
            this._actorLocs[actor] += movement;
        }
        #endregion
    }
}
