using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Classes.MainWindow
{
    public class DraggingControl : Control
    {
        private CharacterControl _CharacterControl;
        public CharacterControl CharacterControl
        {
            get => _CharacterControl;
            set
            {
                _CharacterControl = value;
                Visible = value != null;
                if (Visible)
                {
                    Size = value.Size;
                    BackgroundColor = new Color(0,0,0,175);
                }
            }
        }
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Visible)
            {
                spriteBatch.DrawStringOnCtrl(this,
                                    CharacterControl.Character.Name,
                                    CharacterControl.NameFont,
                                    bounds,
                                    new Color(168 + 15 + 25, 143 + 20 + 25, 102 + 15 + 25, 255),
                                    false,
                                    HorizontalAlignment.Center,
                                    VerticalAlignment.Middle);
            }
        }

        public override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);

            if (CharacterControl != null)
            {
                var m = Input.Mouse;
                Location = new Point(m.Position.X - 15, m.Position.Y - 15);
            }
        }
    }
}
