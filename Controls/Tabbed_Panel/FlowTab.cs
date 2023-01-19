using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Point = Microsoft.Xna.Framework.Point;

namespace Kenedia.Modules.Characters.Controls
{
    public class FlowTab : PanelTab
    {
        private ControlFlowDirection flowDirection = ControlFlowDirection.LeftToRight;
        private Vector2 outerControlPadding = Vector2.Zero;
        private Vector2 controlPadding = Vector2.Zero;

        public ControlFlowDirection FlowDirection
        {
            get => flowDirection;
            set => SetProperty(ref flowDirection, value, true);
        }

        public Vector2 ControlPadding
        {
            get => controlPadding;
            set => SetProperty(ref controlPadding, value, true);
        }

        public Vector2 OuterControlPadding
        {
            get => outerControlPadding;
            set => SetProperty(ref outerControlPadding, value, true);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            ReflowChildLayout(_children.ToArray());
        }

        private void ReflowChildLayout(IEnumerable<Control> allChildren)
        {
            IEnumerable<Control> filteredChildren = allChildren.Where(c => c.GetType() != typeof(Scrollbar) && c.Visible);

            switch (flowDirection)
            {
                case ControlFlowDirection.LeftToRight:
                    ReflowChildLayoutLeftToRight(filteredChildren);
                    break;
                case ControlFlowDirection.RightToLeft:
                    ReflowChildLayoutRightToLeft(filteredChildren);
                    break;
                case ControlFlowDirection.TopToBottom:
                    ReflowChildLayoutTopToBottom(filteredChildren);
                    break;
                case ControlFlowDirection.BottomToTop:
                    ReflowChildLayoutBottomToTop(filteredChildren);
                    break;
                case ControlFlowDirection.SingleLeftToRight:
                    ReflowChildLayoutSingleLeftToRight(filteredChildren);
                    break;
                case ControlFlowDirection.SingleRightToLeft:
                    ReflowChildLayoutSingleRightToLeft(filteredChildren);
                    break;
                case ControlFlowDirection.SingleTopToBottom:
                    ReflowChildLayoutSingleTopToBottom(filteredChildren);
                    break;
                case ControlFlowDirection.SingleBottomToTop:
                    ReflowChildLayoutSingleBottomToTop(filteredChildren);
                    break;
            }
        }

        private void ReflowChildLayoutLeftToRight(IEnumerable<Control> allChildren)
        {
            float outerPadX = outerControlPadding.X;
            float outerPadY = outerControlPadding.Y;

            float nextBottom = outerPadY;
            float currentBottom = outerPadY;
            float lastRight = outerPadX;

            foreach (Control child in allChildren.Where(c => c.Visible))
            {
                // Need to flow over to the next row
                if (child.Width >= ContentRegion.Width - lastRight)
                {
                    currentBottom = nextBottom + controlPadding.Y;
                    lastRight = outerPadX;
                }

                child.Location = new Point((int)lastRight, (int)currentBottom);

                lastRight = child.Right + controlPadding.X;

                // Ensure rows don't overlap
                nextBottom = Math.Max(nextBottom, child.Bottom);
            }
        }

        private void ReflowChildLayoutRightToLeft(IEnumerable<Control> allChildren)
        {
            float outerPadX = outerControlPadding.X;
            float outerPadY = outerControlPadding.Y;

            float nextBottom = outerPadY;
            float currentBottom = outerPadY;
            float lastLeft = ContentRegion.Width - outerPadX;

            foreach (Control child in allChildren.Where(c => c.Visible))
            {
                // Need to flow over to the next row
                if (outerPadX > lastLeft - child.Width)
                {
                    currentBottom = nextBottom + controlPadding.Y;
                    lastLeft = ContentRegion.Width - outerPadX;
                }

                child.Location = new Point((int)(lastLeft - child.Width), (int)currentBottom);

                lastLeft = child.Left - controlPadding.X;

                // Ensure rows don't overlap
                nextBottom = Math.Max(nextBottom, child.Bottom);
            }
        }

        private void ReflowChildLayoutTopToBottom(IEnumerable<Control> allChildren)
        {
            float outerPadX = outerControlPadding.X;
            float outerPadY = outerControlPadding.Y;

            float nextRight = outerPadX;
            float currentRight = outerPadX;
            float lastBottom = outerPadY;

            foreach (Control child in allChildren.Where(c => c.Visible))
            {
                // Need to flow over to the next column
                if (child.Height >= Height - lastBottom)
                {
                    currentRight = nextRight + controlPadding.X;
                    lastBottom = outerPadY;
                }

                child.Location = new Point((int)currentRight, (int)lastBottom);

                lastBottom = child.Bottom + controlPadding.Y;

                // Ensure columns don't overlap
                nextRight = Math.Max(nextRight, child.Right);
            }
        }

        private void ReflowChildLayoutBottomToTop(IEnumerable<Control> allChildren)
        {
            float outerPadX = outerControlPadding.X;
            float outerPadY = outerControlPadding.Y;

            float nextRight = outerPadX;
            float currentRight = outerPadX;
            float lastTop = Height - outerPadY;

            foreach (Control child in allChildren.Where(c => c.Visible))
            {
                // Need to flow over to the next column
                if (outerPadY > lastTop - child.Height)
                {
                    currentRight = nextRight + controlPadding.X;
                    lastTop = Height - outerPadY;
                }

                child.Location = new Point((int)currentRight, (int)(lastTop - child.Height));

                lastTop = child.Top - controlPadding.Y;

                // Ensure columns don't overlap
                nextRight = Math.Max(nextRight, child.Right);
            }
        }

        private void ReflowChildLayoutSingleLeftToRight(IEnumerable<Control> allChildren)
        {
            float outerPadX = outerControlPadding.X;
            float outerPadY = outerControlPadding.Y;

            float lastLeft = outerPadX;

            foreach (Control child in allChildren)
            {
                child.Location = new Point((int)lastLeft, (int)outerPadY);

                lastLeft = child.Right + controlPadding.X;
            }
        }

        private void ReflowChildLayoutSingleRightToLeft(IEnumerable<Control> allChildren)
        {
            float outerPadX = outerControlPadding.X;
            float outerPadY = outerControlPadding.Y;

            float lastLeft = ContentRegion.Width - outerPadX;

            foreach (Control child in allChildren)
            {
                child.Location = new Point((int)(lastLeft - child.Width), (int)outerPadY);

                lastLeft = child.Left - controlPadding.X;
            }
        }

        private void ReflowChildLayoutSingleTopToBottom(IEnumerable<Control> allChildren)
        {
            float outerPadX = outerControlPadding.X;
            float outerPadY = outerControlPadding.Y;

            float lastBottom = outerPadY;

            foreach (Control child in allChildren)
            {
                child.Location = new Point((int)outerPadX, (int)lastBottom);

                lastBottom = child.Bottom + controlPadding.Y;
            }
        }

        private void ReflowChildLayoutSingleBottomToTop(IEnumerable<Control> allChildren)
        {
            float outerPadX = outerControlPadding.X;
            float outerPadY = outerControlPadding.Y;

            float lastTop = Height - outerPadY;

            foreach (Control child in allChildren)
            {
                child.Location = new Point((int)outerPadX, (int)(lastTop - child.Height));

                lastTop = child.Top - controlPadding.Y;
            }
        }
    }
}
