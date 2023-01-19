using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class Tag : FlowPanel
    {
        private readonly Label text;
        private readonly ImageButton delete;
        private readonly ImageButton dummy;

        private Color disabledColor = new(156, 156, 156);
        private Texture2D disabledBackground;
        private AsyncTexture2D background;

        public Tag()
        {
            Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(1620622);
            WidthSizingMode = SizingMode.AutoSize;
            FlowDirection = ControlFlowDirection.SingleLeftToRight;
            OuterControlPadding = new Vector2(3, 3);
            ControlPadding = new Vector2(2, 0);
            AutoSizePadding = new Point(5, 3);
            Height = 26;

            delete = new ImageButton()
            {
                Parent = this,
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(156012),
                HoveredTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(156011),
                TextureRectangle = new Rectangle(4, 4, 24, 24),
                Size = new Point(20, 20),
                BasicTooltipText = string.Format(Strings.common.DeleteItem, Strings.common.Tag),
            };
            delete.Click += Delete_Click;

            dummy = new ImageButton()
            {
                Parent = this,
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(156025),
                TextureRectangle = new Rectangle(44, 48, 43, 46),
                Size = new Point(20, 20),
                Visible = false,
            };

            text = new Label()
            {
                Parent = this,
                AutoSizeWidth = true,
                Height = Height - (int)OuterControlPadding.Y,
            };
        }

        public event EventHandler Deleted;

        public BitmapFont Font
        {
            get { return text.Font; }
            set { text.Font = value; }
        }

        public bool Active { get; set; } = true;

        public bool CanInteract { get; set; } = true;

        public AsyncTexture2D Background
        {
            get => background;
            set
            {
                background = value;
                if (value != null)
                {
                    CreateDisabledBackground(null, null);
                    background.TextureSwapped += CreateDisabledBackground;
                }
            }
        }

        public bool ShowDelete
        {
            get => delete.Visible;
            set
            {
                if (delete != null)
                {
                    delete.Visible = value;
                    dummy.Visible = !value;
                }
            }
        }

        public string Text
        {
            get => text?.Text;
            set
            {
                if (text != null)
                {
                    text.Text = value;
                }
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (background != null)
            {
                AsyncTexture2D texture = Active ? background : disabledBackground != null ? disabledBackground : background;

                spriteBatch.DrawOnCtrl(this, texture, bounds, bounds, Active ? Color.White * 0.98f : disabledColor * 0.8f);
            }

            Color color = Color.Black;

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            if (CanInteract)
            {
                base.OnClick(e);
                Active = !Active;
            }
        }

        private void Delete_Click(object sender, MouseEventArgs e)
        {
            Deleted?.Invoke(this, EventArgs.Empty);
            Dispose();
        }

        private void CreateDisabledBackground(object sender, ValueChangedEventArgs<Texture2D> e)
        {
            disabledBackground = background.Texture.ToGrayScaledPalettable();
            background.TextureSwapped -= CreateDisabledBackground;
        }
    }
}
