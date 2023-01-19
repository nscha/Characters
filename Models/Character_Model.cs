using Blish_HUD;
using Blish_HUD.Content;
using Kenedia.Modules.Characters.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Kenedia.Modules.Characters.Models
{
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
            get => name;
            set => SetProperty(ref name, value);
        }

        public int Level
        {
            get => level;
            set => SetProperty(ref level, value);
        }

        public int Map
        {
            get => map;
            set => SetProperty(ref map, value);
        }

        public List<CharacterCrafting> Crafting { get; set; } = new List<CharacterCrafting>();

        public Gw2Sharp.Models.RaceType Race
        {
            get => race;
            set => SetProperty(ref race, value);
        }

        public Gw2Sharp.Models.ProfessionType Profession
        {
            get => profession;
            set => SetProperty(ref profession, value);
        }

        public SpecializationType Specialization
        {
            get => specialization;
            set => SetProperty(ref specialization, value);
        }

        public DateTimeOffset Created
        {
            get => created;
            set => SetProperty(ref created, value);
        }

        public DateTime LastModified
        {
            get => lastModified;
            set => SetProperty(ref lastModified, value);
        }

        public int OrderIndex { get; set; } = 0;

        public int OrderOffset { get; set; } = 0;

        public DateTime LastLogin
        {
            get => lastLogin.AddMilliseconds(-OrderOffset);
            set => SetProperty(ref lastLogin, value);
        }

        public string IconPath
        {
            get => iconPath;
            set
            {
                iconPath = value;
                icon = null;
                pathChecked = false;
                OnUpdated();
            }
        }

        public string ProfessionName
        {
            get => Characters.ModuleInstance.Data.Professions[Profession].Name;
        }

        public string SpecializationName
        {
            get
            {
                if (Specialization != SpecializationType.None && Enum.IsDefined(typeof(SpecializationType), Specialization))
                {
                    return Characters.ModuleInstance.Data.Specializations[Specialization].Name;
                }

                return ProfessionName;
            }
        }

        public AsyncTexture2D ProfessionIcon
        {
            get => Characters.ModuleInstance.Data.Professions[Profession].IconBig;
        }

        public AsyncTexture2D SpecializationIcon
        {
            get
            {
                if (Specialization != SpecializationType.None && Enum.IsDefined(typeof(SpecializationType), Specialization))
                {
                    return Characters.ModuleInstance.Data.Specializations[Specialization].IconBig;
                }

                return ProfessionIcon;
            }
        }

        public AsyncTexture2D Icon
        {
            get
            {
                if (!pathChecked)
                {
                    string path = Characters.ModuleInstance.BasePath + (IconPath ?? string.Empty);

                    if (IconPath != null && File.Exists(path))
                    {
                        GameService.Graphics.QueueMainThreadRender((graphicsDevice) =>
                        {
                            icon = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(Characters.ModuleInstance.BasePath + IconPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                        });
                    }

                    pathChecked = true;
                }

                if (icon == null)
                {
                    return SpecializationIcon;
                }

                return icon;
            }

            set
            {
                icon = value;
                Updated?.Invoke(this, null);
            }
        }

        public bool HasDefaultIcon
        {
            get
            {
                return Icon == SpecializationIcon;
            }
        }

        public bool Show
        {
            get => show;
            set => SetProperty(ref show, value);
        }

        public TagList Tags { get; set; } = new TagList();

        public int Position
        {
            get => position;
            set
            {
                position = value;
                Save();
            }
        }

        public int Index
        {
            get => index;
            set
            {
                index = value;
                Save();
            }
        }

        public int Age
        {
            get
            {
                // Save today's date.
                DateTimeOffset today = DateTimeOffset.UtcNow;

                // Calculate the age.
                int age = today.Year - Created.Year;

                // Go back to the year in which the person was born in case of a leap year
                if (Created.Date > today.AddYears(-age))
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
                    DateTime birthDay = Created.AddYears(i).DateTime;

                    if (birthDay <= DateTime.UtcNow)
                    {
                        if (birthDay > LastLogin)
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
            Deleted?.Invoke(null, null);
            _ = Characters.ModuleInstance.CharacterModels.Remove(this);
            Save();
        }

        public void Initialize()
        {
            Tags.CollectionChanged += Tags_CollectionChanged;
            initialized = true;
        }

        protected bool SetProperty<T>(ref T property, T newValue, [CallerMemberName] string caller = "")
        {
            if (Equals(property, newValue))
            {
                return false;
            }

            property = newValue;
            if (initialized && caller != nameof(LastLogin) && caller != nameof(LastModified) && caller != nameof(LastLogin))
            {
                OnUpdated();
            }

            return true;
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Characters.Logger.Debug(nameof(this.Tags_CollectionChanged));
            OnUpdated();
        }

        private void OnUpdated()
        {
            Updated?.Invoke(this, EventArgs.Empty);
            Save();
        }

        private void Save() => Characters.ModuleInstance.SaveCharacters = true;
    }
}
