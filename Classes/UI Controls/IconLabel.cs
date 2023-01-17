﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Kenedia.Modules.Characters.Classes;
using Kenedia.Modules.Characters.Classes.UI_Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Kenedia.Modules.Characters.Classes.Data;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    public class IconLabel : Control
    {
        private string _Text;
        public string Text
        {
            get => _Text;
            set
            {
                _Text = value;
                UpdateLayout();
            }
        }
        public Color TextColor = Color.White;
        private AsyncTexture2D _Icon;
        public AsyncTexture2D Icon
        {
            get => _Icon;
            set
            {
                _Icon = value;
                if (value != null) UpdateLayout();
            }
        }
        private BitmapFont _Font = GameService.Content.DefaultFont14;
        public BitmapFont Font
        {
            get => _Font;
            set
            {
                _Font = value;
                if (value != null) UpdateLayout();
            }
        }

        public bool AutoSizeWidth;
        public bool AutoSizeHeight;

        public Rectangle TextureRectangle = Rectangle.Empty;
        private Rectangle IconRectangle = Rectangle.Empty;
        private Rectangle TextRectangle = Rectangle.Empty;

        void UpdateLayout()
        {
            Width = Math.Max((int) Font.MeasureString(Text).Width + 4 + (Icon == null ? 0 : Height + 5), Height);

            IconRectangle = Icon == null ? Rectangle.Empty : new Rectangle(2, 2, LocalBounds.Height - 4, LocalBounds.Height - 4);
            TextRectangle = new Rectangle(IconRectangle.Right + (Icon == null ? 0 : 5), 2, LocalBounds.Width - (IconRectangle.Right + (Icon == null ? 0 : 5) + 2), LocalBounds.Height - 4);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var texture = Icon;
            if(texture != null)
            {
                spriteBatch.DrawOnCtrl(this,
                                        texture,
                                        IconRectangle,
                                        TextureRectangle == Rectangle.Empty ? texture.Bounds : TextureRectangle,
                                        Color.White,
                                        0f,
                                        default);
            }

                spriteBatch.DrawStringOnCtrl(this,
                                        Text,
                                        Font,
                                        TextRectangle,
                                        TextColor,
                                        false,
                                        HorizontalAlignment.Left,
                                        VerticalAlignment.Middle
                                        );

        }
    }
}