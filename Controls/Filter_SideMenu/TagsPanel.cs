namespace Kenedia.Modules.Characters.Controls
{
    using System.Collections.Generic;
    using System.Linq;
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Microsoft.Xna.Framework;
    using Point = Microsoft.Xna.Framework.Point;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    internal class TagsPanel : FlowTab
    {
        private readonly FlowPanel tagPanel;

        public TagsPanel()
        {
            this.Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(156025);
            this.TextureRectangle = new Rectangle(48, 48, 46, 46);
            this.Name = Strings.common.CustomTags;

            this.FlowDirection = ControlFlowDirection.LeftToRight;
            this.WidthSizingMode = SizingMode.Fill;
            this.AutoSizePadding = new Point(5, 5);
            this.HeightSizingMode = SizingMode.Standard;
            this.Height = 250;
            this.OuterControlPadding = new Vector2(5, 5);
            this.ControlPadding = new Vector2(5, 3);
            this.Location = new Point(0, 25);

            this.tagPanel = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                ControlPadding = new Vector2(3, 2),
            };

            foreach (string t in Characters.ModuleInstance.Tags)
            {
                var tag = new Tag()
                {
                    Parent = this.tagPanel,
                    Text = t,
                    Active = false,
                    ShowDelete = false,
                };
                tag.Click += this.Tag_Click;
                this.Tags.Add(tag);
            }

            this.Invalidate();

            Characters.ModuleInstance.Tags.CollectionChanged += this.Tags_CollectionChanged;
        }

        public List<Tag> Tags { get; } = new List<Tag>();

        private void Tag_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.MainWindow.FilterCharacters();
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (string t in Characters.ModuleInstance.Tags)
            {
                var exists = this.tagPanel.Children.Cast<Tag>().ToList().Find(e => e.Text == t) != null;

                if (!exists)
                {
                    var tag = new Tag()
                    {
                        Parent = this.tagPanel,
                        Text = t,
                        Active = false,
                        ShowDelete = false,
                    };
                    tag.Click += this.Tag_Click;
                    this.Tags.Add(tag);
                }
            }

            this.Invalidate();
        }
    }
}
