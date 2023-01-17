using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Classes;
using Kenedia.Modules.Characters.Classes.UI_Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
    class ImageColorToggle : ImageGrayScaled
    {
        public object FilterObject;
        public FilterCategory FilterCategory;
        public Gw2Sharp.Models.ProfessionType Profession;

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            Active = !Active;

            if (FilterObject != null)
            {
                if (Active) Characters.ModuleInstance.MainWindow.CategoryFilters[FilterCategory].Add(FilterObject);
                else Characters.ModuleInstance.MainWindow.CategoryFilters[FilterCategory].Remove(FilterObject);

                Characters.ModuleInstance.MainWindow.FilterCharacters(null, null);
            }
        }
    }
}
