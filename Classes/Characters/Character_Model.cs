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
    public enum SpecializationType
    {
        None = 0,

        Berserker = 18,
        Spellbreaker = 61,
        Bladesworn = 68,

        Dragonhunter = 27,
        Firebrand = 62,
        Willbender = 65,

        Herald = 52,
        Renegade = 63,
        Vindicator = 69,

        Druid = 5,
        Soulbeast = 55,
        Untamed = 72,

        Daredevil = 7,
        Deadeye = 58,
        Specter = 71,

        Scrapper = 43,
        Holosmith = 57,
        Mechanist = 70,

        Reaper = 34,
        Scourge = 60,
        Harbinger = 64,

        Tempest = 48,
        Weaver = 56,
        Catalyst = 67,

        Chronomancer = 40,
        Mirage = 59,
        Virtuoso = 66,
    }

    public class Character_Model
    {
        public void Initialize()
        {
            Tags.CollectionChanged += Tags_CollectionChanged;
            Initialized = true;
        }

        public Character_Model()
        {

        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Characters.Logger.Debug(nameof(Tags_CollectionChanged));
            OnUpdated();
        }

        public string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public int _level;
        public int Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }
        public int _map = 0;
        public int Map
        {
            get => _map;
            set => SetProperty(ref _map, value);
        }

        public List<CharacterCrafting> Crafting = new List<CharacterCrafting>();
        public Gw2Sharp.Models.RaceType _race;
        public Gw2Sharp.Models.RaceType Race
        {
            get => _race;
            set => SetProperty(ref _race, value);
        }
        public Gw2Sharp.Models.ProfessionType _profession;
        public Gw2Sharp.Models.ProfessionType Profession
        {
            get => _profession;
            set => SetProperty(ref _profession, value);
        }
        public SpecializationType _specialization;
        public SpecializationType Specialization
        {
            get => _specialization;
            set => SetProperty(ref _specialization, value);
        }

        public DateTimeOffset _created;
        public DateTimeOffset Created
        {
            get => _created;
            set => SetProperty(ref _created, value);
        }
        public DateTime _lastModified;
        public DateTime LastModified
        {
            get => _lastModified;
            set => SetProperty(ref _lastModified, value);
        }
        public int OrderIndex = 0;
        public int OrderOffset = 0;
        public DateTime _lastLogin;
        public DateTime LastLogin
        {
            get => _lastLogin.AddMilliseconds(-OrderOffset);
            set => SetProperty(ref _lastLogin, value);
        }

        public string _iconPath;
        public string IconPath
        {
            get => _iconPath;
            set
            {
                _iconPath = value;
                _Icon = null;
                OnUpdated();
            }
        }

        private AsyncTexture2D _Icon;
        public AsyncTexture2D Icon
        {
            set
            {
                _Icon = value;
                this.Updated?.Invoke(this, null);
            }
            get
            {
                if (_Icon == null && IconPath != null && IconPath.Length > 1)
                {
                    GameService.Graphics.QueueMainThreadRender((graphicsDevice) =>
                    {
                        _Icon = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(Characters.ModuleInstance.BasePath + IconPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    });
                }
                else if (IconPath == null || _Icon == null)
                {

                    if (Specialization == SpecializationType.None)
                    {
                        return Characters.ModuleInstance.Data.Professions[Profession].IconBig;
                    }
                    else
                    {
                        return Characters.ModuleInstance.Data.Specializations[Specialization].TempIcon;
                    }
                }

                return _Icon;
            }
        }

        private bool _show = true;
        public bool Show
        {
            get => _show;
            set => SetProperty(ref _show, value);

        }
        public TagList Tags = new TagList();

        public int _position;
        public int Position
        {
            get => _position;
            set
            {
                _position = value;
                Save();
            }
        }
        public int _index;
        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                Save();
            }
        }
        public int Age
        {
            get
            {
                // Save today's date.
                var today = DateTimeOffset.UtcNow;

                // Calculate the age.
                var age = today.Year - Created.Year;

                // Go back to the year in which the person was born in case of a leap year
                if (Created.Date > today.AddYears(-age)) age--;

                return age;
            }
        }
        public bool HasBirthdayPresent
        {
            get
            {
                DateTime nextBirthday = DateTime.MinValue;
                for (int i = 1; i < 100; i++)
                {
                    DateTime birthDay = Created.AddYears(i).DateTime;

                    if (birthDay <= DateTime.UtcNow)
                    {
                        if (birthDay > LastLogin) return true;
                    }
                    else
                    {
                        return false;
                    };
                }

                return false;
            }
        }

        public event EventHandler Updated;

        private void OnUpdated()
        {
            this.Updated?.Invoke(this, EventArgs.Empty);
            Save();
        }

        protected bool SetProperty<T>(ref T property, T newValue, [CallerMemberName] string caller = "" )
        {
            if (Equals(property, newValue)) return false;


            property = newValue;
            if(Initialized && (caller != nameof(LastLogin) && caller != nameof(LastModified) && caller != nameof(LastLogin)))
            {
                OnUpdated();
            }
            return true;
        }

        public bool Initialized;

        void Save()
        {
            Characters.ModuleInstance.SaveCharacters = true;
        }

        public event EventHandler Deleted;

        public void Delete()
        {
            this.Deleted?.Invoke(null, null);
            Characters.ModuleInstance.Character_Models.Remove(this);
            Save();
        }
    }
}
