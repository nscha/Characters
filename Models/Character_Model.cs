namespace Kenedia.Modules.Characters.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using Blish_HUD;
    using Blish_HUD.Content;
    using Kenedia.Modules.Characters.Enums;

    public class Character_Model
    {
        private string name;
        private int level;
        private int map = 0;
        private Gw2Sharp.Models.RaceType race;
        private Gw2Sharp.Models.ProfessionType profession;
        private SpecializationType specialization;
        private DateTimeOffset created;
        private DateTime lastModified;
        private DateTime lastLogin;
        private string iconPath;
        private AsyncTexture2D icon;
        private bool show = true;
        private int position;
        private int index;
        private bool initialized;
        private bool pathChecked;

        public Character_Model()
        {
        }

        public event EventHandler Updated;

        public event EventHandler Deleted;

        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        public int Level
        {
            get => this.level;
            set => this.SetProperty(ref this.level, value);
        }

        public int Map
        {
            get => this.map;
            set => this.SetProperty(ref this.map, value);
        }

        public List<CharacterCrafting> Crafting { get; set; } = new List<CharacterCrafting>();

        public Gw2Sharp.Models.RaceType Race
        {
            get => this.race;
            set => this.SetProperty(ref this.race, value);
        }

        public Gw2Sharp.Models.ProfessionType Profession
        {
            get => this.profession;
            set => this.SetProperty(ref this.profession, value);
        }

        public SpecializationType Specialization
        {
            get => this.specialization;
            set => this.SetProperty(ref this.specialization, value);
        }

        public DateTimeOffset Created
        {
            get => this.created;
            set => this.SetProperty(ref this.created, value);
        }

        public DateTime LastModified
        {
            get => this.lastModified;
            set => this.SetProperty(ref this.lastModified, value);
        }

        public int OrderIndex { get; set; } = 0;

        public int OrderOffset { get; set; } = 0;

        public DateTime LastLogin
        {
            get => this.lastLogin.AddMilliseconds(-this.OrderOffset);
            set => this.SetProperty(ref this.lastLogin, value);
        }

        public string IconPath
        {
            get => this.iconPath;
            set
            {
                this.iconPath = value;
                this.icon = null;
                this.pathChecked = false;
                this.OnUpdated();
            }
        }

        public string ProfessionName
        {
            get => Characters.ModuleInstance.Data.Professions[this.Profession].Name;
        }

        public string SpecializationName
        {
            get
            {
                if (this.Specialization != SpecializationType.None && Enum.IsDefined(typeof(SpecializationType), this.Specialization))
                {
                    return Characters.ModuleInstance.Data.Specializations[this.Specialization].Name;
                }

                return this.ProfessionName;
            }
        }

        public AsyncTexture2D ProfessionIcon
        {
            get => Characters.ModuleInstance.Data.Professions[this.Profession].IconBig;
        }

        public AsyncTexture2D SpecializationIcon
        {
            get
            {
                if (this.Specialization != SpecializationType.None && Enum.IsDefined(typeof(SpecializationType), this.Specialization))
                {
                    return Characters.ModuleInstance.Data.Specializations[this.Specialization].IconBig;
                }

                return this.ProfessionIcon;
            }
        }

        public AsyncTexture2D Icon
        {
            get
            {
                if (!this.pathChecked)
                {
                    var path = Characters.ModuleInstance.BasePath + (this.IconPath ?? string.Empty);

                    if (this.IconPath != null && File.Exists(path))
                    {
                        GameService.Graphics.QueueMainThreadRender((graphicsDevice) =>
                        {
                            this.icon = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(Characters.ModuleInstance.BasePath + this.IconPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                        });
                    }

                    this.pathChecked = true;
                }

                if (this.icon == null)
                {
                    return this.SpecializationIcon;
                }

                return this.icon;
            }

            set
            {
                this.icon = value;
                this.Updated?.Invoke(this, null);
            }
        }

        public bool HasDefaultIcon
        {
            get
            {
                return this.Icon == this.SpecializationIcon;
            }
        }

        public bool Show
        {
            get => this.show;
            set => this.SetProperty(ref this.show, value);
        }

        public TagList Tags { get; set; } = new TagList();

        public int Position
        {
            get => this.position;
            set
            {
                this.position = value;
                this.Save();
            }
        }

        public int Index
        {
            get => this.index;
            set
            {
                this.index = value;
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
                    }
                }

                return false;
            }
        }

        public void Delete()
        {
            this.Deleted?.Invoke(null, null);
            Characters.ModuleInstance.CharacterModels.Remove(this);
            this.Save();
        }

        public void Initialize()
        {
            this.Tags.CollectionChanged += this.Tags_CollectionChanged;
            this.initialized = true;
        }

        protected bool SetProperty<T>(ref T property, T newValue, [CallerMemberName] string caller = "")
        {
            if (Equals(property, newValue))
            {
                return false;
            }

            property = newValue;
            if (this.initialized && caller != nameof(this.LastLogin) && caller != nameof(this.LastModified) && caller != nameof(this.LastLogin))
            {
                this.OnUpdated();
            }

            return true;
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Characters.Logger.Debug(nameof(this.Tags_CollectionChanged));
            this.OnUpdated();
        }

        private void OnUpdated()
        {
            this.Updated?.Invoke(this, EventArgs.Empty);
            this.Save();
        }

        private void Save()
        {
            Characters.ModuleInstance.SaveCharacters = true;
        }
    }
}
