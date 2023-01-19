using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class SizeablePanel : Container
    {
        private const int RESIZEHANDLESIZE = 16;

        private readonly AsyncTexture2D resizeTexture = AsyncTexture2D.FromAssetId(156009);
        private readonly AsyncTexture2D resizeTextureHovered = AsyncTexture2D.FromAssetId(156010);

        private bool dragging;
        private bool resizing;
        private bool mouseOverResizeHandle;

        private Point resizeStart;
        private Point dragStart;
        private Point draggingStart;

        private Rectangle resizeHandleBounds = Rectangle.Empty;

        public SizeablePanel()
        {
        }

        public bool ShowResizeOnlyOnMouseOver { get; set; }

        public Point MaxSize { get; set; } = new Point(499, 499);

        public Color TintColor { get; set; } = Color.Black * 0.5f;

        public bool TintOnHover { get; set; }

        private Rectangle ResizeCorner
        {
            get => new(LocalBounds.Right - 15, LocalBounds.Bottom - 15, 15, 15);
        }

        public void ToggleVisibility() => Visible = !Visible;

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            dragging = dragging && MouseOver;
            resizing = resizing && MouseOver;
            mouseOverResizeHandle = mouseOverResizeHandle && MouseOver;

            if (dragging)
            {
                Location = Input.Mouse.Position.Add(new Point(-draggingStart.X, -draggingStart.Y));
            }

            if (resizing)
            {
                Point nOffset = Input.Mouse.Position - dragStart;
                Point newSize = resizeStart + nOffset;
                Size = new Point(MathHelper.Clamp(newSize.X, 50, MaxSize.X), MathHelper.Clamp(newSize.Y, 25, MaxSize.Y));
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            resizeHandleBounds = new Rectangle(
                Width - resizeTexture.Width,
                Height - resizeTexture.Height,
                resizeTexture.Width,
                resizeTexture.Height);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (TintOnHover && MouseOver)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    ContentService.Textures.Pixel,
                    bounds,
                    Rectangle.Empty,
                    TintColor,
                    0f,
                    default);
            }

            if (resizeTexture != null && (!ShowResizeOnlyOnMouseOver || MouseOver))
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    resizing || mouseOverResizeHandle ? resizeTextureHovered : resizeTexture,
                    new Rectangle(bounds.Right - resizeTexture.Width - 1, bounds.Bottom - resizeTexture.Height - 1, resizeTexture.Width, resizeTexture.Height),
                    resizeTexture.Bounds,
                    Color.White,
                    0f,
                    default);
            }

            // var color = MouseOver ? ContentService.Colors.ColonialWhite : Color.Transparent;
            Color color = ContentService.Colors.ColonialWhite;

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

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            base.OnLeftMouseButtonReleased(e);
            dragging = false;
            resizing = false;
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);

            resizing = ResizeCorner.Contains(e.MousePosition);
            resizeStart = Size;
            dragStart = Input.Mouse.Position;

            dragging = !resizing;
            draggingStart = dragging ? RelativeMousePosition : Point.Zero;
        }

        protected virtual Point HandleWindowResize(Point newSize) => new Point(
                MathHelper.Clamp(newSize.X, ContentRegion.X, 1024),
                MathHelper.Clamp(newSize.Y, ContentRegion.Y, 1024));

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            ResetMouseRegionStates();

            if (resizeHandleBounds.Contains(RelativeMousePosition)
                  && RelativeMousePosition.X > resizeHandleBounds.Right - RESIZEHANDLESIZE
                  && RelativeMousePosition.Y > resizeHandleBounds.Bottom - RESIZEHANDLESIZE)
            {
                mouseOverResizeHandle = true;
            }

            base.OnMouseMoved(e);
        }

        private void ResetMouseRegionStates() => mouseOverResizeHandle = false;
    }
}
