using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Archidamas;

namespace Hero
{
    struct StatSheet
    {
        public int ID { get; set; }
        public int Health { get; set; }
    }

    interface IDirectorService
    {
    }

    /// <summary>
    /// The director component ties the stage and map together with game logic.
    /// It handles movement, actions and statistics.
    /// </summary>
    class DirectorComponent : GameComponent, IDirectorService
    {
        IKeyService KeyService { get; set; }
        IMapService MapService { get; set; }
        IStageService StageService { get; set; }
        ICameraService CameraService { get; set; }

        Dictionary<IActor, StatSheet> _statSheets;

        public DirectorComponent(Game game)
            : base(game)
        {
            game.Components.Add(this);
            game.Services.AddService(typeof(IDirectorService), this);
        }

        public override void Initialize()
        {
            this.KeyService = (IKeyService)Game.Services.GetService(typeof(IKeyService));
            this.MapService = (IMapService)Game.Services.GetService(typeof(IMapService));
            this.StageService = (IStageService)Game.Services.GetService(typeof(IStageService));
            this.CameraService = (ICameraService)Game.Services.GetService(typeof(ICameraService));

            //Initialize Player
            this._statSheets = new Dictionary<IActor, StatSheet>();
            int id = this.StageService.AddNewActor(ActorType.Player);
            StatSheet s = new StatSheet();
            s.ID = id;
            s.Health = 100;
            this._statSheets.Add(this.StageService.GetActorByID(id), s);

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            this.UpdatePlayerMovement(gameTime);
            base.Update(gameTime);
        }

        void UpdatePlayerMovement(GameTime gameTime)
        {
            IActor player = this.StageService.GetActorByID(0);

            //Calculate facing
            Dictionary<Keys, bool> ks = this.KeyService.KeyStatus;
            if (ks[Keys.W]) player.Facing = Vector2.UnitY * (-1);
            else if (ks[Keys.S]) player.Facing = Vector2.UnitY * (1);
            else if (ks[Keys.A]) player.Facing = Vector2.UnitX * (-1);
            else if (ks[Keys.D]) player.Facing = Vector2.UnitX * (1);
            else return;

            //Calculate collision bounds
            Vector2 proposed = player.Facing * 1F;
            Vector2 loc = this.StageService.GetActorLoc(player);
            Vector2 newloc = loc + proposed;
            Vector2 c1, c2 = Vector2.Zero;
            c1 = (player.Facing * player.Radius) + newloc;
            c2 = (player.Facing * player.Radius) + newloc;
            if (proposed.X == 0)
            {
                c1.X += player.Radius;
                c2.X -= player.Radius;
            }
            else if (proposed.Y == 0)
            {
                c1.Y += player.Radius;
                c2.Y -= player.Radius;
            }

            //Water check
            bool water = this.MapService.IsWaterAt(this.MapService.GetGridRef(c1))
                & this.MapService.IsWaterAt(this.MapService.GetGridRef(c2));
       
            if (proposed != Vector2.Zero & !water) 
                this.StageService.MoveActor(this.StageService.GetActorByID(0), Vector2.Normalize(proposed) * 2);
        }
        void CheckMapCollisionAt(int x, int y)
        {

        }
        void UpdateStatus(GameTime gameTime)
        {
        }
    }
}
