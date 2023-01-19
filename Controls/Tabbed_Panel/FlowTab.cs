namespace Kenedia.Modules.Characters.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Blish_HUD.Controls;
    using Microsoft.Xna.Framework;
    using Point = Microsoft.Xna.Framework.Point;

    public class FlowTab : PanelTab
    {
        private ControlFlowDirection flowDirection = ControlFlowDirection.LeftToRight;
        private Vector2 outerControlPadding = Vector2.Zero;
        private Vector2 controlPadding = Vector2.Zero;

        public ControlFlowDirection FlowDirection
        {
            get => this.flowDirection;
            set => this.SetProperty(ref this.flowDirection, value, true);
        }

        public Vector2 ControlPadding
        {
            get => this.controlPadding;
            set => this.SetProperty(ref this.controlPadding, value, true);
        }

        public Vector2 OuterControlPadding
        {
            get => this.outerControlPadding;
            set => this.SetProperty(ref this.outerControlPadding, value, true);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            this.ReflowChildLayout(this._children.ToArray());
        }

        private void ReflowChildLayout(IEnumerable<Control> allChildren)
        {
            var filteredChildren = allChildren.Where(c => c.GetType() != typeof(Scrollbar) && c.Visible);

            switch (this.flowDirection)
            {
                case ControlFlowDirection.LeftToRight:
                    this.ReflowChildLayoutLeftToRight(filteredChildren);
                    break;
                case ControlFlowDirection.RightToLeft:
                    this.ReflowChildLayoutRightToLeft(filteredChildren);
                    break;
                case ControlFlowDirection.TopToBottom:
                    this.ReflowChildLayoutTopToBottom(filteredChildren);
                    break;
                case ControlFlowDirection.BottomToTop:
                    this.ReflowChildLayoutBottomToTop(filteredChildren);
                    break;
                case ControlFlowDirection.SingleLeftToRight:
                    this.ReflowChildLayoutSingleLeftToRight(filteredChildren);
                    break;
                case ControlFlowDirection.SingleRightToLeft:
                    this.ReflowChildLayoutSingleRightToLeft(filteredChildren);
                    break;
                case ControlFlowDirection.SingleTopToBottom:
                    this.ReflowChildLayoutSingleTopToBottom(filteredChildren);
                    break;
                case ControlFlowDirection.SingleBottomToTop:
                    this.ReflowChildLayoutSingleBottomToTop(filteredChildren);
                    break;
            }
        }

        private void ReflowChildLayoutLeftToRight(IEnumerable<Control> allChildren)
        {
            float outerPadX = this.outerControlPadding.X;
            float outerPadY = this.outerControlPadding.Y;

            float nextBottom = outerPadY;
            float currentBottom = outerPadY;
            float lastRight = outerPadX;

            foreach (var child in allChildren.Where(c => c.Visible))
            {
                // Need to flow over to the next row
                if (child.Width >= this.ContentRegion.Width - lastRight)
                {
                    currentBottom = nextBottom + this.controlPadding.Y;
                    lastRight = outerPadX;
                }

                child.Location = new Point((int)lastRight, (int)currentBottom);

                lastRight = child.Right + this.controlPadding.X;

                // Ensure rows don't overlap
                nextBottom = Math.Max(nextBottom, child.Bottom);
            }
        }

        private void ReflowChildLayoutRightToLeft(IEnumerable<Control> allChildren)
        {
            float outerPadX = this.outerControlPadding.X;
            float outerPadY = this.outerControlPadding.Y;

            float nextBottom = outerPadY;
            float currentBottom = outerPadY;
            float lastLeft = this.ContentRegion.Width - outerPadX;

            foreach (var child in allChildren.Where(c => c.Visible))
            {
                // Need to flow over to the next row
                if (outerPadX > lastLeft - child.Width)
                {
                    currentBottom = nextBottom + this.controlPadding.Y;
                    lastLeft = this.ContentRegion.Width - outerPadX;
                }

                child.Location = new Point((int)(lastLeft - child.Width), (int)currentBottom);

                lastLeft = child.Left - this.controlPadding.X;

                // Ensure rows don't overlap
                nextBottom = Math.Max(nextBottom, child.Bottom);
            }
        }

        private void ReflowChildLayoutTopToBottom(IEnumerable<Control> allChildren)
        {
            float outerPadX = this.outerControlPadding.X;
            float outerPadY = this.outerControlPadding.Y;

            float nextRight = outerPadX;
            float currentRight = outerPadX;
            float lastBottom = outerPadY;

            foreach (var child in allChildren.Where(c => c.Visible))
            {
                // Need to flow over to the next column
                if (child.Height >= this.Height - lastBottom)
                {
                    currentRight = nextRight + this.controlPadding.X;
                    lastBottom = outerPadY;
                }

                child.Location = new Point((int)currentRight, (int)lastBottom);

                lastBottom = child.Bottom + this.controlPadding.Y;

                // Ensure columns don't overlap
                nextRight = Math.Max(nextRight, child.Right);
            }
        }

        private void ReflowChildLayoutBottomToTop(IEnumerable<Control> allChildren)
        {
            float outerPadX = this.outerControlPadding.X;
            float outerPadY = this.outerControlPadding.Y;

            float nextRight = outerPadX;
            float currentRight = outerPadX;
            float lastTop = this.Height - outerPadY;

            foreach (var child in allChildren.Where(c => c.Visible))
            {
                // Need to flow over to the next column
                if (outerPadY > lastTop - child.Height)
                {
                    currentRight = nextRight + this.controlPadding.X;
                    lastTop = this.Height - outerPadY;
                }

                child.Location = new Point((int)currentRight, (int)(lastTop - child.Height));

                lastTop = child.Top - this.controlPadding.Y;

                // Ensure columns don't overlap
                nextRight = Math.Max(nextRight, child.Right);
            }
        }

        private void ReflowChildLayoutSingleLeftToRight(IEnumerable<Control> allChildren)
        {
            float outerPadX = this.outerControlPadding.X;
            float outerPadY = this.outerControlPadding.Y;

            var lastLeft = outerPadX;

            foreach (var child in allChildren)
            {
                child.Location = new Point((int)lastLeft, (int)outerPadY);

                lastLeft = child.Right + this.controlPadding.X;
            }
        }

        private void ReflowChildLayoutSingleRightToLeft(IEnumerable<Control> allChildren)
        {
            float outerPadX = this.outerControlPadding.X;
            float outerPadY = this.outerControlPadding.Y;

            var lastLeft = this.ContentRegion.Width - outerPadX;

            foreach (var child in allChildren)
            {
                child.Location = new Point((int)(lastLeft - child.Width), (int)outerPadY);

                lastLeft = child.Left - this.controlPadding.X;
            }
        }

        private void ReflowChildLayoutSingleTopToBottom(IEnumerable<Control> allChildren)
        {
            float outerPadX = this.outerControlPadding.X;
            float outerPadY = this.outerControlPadding.Y;

            var lastBottom = outerPadY;

            foreach (var child in allChildren)
            {
                child.Location = new Point((int)outerPadX, (int)lastBottom);

                lastBottom = child.Bottom + this.controlPadding.Y;
            }
        }

        private void ReflowChildLayoutSingleBottomToTop(IEnumerable<Control> allChildren)
        {
            float outerPadX = this.outerControlPadding.X;
            float outerPadY = this.outerControlPadding.Y;

            var lastTop = this.Height - outerPadY;

            foreach (var child in allChildren)
            {
                child.Location = new Point((int)outerPadX, (int)(lastTop - child.Height));

                lastTop = child.Top - this.controlPadding.Y;
            }
        }
    }
}
