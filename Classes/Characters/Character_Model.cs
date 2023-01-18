namespace Kenedia.Modules.Characters.Classes
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using Blish_HUD;
    using Blish_HUD.Content;

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
            this.Tags.CollectionChanged += this.Tags_CollectionChanged;
            this.Initialized = true;
        }

        public Character_Model()
        {
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Characters.Logger.Debug(nameof(this.Tags_CollectionChanged));
            this.OnUpdated();
        }

        public string _name;

        public string Name
        {
            get => this._name;
            set => this.SetProperty(ref this._name, value);
        }

        public int _level;

        public int Level
        {
            get => this._level;
            set => this.SetProperty(ref this._level, value);
        }

        public int _map = 0;

        public int Map
        {
            get => this._map;
            set => this.SetProperty(ref this._map, value);
        }

        public List<CharacterCrafting> Crafting = new List<CharacterCrafting>();
        public Gw2Sharp.Models.RaceType _race;

        public Gw2Sharp.Models.RaceType Race
        {
            get => this._race;
            set => this.SetProperty(ref this._race, value);
        }

        public Gw2Sharp.Models.ProfessionType _profession;

        public Gw2Sharp.Models.ProfessionType Profession
        {
            get => this._profession;
            set => this.SetProperty(ref this._profession, value);
        }

        public SpecializationType _specialization;

        public SpecializationType Specialization
        {
            get => this._specialization;
            set => this.SetProperty(ref this._specialization, value);
        }

        public DateTimeOffset _created;

        public DateTimeOffset Created
        {
            get => this._created;
            set => this.SetProperty(ref this._created, value);
        }

        public DateTime _lastModified;

        public DateTime LastModified
        {
            get => this._lastModified;
            set => this.SetProperty(ref this._lastModified, value);
        }

        public int OrderIndex = 0;
        public int OrderOffset = 0;
        public DateTime _lastLogin;

        public DateTime LastLogin
        {
            get => this._lastLogin.AddMilliseconds(-this.OrderOffset);
            set => this.SetProperty(ref this._lastLogin, value);
        }

        public string _iconPath;

        public string IconPath
        {
            get => this._iconPath;
            set
            {
                this._iconPath = value;
                this._Icon = null;
                this.OnUpdated();
            }
        }

        private AsyncTexture2D _Icon;

        public AsyncTexture2D Icon
        {
            set
            {
                this._Icon = value;
                this.Updated?.Invoke(this, null);
            }

            get
            {
                if (this._Icon == null && this.IconPath != null && this.IconPath.Length > 1)
                {
                    GameService.Graphics.QueueMainThreadRender((graphicsDevice) =>
                    {
                        this._Icon = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(Characters.ModuleInstance.BasePath + this.IconPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    });
                }
                else if (this.IconPath == null || this._Icon == null)
                {

                    if (Enum.IsDefined(typeof(SpecializationType), this.Specialization) && this.Specialization != SpecializationType.None)
                    {
                        return Characters.ModuleInstance.Data.Specializations[this.Specialization].IconBig;
                    }
                    else
                    {
                        return Characters.ModuleInstance.Data.Professions[this.Profession].IconBig;
                    }
                }

                return this._Icon;
            }
        }

        private bool _show = true;

        public bool Show
        {
            get => this._show;
            set => this.SetProperty(ref this._show, value);
        }

        public TagList Tags = new TagList();

        public int _position;

        public int Position
        {
            get => this._position;
            set
            {
                this._position = value;
                this.Save();
            }
        }

        public int _index;

        public int Index
        {
            get => this._index;
            set
            {
                this._index = value;
                this.Save();
            }
        }

        public int Age
        {
            get
            {
                // Save today's date.
                var today = DateTimeOffset.UtcNow;

                // Calculate the age.
                var age = today.Year - this.Created.Year;

                // Go back to the year in which the person was born in case of a leap year
                if (this.Created.Date > today.AddYears(-age))
                {
                    age--;
                }

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
                    DateTime birthDay = this.Created.AddYears(i).DateTime;

                    if (birthDay <= DateTime.UtcNow)
                    {
                        if (birthDay > this.LastLogin)
                        {
                            return true;
                        }
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
            this.Save();
        }

        protected bool SetProperty<T>(ref T property, T newValue, [CallerMemberName] string caller = "")
        {
            if (Equals(property, newValue))
            {
                return false;
            }

            property = newValue;
            if (this.Initialized && caller != nameof(this.LastLogin) && caller != nameof(this.LastModified) && caller != nameof(this.LastLogin))
            {
                this.OnUpdated();
            }
            return true;
        }

        public bool Initialized;

        private void Save()
        {
            Characters.ModuleInstance.SaveCharacters = true;
        }

        public event EventHandler Deleted;

        public void Delete()
        {
            this.Deleted?.Invoke(null, null);
            Characters.ModuleInstance.CharacterModels.Remove(this);
            this.Save();
        }
    }
}
