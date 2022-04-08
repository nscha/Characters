using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Kenedia.Modules.Characters
{
    public static class Textures
    {
        public static bool Loaded;
        public static List<string> CustomImageNames = new List<string>();
        public static Texture2D[] CustomImages;
        public static Texture2D[] CustomGlobalImages;

        public static Texture2D[] Races;
        public static Texture2D[] RacesDisabled;

        public static Texture2D[] Professions;
        public static Texture2D[] ProfessionsWhite;
        public static Texture2D[] ProfessionsDisabled;

        public static Texture2D[] Specializations;
        public static Texture2D[] SpecializationsWhite;
        public static Texture2D[] SpecializationsDisabled;

        public static Texture2D[] Icons;

        public static Texture2D[] Crafting;
        public static Texture2D[] CraftingDisabled;

        public static Texture2D[] Backgrounds;
        public static Texture2D[] Emblems;
    }
}
