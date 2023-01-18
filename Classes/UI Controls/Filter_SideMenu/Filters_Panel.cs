namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    using System.Collections.Generic;
    using System.Linq;
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Microsoft.Xna.Framework;
    using Point = Microsoft.Xna.Framework.Point;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    internal class Filters_Panel : FlowTab
    {
        private readonly List<ImageColorToggle> _toggles = new List<ImageColorToggle>();

        public void ResetToggles()
        {
            foreach (ImageColorToggle t in this._toggles)
            {
                t.Active = false;
            }
        }

        public Filters_Panel()
        {
            this.Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(440021);
            this.Name = "Filter Toggles";
            this.FlowDirection = ControlFlowDirection.TopToBottom;
            this.WidthSizingMode = SizingMode.Fill;
            this.AutoSizePadding = new Point(5, 5);
            this.HeightSizingMode = SizingMode.Standard;
            this.Height = 250;
            this.OuterControlPadding = new Vector2(5, 5);
            this.ControlPadding = new Vector2(5, 3);
            this.Location = new Point(0, 25);

            var toggleDir = new Dictionary<Gw2Sharp.Models.ProfessionType, List<ImageColorToggle>>() {
                {
                    Gw2Sharp.Models.ProfessionType.Guardian,
                    new List<ImageColorToggle>()
                },
                {
                    Gw2Sharp.Models.ProfessionType.Warrior,
                    new List<ImageColorToggle>()
                },
                {
                    Gw2Sharp.Models.ProfessionType.Engineer,
                    new List<ImageColorToggle>()
                },
                {
                    Gw2Sharp.Models.ProfessionType.Ranger,
                    new List<ImageColorToggle>()
                },
                {
                    Gw2Sharp.Models.ProfessionType.Thief,
                    new List<ImageColorToggle>()
                },
                {
                    Gw2Sharp.Models.ProfessionType.Elementalist,
                    new List<ImageColorToggle>()
                },
                {
                    Gw2Sharp.Models.ProfessionType.Mesmer,
                    new List<ImageColorToggle>()
                },
                {
                    Gw2Sharp.Models.ProfessionType.Necromancer,
                    new List<ImageColorToggle>()
                },
                {
                    Gw2Sharp.Models.ProfessionType.Revenant,
                    new List<ImageColorToggle>()
                },
            };

            var profs = Characters.ModuleInstance.Data.Professions.ToDictionary(entry => entry.Key, entry => entry.Value);
            profs = profs.OrderBy(e => e.Value.WeightClass).ThenBy(e => e.Value.APIId).ToDictionary(e => e.Key, e => e.Value);

            // Profession All Specs
            foreach (KeyValuePair<Gw2Sharp.Models.ProfessionType, Data.Profession> profession in profs)
            {
                this._toggles.Add(new ImageColorToggle()
                {
                    Size = new Point(23, 23),
                    Texture = profession.Value.IconBig,
                    FilterObject = profession.Key,
                    FilterCategory = FilterCategory.Profession,
                    BasicTooltipText = "Any " + profession.Value.Name,
                    UseGrayScale = false,
                    ColorActive = profession.Value.Color,
                    ColorHovered = profession.Value.Color,
                    ColorInActive = profession.Value.Color * 0.5f,
                    Alpha = 0.7f,
                });
            }

            foreach (KeyValuePair<Gw2Sharp.Models.ProfessionType, Data.Profession> profession in profs)
            {
                this._toggles.Add(new ImageColorToggle()
                {
                    Size = new Point(23, 23),
                    Texture = profession.Value.IconBig,
                    FilterObject = profession.Key,
                    FilterCategory = FilterCategory.ProfessionSpecialization,
                    BasicTooltipText = "Core " + profession.Value.Name,
                });
            }

            var specToggles = new List<ImageColorToggle>();
            foreach (KeyValuePair<SpecializationType, Data.Specialization> specialization in Characters.ModuleInstance.Data.Specializations)
            {
                specToggles.Add(new ImageColorToggle()
                {
                    Size = new Point(23, 23),
                    Texture = specialization.Value.IconBig,
                    FilterObject = specialization.Key,
                    FilterCategory = FilterCategory.Specialization,
                    Profession = specialization.Value.Profession,
                    BasicTooltipText = specialization.Value.Name,
                });
            }


            for (int i = 0; i < 3; i++)
            {
                foreach (KeyValuePair<Gw2Sharp.Models.ProfessionType, Data.Profession> p in profs)
                {
                    var t = specToggles.Find(e => p.Key == e.Profession && !this._toggles.Contains(e));
                    if (t != null)
                    {
                        this._toggles.Add(t);
                    }
                }
            }

            // Crafting Professions
            foreach (KeyValuePair<int, Data.CrafingProfession> crafting in Characters.ModuleInstance.Data.CrafingProfessions)
            {
                if (crafting.Key > 0)
                {
                    var img = new ImageColorToggle()
                    {
                        Size = new Point(23, 23),
                        Texture = crafting.Value.Icon,
                        UseGrayScale = false,
                        FilterObject = (int)crafting.Key,
                        FilterCategory = FilterCategory.Crafting,
                        BasicTooltipText = crafting.Value.Name,
                    };
                    img.TextureRectangle = crafting.Key > 0 ? new Rectangle(8, 7, 17, 19) : new Rectangle(4, 4, 24, 24);
                    img.SizeRectangle = new Rectangle(4, 4, 20, 20);
                    this._toggles.Add(img);
                }
            }

            this._toggles.Add(new ImageColorToggle()
            {
                Size = new Point(23, 23),
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(605021),
                UseGrayScale = true,
                TextureRectangle = new Rectangle(4, 4, 24, 24),
                FilterObject = "Hidden",
                FilterCategory = FilterCategory.Hidden,
                BasicTooltipText = "Show Hidden Characters",
            });

            this._toggles.Add(new ImageColorToggle()
            {
                Size = new Point(23, 23),
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(593864),
                UseGrayScale = true,
                TextureRectangle = new Rectangle(1, 0, 30, 32),
                FilterCategory = FilterCategory.Birthday,
                FilterObject = "Birthday",
                BasicTooltipText = "Show Characters with Birthday Gifts",
            });

            foreach (KeyValuePair<Gw2Sharp.Models.RaceType, Data.Race> race in Characters.ModuleInstance.Data.Races)
            {
                this._toggles.Add(new ImageColorToggle()
                {
                    Size = new Point(23, 23),
                    Texture = race.Value.Icon,
                    UseGrayScale = true,
                    FilterObject = race.Key,
                    FilterCategory = FilterCategory.Race,
                    BasicTooltipText = race.Value.Name,
                });
            }

            foreach (ImageColorToggle t in this._toggles)
            {
                t.Parent = this;
            }

            this.RecalculateLayout();
        }
    }
}
