using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Controls;
using Kenedia.Modules.Characters.Enums;
using Kenedia.Modules.Characters.Extensions;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using static Kenedia.Modules.Characters.Services.SettingsModel;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Views
{
    public class MainWindow : StandardWindow
    {
        private readonly AsyncTexture2D _windowEmblem = GameService.Content.DatAssetCache.GetTextureFromAssetId(156015);

        private readonly Image _displaySettingsButton;
        private readonly ImageButton _randomButton;
        private readonly ImageButton _clearButton;
        private readonly FlowPanel _dropdownPanel;
        private readonly SettingsSideMenu _settingsSideMenu;
        private readonly FilterSideMenu _filterSideMenu;
        private readonly FlowPanel _buttonPanel;

        private bool _filterCharacters;
        private bool _updateLayout;
        private double _tick = 0;
        private double _filterTick = 0;

        private Rectangle _emblemRectangle;
        private Rectangle _titleRectangle;
        private BitmapFont _titleFont;

        public MainWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion)
            : base(background, windowRegion, contentRegion)
        {
            ContentPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(0, 35),
            };

            _ = new Dummy()
            {
                Parent = ContentPanel,
                Width = ContentPanel.Width,
                Height = 3,
            };

            CharactersPanel = new FlowPanel()
            {
                Parent = ContentPanel,
                Size = Size,
                ControlPadding = new Vector2(2, 4),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                CanScroll = true,
            };

            DraggingControl.LeftMouseButtonReleased += DraggingControl_LeftMouseButtonReleased;

            _dropdownPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(0, 2),
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(5, 0),
            };

            FilterBox = new TextBox()
            {
                Parent = _dropdownPanel,
                PlaceholderText = Strings.common.Search,
                Width = 100,
            };
            FilterBox.TextChanged += FilterCharacters;
            FilterBox.Click += FilterBox_Click;
            FilterBox.EnterPressed += FilterBox_EnterPressed;

            _clearButton = new ImageButton()
            {
                Parent = this,
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(2175783),
                HoveredTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(2175782),
                ClickedTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(2175784),
                Size = new Point(20, 20),
                BasicTooltipText = Strings.common.ClearFilters,
                Visible = false,
            };
            _clearButton.Click += ClearButton_Click;

            _buttonPanel = new FlowPanel()
            {
                Parent = _dropdownPanel,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Padding = new(15),
            };
            _buttonPanel.Resized += ButtonPanel_Resized;
            _randomButton = new()
            {
                Parent = _buttonPanel,
                Size = new Point(25, 25),
                BasicTooltipText = Strings.common.RandomButton_Tooltip,
                Texture = Characters.ModuleInstance.TextureManager.GetIcon(TextureManager.Icons.Dice),
                HoveredTexture = Characters.ModuleInstance.TextureManager.GetIcon(TextureManager.Icons.Dice_Hovered),
                Visible = Characters.ModuleInstance.Settings.ShowRandomButton.Value,
            };
            _randomButton.Click += RandomButton_Click;
            Characters.ModuleInstance.Settings.ShowRandomButton.SettingChanged += ShowRandomButton_SettingChanged;

            _displaySettingsButton = new()
            {
                Parent = _buttonPanel,
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(155052),
                Size = new Point(25, 25),
                BasicTooltipText = string.Format(Strings.common.ShowItem, string.Format(Strings.common.ItemSettings, Strings.common.Display)),
            };
            _displaySettingsButton.MouseEntered += DisplaySettingsButton_MouseEntered;
            _displaySettingsButton.MouseLeft += DisplaySettingsButton_MouseLeft;
            _displaySettingsButton.Click += DisplaySettingsButton_Click;

            _settingsSideMenu = new SettingsSideMenu()
            {
                TextureOffset = new Point(25, 25),
                Visible = false,
            };

            _filterSideMenu = new FilterSideMenu()
            {
                TextureOffset = new Point(25, 25),
                Visible = false,
            };

            CharacterEdit = new CharacterEdit()
            {
                TextureOffset = new Point(25, 25),
                Visible = false,
            };
            CharacterEdit.Shown += CharacterEdit_Shown;

            Characters.ModuleInstance.LanguageChanged += ModuleInstance_LanguageChanged;
        }

        private void ShowRandomButton_SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            _randomButton.Visible = Characters.ModuleInstance.Settings.ShowRandomButton.Value;
            _buttonPanel.Invalidate();
        }

        private void RandomButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            var selection = CharactersPanel.Children.Where(e => e.Visible).ToList();
            int r = RandomService.Rnd.Next(selection.Count);
            var entry = (CharacterControl)selection[r];

            if (entry != null)
            {
                CharacterSwapping.Start(entry.Character);
            }
        }

        private void ButtonPanel_Resized(object sender, ResizedEventArgs e)
        {
            FilterBox.Width = _dropdownPanel.Width - _buttonPanel.Width - 2;
            _clearButton.Location = new Point(FilterBox.LocalBounds.Right - 25, FilterBox.LocalBounds.Top + 5);
        }

        private void CharacterSorting_Finished(object sender, EventArgs e)
        {
            SortCharacters();
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

        public FlowPanel CharactersPanel { get; private set; }

        public FlowPanel ContentPanel { get; private set; }

        public void FilterCharacters(object sender = null, EventArgs e = null)
        {
            _filterCharacters = true;
        }

        public void PerformFiltering()
        {
            bool any = Characters.ModuleInstance.Settings.ResultMatchingBehavior.Value == MatchingBehavior.MatchAny;
            bool all = Characters.ModuleInstance.Settings.ResultMatchingBehavior.Value == MatchingBehavior.MatchAll;

            Regex regex = new(@"\w+|""[\w\s]*""");
            var x = regex.Matches(FilterBox.Text.Trim().ToLower()).Cast<Match>().ToList();

            List<string> textStrings = new();
            foreach (Match match in x) { Debug.WriteLine(match.ToString()); textStrings.Add(match.ToString().Replace("\"", "")); }

            bool matchAny = FilterBox.Text.Trim().Length == 0;
            SettingsModel s = Characters.ModuleInstance.Settings;
            Data data = Characters.ModuleInstance.Data;

            IEnumerable<Tag> activeTags = _filterSideMenu.Tags.Where(e => e.Active);

            bool anyTag = activeTags.Count() == 0;
            bool raceAny = CategoryFilters[FilterCategory.Race].Count == 0;
            bool craftAny = CategoryFilters[FilterCategory.Crafting].Count == 0;
            bool profAny = CategoryFilters[FilterCategory.Profession].Count == 0;
            bool specProfAny = CategoryFilters[FilterCategory.ProfessionSpecialization].Count == 0;
            bool specAny = CategoryFilters[FilterCategory.Specialization].Count == 0;
            bool birthAny = CategoryFilters[FilterCategory.Birthday].Count == 0;
            bool anyCategory = raceAny && craftAny && profAny && specProfAny && specAny && birthAny;
            bool includeHidden = CategoryFilters[FilterCategory.Hidden].Count == 1;

            foreach (CharacterControl c in CharactersPanel.Children)
            {
                bool crafting_Any = c.Character.Crafting.Select(e => e.Id).Any(e => CategoryFilters[FilterCategory.Crafting].Contains(e) && (!s.CheckOnlyMaxCrafting.Value || c.Character.Crafting.Find(a => a.Id == e).Rating == data.CrafingProfessions[e].MaxRating));
                bool crafting_All = CategoryFilters[FilterCategory.Crafting].Select(e => (int)e).All(e => c.Character.Crafting.Find(a => a.Id == e && (!s.CheckOnlyMaxCrafting.Value || a.Rating == data.CrafingProfessions[e].MaxRating)) != null);

                if (matchAny && anyCategory && anyTag)
                {
                    c.Visible = c.Character.Show || includeHidden;
                    continue;
                }

                List<FilterTag> filterCategories = new()
                {
                    new FilterTag()
                    {
                        Result = raceAny || CategoryFilters[FilterCategory.Race].Contains(c.Character.Race),
                    },
                    new FilterTag()
                    {
                        Result = craftAny || (any ? crafting_Any : crafting_All),
                    },
                    new FilterTag()
                    {
                        Result = profAny || ((any || CategoryFilters[FilterCategory.Profession].Count == 1) && CategoryFilters[FilterCategory.Profession].Contains(c.Character.Profession)),
                    },
                    new FilterTag()
                    {
                        Result = birthAny || c.Character.HasBirthdayPresent,
                    },
                    new FilterTag()
                    {
                        Result = (specProfAny && specAny) || ((any || CategoryFilters[FilterCategory.ProfessionSpecialization].Count == 1) && c.Character.Specialization == SpecializationType.None && CategoryFilters[FilterCategory.ProfessionSpecialization].Contains(c.Character.Profession)) || CategoryFilters[FilterCategory.Specialization].Contains(c.Character.Specialization),
                    },
                };

                List<FilterTag> filterTags = _filterSideMenu.Tags.Where(e => e.Active).Select(e => e.Text).ToList().CreateFilterTagList();
                List<FilterTag> filterStrings = textStrings.CreateFilterTagList();

                if (!anyTag)
                {
                    filterTags.ForEach(ft => ft.Result = c.Character.Tags.Contains(ft.Tag));
                }

                if (!matchAny)
                {
                    if (s.CheckName.Value)
                    {
                        string value = c.Character.Name.ToString();

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
                        string value = c.Character.Level.ToString();

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
                        Services.Data.Race value = c.Character.Race.GetData();

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
                        Services.Data.Profession value = c.Character.Profession.GetData();

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

                        Services.Data.Specialization value2 = c.Character.Specialization.GetData();

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
                        Map value = Characters.ModuleInstance.Data.GetMapById(c.Character.Map);

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
                            Services.Data.CrafingProfession value = crafting.Id.GetData();

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
                        List<string> tags = c.Character.Tags.ToList().ConvertAll(d => d.ToLower());

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

                bool matched = false;
                bool catMatched = false;
                bool tagMatched = false;

                if (s.ResultFilterBehavior.Value == FilterBehavior.Exclude)
                {
                    matched = !matchAny && (any ? filterStrings.Where(r => r.Result == true).Count() > 0 : filterStrings.Where(r => r.Result == true).Count() == filterStrings.Count);
                    tagMatched = !anyTag && (any ? filterTags.Where(r => r.Result == true).Count() > 0 : filterTags.Where(r => r.Result == true).Count() == filterTags.Count);
                    catMatched = !anyCategory && filterCategories.Where(r => r.Result == true).Count() == filterCategories.Count;
                }
                else
                {
                    matched = matchAny || (any ? filterStrings.Where(r => r.Result == true).Count() > 0 : filterStrings.Where(r => r.Result == true).Count() == filterStrings.Count);
                    catMatched = anyCategory || filterCategories.Where(r => r.Result == true).Count() == filterCategories.Count;
                    tagMatched = anyTag || (any ? filterTags.Where(r => r.Result == true).Count() > 0 : filterTags.Where(r => r.Result == true).Count() == filterTags.Count);
                }

                c.Visible = (c.Character.Show || includeHidden) && (s.ResultFilterBehavior.Value == FilterBehavior.Include ? matched && catMatched && tagMatched : !matched && !catMatched && !tagMatched);
            }

            _clearButton.Visible = !anyCategory || !matchAny || !anyTag;
            SortCharacters();
            CharactersPanel.Invalidate();
        }

        public void UpdateLayout()
        {
            _updateLayout = false;
            CharacterPanelLayout panelLayout = Characters.ModuleInstance.Settings.PanelLayout.Value;
            PanelSizes panelSize = Characters.ModuleInstance.Settings.PanelSize.Value;

            Point size = Point.Zero;
            MonoGame.Extended.BitmapFonts.BitmapFont nameFont = GameService.Content.DefaultFont14;
            MonoGame.Extended.BitmapFonts.BitmapFont font = GameService.Content.DefaultFont12;
            string testString = Characters.ModuleInstance.CharacterModels.Aggregate(string.Empty, (max, cur) => max.Length > cur.Name.Length ? max : cur.Name);

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
                        Point newSize = new(Math.Min(size.X, size.Y), Math.Min(size.X, size.Y));
                        foreach (CharacterControl c in CharactersPanel.Children)
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
                        Point newSize = size;
                        foreach (CharacterControl c in CharactersPanel.Children)
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
                        Point newSize = size;
                        foreach (CharacterControl c in CharactersPanel.Children)
                        {
                            c.Size = newSize;
                            c.Font = font;
                            c.NameFont = nameFont;
                            c.UpdateLayout();
                        }

                        break;
                    }
            }

            int maxWidth = CharactersPanel.Children.Count > 0 ? CharactersPanel.Children.Cast<CharacterControl>().Max(t => t.TotalWidth) : Width;

            foreach (CharacterControl c in CharactersPanel.Children)
            {
                c.Width = maxWidth;
            }
        }

        public void SortCharacters()
        {
            ESortOrder order = Characters.ModuleInstance.Settings.SortOrder.Value;
            ESortType sort = Characters.ModuleInstance.Settings.SortType.Value;

            switch (sort)
            {
                case ESortType.SortByName:
                    {
                        switch (order)
                        {
                            case ESortOrder.Ascending:
                                CharactersPanel.SortChildren<CharacterControl>((a, b) => a.Character.Name.CompareTo(b.Character.Name));
                                break;

                            case ESortOrder.Descending:
                                CharactersPanel.SortChildren<CharacterControl>((a, b) => b.Character.Name.CompareTo(a.Character.Name));
                                break;
                        }

                        break;
                    }

                case ESortType.SortByLastLogin:
                    {
                        switch (order)
                        {
                            case ESortOrder.Ascending:
                                CharactersPanel.SortChildren<CharacterControl>((a, b) => b.Character.LastLogin.CompareTo(a.Character.LastLogin));
                                break;

                            case ESortOrder.Descending:
                                CharactersPanel.SortChildren<CharacterControl>((a, b) => a.Character.LastLogin.CompareTo(b.Character.LastLogin));
                                break;
                        }

                        break;
                    }

                case ESortType.SortByMap:
                    {
                        switch (order)
                        {
                            case ESortOrder.Ascending:
                                CharactersPanel.SortChildren<CharacterControl>((a, b) => a.Character.Map.CompareTo(b.Character.Map));
                                break;

                            case ESortOrder.Descending:
                                CharactersPanel.SortChildren<CharacterControl>((a, b) => b.Character.Map.CompareTo(a.Character.Map));
                                break;
                        }

                        break;
                    }

                case ESortType.SortByProfession:
                    {
                        switch (order)
                        {
                            case ESortOrder.Ascending:
                                CharactersPanel.SortChildren<CharacterControl>((a, b) => a.Character.Profession.CompareTo(b.Character.Profession));
                                break;

                            case ESortOrder.Descending:
                                CharactersPanel.SortChildren<CharacterControl>((a, b) => b.Character.Profession.CompareTo(a.Character.Profession));
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
                        CharactersPanel.SortChildren<CharacterControl>((a, b) => a.Index.CompareTo(b.Index));

                        int i = 0;
                        foreach (CharacterControl c in CharactersPanel.Children)
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
            Blish_HUD.Input.MouseHandler m = Input.Mouse;
            CharacterControl lastControl = characterControl;

            int i = 0;
            foreach (CharacterControl c in CharactersPanel.Children)
            {
                c.Index = i;
                i++;
            }

            foreach (CharacterControl c in CharactersPanel.Children)
            {
                if (c.AbsoluteBounds.Contains(m.Position))
                {
                    return characterControl.Index > c.Index ? c.Index - 0.1 : c.Index + 0.1;
                }

                lastControl = c;
            }

            return lastControl.AbsoluteBounds.Bottom < m.Position.Y || (lastControl.AbsoluteBounds.Top < m.Position.Y && lastControl.AbsoluteBounds.Right < m.Position.X)
                ? CharacterControls.Count + 1
                : characterControl.Index;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            string versionText = $"v. {Characters.ModuleInstance.Version}";
            BasicTooltipText = MouseOverTitleBar ? versionText : null;

            if (_filterCharacters && gameTime.TotalGameTime.TotalMilliseconds - _filterTick > Characters.ModuleInstance.Settings.FilterDelay.Value)
            {
                _filterTick = gameTime.TotalGameTime.TotalMilliseconds;
                PerformFiltering();
                _filterCharacters = false;
            }

            if (gameTime.TotalGameTime.TotalMilliseconds - _tick > 50)
            {
                _tick = gameTime.TotalGameTime.TotalMilliseconds;

                if (_updateLayout)
                {
                    UpdateLayout();
                }
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _emblemRectangle = new(-43, -58, 128, 128);

            _titleFont = GameService.Content.DefaultFont32;
            var titleBounds = _titleFont.GetStringRectangle(Characters.ModuleInstance.Name);

            if (titleBounds.Width > LocalBounds.Width - (_emblemRectangle.Width - 15))
            {
                _titleFont = GameService.Content.DefaultFont18;
                titleBounds = _titleFont.GetStringRectangle(Characters.ModuleInstance.Name);
            }

             _titleRectangle = new(65, 5, (int)titleBounds.Width, Math.Max(30, (int)titleBounds.Height));
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            spriteBatch.DrawOnCtrl(
                this,
                _windowEmblem,
                _emblemRectangle,
                _windowEmblem.Bounds,
                Color.White,
                0f,
                default);

            if (_titleRectangle.Width < bounds.Width - (_emblemRectangle.Width - 20))
            {
                spriteBatch.DrawStringOnCtrl(
                    this,
                    $"{Characters.ModuleInstance.Name}",
                    _titleFont,
                    _titleRectangle,
                    ContentService.Colors.ColonialWhite, // new Color(247, 231, 182, 97),
                    false,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Bottom);
            }
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);

            _filterSideMenu?.Hide();
            _settingsSideMenu?.Hide();
            CharacterEdit?.Hide();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            if (ContentPanel != null)
            {
                ContentPanel.Size = new Point(ContentRegion.Size.X, ContentRegion.Size.Y - 35);
            }

            if (_dropdownPanel != null)
            {
                //_dropdownPanel.Size = new Point(ContentRegion.Size.X, 31);
                FilterBox.Width = _dropdownPanel.Width - _buttonPanel.Width - 2;
                _clearButton.Location = new Point(FilterBox.LocalBounds.Right - 23, FilterBox.LocalBounds.Top + 6);
            }

            if (e.CurrentSize.Y < 135)
            {
                Size = new Point(Size.X, 135);
            }

            if (_settingsSideMenu != null && _settingsSideMenu.Visible)
            {
                _settingsSideMenu.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            }

            if (_filterSideMenu != null && _filterSideMenu.Visible)
            {
                _filterSideMenu.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            }

            if (CharacterEdit != null && CharacterEdit.Visible)
            {
                CharacterEdit.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            }
        }

        protected override void OnMoved(MovedEventArgs e)
        {
            base.OnMoved(e);

            if (_settingsSideMenu != null && _settingsSideMenu.Visible)
            {
                _settingsSideMenu.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            }

            if (_filterSideMenu != null && _filterSideMenu.Visible)
            {
                _filterSideMenu.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            }

            if (CharacterEdit != null && CharacterEdit.Visible)
            {
                CharacterEdit.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            CharacterSorting.Completed -= CharacterSorting_Finished;
            // if(CharacterControls.Count >0) CharacterControls?.DisposeAll();
            //ContentPanel?.DisposeAll();
            //CharactersPanel?.Dispose();
            DraggingControl?.Dispose();
            _settingsSideMenu?.Dispose();
            _filterSideMenu?.Dispose();
            CharacterEdit?.Dispose();

            _dropdownPanel?.Dispose();
            _displaySettingsButton?.Dispose();
            FilterBox?.Dispose();
        }

        private void FilterBox_EnterPressed(object sender, EventArgs e)
        {
            if (Characters.ModuleInstance.Settings.EnterToLogin.Value)
            {
                PerformFiltering();
                var c = (CharacterControl)CharactersPanel.Children.Where(e => e.Visible).FirstOrDefault();

                if (c != null)
                {
                    Characters.ModuleInstance.SwapTo(c.Character);
                }
            }
        }

        private void CharacterEdit_Shown(object sender, EventArgs e)
        {
            _filterSideMenu?.Hide();
            _settingsSideMenu?.Hide();

            if (CharacterEdit != null && CharacterEdit.Visible)
            {
                CharacterEdit.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            }
        }

        private void ModuleInstance_LanguageChanged(object sender, EventArgs e)
        {
            _tick += 10;
            _updateLayout = true;
        }

        private void ClearButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            ResetFilters();
        }

        private void ResetFilters()
        {
            CategoryFilters[FilterCategory.Race].Clear();
            CategoryFilters[FilterCategory.Crafting].Clear();
            CategoryFilters[FilterCategory.Profession].Clear();
            CategoryFilters[FilterCategory.ProfessionSpecialization].Clear();
            CategoryFilters[FilterCategory.Specialization].Clear();
            CategoryFilters[FilterCategory.Hidden].Clear();
            CategoryFilters[FilterCategory.Birthday].Clear();

            _filterSideMenu.ResetToggles();
            FilterBox.Text = null;
            _filterCharacters = true;
            _filterSideMenu.Tags.ForEach(t => t.Active = false);
        }

        private void FilterBox_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _settingsSideMenu.Hide();
            _filterSideMenu.Show();
            CharacterEdit.Hide();
        }

        private void DisplaySettingsButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _filterSideMenu.Hide();
            _settingsSideMenu.Show();
            _settingsSideMenu.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            _settingsSideMenu.Opacity = 1f;
            CharacterEdit.Hide();
        }

        private void DisplaySettingsButton_MouseLeft(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _displaySettingsButton.Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(155052);
        }

        private void DisplaySettingsButton_MouseEntered(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _displaySettingsButton.Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(157110);
        }

        private void DraggingControl_LeftMouseButtonReleased(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            SetNewIndex(DraggingControl.CharacterControl);
            DraggingControl.CharacterControl = null;
        }
    }
}
