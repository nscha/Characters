using System.Collections.Generic;

namespace Kenedia.Modules.Characters.Classes
{
    public class _Names
    {
        public string de { get; set; }
        public string en { get; set; }
        public string es { get; set; }
        public string fr { get; set; }
    }

    public class Map : _jsonMap
    {
        public string Name
        {
            get
            {

                switch (Blish_HUD.SettingsService.Overlay.UserLocale.Value)
                {
                    case Gw2Sharp.WebApi.Locale.German:
                        return _Names.de;

                    case Gw2Sharp.WebApi.Locale.French:
                        return _Names.fr;

                    case Gw2Sharp.WebApi.Locale.Spanish:
                        return _Names.es;

                    default:
                        return _Names.en;
                }
            }
            set
            {
                switch (Blish_HUD.SettingsService.Overlay.UserLocale.Value)
                {
                    case Gw2Sharp.WebApi.Locale.German:
                        _Names.de = value;
                        break;

                    case Gw2Sharp.WebApi.Locale.French:
                        _Names.fr = value;
                        break;

                    case Gw2Sharp.WebApi.Locale.Spanish:
                        _Names.es = value;
                        break;

                    default:
                        _Names.en = value;
                        break;
                }
            }
        }
    }

    public class _jsonMap
    {
        public _Names _Names = new _Names();
        public int Id;
        public int API_Id;
        public IReadOnlyList<int> Floors;
        public int DefaultFloor;
        public int ContinentId;
    }
}
