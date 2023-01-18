using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using static Kenedia.Modules.Characters.Classes.Data;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    public class CraftingControl : Control
    {
        private AsyncTexture2D CraftingIcon = GameService.Content.DatAssetCache.GetTextureFromAssetId(156711);
        private BitmapFont _Font = GameService.Content.DefaultFont14;
        public BitmapFont Font
        {
            get => _Font;
            set
            {
                _Font = value;
                if(value != null)
                {
                    Width = Height + 4 + (int) value.MeasureString("No Crafting Profession").Width;
                }
            }
        }
        public CraftingControl()
        {

        }
        public Character_Model Character;

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            string ToolTipText = null;
            spriteBatch.DrawOnCtrl(this,
                                    CraftingIcon,
                                    new Rectangle(4, 2, bounds.Height - 4, bounds.Height - 4),
                                    new Rectangle(6, 6, 20, 20),
                                    Color.White,
                                    0f,
                                    default);

            if (Character != null && Character.Crafting.Count > 0)
            {
                var CraftingDictionary = Characters.ModuleInstance.Data.CrafingProfessions;

                int i = 0;
                foreach (CharacterCrafting crafting in Character.Crafting)
                {
                    CrafingProfession craftingProfession;
                    CraftingDictionary.TryGetValue(crafting.Id, out craftingProfession);
                    var onlyMax = Characters.ModuleInstance.Settings.Show_OnlyMaxCrafting.Value;

                    if (craftingProfession.Icon != null && (!onlyMax || crafting.Rating == craftingProfession.MaxRating))
                    {
                        var craftBounds = new Rectangle(bounds.Height + 6 + i * (bounds.Height), 2, bounds.Height - 4, bounds.Height - 4);

                        spriteBatch.DrawOnCtrl(this,
                                                craftingProfession.Icon,
                                                craftBounds,
                                                new Rectangle(8, 8, 16, 16),
                                                Color.White,
                                                0f,
                                                default);

                        i++;

                        if (craftBounds.Contains(RelativeMousePosition)) ToolTipText = craftingProfession.Name + " (" + crafting.Rating + "/" + craftingProfession.MaxRating + ")";
                    }
                }            
            }
            else
            {
                spriteBatch.DrawStringOnCtrl(this,
                                        "No Crafting Profession",
                                        Font,
                                        new Rectangle(bounds.Height + 4, 0, bounds.Width - (bounds.Height + 4), bounds.Height),
                                        Color.Gray, //new Color(247, 231, 182, 97),
                                        false,
                                        HorizontalAlignment.Left,
                                        VerticalAlignment.Middle
                                        );

            }

            BasicTooltipText = ToolTipText;
        }
    }
}
