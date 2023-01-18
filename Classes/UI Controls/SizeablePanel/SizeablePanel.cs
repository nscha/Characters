namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    using Blish_HUD;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Color = Microsoft.Xna.Framework.Color;
    using Point = Microsoft.Xna.Framework.Point;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class SizeablePanel : Container
    {
        private bool _dragging;
        private bool _resizing;

        private const int RESIZEHANDLESIZE = 16;

        protected Rectangle ResizeHandleBounds { get; private set; } = Rectangle.Empty;

        private bool mouseOverResizeHandle;
        public bool ShowResizeOnlyOnMouseOver;
        public Point MaxSize = new Point(499, 499);

        private Point _resizeStart;
        private Point _dragStart;
        private Point _draggingStart;

        private Rectangle ResizeCorner
        {
            get => new Rectangle(this.LocalBounds.Right - 15, this.LocalBounds.Bottom - 15, 15, 15);
        }

        public Color TintColor = Color.Black * 0.5f;
        public bool TintOnHover;
        private readonly AsyncTexture2D _resizeTexture = AsyncTexture2D.FromAssetId(156009);
        private readonly AsyncTexture2D _resizeTextureHovered = AsyncTexture2D.FromAssetId(156010);

        public SizeablePanel()
        {
        }

        public void ToggleVisibility()
        {
            this.Visible = !this.Visible;
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            base.OnLeftMouseButtonReleased(e);
            this._dragging = false;
            this._resizing = false;
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);

            this._resizing = this.ResizeCorner.Contains(e.MousePosition);
            this._resizeStart = this.Size;
            this._dragStart = Input.Mouse.Position;

            this._dragging = !this._resizing;
            this._draggingStart = this._dragging ? this.RelativeMousePosition : Point.Zero;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            this._dragging = this._dragging && this.MouseOver;
            this._resizing = this._resizing && this.MouseOver;
            this.mouseOverResizeHandle = this.mouseOverResizeHandle && this.MouseOver;

            if (this._dragging)
            {
                this.Location = Input.Mouse.Position.Add(new Point(-this._draggingStart.X, -this._draggingStart.Y));
            }

            if (this._resizing)
            {
                var nOffset = Input.Mouse.Position - this._dragStart;
                var newSize = this._resizeStart + nOffset;
                this.Size = new Point(MathHelper.Clamp(newSize.X, 50, this.MaxSize.X), MathHelper.Clamp(newSize.Y, 25, this.MaxSize.Y));
            }
        }

        protected virtual Point HandleWindowResize(Point newSize)
        {
            return new Point(
                MathHelper.Clamp(newSize.X, this.ContentRegion.X, 1024),
                MathHelper.Clamp(newSize.Y, this.ContentRegion.Y, 1024));
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            this.ResetMouseRegionStates();

            if (this.ResizeHandleBounds.Contains(this.RelativeMousePosition)
                  && this.RelativeMousePosition.X > this.ResizeHandleBounds.Right - RESIZEHANDLESIZE
                  && this.RelativeMousePosition.Y > this.ResizeHandleBounds.Bottom - RESIZEHANDLESIZE)
            {
                this.mouseOverResizeHandle = true;
            }

            base.OnMouseMoved(e);
        }

        private void ResetMouseRegionStates()
        {
            this.mouseOverResizeHandle = false;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            this.ResizeHandleBounds = new Rectangle(
                this.Width - this._resizeTexture.Width,
                this.Height - this._resizeTexture.Height,
                this._resizeTexture.Width,
                this._resizeTexture.Height);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (this.TintOnHover && this.MouseOver)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    ContentService.Textures.Pixel,
                    bounds,
                    Rectangle.Empty,
                    this.TintColor,
                    0f,
                    default);
            }

            if (this._resizeTexture != null && (!this.ShowResizeOnlyOnMouseOver || this.MouseOver))
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    this._resizing || this.mouseOverResizeHandle ? this._resizeTextureHovered : this._resizeTexture,
                    new Rectangle(bounds.Right - this._resizeTexture.Width - 1, bounds.Bottom - this._resizeTexture.Height - 1, this._resizeTexture.Width, this._resizeTexture.Height),
                    this._resizeTexture.Bounds,
                    Color.White,
                    0f,
                    default);
            }

            // var color = MouseOver ? ContentService.Colors.ColonialWhite : Color.Transparent;
            var color = ContentService.Colors.ColonialWhite;

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
    }
}
