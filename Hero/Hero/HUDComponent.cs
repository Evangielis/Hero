using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hero
{
    interface IPercentageBar
    {
        string Name { get; set; }
        Texture2D BarTexture { get; set; }
        Color Color { get; set; }
    }
    class PercBar : IPercentageBar
    {
        string Name { get; set; }
        Texture2D BarTexture { get; set; }
        Color Color { get; set; }

        string IPercentageBar.Name
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }
        Texture2D IPercentageBar.BarTexture
        {
            get
            {
                return this.BarTexture;
            }
            set
            {
                this.BarTexture = value;
            }
        }
        Color IPercentageBar.Color
        {
            get
            {
                return this.Color;
            }
            set
            {
                this.Color = value;
            }
        }
    }

    interface IHUDService
    {
        float HealthPerc { get; set; }
        float StaminaPerc { get; set; }
    }

    /// <summary>
    /// The HUD Component forms an isolated layer on top of the game.
    /// It is independent of all other game services and presents interactive
    /// methods which can be used to update its functions.
    /// </summary>
    class HUDComponent : DrawableGameComponent, IHUDService
    {
        const int DRAW_ORDER = 3;
        const int PERC_BAR_MAX = 200;
        const int PERC_BAR_HEIGHT = 16;

        SpriteBatch SpriteBatch { get; set; }

        IPercentageBar HealthBar { get; set; }
        IPercentageBar StaminaBar { get; set; }

        List<IPercentageBar> _allPercBars;
        Dictionary<IPercentageBar, Rectangle> _percBarAreas;
        Dictionary<IPercentageBar, float> _percBarValues;

        public HUDComponent(Game game)
            : base(game)
        {
            game.Components.Add(this);
            game.Services.AddService(typeof(IHUDService), this);
        }

        public override void Initialize()
        {
            this.DrawOrder = DRAW_ORDER;

            this._allPercBars = new List<IPercentageBar>();
            this._percBarAreas = new Dictionary<IPercentageBar, Rectangle>();
            this._percBarValues = new Dictionary<IPercentageBar, float>();

            base.Initialize();
        }
        void SetPercBarValue(IPercentageBar bar, float val)
        {
            float newval = MathHelper.Clamp(val, 0, 100);
            if (this._percBarValues[bar].Equals(newval))
                return;
            this._percBarValues[bar] = newval;
            
            //Change Area
            int newP = (int)(newval * PERC_BAR_MAX) / 100;
            int oldP = this._percBarAreas[bar].Width;
            Rectangle r = this._percBarAreas[bar];
            r.Inflate((newP - oldP) / 2, 0);
            r.Offset((newP - oldP) / 2, 0);
            this._percBarAreas[bar] = r;
        }

        protected override void LoadContent()
        {
            this.SpriteBatch = new SpriteBatch(Game.GraphicsDevice);

            //Load default bars
            this.HealthBar = this.LoadPercBar("Health", 0, 0, Color.Red);
            this.StaminaBar = this.LoadPercBar("Stamina", 0, 16, Color.Yellow);

            base.LoadContent();
        }
        IPercentageBar LoadPercBar(string name, int x, int y, Color color, int height = PERC_BAR_HEIGHT)
        {
            IPercentageBar bar = new PercBar();
            bar.Name = name;
            bar.BarTexture = Game.Content.Load<Texture2D>("Blank");
            bar.Color = color;
            this._allPercBars.Add(bar);
            this._percBarValues.Add(bar, 100);
            this._percBarAreas.Add(bar, new Rectangle(x, y, PERC_BAR_MAX, height));
            return bar;
        }

        public override void Draw(GameTime gameTime)
        {
            this.SpriteBatch.Begin(SpriteSortMode.BackToFront, null);
            
            foreach (IPercentageBar b in this._allPercBars) 
                DrawPercBar(b);


            this.SpriteBatch.End();

            base.Draw(gameTime);
        }
        void DrawPercBar(IPercentageBar bar)
        {
            this.SpriteBatch.Draw(bar.BarTexture, this._percBarAreas[bar], bar.Color);
        }

        #region IHUDService Implementation
        float IHUDService.HealthPerc
        {
            get
            {
                return this._percBarValues[this.HealthBar];
            }
            set
            {
                this.SetPercBarValue(this.HealthBar, value);
            }
        }

        float IHUDService.StaminaPerc
        {
            get
            {
                return this._percBarValues[this.StaminaBar];
            }
            set
            {
                this.SetPercBarValue(this.StaminaBar, value);
            }
        }
        #endregion
    }
}
