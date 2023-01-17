using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Kenedia.Modules.Characters.Classes;
using Kenedia.Modules.Characters.Classes.UI_Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections;
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

namespace Kenedia.Modules.Characters.Classes.MainWindow
{
    public class MainWindow : StandardWindow
    {
        public List<CharacterControl> CharacterControls = new List<CharacterControl>();
        public FlowPanel ContentPanel;
        public DraggingControl DraggingControl = new DraggingControl()
        {
            Parent = GameService.Graphics.SpriteScreen,
            Visible = false,
            ZIndex = 999,
            Enabled = false,
        };

        public Dictionary<object, List<object>> CategoryFilters = new Dictionary<object, List<object>>()
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
        public Image DisplaySettingsButton;
        public ImageButton ClearButton;
        public TextBox FilterBox;
        public FlowPanel DropdownPanel;
        public SettingsSideMenu SettingsSideMenu;
        public Filter_SideMenu Filter_SideMenu;
        public CharacterEdit CharacterEdit;

        private bool _filterCharacters;
        private bool _updateLayout;
        double _tick = 0;
        double _filterTick = 0;

        public MainWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {
            ContentPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(0, 38),
                Size = this.Size,
                ControlPadding = new Vector2(2, 4),
                CanScroll = true,
            };
            //ContentPanel.BackgroundColor = Color.Magenta;

            DraggingControl.LeftMouseButtonReleased += DraggingControl_LeftMouseButtonReleased;

            DropdownPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(0, 2),
                FlowDirection = ControlFlowDirection.SingleRightToLeft,
                OuterControlPadding = new Vector2(0, 2),
                ControlPadding = new Vector2(6, 0),
            };

            DisplaySettingsButton = new Image()
            {
                Parent = DropdownPanel,
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(155052),
                Size = new Point(25, 25),
                BasicTooltipText = "Show Display Settings",
            };
            DisplaySettingsButton.MouseEntered += DisplaySettingsButton_MouseEntered;
            DisplaySettingsButton.MouseLeft += DisplaySettingsButton_MouseLeft;
            DisplaySettingsButton.Click += DisplaySettingsButton_Click;

            ClearButton = new ImageButton()
            {
                Parent = this,
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(2175783),
                HoveredTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(2175782),
                ClickedTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(2175784),
                Size = new Point(20, 20),
                BasicTooltipText = "Clear Filters",
                Visible = false,
            };
            ClearButton.Click += ClearButton_Click;
            //156869

            FilterBox = new TextBox()
            {
                Parent = DropdownPanel,
                PlaceholderText = "Search ...",
                Width = DropdownPanel.Width - DisplaySettingsButton.Width - 5,
            };
            FilterBox.TextChanged += FilterCharacters;
            FilterBox.Click += FilterBox_Click;

            SettingsSideMenu = new SettingsSideMenu()
            {
                TextureOffset = new Point(25, 25),
                Visible = false,
            };

            Filter_SideMenu = new Filter_SideMenu()
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

