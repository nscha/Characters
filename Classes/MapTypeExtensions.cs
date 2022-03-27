using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapType = Gw2Sharp.Models.MapType;

namespace Kenedia.Modules.Characters
{
    internal static class MapTypeExtensions
    {
        public static bool IsWorldVsWorld(this MapType type)
        {
            switch (type)
            {
                case MapType.Center:
                case MapType.BlueHome:
                case MapType.GreenHome:
                case MapType.RedHome:
                case MapType.EdgeOfTheMists: return true;
                default: return false;
            }
        }
        public static bool IsCompetitive(this MapType type)
        {
            switch (type)
            {
                case MapType.Pvp:
                case MapType.UserTournament:
                case MapType.Tournament:
                case MapType.RedHome: return true;
                default: return false;
            }
        }
    }
}
