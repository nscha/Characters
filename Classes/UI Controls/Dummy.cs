namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    using System.Collections.Generic;
    using Blish_HUD.Controls;
    using Microsoft.Xna.Framework.Graphics;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public static class FilterTag_ListExtension
    {
        public static List<FilterTag> CreateFilterTagList(this List<string> strings)
        {
            var list = new List<FilterTag>();
            foreach (string s in strings)
            {
                list.Add(new FilterTag()
                {
                    Tag = s,
                });
            }

            return list;
        }
    }

    public class FilterTag
    {
        public string Tag;
        public bool Result = false;

        public static implicit operator string(FilterTag asyncTexture2D)
        {
            return asyncTexture2D.Tag;
        }
    }

    public class Dummy : Control
    {
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
        }
    }
}
