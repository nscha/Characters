using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Classes
{
    class JsonCharacter_Model
    {
        public string Name;
        public int Level;
        public int Map;
        public List<CharacterCrafting> Crafting;
        public Gw2Sharp.Models.RaceType Race;
        public Gw2Sharp.Models.ProfessionType Profession;
        public SpecializationType Specialization;
        public DateTimeOffset Created;
        public DateTime LastModified;
        public DateTime LastLogin;
        public string IconPath;
        public List<string> Tags;
        public int Index;
        public int Position;
        public bool Show;

        public JsonCharacter_Model(Character_Model c)
        {
            Name = c.Name;
            Level = c.Level;
            Map = c.Map;
            Crafting = c.Crafting;
            Race = c.Race;
            Profession = c.Profession;
            Specialization = c.Specialization;
            Created = c.Created;
            LastModified = c.LastModified;
            LastLogin = c.LastLogin;
            IconPath = c.IconPath;
            Tags = c.Tags.ToList();
            Index = c.Index;
            Position = c.Position;
            Show = c.Show;
        }
    }
}
