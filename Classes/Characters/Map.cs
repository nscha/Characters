namespace Kenedia.Modules.Characters.Classes
{
    public class Map : JsonMap
    {
        public string Name
        {
            get
            {
                return Blish_HUD.GameService.Overlay.UserLocale.Value switch
                {
                    Gw2Sharp.WebApi.Locale.German => this.Names.De,
                    Gw2Sharp.WebApi.Locale.French => this.Names.Fr,
                    Gw2Sharp.WebApi.Locale.Spanish => this.Names.Es,
                    _ => this.Names.En,
                };
            }

            set
            {
                switch (Blish_HUD.GameService.Overlay.UserLocale.Value)
                {
                    case Gw2Sharp.WebApi.Locale.German:
                        this.Names.De = value;
                        break;

                    case Gw2Sharp.WebApi.Locale.French:
                        this.Names.Fr = value;
                        break;

                    case Gw2Sharp.WebApi.Locale.Spanish:
                        this.Names.Es = value;
                        break;

                    default:
                        this.Names.En = value;
                        break;
                }
            }
        }
    }
}
