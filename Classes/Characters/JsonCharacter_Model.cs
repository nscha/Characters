namespace Kenedia.Modules.Characters.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class JsonCharacter_Model
    {
        public JsonCharacter_Model(Character_Model c)
        {
            this.Name = c.Name;
            this.Level = c.Level;
            this.Map = c.Map;
            this.Crafting = c.Crafting;
            this.Race = c.Race;
            this.Profession = c.Profession;
            this.Specialization = c.Specialization;
            this.Created = c.Created;
            this.LastModified = c.LastModified;
            this.LastLogin = c.LastLogin;
            this.IconPath = c.IconPath;
            this.Tags = c.Tags.ToList();
            this.Index = c.Index;
            this.Position = c.Position;
            this.Show = c.Show;
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
