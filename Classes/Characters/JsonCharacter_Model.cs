namespace Kenedia.Modules.Characters.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class JsonCharacter_Model
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
    }
}
