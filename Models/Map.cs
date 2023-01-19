namespace Kenedia.Modules.Characters.Models
{
    public class Map : JsonMap
    {
        public string Name
        {
            get => Blish_HUD.GameService.Overlay.UserLocale.Value switch
            {
                Gw2Sharp.WebApi.Locale.German => Names.De,
                Gw2Sharp.WebApi.Locale.French => Names.Fr,
                Gw2Sharp.WebApi.Locale.Spanish => Names.Es,
                _ => Names.En,
            };

            set
            {
                switch (Blish_HUD.GameService.Overlay.UserLocale.Value)
                {
                    case Gw2Sharp.WebApi.Locale.German:
                        Names.De = value;
                        break;

                    case Gw2Sharp.WebApi.Locale.French:
                        Names.Fr = value;
                        break;

                    case Gw2Sharp.WebApi.Locale.Spanish:
                        Names.Es = value;
                        break;

                    default:
                        Names.En = value;
                        break;
                }
            }
        }
    }
}
