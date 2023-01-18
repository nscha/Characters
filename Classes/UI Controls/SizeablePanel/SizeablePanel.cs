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
        private bool dragging;
        private bool resizing;

        private const int RESIZEHANDLESIZE = 16;

        protected Rectangle ResizeHandleBounds { get; private set; } = Rectangle.Empty;

        private bool mouseOverResizeHandle;
        public bool ShowResizeOnlyOnMouseOver;
        public Point MaxSize = new Point(499, 499);

        private Point resizeStart;
        private Point dragStart;
        private Point draggingStart;

        private Rectangle ResizeCorner
        {
            get => new Rectangle(this.LocalBounds.Right - 15, this.LocalBounds.Bottom - 15, 15, 15);
        }

        public Color TintColor = Color.Black * 0.5f;
        public bool TintOnHover;
        private readonly AsyncTexture2D resizeTexture = AsyncTexture2D.FromAssetId(156009);
        private readonly AsyncTexture2D resizeTextureHovered = AsyncTexture2D.FromAssetId(156010);

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
            this.dragging = false;
            this.resizing = false;
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);

            this.resizing = this.ResizeCorner.Contains(e.MousePosition);
            this.resizeStart = this.Size;
            this.dragStart = Input.Mouse.Position;

            this.dragging = !this.resizing;
            this.draggingStart = this.dragging ? this.RelativeMousePosition : Point.Zero;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            this.dragging = this.dragging && this.MouseOver;
            this.resizing = this.resizing && this.MouseOver;
            this.mouseOverResizeHandle = this.mouseOverResizeHandle && this.MouseOver;

            if (this.dragging)
            {
                this.Location = Input.Mouse.Position.Add(new Point(-this.draggingStart.X, -this.draggingStart.Y));
            }

            if (this.resizing)
            {
                var nOffset = Input.Mouse.Position - this.dragStart;
                var newSize = this.resizeStart + nOffset;
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
                this.Width - this.resizeTexture.Width,
                this.Height - this.resizeTexture.Height,
                this.resizeTexture.Width,
                this.resizeTexture.Height);
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

            if (this.resizeTexture != null && (!this.ShowResizeOnlyOnMouseOver || this.MouseOver))
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    this.resizing || this.mouseOverResizeHandle ? this.resizeTextureHovered : this.resizeTexture,
                    new Rectangle(bounds.Right - this.resizeTexture.Width - 1, bounds.Bottom - this.resizeTexture.Height - 1, this.resizeTexture.Width, this.resizeTexture.Height),
                    this.resizeTexture.Bounds,
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
