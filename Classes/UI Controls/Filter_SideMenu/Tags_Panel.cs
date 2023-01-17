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
    class Tags_Panel : FlowTab
    {
        FlowPanel _tagPanel;
        public List<Tag> Tags = new List<Tag>();
        public Tags_Panel()
        {
            Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(156025);
            TextureRectangle = new Rectangle(48, 48, 46, 46);
            Name = "Custom Tags";

            FlowDirection = ControlFlowDirection.LeftToRight;
            WidthSizingMode = SizingMode.Fill;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.Standard;
            Height = 250;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 3);
            Location = new Point(0, 25);

            _tagPanel = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                ControlPadding = new Vector2(3, 2)
            };

            foreach (string t in Characters.ModuleInstance.Tags)
            {
                var tag = new Tag()
                {
                    Parent = _tagPanel,
                    Text = t,
                    Active = false,
                    ShowDelete = false,
                };
                tag.Click += Tag_Click;
                Tags.Add(tag);
            }
            Invalidate();

            Characters.ModuleInstance.Tags.CollectionChanged += Tags_CollectionChanged;
        }

        private void Tag_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.MainWindow.FilterCharacters();
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (string t in Characters.ModuleInstance.Tags)
            {
                var exists = _tagPanel.Children.Cast<Tag>().ToList().Find(e => e.Text == t) != null;

                if (!exists)
                {
                    var tag = new Tag()
                    {
                        Parent = _tagPanel,
                        Text = t,
                        Active = false,
                        ShowDelete = false,
                    };
                    tag.Click += Tag_Click;
                    Tags.Add(tag);
                }
            }

            Invalidate();
        }
    }
}
