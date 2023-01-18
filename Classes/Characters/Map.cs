namespace Kenedia.Modules.Characters.Classes
{
    using System.Collections.Generic;

    public class Names
    {
        public string De { get; set; }

        public string En { get; set; }

        public string Es { get; set; }

        public string Fr { get; set; }
    }

    public class Map : JsonMap
    {
        public string Name
        {
            get
            {

                switch (Blish_HUD.SettingsService.Overlay.UserLocale.Value)
                {
                    case Gw2Sharp.WebApi.Locale.German:
                        return this._Names.De;

                    case Gw2Sharp.WebApi.Locale.French:
                        return this._Names.Fr;

                    case Gw2Sharp.WebApi.Locale.Spanish:
                        return this._Names.Es;

                    default:
                        return this._Names.En;
                }
            }

            set
            {
                switch (Blish_HUD.SettingsService.Overlay.UserLocale.Value)
                {
                    case Gw2Sharp.WebApi.Locale.German:
                        this._Names.De = value;
                        break;

                    case Gw2Sharp.WebApi.Locale.French:
                        this._Names.Fr = value;
                        break;

                    case Gw2Sharp.WebApi.Locale.Spanish:
                        this._Names.Es = value;
                        break;

                    default:
                        this._Names.En = value;
                        break;
                }
            }
        }
    }

    public class JsonMap
    {
        public Names _Names = new Names();
        public int Id;
        public int APIId;
        public IReadOnlyList<int> Floors;
        public int DefaultFloor;
        public int ContinentId;
    }
}