            ClearButton.Location = new Point(FilterBox.LocalBounds.Right - 25, FilterBox.LocalBounds.Top + 5);
            Characters.ModuleInstance.LanguageChanged += ModuleInstance_LanguageChanged;
        }

        private void CharacterEdit_Shown(object sender, EventArgs e)
        {
            Filter_SideMenu?.Hide();
            SettingsSideMenu?.Hide();

            if (CharacterEdit != null && CharacterEdit.Visible) CharacterEdit.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
        }

        private void ModuleInstance_LanguageChanged(object sender, EventArgs e)
        {
            _tick = _tick + 10;
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

            Filter_SideMenu.ResetToggles();
            FilterBox.Text = null;
            _filterCharacters = true;
            Filter_SideMenu.Tags.ForEach(t => t.Active = false);
        }

        private void FilterBox_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            SettingsSideMenu.Hide();
            Filter_SideMenu.Show();
            CharacterEdit.Hide();
        }

        private void DisplaySettingsButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            Filter_SideMenu.Hide();
            SettingsSideMenu.Show();
            SettingsSideMenu.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            SettingsSideMenu.Opacity = 1f;
            CharacterEdit.Hide();
        }

        public void FilterCharacters(object sender = null, EventArgs e = null)
        {
            _filterCharacters = true;
        }
        public void PerformFiltering()
        {
            var any = Characters.ModuleInstance.Settings.FilterMatching.Value == MatchingBehavior.MatchAny;
            var all = Characters.ModuleInstance.Settings.FilterMatching.Value == MatchingBehavior.MatchAll;

            var textStrings = FilterBox.Text.Trim().ToLower().Split(' ').ToList();
            var matchAny = FilterBox.Text.Trim().Length == 0;
            var s = Characters.ModuleInstance.Settings;
            var data = Characters.ModuleInstance.Data;

            var activeTags = Filter_SideMenu.Tags.Where(e => e.Active);

            var anyTag = activeTags.Count() == 0;
            var raceAny = CategoryFilters[FilterCategory.Race].Count == 0;
            var craftAny = CategoryFilters[FilterCategory.Crafting].Count == 0;
            var profAny = CategoryFilters[FilterCategory.Profession].Count == 0;
            var specProfAny = CategoryFilters[FilterCategory.ProfessionSpecialization].Count == 0;
            var specAny = CategoryFilters[FilterCategory.Specialization].Count == 0;
            var birthAny = CategoryFilters[FilterCategory.Birthday].Count == 0;
            var anyCategory = raceAny && craftAny && profAny && specProfAny && specAny && birthAny;
            var includeHidden = CategoryFilters[FilterCategory.Hidden].Count == 1;

            foreach (CharacterControl c in ContentPanel.Children)
            {
                var crafting_Any = (c.Character.Crafting.Select(e => e.Id).Any(e => CategoryFilters[FilterCategory.Crafting].Contains(e) && (!s.Check_OnlyMaxCrafting.Value || c.Character.Crafting.Find(a => a.Id == e).Rating == data.CrafingProfessions[e].MaxRating)));
                var crafting_All = CategoryFilters[FilterCategory.Crafting].Select(e => (int) e).All(e => c.Character.Crafting.Find(a => a.Id == e && (!s.Check_OnlyMaxCrafting.Value || a.Rating == data.CrafingProfessions[e].MaxRating)) != null);

                if (matchAny && anyCategory && anyTag)
                {
                    c.Visible = (c.Character.Show || includeHidden);
                    continue;
                }

                List<FilterTag> filterCategories = new List<FilterTag>()
                {
                    new FilterTag()
                    {
                        Result = raceAny || CategoryFilters[FilterCategory.Race].Contains(c.Character.Race)
                    },
                    new FilterTag()
                    {
                        Result = craftAny|| (any ? crafting_Any : crafting_All),
                    },
                    new FilterTag()
                    {
                        Result = profAny|| (any || CategoryFilters[FilterCategory.Profession].Count == 1) && CategoryFilters[FilterCategory.Profession].Contains(c.Character.Profession)
                    },
                    new FilterTag()
                    {
                        Result = birthAny|| c.Character.HasBirthdayPresent,
                    },
                    new FilterTag()
                    {
                        Result = specProfAny && specAny || (any ||CategoryFilters[FilterCategory.ProfessionSpecialization].Count == 1 ) && (c.Character.Specialization == SpecializationType.None && CategoryFilters[FilterCategory.ProfessionSpecialization].Contains(c.Character.Profession)) || CategoryFilters[FilterCategory.Specialization].Contains(c.Character.Specialization)
                    },
                };

                List<FilterTag> filterTags = Filter_SideMenu.Tags.Where(e => e.Active).Select(e => e.Text).ToList().CreateFilterTagList();
                List<FilterTag> filterStrings = textStrings.CreateFilterTagList();

                if (!anyTag)
                {
                    filterTags.ForEach(ft => ft.Result = c.Character.Tags.Contains(ft.Tag));
                }

                if (!matchAny)
                {
                    if (s.Check_Name.Value)
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

                        //visible = tag != null ? s.FilterDirection.Value == FilterBehavior.Include : visible;
                    }

                    if (s.Check_Level.Value)
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

                        //visible = tag != null ? s.FilterDirection.Value == FilterBehavior.Include : visible;
                    }

                    if (s.Check_Race.Value)
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

                        //visible = filterStrings.Find(ex =>
                        //{
                        //    var value = c.Character.Race.GetData();
                        //    return value != null && value.Name.ToLower().Contains(ex);
                        //}) != null ? s.FilterDirection.Value == FilterBehavior.Include : visible;
                    }

                    if (s.Check_Profession.Value)
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

                        //visible = filterStrings.Find(ex =>
                        //{
                        //    var value = c.Character.Profession.GetData();
                        //    return value != null && value.Name.ToLower().Contains(ex);
                        //}) != null ? s.FilterDirection.Value == FilterBehavior.Include : visible;

                        //visible = filterStrings.Find(ex =>
                        //{
                        //    var value = c.Character.Specialization.GetData();
                        //    return value != null && value.Name.ToLower().Contains(ex);
                        //}) != null ? s.FilterDirection.Value == FilterBehavior.Include : visible;
                    }

                    if (s.Check_Map.Value)
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

                        //visible = filterStrings.Find(ex =>
                        //{
                        //    var value = Characters.ModuleInstance.Data.GetMapById(c.Character.Map);
                        //    return value != null && value.Name.ToLower().Contains(ex);
                        //}) != null ? s.FilterDirection.Value == FilterBehavior.Include : visible;
                    }

                    if (s.Check_Crafting.Value)
                    {
                        foreach (CharacterCrafting crafting in c.Character.Crafting)
                        {
                            var value = crafting.Id.GetData();

                            if (value != null)
                            {
                                foreach (FilterTag ex in filterStrings)
                                {
                                    if (value.Name.ToLower().Contains(ex) && (!s.Check_OnlyMaxCrafting.Value || value.MaxRating == crafting.Rating))
                                    {
                                        ex.Result = true;
                                    }
                                }
                            }

                            //visible = value != null && (!s.Check_OnlyMaxCrafting.Value || crafting.Rating == value.MaxRating) && filterStrings.Find(ex =>
                            //{
                            //    return value != null && value.Name.ToLower().Contains(ex);
                            //}) != null ? s.FilterDirection.Value == FilterBehavior.Include : visible;
                        }
                    }

                    if (s.Check_Tags.Value)
                    {
                        var tags = c.Character.Tags.ToList().ConvertAll(d => d.ToLower()); ;

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

                        //visible = tag != null ? s.FilterDirection.Value == FilterBehavior.Include : visible;
                    }
                }

                var matched = matchAny || (any ? filterStrings.Where(r => r.Result == true).Count() > 0 : filterStrings.Where(r => r.Result == true).Count() == filterStrings.Count);
                var catMatched = anyCategory || filterCategories.Where(r => r.Result == true).Count() == filterCategories.Count;
                var tagMatched = anyTag || (any ? filterTags.Where(r => r.Result == true).Count() > 0 : filterTags.Where(r => r.Result == true).Count() == filterTags.Count);

                c.Visible = (c.Character.Show || includeHidden) && (s.FilterDirection.Value == FilterBehavior.Include ? matched && catMatched && tagMatched : !matched && !catMatched && !tagMatched);
            }
            ClearButton.Visible = !anyCategory || !matchAny || !anyTag;
            SortCharacters();
            ContentPanel.Invalidate();
        }

        private void DisplaySettingsButton_MouseLeft(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            DisplaySettingsButton.Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(155052);
        }

        private void DisplaySettingsButton_MouseEntered(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            DisplaySettingsButton.Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(157110);
        }


        public void UpdateLayout()
        {
            _updateLayout = false;
            var PanelLayout = Characters.ModuleInstance.Settings.PanelLayout.Value;
            var PanelSize = Characters.ModuleInstance.Settings.PanelSize.Value;

            var size = Point.Zero;
            var nameFont = GameService.Content.DefaultFont14;
            var font = GameService.Content.DefaultFont12;
            var testString = Characters.ModuleInstance.Character_Models.Aggregate("", (max, cur) => max.Length > cur.Name.Length ? max : cur.Name);

            switch (PanelSize)
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
            switch (PanelLayout)
            {
                case CharacterPanelLayout.OnlyIcons:
                    {
                        var newSize = new Point(Math.Min(size.X, size.Y), Math.Min(size.X, size.Y));
                        foreach (CharacterControl c in ContentPanel.Children)
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
                        foreach (CharacterControl c in ContentPanel.Children)
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
                        foreach (CharacterControl c in ContentPanel.Children)
                        {
                            c.Size = newSize;
                            c.Font = font;
                            c.NameFont = nameFont;
                            c.UpdateLayout();
                        }
                        break;
                    }
            }

            var maxWidth = ContentPanel.Children.Count > 0 ? ContentPanel.Children.Cast<CharacterControl>().Max(t => t.TotalWidth) : Width;

            foreach (CharacterControl c in ContentPanel.Children) { c.Width = maxWidth; }
        }

        private void DraggingControl_LeftMouseButtonReleased(object sender, Blish_HUD.Input.MouseEventArgs e)
        {

            SetNewIndex(DraggingControl.CharacterControl);
            DraggingControl.CharacterControl = null;
        }

        public void SortCharacters()
        {
            var order = Characters.ModuleInstance.Settings.Sort_Order.Value;
            var sort = Characters.ModuleInstance.Settings.Sort_Type.Value;

            switch (sort)
            {
                case SortType.SortByName:
                    {
                        switch (order)
                        {
                            case SortOrder.Ascending:
                                ContentPanel.SortChildren<CharacterControl>((a, b) => a.Character.Name.CompareTo(b.Character.Name));
                                break;

                            case SortOrder.Descending:
                                ContentPanel.SortChildren<CharacterControl>((a, b) => b.Character.Name.CompareTo(a.Character.Name));
                                break;
                        }
                        break;
                    }
                case SortType.SortByLastLogin:
                    {
                        switch (order)
                        {
                            case SortOrder.Ascending:
                                ContentPanel.SortChildren<CharacterControl>((a, b) => b.Character.LastLogin.CompareTo(a.Character.LastLogin));
                                break;

                            case SortOrder.Descending:
                                ContentPanel.SortChildren<CharacterControl>((a, b) => a.Character.LastLogin.CompareTo(b.Character.LastLogin));
                                break;
                        }
                        break;
                    }
                case SortType.SortByMap:
                    {
                        switch (order)
                        {
                            case SortOrder.Ascending:
                                ContentPanel.SortChildren<CharacterControl>((a, b) => a.Character.Map.CompareTo(b.Character.Map));
                                break;

                            case SortOrder.Descending:
                                ContentPanel.SortChildren<CharacterControl>((a, b) => b.Character.Map.CompareTo(a.Character.Map));
                                break;
                        }
                        break;
                    }
                case SortType.SortByProfession:
                    {
                        switch (order)
                        {
                            case SortOrder.Ascending:
                                ContentPanel.SortChildren<CharacterControl>((a, b) => a.Character.Profession.CompareTo(b.Character.Profession));
                                break;

                            case SortOrder.Descending:
                                ContentPanel.SortChildren<CharacterControl>((a, b) => b.Character.Profession.CompareTo(a.Character.Profession));
                                break;
                        }
                        break;
                    }
                case SortType.SortByTag:
                    {
                        break;
                    }
                case SortType.Custom:
                    {
                        ContentPanel.SortChildren<CharacterControl>((a, b) => a.Index.CompareTo(b.Index));

                        var i = 0;
                        foreach (CharacterControl c in ContentPanel.Children)
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
            characterControl.Index = Characters.ModuleInstance.MainWindow.getHoveredIndex(characterControl);

            Characters.ModuleInstance.MainWindow.SortCharacters();
        }

        public double getHoveredIndex(CharacterControl characterControl)
        {
            var m = Input.Mouse;
            CharacterControl lastControl = characterControl;

            var i = 0;
            foreach (CharacterControl c in ContentPanel.Children)
            {
                c.Index = i;
                i++;
            }

            foreach (CharacterControl c in ContentPanel.Children)
            {
                if (c.AbsoluteBounds.Contains(m.Position))
                {
                    return characterControl.Index > c.Index ? c.Index - 0.1 : c.Index + 0.1;
                }

                lastControl = c;
            }

            if (lastControl.AbsoluteBounds.Bottom < m.Position.Y || lastControl.AbsoluteBounds.Top < m.Position.Y && lastControl.AbsoluteBounds.Right < m.Position.X)
            {
                return CharacterControls.Count + 1;
            }

            return characterControl.Index;
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            if (ContentPanel != null) ContentPanel.Size = new Point(ContentRegion.Size.X, ContentRegion.Size.Y - 35);

            if (DropdownPanel != null)
            {
                DropdownPanel.Size = new Point(ContentRegion.Size.X, 31);
                FilterBox.Width = DropdownPanel.Width - DisplaySettingsButton.Width - 5;
                ClearButton.Location = new Point(FilterBox.LocalBounds.Right - 23, FilterBox.LocalBounds.Top + 6);
            }

            if (e.CurrentSize.Y < 135)
            {
                Size = new Point(Size.X, 135);
            }

            if (SettingsSideMenu != null && SettingsSideMenu.Visible) SettingsSideMenu.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            if (Filter_SideMenu != null && Filter_SideMenu.Visible) Filter_SideMenu.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            if (CharacterEdit != null && CharacterEdit.Visible) CharacterEdit.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
        }

        protected override void OnMoved(MovedEventArgs e)
        {
            base.OnMoved(e);

            if (SettingsSideMenu != null && SettingsSideMenu.Visible) SettingsSideMenu.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            if (Filter_SideMenu != null && Filter_SideMenu.Visible) Filter_SideMenu.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
            if (CharacterEdit != null && CharacterEdit.Visible) CharacterEdit.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            CharacterControls?.DisposeAll();

            ContentPanel?.Dispose();
            DraggingControl?.Dispose();
            SettingsSideMenu?.Dispose();
            Filter_SideMenu?.Dispose();
            CharacterEdit?.Dispose();

            DropdownPanel?.Dispose();
            DisplaySettingsButton?.Dispose();
            FilterBox?.Dispose();
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);
            var texture = Characters.ModuleInstance.TextureManager.getEmblem(_Emblems.Characters);
            spriteBatch.DrawOnCtrl(this,
                                    texture,
                                    new Rectangle(-43, -58, 128, 128),
                                    texture.Bounds,
                                    Color.White,
                                    0f,
                                    default);

            if (bounds.Width >= 190)
            {
                spriteBatch.DrawStringOnCtrl(this,
                                        "Characters", //$"{Characters.ModuleInstance.Name}",
                                        bounds.Width >= 245 ? GameService.Content.DefaultFont32 : GameService.Content.DefaultFont18,
                                        new Rectangle(65, 5, bounds.Width - (128 - 43 + 15), 30),
                                        ContentService.Colors.ColonialWhite, //new Color(247, 231, 182, 97),
                                        false,
                                        HorizontalAlignment.Left,
                                        VerticalAlignment.Middle
                                        );
            }

            //Emblem = TextureManager.getEmblem(_Emblems.Characters),
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);

            Filter_SideMenu?.Hide();
            SettingsSideMenu?.Hide();
            CharacterEdit?.Hide();
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (_filterCharacters && gameTime.TotalGameTime.TotalMilliseconds - _filterTick > Characters.ModuleInstance.Settings.FilterDelay.Value)
            {
                _filterTick = gameTime.TotalGameTime.TotalMilliseconds;
                PerformFiltering();
                _filterCharacters = false;
            }

            if (gameTime.TotalGameTime.TotalMilliseconds - _tick > 50)
            {
                _tick = gameTime.TotalGameTime.TotalMilliseconds;

                if (_updateLayout) UpdateLayout();
            }
        }
    }
}
