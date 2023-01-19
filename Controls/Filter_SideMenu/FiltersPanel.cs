using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Enums;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    internal class FiltersPanel : FlowTab
    {
        private readonly List<ImageColorToggle> toggles = new();

        public FiltersPanel()
        {
            Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(440021);
            Name = Strings.common.FilterToggles;
            FlowDirection = ControlFlowDirection.TopToBottom;
            WidthSizingMode = SizingMode.Fill;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.Standard;
            Height = 250;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 3);
            Location = new Point(0, 25);

            Dictionary<Gw2Sharp.Models.ProfessionType, List<ImageColorToggle>> toggleDir = new()
            {
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

            Dictionary<Gw2Sharp.Models.ProfessionType, Data.Profession> profs = Characters.ModuleInstance.Data.Professions.ToDictionary(entry => entry.Key, entry => entry.Value);
            profs = profs.OrderBy(e => e.Value.WeightClass).ThenBy(e => e.Value.APIId).ToDictionary(e => e.Key, e => e.Value);

            // Profession All Specs
            foreach (KeyValuePair<Gw2Sharp.Models.ProfessionType, Data.Profession> profession in profs)
            {
                toggles.Add(new ImageColorToggle()
                {
                    Size = new Point(23, 23),
                    Texture = profession.Value.IconBig,
                    FilterObject = profession.Key,
                    FilterCategory = FilterCategory.Profession,
                    BasicTooltipText = string.Format(Strings.common.AnyProfession, profession.Value.Name),
                    UseGrayScale = false,
                    ColorActive = profession.Value.Color,
                    ColorHovered = profession.Value.Color,
                    ColorInActive = profession.Value.Color * 0.5f,
                    Alpha = 0.7f,
                });
            }

            foreach (KeyValuePair<Gw2Sharp.Models.ProfessionType, Data.Profession> profession in profs)
            {
                toggles.Add(new ImageColorToggle()
                {
                    Size = new Point(23, 23),
                    Texture = profession.Value.IconBig,
                    FilterObject = profession.Key,
                    FilterCategory = FilterCategory.ProfessionSpecialization,
                    BasicTooltipText = string.Format(Strings.common.CoreProfession, profession.Value.Name),
                });
            }

            List<ImageColorToggle> specToggles = new();
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
                    ImageColorToggle t = specToggles.Find(e => p.Key == e.Profession && !toggles.Contains(e));
                    if (t != null)
                    {
                        toggles.Add(t);
                    }
                }
            }

            // Crafting Professions
            foreach (KeyValuePair<int, Data.CrafingProfession> crafting in Characters.ModuleInstance.Data.CrafingProfessions)
            {
                if (crafting.Key > 0)
                {
                    ImageColorToggle img = new()
                    {
                        Size = new Point(23, 23),
                        Texture = crafting.Value.Icon,
                        UseGrayScale = false,
                        FilterObject = (int)crafting.Key,
                        FilterCategory = FilterCategory.Crafting,
                        BasicTooltipText = crafting.Value.Name,
                        TextureRectangle = crafting.Key > 0 ? new Rectangle(8, 7, 17, 19) : new Rectangle(4, 4, 24, 24),
                        SizeRectangle = new Rectangle(4, 4, 20, 20)
                    };
                    toggles.Add(img);
                }
            }

            toggles.Add(new ImageColorToggle()
            {
                Size = new Point(23, 23),
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(605021),
                UseGrayScale = true,
                TextureRectangle = new Rectangle(4, 4, 24, 24),
                FilterObject = Strings.common.Hidden,
                FilterCategory = FilterCategory.Hidden,
                BasicTooltipText = Strings.common.ShowHidden_Tooltip,
            });

            toggles.Add(new ImageColorToggle()
            {
                Size = new Point(23, 23),
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(593864),
                UseGrayScale = true,
                TextureRectangle = new Rectangle(1, 0, 30, 32),
                FilterCategory = FilterCategory.Birthday,
                FilterObject = Strings.common.Birthday,
                BasicTooltipText = Strings.common.Show_Birthday_Tooltip,
            });

            foreach (KeyValuePair<Gw2Sharp.Models.RaceType, Data.Race> race in Characters.ModuleInstance.Data.Races)
            {
                toggles.Add(new ImageColorToggle()
                {
                    Size = new Point(23, 23),
                    Texture = race.Value.Icon,
                    UseGrayScale = true,
                    FilterObject = race.Key,
                    FilterCategory = FilterCategory.Race,
                    BasicTooltipText = race.Value.Name,
                });
            }

            foreach (ImageColorToggle t in toggles)
            {
                t.Parent = this;
            }

            RecalculateLayout();
        }

        public void ResetToggles()
        {
            foreach (ImageColorToggle t in toggles)
            {
                t.Active = false;
            }
        }
    }
}
