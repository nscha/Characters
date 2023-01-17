using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Classes;
using Kenedia.Modules.Characters.Classes.UI_Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    public class ImageTextureToggle : Control
    {
        public AsyncTexture2D ActiveTexture;
        public AsyncTexture2D InactiveTexture;
        public string ActiveText;
        public string InactiveText;

        private bool Active;

        public Rectangle TextureRectangle;

        public Color ColorHovered = new Color(255, 255, 255, 255);
        public Color ColorActive = new Color(200, 200, 200, 255);
        public Color ColorInactive = new Color(200, 200, 200, 255);

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (ActiveTexture != null)
            {
                var texture = Active ? ActiveTexture : InactiveTexture != null ? InactiveTexture : ActiveTexture;

                spriteBatch.DrawOnCtrl(this,
                                        texture,
                                        new Rectangle(bounds.Left, bounds.Top, bounds.Height, bounds.Height),
                                        TextureRectangle == Rectangle.Empty ? texture.Bounds : TextureRectangle,
                                        MouseOver ? ColorHovered : Active ? ColorActive : ColorInactive,
                                        0f,
                                        default);

            }

            if (ActiveText != null)
            {
                var text = Active ? ActiveText : InactiveText != null ? InactiveText : ActiveText;

                spriteBatch.DrawStringOnCtrl(this,
                                        text,
                                        GameService.Content.DefaultFont14,
                                        new Rectangle(bounds.Left + bounds.Height + 3, bounds.Top, bounds.Width - bounds.Height - 3, bounds.Height),
                                        Color.White,
                                        false,
                                        false,
                                        0,
                                        HorizontalAlignment.Left,
                                        VerticalAlignment.Middle);
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            Active = !Active;
        }
    }
}

