using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    internal class TagsPanel : FlowTab
    {
        private readonly FlowPanel tagPanel;

        public TagsPanel()
        {
            Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(156025);
            TextureRectangle = new Rectangle(48, 48, 46, 46);
            Name = Strings.common.CustomTags;

            FlowDirection = ControlFlowDirection.LeftToRight;
            WidthSizingMode = SizingMode.Fill;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.Standard;
            Height = 250;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 3);
            Location = new Point(0, 25);

            tagPanel = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                ControlPadding = new Vector2(3, 2),
            };

            foreach (string t in Characters.ModuleInstance.Tags)
            {
                Tag tag = new()
                {
                    Parent = tagPanel,
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

        public List<Tag> Tags { get; } = new List<Tag>();

        private void Tag_Click(object sender, MouseEventArgs e) => Characters.ModuleInstance.MainWindow.FilterCharacters();

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (string t in Characters.ModuleInstance.Tags)
            {
                bool exists = tagPanel.Children.Cast<Tag>().ToList().Find(e => e.Text == t) != null;

                if (!exists)
                {
                    Tag tag = new()
                    {
                        Parent = tagPanel,
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
