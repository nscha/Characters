using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using Newtonsoft.Json;
using System;
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

namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    public static class FilterTag_ListExtension
    {
        public static List<FilterTag> CreateFilterTagList(this List<string> strings)
        {
            var list = new List<FilterTag>();
            foreach(string s in strings)
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
