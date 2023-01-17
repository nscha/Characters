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
    public class FlowTab : PanelTab
    {
        protected ControlFlowDirection _flowDirection = ControlFlowDirection.LeftToRight;
        public ControlFlowDirection FlowDirection
        {
            get => _flowDirection;
            set => SetProperty(ref _flowDirection, value, true);
        }
        protected Vector2 _controlPadding = Vector2.Zero;
        public Vector2 ControlPadding
        {
            get => _controlPadding;
            set => SetProperty(ref _controlPadding, value, true);
        }

        protected Vector2 _outerControlPadding = Vector2.Zero;
        public Vector2 OuterControlPadding
        {
            get => _outerControlPadding;
            set => SetProperty(ref _outerControlPadding, value, true);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            ReflowChildLayout(_children.ToArray());

        }

        private void ReflowChildLayout(IEnumerable<Control> allChildren)
        {
            var filteredChildren = allChildren.Where(c => c.GetType() != typeof(Scrollbar) && c.Visible);

            switch (_flowDirection)
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
            float outerPadX = _outerControlPadding.X;
            float outerPadY = _outerControlPadding.Y;

            float nextBottom = outerPadY;
            float currentBottom = outerPadY;
            float lastRight = outerPadX;

            foreach (var child in allChildren.Where(c => c.Visible))
            {
                // Need to flow over to the next row
                if (child.Width >= this.ContentRegion.Width - lastRight)
                {
                    currentBottom = nextBottom + _controlPadding.Y;
                    lastRight = outerPadX;
                }

                child.Location = new Point((int)lastRight, (int)currentBottom);

                lastRight = child.Right + _controlPadding.X;

                // Ensure rows don't overlap
                nextBottom = Math.Max(nextBottom, child.Bottom);
            }
        }

        private void ReflowChildLayoutRightToLeft(IEnumerable<Control> allChildren)
        {
            float outerPadX = _outerControlPadding.X;
            float outerPadY = _outerControlPadding.Y;

            float nextBottom = outerPadY;
            float currentBottom = outerPadY;
            float lastLeft = this.ContentRegion.Width - outerPadX;

            foreach (var child in allChildren.Where(c => c.Visible))
            {
                // Need to flow over to the next row
                if (outerPadX > lastLeft - child.Width)
                {
                    currentBottom = nextBottom + _controlPadding.Y;
                    lastLeft = this.ContentRegion.Width - outerPadX;
                }

                child.Location = new Point((int)(lastLeft - child.Width), (int)currentBottom);

                lastLeft = child.Left - _controlPadding.X;

                // Ensure rows don't overlap
                nextBottom = Math.Max(nextBottom, child.Bottom);
            }
        }

        private void ReflowChildLayoutTopToBottom(IEnumerable<Control> allChildren)
        {
            float outerPadX = _outerControlPadding.X;
            float outerPadY = _outerControlPadding.Y;

            float nextRight = outerPadX;
            float currentRight = outerPadX;
            float lastBottom = outerPadY;

            foreach (var child in allChildren.Where(c => c.Visible))
            {
                // Need to flow over to the next column
                if (child.Height >= this.Height - lastBottom)
                {
                    currentRight = nextRight + _controlPadding.X;
                    lastBottom = outerPadY;
                }

                child.Location = new Point((int)currentRight, (int)lastBottom);

                lastBottom = child.Bottom + _controlPadding.Y;

                // Ensure columns don't overlap
                nextRight = Math.Max(nextRight, child.Right);
            }
        }

        private void ReflowChildLayoutBottomToTop(IEnumerable<Control> allChildren)
        {
            float outerPadX = _outerControlPadding.X;
            float outerPadY = _outerControlPadding.Y;

            float nextRight = outerPadX;
            float currentRight = outerPadX;
            float lastTop = this.Height - outerPadY;

            foreach (var child in allChildren.Where(c => c.Visible))
            {
                // Need to flow over to the next column
                if (outerPadY > lastTop - child.Height)
                {
                    currentRight = nextRight + _controlPadding.X;
                    lastTop = this.Height - outerPadY;
                }

                child.Location = new Point((int)currentRight, (int)(lastTop - child.Height));

                lastTop = child.Top - _controlPadding.Y;

                // Ensure columns don't overlap
                nextRight = Math.Max(nextRight, child.Right);
            }
        }

        private void ReflowChildLayoutSingleLeftToRight(IEnumerable<Control> allChildren)
        {
            float outerPadX = _outerControlPadding.X;
            float outerPadY = _outerControlPadding.Y;

            var lastLeft = outerPadX;

            foreach (var child in allChildren)
            {
                child.Location = new Point((int)lastLeft, (int)outerPadY);

                lastLeft = child.Right + _controlPadding.X;
            }
        }

        private void ReflowChildLayoutSingleRightToLeft(IEnumerable<Control> allChildren)
        {
            float outerPadX = _outerControlPadding.X;
            float outerPadY = _outerControlPadding.Y;

            var lastLeft = this.ContentRegion.Width - outerPadX;

            foreach (var child in allChildren)
            {
                child.Location = new Point((int)(lastLeft - child.Width), (int)outerPadY);

                lastLeft = child.Left - _controlPadding.X;
            }
        }

        private void ReflowChildLayoutSingleTopToBottom(IEnumerable<Control> allChildren)
        {
            float outerPadX = _outerControlPadding.X;
            float outerPadY = _outerControlPadding.Y;

            var lastBottom = outerPadY;

            foreach (var child in allChildren)
            {
                child.Location = new Point((int)outerPadX, (int)lastBottom);

                lastBottom = child.Bottom + _controlPadding.Y;
            }
        }

        private void ReflowChildLayoutSingleBottomToTop(IEnumerable<Control> allChildren)
        {
            float outerPadX = _outerControlPadding.X;
            float outerPadY = _outerControlPadding.Y;

            var lastTop = this.Height - outerPadY;

            foreach (var child in allChildren)
            {
                child.Location = new Point((int)outerPadX, (int)(lastTop - child.Height));

                lastTop = child.Top - _controlPadding.Y;
            }
        }
    }
}
