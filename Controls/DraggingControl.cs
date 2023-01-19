namespace Kenedia.Modules.Characters.Controls
{
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Color = Microsoft.Xna.Framework.Color;
    using Point = Microsoft.Xna.Framework.Point;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class DraggingControl : Control
    {
        private CharacterControl characterControl;

        public CharacterControl CharacterControl
        {
            get => this.characterControl;
            set
            {
                this.characterControl = value;
                this.Visible = value != null;
                if (this.Visible)
                {
                    this.Size = value.Size;
                    this.BackgroundColor = new Color(0, 0, 0, 175);
                }
            }
        }

        public override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);

            if (this.CharacterControl != null)
            {
                var m = Input.Mouse;
                this.Location = new Point(m.Position.X - 15, m.Position.Y - 15);
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this.Visible)
            {
                spriteBatch.DrawStringOnCtrl(
                    this,
                    this.CharacterControl.Character.Name,
                    this.CharacterControl.NameFont,
                    bounds,
                    new Color(168 + 15 + 25, 143 + 20 + 25, 102 + 15 + 25, 255),
                    false,
                    HorizontalAlignment.Center,
                    VerticalAlignment.Middle);
            }
        }
    }
}
