using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Enums;
using Kenedia.Modules.Characters.Interfaces;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Characters.Views;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Threading.Tasks;

namespace Kenedia.Modules.Characters.Controls.SideMenu
{
    public class SideMenuToggles : FlowTab, ILocalizable
    {
        private List<Tag> _tags = new();
        private readonly FlowPanel _toggleFlowPanel;
        private readonly FlowPanel _tagFlowPanel;
        private readonly List<ImageColorToggle> _toggles = new();
        private Rectangle _contentRectangle;

        public event EventHandler TogglesChanged;

        public SideMenuToggles()
        {
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.AutoSize;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 3);
            Location = new Point(0, 25);

            _toggleFlowPanel = new()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.TopToBottom,
                ControlPadding = new Vector2(5, 3),
                Height = 286,
                Width = Width,
            };

            _tagFlowPanel = new()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 3),
                Width = Width,
            };

            CreateToggles();
            CreateTags();

            _ = Task.Run(async () => { await Task.Delay(250); CalculateTagPanelSize(); });
            Characters.ModuleInstance.LanguageChanged += OnLanguageChanged;
            Characters.ModuleInstance.Tags.CollectionChanged += Tags_CollectionChanged;
            OnLanguageChanged();
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CreateTags();
            CalculateTagPanelSize();
        }

        private void CalculateTagPanelSize()
        {
            if (Visible)
            {
                int width = _tagFlowPanel.Width - (int)(_tagFlowPanel.OuterControlPadding.X * 2);
                int? height = null;

                int curWidth = 0;
                foreach (var tag in _tags)
                {
                    height ??= tag.Height + (int)_tagFlowPanel.ControlPadding.Y + (int)(_tagFlowPanel.OuterControlPadding.Y * 2);

                    int newWidth = curWidth + tag.Width + (int)_tagFlowPanel.ControlPadding.X;

                    if (newWidth >= width)
                    {
                        height += tag.Height + (int)_tagFlowPanel.ControlPadding.Y;
                        curWidth = 0;
                    }

                    curWidth += tag.Width + (int)_tagFlowPanel.ControlPadding.X;
                }

                _tagFlowPanel.Height = (height ?? 0) + (int)(_tagFlowPanel.OuterControlPadding.Y * 2);
            }
        }

        private void CreateTags()
        {
            int i = 0;
            _tags.Clear();
            _tagFlowPanel.Children.Clear();

            foreach (string tag in Characters.ModuleInstance.Tags)
            {
                i++;
                Tag t;
                _tags.Add(t = new Tag()
                {
                    Parent = _tagFlowPanel,
                    Text = tag,
                });
            }
        }

        private Dictionary<string, SearchFilter<Character_Model>> SearchFilters => Characters.ModuleInstance.SearchFilters;

        private void CreateToggles()
        {
            void action(bool active, string entry)
            {
                SearchFilters[entry].IsEnabled = active;

                foreach (var filter in SearchFilters)
                {
                    //Debug.WriteLine($"{filter.Key} is enabled: {filter.Value.IsEnabled}.");
                }

                MainWindow?.PerformFiltering();
            }

            var profs = Characters.ModuleInstance.Data.Professions.ToDictionary(entry => entry.Key, entry => entry.Value);
            profs = profs.OrderBy(e => e.Value.WeightClass).ThenBy(e => e.Value.APIId).ToDictionary(e => e.Key, e => e.Value);

            // Profession All Specs
            foreach (var profession in profs)
            {
                _toggles.Add(new ImageColorToggle((b) => action(b, $"Core {profession.Value.Name}"))
                {
                    Texture = profession.Value.IconBig,
                    UseGrayScale = false,
                    ColorActive = profession.Value.Color,
                    ColorHovered = profession.Value.Color,
                    ColorInActive = profession.Value.Color * 0.5f,
                    Active = SearchFilters[$"Core {profession.Value.Name}"].IsEnabled,
                    Alpha = 0.7f,
                });
            }

            foreach (KeyValuePair<Gw2Sharp.Models.ProfessionType, Data.Profession> profession in profs)
            {
                _toggles.Add(new ImageColorToggle((b) => action(b, profession.Value.Name))
                {
                    Texture = profession.Value.IconBig,
                    Active = SearchFilters[profession.Value.Name].IsEnabled,
                });
            }

            List<ImageColorToggle> specToggles = new();
            foreach (KeyValuePair<SpecializationType, Data.Specialization> specialization in Characters.ModuleInstance.Data.Specializations)
            {
                specToggles.Add(new ImageColorToggle((b) => action(b, specialization.Value.Name))
                {
                    Texture = specialization.Value.IconBig,
                    Profession = specialization.Value.Profession,
                    Active = SearchFilters[specialization.Value.Name].IsEnabled,
                });
            }

            for (int i = 0; i < 3; i++)
            {
                foreach (KeyValuePair<Gw2Sharp.Models.ProfessionType, Data.Profession> p in profs)
                {
                    ImageColorToggle t = specToggles.Find(e => p.Key == e.Profession && !_toggles.Contains(e));
                    if (t != null)
                    {
                        _toggles.Add(t);
                    }
                }
            }

            // Crafting Professions
            foreach (KeyValuePair<int, Data.CrafingProfession> crafting in Characters.ModuleInstance.Data.CrafingProfessions)
            {

                if (crafting.Key > 0)
                {
                    ImageColorToggle img = new((b) => action(b, crafting.Value.Name))
                    {
                        Texture = crafting.Value.Icon,
                        UseGrayScale = false,
                        TextureRectangle = crafting.Key > 0 ? new Rectangle(8, 7, 17, 19) : new Rectangle(4, 4, 24, 24),
                        SizeRectangle = new Rectangle(4, 4, 20, 20),
                        Active = SearchFilters[crafting.Value.Name].IsEnabled,
                    };
                    _toggles.Add(img);
                }
            }

            _toggles.Add(new ImageColorToggle((b) => action(b, "Hidden"))
            {
                Texture = AsyncTexture2D.FromAssetId(605021),
                UseGrayScale = true,
                TextureRectangle = new Rectangle(4, 4, 24, 24),
                BasicTooltipText = Strings.common.ShowHidden_Tooltip,
            });

            _toggles.Add(new ImageColorToggle((b) => action(b, "Birthday"))
            {
                Texture = AsyncTexture2D.FromAssetId(593864),
                UseGrayScale = true,
                TextureRectangle = new Rectangle(1, 0, 30, 32),
                FilterCategory = FilterCategory.Birthday,
                FilterObject = Strings.common.Birthday,
                BasicTooltipText = Strings.common.Show_Birthday_Tooltip,
            });

            foreach (KeyValuePair<Gw2Sharp.Models.RaceType, Data.Race> race in Characters.ModuleInstance.Data.Races)
            {
                _toggles.Add(new ImageColorToggle((b) => action(b, race.Value.Name))
                {
                    Texture = race.Value.Icon,
                    UseGrayScale = true,
                    FilterObject = race.Key,
                    FilterCategory = FilterCategory.Race,
                    BasicTooltipText = race.Value.Name,
                });
            }

            foreach (ImageColorToggle t in _toggles)
            {
                t.Parent = _toggleFlowPanel;
                t.Size = new Point(29, 29);
            }
        }

        private SettingsModel Settings => Characters.ModuleInstance.Settings;

        private MainWindow MainWindow => Characters.ModuleInstance.MainWindow;

        private Characters ModuleInstance => Characters.ModuleInstance;

        public void OnLanguageChanged(object s = null, EventArgs e = null)
        {

        }

        public void OnTogglesChanged(object s = null, EventArgs e = null)
        {
            TogglesChanged?.Invoke(this, e);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            Characters.ModuleInstance.LanguageChanged -= OnLanguageChanged;
            Characters.ModuleInstance.Tags.CollectionChanged -= Tags_CollectionChanged;
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            _contentRectangle = new Rectangle((int)OuterControlPadding.X, (int)OuterControlPadding.Y, Width - ((int)OuterControlPadding.X * 2), Height - ((int)OuterControlPadding.Y * 2));
            _toggleFlowPanel.Width = _contentRectangle.Width;

            _tagFlowPanel.Width = _contentRectangle.Width;
            CalculateTagPanelSize();
        }
    }
}
