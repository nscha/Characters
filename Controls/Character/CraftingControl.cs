namespace Kenedia.Modules.Characters.Controls
{
    using Blish_HUD;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Kenedia.Modules.Characters.Models;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended.BitmapFonts;
    using static Kenedia.Modules.Characters.Services.Data;
    using Color = Microsoft.Xna.Framework.Color;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class CraftingControl : Control
    {
        private readonly AsyncTexture2D craftingIcon = GameService.Content.DatAssetCache.GetTextureFromAssetId(156711);

        private BitmapFont font = GameService.Content.DefaultFont14;

        public CraftingControl()
        {
        }

        public Character_Model Character { get; set; }

        public BitmapFont Font
        {
            get => this.font;
            set
            {
                this.font = value;
                if (value != null)
                {
                    this.Width = this.Height + 4 + (int)value.MeasureString(Strings.common.NoCraftingProfession).Width;
                }
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            string toolTipText = null;
            spriteBatch.DrawOnCtrl(
                this,
                this.craftingIcon,
                new Rectangle(4, 2, bounds.Height - 4, bounds.Height - 4),
                new Rectangle(6, 6, 20, 20),
                Color.White,
                0f,
                default);

            var craftingDisplayed = false;
            if (this.Character != null && this.Character.Crafting.Count > 0)
            {
                var craftingDictionary = Characters.ModuleInstance.Data.CrafingProfessions;

                int i = 0;
                foreach (CharacterCrafting crafting in this.Character.Crafting)
                {
                    craftingDictionary.TryGetValue(crafting.Id, out CrafingProfession craftingProfession);
                    if (craftingProfession != null)
                    {
                        var onlyMax = Characters.ModuleInstance.Settings.ShowOnlyMaxCrafting.Value;

                        if (craftingProfession.Icon != null && (!onlyMax || crafting.Rating == craftingProfession.MaxRating))
                        {
                            craftingDisplayed = true;
                            var craftBounds = new Rectangle(bounds.Height + 6 + (i * bounds.Height), 2, bounds.Height - 4, bounds.Height - 4);

                            spriteBatch.DrawOnCtrl(
                                this,
                                craftingProfession.Icon,
                                craftBounds,
                                new Rectangle(8, 8, 16, 16),
                                Color.White,
                                0f,
                                default);

                            i++;

                            if (craftBounds.Contains(this.RelativeMousePosition))
                            {
                                toolTipText = craftingProfession.Name + " (" + crafting.Rating + "/" + craftingProfession.MaxRating + ")";
                            }
                        }
                    }
                }
            }

            if (!craftingDisplayed)
            {
                spriteBatch.DrawStringOnCtrl(
                    this,
                    Strings.common.NoCraftingProfession,
                    this.Font,
                    new Rectangle(bounds.Height + 4, 0, bounds.Width - (bounds.Height + 4), bounds.Height),
                    Color.Gray,
                    false,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Middle);
            }

            this.BasicTooltipText = toolTipText;
        }
    }
}
