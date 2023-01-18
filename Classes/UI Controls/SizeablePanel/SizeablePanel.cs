using Blish_HUD;
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
    public class SizeablePanel : Container
    {
        bool _dragging;
        bool _resizing;

        private const int RESIZEHANDLE_SIZE = 16;
        protected Rectangle ResizeHandleBounds { get; private set; } = Rectangle.Empty;
        bool MouseOverResizeHandle;
        public bool ShowResizeOnlyOnMouseOver;
        public Point MaxSize = new Point(499, 499);

        Point _resizeStart;
        Point _dragStart;
        Point _draggingStart;
        Rectangle _resizeCorner
        {
            get => new Rectangle(LocalBounds.Right - 15, LocalBounds.Bottom - 15, 15, 15);
        }
        public Color TintColor = Color.Black * 0.5f;
        public bool TintOnHover;

        AsyncTexture2D _resizeTexture = AsyncTexture2D.FromAssetId(156009);
        AsyncTexture2D _resizeTextureHovered = AsyncTexture2D.FromAssetId(156010);

        public SizeablePanel()
        {

        }

        public void ToggleVisibility()
        {
            Visible = !Visible;
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            base.OnLeftMouseButtonReleased(e);
            _dragging = false;
            _resizing = false;
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);

            _resizing = _resizeCorner.Contains(e.MousePosition);
            _resizeStart = this.Size;
            _dragStart = Input.Mouse.Position;

            _dragging = !_resizing;
            _draggingStart = _dragging ? RelativeMousePosition : Point.Zero;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            _dragging = _dragging && MouseOver;
            _resizing = _resizing && MouseOver;
            MouseOverResizeHandle = MouseOverResizeHandle && MouseOver;

            if (_dragging)
            {
                Location = Input.Mouse.Position.Add(new Point(-_draggingStart.X, -_draggingStart.Y));
            }

            if (_resizing)
            {
                var nOffset = Input.Mouse.Position - _dragStart;
                var newSize = _resizeStart + nOffset;
                this.Size = new Point(MathHelper.Clamp(newSize.X, 50, MaxSize.X), MathHelper.Clamp(newSize.Y, 25, MaxSize.Y));
            }
        }
        protected virtual Point HandleWindowResize(Point newSize)
        {
            return new Point(MathHelper.Clamp(newSize.X, this.ContentRegion.X, 1024),
                             MathHelper.Clamp(newSize.Y, this.ContentRegion.Y, 1024));
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            ResetMouseRegionStates();

            if (this.ResizeHandleBounds.Contains(this.RelativeMousePosition)
                  && this.RelativeMousePosition.X > this.ResizeHandleBounds.Right - RESIZEHANDLE_SIZE
                  && this.RelativeMousePosition.Y > this.ResizeHandleBounds.Bottom - RESIZEHANDLE_SIZE)
            {
                this.MouseOverResizeHandle = true;
            }

            base.OnMouseMoved(e);
        }

        private void ResetMouseRegionStates()
        {
            this.MouseOverResizeHandle = false;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            this.ResizeHandleBounds = new Rectangle(this.Width - _resizeTexture.Width,
                                                    this.Height - _resizeTexture.Height,
                                                    _resizeTexture.Width,
                                                    _resizeTexture.Height);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (TintOnHover && MouseOver)
            {
                spriteBatch.DrawOnCtrl(this,
                        ContentService.Textures.Pixel,
                        bounds,
                        Rectangle.Empty,
                        TintColor,
                        0f,
                        default);
            }

            if (_resizeTexture != null && (!ShowResizeOnlyOnMouseOver || MouseOver))
            {
                spriteBatch.DrawOnCtrl(this,
                        _resizing || MouseOverResizeHandle ? _resizeTextureHovered :  _resizeTexture,
                        new Rectangle(bounds.Right - _resizeTexture.Width - 1, bounds.Bottom - _resizeTexture.Height - 1, _resizeTexture.Width, _resizeTexture.Height),
                        _resizeTexture.Bounds,
                        Color.White,
                        0f,
                        default);
            }

            //var color = MouseOver ? ContentService.Colors.ColonialWhite : Color.Transparent;
            var color = ContentService.Colors.ColonialWhite;

            //Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            //Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);
        }
    }
}
