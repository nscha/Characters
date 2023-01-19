namespace Kenedia.Modules.Characters.Views
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Blish_HUD;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Kenedia.Modules.Characters.Controls;
    using Kenedia.Modules.Characters.Enums;
    using Kenedia.Modules.Characters.Extensions;
    using Kenedia.Modules.Characters.Models;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using static Kenedia.Modules.Characters.Services.SettingsModel;
    using Color = Microsoft.Xna.Framework.Color;
    using Point = Microsoft.Xna.Framework.Point;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class MainWindow : StandardWindow
    {
        private readonly AsyncTexture2D windowEmblem = GameService.Content.DatAssetCache.GetTextureFromAssetId(156015);

        private readonly Image displaySettingsButton;
        private readonly ImageButton clearButton;
        private readonly FlowPanel dropdownPanel;
        private readonly SettingsSideMenu settingsSideMenu;
        private readonly FilterSideMenu filterSideMenu;

        private bool filterCharacters;
        private bool updateLayout;
        private double tick = 0;
        private double filterTick = 0;

        public MainWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion)
            : base(background, windowRegion, contentRegion)
        {
            this.ContentPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(0, 38),
                Size = this.Size,
                ControlPadding = new Vector2(2, 4),
                CanScroll = true,
            };

            this.DraggingControl.LeftMouseButtonReleased += this.DraggingControl_LeftMouseButtonReleased;

            this.dropdownPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(0, 2),
                FlowDirection = ControlFlowDirection.SingleRightToLeft,
                OuterControlPadding = new Vector2(0, 2),
                ControlPadding = new Vector2(6, 0),
            };

            this.displaySettingsButton = new Image()
            {
                Parent = this.dropdownPanel,
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(155052),
                Size = new Point(25, 25),
                BasicTooltipText = string.Format(Strings.common.ShowItem, string.Format(Strings.common.ItemSettings, Strings.common.Display)),
            };
            this.displaySettingsButton.MouseEntered += this.DisplaySettingsButton_MouseEntered;
            this.displaySettingsButton.MouseLeft += this.DisplaySettingsButton_MouseLeft;
            this.displaySettingsButton.Click += this.DisplaySettingsButton_Click;

            this.clearButton = new ImageButton()
            {
                Parent = this,
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(2175783),
                HoveredTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(2175782),
                ClickedTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(2175784),
                Size = new Point(20, 20),
                BasicTooltipText = Strings.common.ClearFilters,
                Visible = false,
            };
            this.clearButton.Click += this.ClearButton_Click;

            this.FilterBox = new TextBox()
            {
                Parent = this.dropdownPanel,
                PlaceholderText = Strings.common.Search,
                Width = this.dropdownPanel.Width - this.displaySettingsButton.Width - 5,
            };
            this.FilterBox.TextChanged += this.FilterCharacters;
            this.FilterBox.Click += this.FilterBox_Click;
            this.FilterBox.EnterPressed += this.FilterBox_EnterPressed;

            this.settingsSideMenu = new SettingsSideMenu()
            {
                TextureOffset = new Point(25, 25),
                Visible = false,
            };

            this.filterSideMenu = new FilterSideMenu()
            {
                TextureOffset = new Point(25, 25),
                Visible = false,
            };

            this.CharacterEdit = new CharacterEdit()
            {
                TextureOffset = new Point(25, 25),
                Visible = false,
            };
            this.CharacterEdit.Shown += this.CharacterEdit_Shown;

            this.clearButton.Location = new Point(this.FilterBox.LocalBounds.Right - 25, this.FilterBox.LocalBounds.Top + 5);
            Characters.ModuleInstance.LanguageChanged += this.ModuleInstance_LanguageChanged;
        }

        public Dictionary<object, List<object>> CategoryFilters { get; set; } = new Dictionary<object, List<object>>()
        {
            {
                FilterCategory.Race,
                new List<object>()
            },
            {
                FilterCategory.Crafting,
                new List<object>()
            },
            {
                FilterCategory.Profession,
                new List<object>()
            },
            {
                FilterCategory.ProfessionSpecialization,
                new List<object>()
            },
            {
                FilterCategory.Specialization,
                new List<object>()
            },
            {
                FilterCategory.Hidden,
                new List<object>()
            },
            {
                FilterCategory.Birthday,
                new List<object>()
            },
        };

        public List<CharacterControl> CharacterControls { get; set; } = new List<CharacterControl>();

        public CharacterEdit CharacterEdit { get; set; }

        public TextBox FilterBox { get; set; }

        public DraggingControl DraggingControl { get; set; } = new DraggingControl()
        {
            Parent = GameService.Graphics.SpriteScreen,
            Visible = false,
            ZIndex = 999,
            Enabled = false,
        };

        public FlowPanel ContentPanel { get; private set; }

        public void FilterCharacters(object sender = null, EventArgs e = null)
        {
            this.filterCharacters = true;
        }

        public void PerformFiltering()
        {
            var any = Characters.ModuleInstance.Settings.FilterMatching.Value == MatchingBehavior.MatchAny;
            var all = Characters.ModuleInstance.Settings.FilterMatching.Value == MatchingBehavior.MatchAll;

            var textStrings = this.FilterBox.Text.Trim().ToLower().Split(' ').ToList();
            var matchAny = this.FilterBox.Text.Trim().Length == 0;
            var s = Characters.ModuleInstance.Settings;
            var data = Characters.ModuleInstance.Data;

            var activeTags = this.filterSideMenu.Tags.Where(e => e.Active);

            var anyTag = activeTags.Count() == 0;
            var raceAny = this.CategoryFilters[FilterCategory.Race].Count == 0;
            var craftAny = this.CategoryFilters[FilterCategory.Crafting].Count == 0;
            var profAny = this.CategoryFilters[FilterCategory.Profession].Count == 0;
            var specProfAny = this.CategoryFilters[FilterCategory.ProfessionSpecialization].Count == 0;
            var specAny = this.CategoryFilters[FilterCategory.Specialization].Count == 0;
            var birthAny = this.CategoryFilters[FilterCategory.Birthday].Count == 0;
            var anyCategory = raceAny && craftAny && profAny && specProfAny && specAny && birthAny;
            var includeHidden = this.CategoryFilters[FilterCategory.Hidden].Count == 1;

            foreach (CharacterControl c in this.ContentPanel.Children)
            {
                var crafting_Any = c.Character.Crafting.Select(e => e.Id).Any(e => this.CategoryFilters[FilterCategory.Crafting].Contains(e) && (!s.CheckOnlyMaxCrafting.Value || c.Character.Crafting.Find(a => a.Id == e).Rating == data.CrafingProfessions[e].MaxRating));
                var crafting_All = this.CategoryFilters[FilterCategory.Crafting].Select(e => (int)e).All(e => c.Character.Crafting.Find(a => a.Id == e && (!s.CheckOnlyMaxCrafting.Value || a.Rating == data.CrafingProfessions[e].MaxRating)) != null);

                if (matchAny && anyCategory && anyTag)
                {
                    c.Visible = c.Character.Show || includeHidden;
                    continue;
                }

                List<FilterTag> filterCategories = new List<FilterTag>()
                {
                    new FilterTag()
                    {
                        Result = raceAny || this.CategoryFilters[FilterCategory.Race].Contains(c.Character.Race),
                    },
                    new FilterTag()
                    {
                        Result = craftAny || (any ? crafting_Any : crafting_All),
                    },
                    new FilterTag()
                    {
                        Result = profAny || ((any || this.CategoryFilters[FilterCategory.Profession].Count == 1) && this.CategoryFilters[FilterCategory.Profession].Contains(c.Character.Profession)),
                    },
                    new FilterTag()
                    {
                        Result = birthAny || c.Character.HasBirthdayPresent,
                    },
                    new FilterTag()
                    {
                        Result = (specProfAny && specAny) || ((any || this.CategoryFilters[FilterCategory.ProfessionSpecialization].Count == 1) && c.Character.Specialization == SpecializationType.None && this.CategoryFilters[FilterCategory.ProfessionSpecialization].Contains(c.Character.Profession)) || this.CategoryFilters[FilterCategory.Specialization].Contains(c.Character.Specialization),
                    },
                };

                List<FilterTag> filterTags = this.filterSideMenu.Tags.Where(e => e.Active).Select(e => e.Text).ToList().CreateFilterTagList();
                List<FilterTag> filterStrings = textStrings.CreateFilterTagList();

                if (!anyTag)
                {
                    filterTags.ForEach(ft => ft.Result = c.Character.Tags.Contains(ft.Tag));
                }

                if (!matchAny)
                {
                    if (s.CheckName.Value)
                    {
                        var value = c.Character.Name.ToString();

                        if (value != null)
                        {
                            foreach (FilterTag ex in filterStrings)
                            {
                                if (value.ToLower().Contains(ex))
                                {
                                    ex.Result = true;
                                }
                            }
                        }

                        // visible = tag != null ? s.FilterDirection.Value == FilterBehavior.Include : visible;
                    }

                    if (s.CheckLevel.Value)
                    {
                        var value = c.Character.Level.ToString();

                        if (value != null)
                        {
                            foreach (FilterTag ex in filterStrings)
                            {
                                if (value.ToLower().Contains(ex))
                                {
                                    ex.Result = true;
                                }
                            }
                        }

                        // visible = tag != null ? s.FilterDirection.Value == FilterBehavior.Include : visible;
                    }

                    if (s.CheckRace.Value)
                    {
                        var value = c.Character.Race.GetData();

                        if (value != null)
                        {
                            foreach (FilterTag ex in filterStrings)
                            {
                                if (value.Name.ToLower().Contains(ex))
                                {
                                    ex.Result = true;
                                }
                            }
                        }

                        // visible = filterStrings.Find(ex =>
                        // {
                        //    var value = c.Character.Race.GetData();
                        //    return value != null && value.Name.ToLower().Contains(ex);
                        // }) != null ? s.FilterDirection.Value == FilterBehavior.Include : visible;
                    }

                    if (s.CheckProfession.Value)
                    {
                        var value = c.Character.Profession.GetData();

                        if (value != null)
                        {
                            foreach (FilterTag ex in filterStrings)
                            {
                                if (value.Name.ToLower().Contains(ex))
                                {
                                    ex.Result = true;
                                }
                            }
                        }

                        var value2 = c.Character.Specialization.GetData();

                        if (value2 != null)
                        {
                            foreach (FilterTag ex in filterStrings)
                            {
                                if (value2.Name.ToLower().Contains(ex))
                                {
                                    ex.Result = true;
                                }
                            }
                        }

                        // visible = filterStrings.Find(ex =>
                        // {
                        //    var value = c.Character.Profession.GetData();
                        //    return value != null && value.Name.ToLower().Contains(ex);
                        // }) != null ? s.FilterDirection.Value == FilterBehavior.Include : visible;

                        // visible = filterStrings.Find(ex =>
                        // {
                        //    var value = c.Character.Specialization.GetData();
                        //    return value != null && value.Name.ToLower().Contains(ex);
                        // }) != null ? s.FilterDirection.Value == FilterBehavior.Include : visible;
                    }

                    if (s.CheckMap.Value)
                    {
                        var value = Characters.ModuleInstance.Data.GetMapById(c.Character.Map);

                        if (value != null)
                        {
                            foreach (FilterTag ex in filterStrings)
                            {
                                if (value.Name.ToLower().Contains(ex))
                                {
                                    ex.Result = true;
                                }
                            }
                        }

                        // visible = filterStrings.Find(ex =>
                        // {
                        //    var value = Characters.ModuleInstance.Data.GetMapById(c.Character.Map);
                        //    return value != null && value.Name.ToLower().Contains(ex);
                        // }) != null ? s.FilterDirection.Value == FilterBehavior.Include : visible;
                    }

                    if (s.CheckCrafting.Value)
                    {
                        foreach (CharacterCrafting crafting in c.Character.Crafting)
                        {
                            var value = crafting.Id.GetData();

                            if (value != null)
                            {
                                foreach (FilterTag ex in filterStrings)
                                {
                                    if (value.Name.ToLower().Contains(ex) && (!s.CheckOnlyMaxCrafting.Value || value.MaxRating == crafting.Rating))
                                    {
                                        ex.Result = true;
                                    }
                                }
                            }

                            // visible = value != null && (!s.Check_OnlyMaxCrafting.Value || crafting.Rating == value.MaxRating) && filterStrings.Find(ex =>
                            // {
                            //    return value != null && value.Name.ToLower().Contains(ex);
                            // }) != null ? s.FilterDirection.Value == FilterBehavior.Include : visible;
                        }
                    }

                    if (s.CheckTags.Value)
                    {
                        var tags = c.Character.Tags.ToList().ConvertAll(d => d.ToLower());

                        if (tags != null)
                        {
                            foreach (FilterTag ex in filterStrings)
                            {
                                if (tags.Find(t => t.Contains(ex)) != null)
                                {
                                    ex.Result = true;
                                }
                            }
                        }

                        // visible = tag != null ? s.FilterDirection.Value == FilterBehavior.Include : visible;
                    }
                }

                var matched = matchAny || (any ? filterStrings.Where(r => r.Result == true).Count() > 0 : filterStrings.Where(r => r.Result == true).Count() == filterStrings.Count);
                var catMatched = anyCategory || filterCategories.Where(r => r.Result == true).Count() == filterCategories.Count;
                var tagMatched = anyTag || (any ? filterTags.Where(r => r.Result == true).Count() > 0 : filterTags.Where(r => r.Result == true).Count() == filterTags.Count);

                c.Visible = (c.Character.Show || includeHidden) && (s.FilterDirection.Value == FilterBehavior.Include ? matched && catMatched && tagMatched : !matched && !catMatched && !tagMatched);
            }

            this.clearButton.Visible = !anyCategory || !matchAny || !anyTag;
            this.SortCharacters();
            this.ContentPanel.Invalidate();
        }

        public void UpdateLayout()
        {
            this.updateLayout = false;
            var panelLayout = Characters.ModuleInstance.Settings.PanelLayout.Value;
            var panelSize = Characters.ModuleInstance.Settings.PanelSize.Value;

            var size = Point.Zero;
            var nameFont = GameService.Content.DefaultFont14;
            var font = GameService.Content.DefaultFont12;
            var testString = Characters.ModuleInstance.CharacterModels.Aggregate(string.Empty, (max, cur) => max.Length > cur.Name.Length ? max : cur.Name);

            switch (panelSize)
            {
                case PanelSizes.Small:
                    {
                        font = GameService.Content.DefaultFont12;
                        nameFont = GameService.Content.DefaultFont16;
                        size = new Point(48 + 5 + (int)nameFont.MeasureString(testString).Width, 48);
                        break;
                    }

                case PanelSizes.Normal:
                    {
                        font = GameService.Content.DefaultFont14;
                        nameFont = GameService.Content.DefaultFont18;
                        size = new Point(64 + 5 + (int)nameFont.MeasureString(testString).Width, 64);
                        break;
                    }

                case PanelSizes.Large:
                    {
                        font = GameService.Content.DefaultFont16;
                        nameFont = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size20, ContentService.FontStyle.Regular);
                        size = new Point(96 + 5 + (int)nameFont.MeasureString(testString).Width, 96);
                        break;
                    }

                case PanelSizes.Custom:
                    {
                        font = GameService.Content.DefaultFont18;
                        nameFont = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size22, ContentService.FontStyle.Regular);
                        size = new Point(96 + 5 + (int)nameFont.MeasureString(testString).Width, 96);
                        break;
                    }
            }

            switch (panelLayout)
            {
                case CharacterPanelLayout.OnlyIcons:
                    {
                        var newSize = new Point(Math.Min(size.X, size.Y), Math.Min(size.X, size.Y));
                        foreach (CharacterControl c in this.ContentPanel.Children)
                        {
                            c.Size = newSize;
                            c.Font = font;
                            c.NameFont = nameFont;
                            c.UpdateLayout();
                        }

                        break;
                    }

                case CharacterPanelLayout.OnlyText:
                    {
                        var newSize = size;
                        foreach (CharacterControl c in this.ContentPanel.Children)
                        {
                            c.Size = newSize;
                            c.Font = font;
                            c.NameFont = nameFont;
                            c.UpdateLayout();
                        }

                        break;
                    }

                case CharacterPanelLayout.IconAndText:
                    {
                        var newSize = size;
                        foreach (CharacterControl c in this.ContentPanel.Children)
                        {
                            c.Size = newSize;
                            c.Font = font;
                            c.NameFont = nameFont;
                            c.UpdateLayout();
                        }

                        break;
                    }
            }

            var maxWidth = this.ContentPanel.Children.Count > 0 ? this.ContentPanel.Children.Cast<CharacterControl>().Max(t => t.TotalWidth) : this.Width;

            foreach (CharacterControl c in this.ContentPanel.Children)
            {
                c.Width = maxWidth;
            }
        }

        public void SortCharacters()
        {
            var order = Characters.ModuleInstance.Settings.SortOrder.Value;
            var sort = Characters.ModuleInstance.Settings.SortType.Value;

            switch (sort)
            {
                case ESortType.SortByName:
                    {
                        switch (order)
                        {
                            case ESortOrder.Ascending:
                                this.ContentPanel.SortChildren<CharacterControl>((a, b) => a.Character.Name.CompareTo(b.Character.Name));
                                break;

                            case ESortOrder.Descending:
                                this.ContentPanel.SortChildren<CharacterControl>((a, b) => b.Character.Name.CompareTo(a.Character.Name));
                                break;
                        }

                        break;
                    }

                case ESortType.SortByLastLogin:
                    {
                        switch (order)
                        {
                            case ESortOrder.Ascending:
                                this.ContentPanel.SortChildren<CharacterControl>((a, b) => b.Character.LastLogin.CompareTo(a.Character.LastLogin));
                                break;

                            case ESortOrder.Descending:
                                this.ContentPanel.SortChildren<CharacterControl>((a, b) => a.Character.LastLogin.CompareTo(b.Character.LastLogin));
                                break;
                        }

                        break;
                    }

                case ESortType.SortByMap:
                    {
                        switch (order)
                        {
                            case ESortOrder.Ascending:
                                this.ContentPanel.SortChildren<CharacterControl>((a, b) => a.Character.Map.CompareTo(b.Character.Map));
                                break;

                            case ESortOrder.Descending:
                                this.ContentPanel.SortChildren<CharacterControl>((a, b) => b.Character.Map.CompareTo(a.Character.Map));
                                break;
                        }

                        break;
                    }

                case ESortType.SortByProfession:
                    {
                        switch (order)
                        {
                            case ESortOrder.Ascending:
                                this.ContentPanel.SortChildren<CharacterControl>((a, b) => a.Character.Profession.CompareTo(b.Character.Profession));
                                break;

                            case ESortOrder.Descending:
                                this.ContentPanel.SortChildren<CharacterControl>((a, b) => b.Character.Profession.CompareTo(a.Character.Profession));
                                break;
                        }

                        break;
                    }

                case ESortType.SortByTag:
                    {
                        break;
                    }

                case ESortType.Custom:
                    {
                        this.ContentPanel.SortChildren<CharacterControl>((a, b) => a.Index.CompareTo(b.Index));

                        var i = 0;
                        foreach (CharacterControl c in this.ContentPanel.Children)
                        {
                            c.Index = i;
                            i++;
                        }

                        break;
                    }
            }
        }

        public void SetNewIndex(CharacterControl characterControl)
        {
            characterControl.Index = Characters.ModuleInstance.MainWindow.GetHoveredIndex(characterControl);

            Characters.ModuleInstance.MainWindow.SortCharacters();
        }

        public double GetHoveredIndex(CharacterControl characterControl)
        {
            var m = Input.Mouse;
            CharacterControl lastControl = characterControl;

            var i = 0;
            foreach (CharacterControl c in this.ContentPanel.Children)
            {
                c.Index = i;
                i++;
            }

            foreach (CharacterControl c in this.ContentPanel.Children)
            {
                if (c.AbsoluteBounds.Contains(m.Position))
                {
                    return characterControl.Index > c.Index ? c.Index - 0.1 : c.Index + 0.1;
                }

                lastControl = c;
            }

            if (lastControl.AbsoluteBounds.Bottom < m.Position.Y || (lastControl.AbsoluteBounds.Top < m.Position.Y && lastControl.AbsoluteBounds.Right < m.Position.X))
            {
                return this.CharacterControls.Count + 1;
            }

            return characterControl.Index;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (this.filterCharacters && gameTime.TotalGameTime.TotalMilliseconds - this.filterTick > Characters.ModuleInstance.Settings.FilterDelay.Value)
            {
                this.filterTick = gameTime.TotalGameTime.TotalMilliseconds;
                this.PerformFiltering();
                this.filterCharacters = false;
            }

            if (gameTime.TotalGameTime.TotalMilliseconds - this.tick > 50)
            {
                this.tick = gameTime.TotalGameTime.TotalMilliseconds;

                if (this.updateLayout)
                {
                    this.UpdateLayout();
                }
            }
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            spriteBatch.DrawOnCtrl(
                this,
                this.windowEmblem,
                new Rectangle(-43, -58, 128, 128),
                this.windowEmblem.Bounds,
                Color.White,
                0f,
                default);

            if (bounds.Width >= 190)
            {
                spriteBatch.DrawStringOnCtrl(
                    this,
                    $"{Characters.ModuleInstance.Name}",
                    bounds.Width >= 245 ? GameService.Content.DefaultFont32 : GameService.Content.DefaultFont18,
                    new Rectangle(65, 5, bounds.Width - (128 - 43 + 15), 30),
                    ContentService.Colors.ColonialWhite, // new Color(247, 231, 182, 97),
                    false,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Middle);
            }
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);

            this.filterSideMenu?.Hide();
            this.settingsSideMenu?.Hide();
            this.CharacterEdit?.Hide();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            if (this.ContentPanel != null)
            {
                this.ContentPanel.Size = new Point(this.ContentRegion.Size.X, this.ContentRegion.Size.Y - 35);
            }

            if (this.dropdownPanel != null)
            {
                this.dropdownPanel.Size = new Point(this.ContentRegion.Size.X, 31);
                this.FilterBox.Width = this.dropdownPanel.Width - this.displaySettingsButton.Width - 5;
                this.clearButton.Location = new Point(this.FilterBox.LocalBounds.Right - 23, this.FilterBox.LocalBounds.Top + 6);
            }

            if (e.CurrentSize.Y < 135)
            {
                this.Size = new Point(this.Size.X, 135);
            }

            if (this.settingsSideMenu != null && this.settingsSideMenu.Visible)
            {
                this.settingsSideMenu.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            }

            if (this.filterSideMenu != null && this.filterSideMenu.Visible)
            {
                this.filterSideMenu.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            }

            if (this.CharacterEdit != null && this.CharacterEdit.Visible)
            {
                this.CharacterEdit.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            }
        }

        protected override void OnMoved(MovedEventArgs e)
        {
            base.OnMoved(e);

            if (this.settingsSideMenu != null && this.settingsSideMenu.Visible)
            {
                this.settingsSideMenu.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            }

            if (this.filterSideMenu != null && this.filterSideMenu.Visible)
            {
                this.filterSideMenu.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            }

            if (this.CharacterEdit != null && this.CharacterEdit.Visible)
            {
                this.CharacterEdit.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            this.CharacterControls?.DisposeAll();

            this.ContentPanel?.Dispose();
            this.DraggingControl?.Dispose();
            this.settingsSideMenu?.Dispose();
            this.filterSideMenu?.Dispose();
            this.CharacterEdit?.Dispose();

            this.dropdownPanel?.Dispose();
            this.displaySettingsButton?.Dispose();
            this.FilterBox?.Dispose();
        }

        private void FilterBox_EnterPressed(object sender, EventArgs e)
        {
            if (Characters.ModuleInstance.Settings.EnterToLogin.Value)
            {
                this.PerformFiltering();
                var c = (CharacterControl)this.ContentPanel.Children.Where(e => e.Visible).FirstOrDefault();

                if (c != null)
                {
                    Debug.WriteLine(c.Character.Name);
                    Characters.ModuleInstance.SwapTo(c.Character);
                }
            }
        }

        private void CharacterEdit_Shown(object sender, EventArgs e)
        {
            this.filterSideMenu?.Hide();
            this.settingsSideMenu?.Hide();

            if (this.CharacterEdit != null && this.CharacterEdit.Visible)
            {
                this.CharacterEdit.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            }
        }

        private void ModuleInstance_LanguageChanged(object sender, EventArgs e)
        {
            this.tick = this.tick + 10;
            this.updateLayout = true;
        }

        private void ClearButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            this.ResetFilters();
        }

        private void ResetFilters()
        {
            this.CategoryFilters[FilterCategory.Race].Clear();
            this.CategoryFilters[FilterCategory.Crafting].Clear();
            this.CategoryFilters[FilterCategory.Profession].Clear();
            this.CategoryFilters[FilterCategory.ProfessionSpecialization].Clear();
            this.CategoryFilters[FilterCategory.Specialization].Clear();
            this.CategoryFilters[FilterCategory.Hidden].Clear();
            this.CategoryFilters[FilterCategory.Birthday].Clear();

            this.filterSideMenu.ResetToggles();
            this.FilterBox.Text = null;
            this.filterCharacters = true;
            this.filterSideMenu.Tags.ForEach(t => t.Active = false);
        }

        private void FilterBox_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            this.settingsSideMenu.Hide();
            this.filterSideMenu.Show();
            this.CharacterEdit.Hide();
        }

        private void DisplaySettingsButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            this.filterSideMenu.Hide();
            this.settingsSideMenu.Show();
            this.settingsSideMenu.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            this.settingsSideMenu.Opacity = 1f;
            this.CharacterEdit.Hide();
        }

        private void DisplaySettingsButton_MouseLeft(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            this.displaySettingsButton.Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(155052);
        }

        private void DisplaySettingsButton_MouseEntered(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            this.displaySettingsButton.Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(157110);
        }

        private void DraggingControl_LeftMouseButtonReleased(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            this.SetNewIndex(this.DraggingControl.CharacterControl);
            this.DraggingControl.CharacterControl = null;
        }
    }
}
