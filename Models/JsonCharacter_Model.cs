using Kenedia.Modules.Characters.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kenedia.Modules.Characters.Models
{
    internal class JsonCharacter_Model
    {
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

        public string Name { get; set; }

        public int Level { get; set; }

        public int Map { get; set; }

        public List<CharacterCrafting> Crafting { get; set; }

        public Gw2Sharp.Models.RaceType Race { get; set; }

        public Gw2Sharp.Models.ProfessionType Profession { get; set; }

        public SpecializationType Specialization { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTime LastModified { get; set; }

        public DateTime LastLogin { get; set; }

        public string IconPath { get; set; }

        public List<string> Tags { get; set; }

        public int Index { get; set; }

        public int Position { get; set; }

        public bool Show { get; set; }
    }
}
