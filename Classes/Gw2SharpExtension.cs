using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Kenedia.Modules.Characters.Classes;
using Kenedia.Modules.Characters.Classes.UI_Controls;
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
using Locale = Gw2Sharp.WebApi.Locale;
using static Kenedia.Modules.Characters.Classes.Data;

namespace Kenedia.Modules.Characters.Classes
{
    public static class Gw2SharpExtension
    {
        public static CrafingProfession GetData(this int key)
        {
            CrafingProfession value;
            Characters.ModuleInstance.Data.CrafingProfessions.TryGetValue(key, out value);

            return value;
        }

        public static Profession GetData(this Gw2Sharp.Models.ProfessionType key)
        {
            Profession value;
            Characters.ModuleInstance.Data.Professions.TryGetValue(key, out value);

            return value;
        }

        public static Specialization GetData(this SpecializationType key)
        {
            Specialization value;
            Characters.ModuleInstance.Data.Specializations.TryGetValue(key, out value);

            return value;
        }

        public static Race GetData(this Gw2Sharp.Models.RaceType key)
        {
            Race value;
            Characters.ModuleInstance.Data.Races.TryGetValue(key, out value);

            return value;
        }
    }
}
