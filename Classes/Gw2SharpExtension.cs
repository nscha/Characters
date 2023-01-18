using static Kenedia.Modules.Characters.Classes.Data;

namespace Kenedia.Modules.Characters.Classes
{
    public static class Gw2SharpExtension
    {
        public static CrafingProfession GetData(this int key)
        {
            CrafingProfession value;
            Characters.ModuleInstance.Data.CrafingProfessions.TryGetValue(key, out value);

            return value;
        }

        public static Profession GetData(this Gw2Sharp.Models.ProfessionType key)
        {
            Profession value;
            Characters.ModuleInstance.Data.Professions.TryGetValue(key, out value);

            return value;
        }

        public static Specialization GetData(this SpecializationType key)
        {
            Specialization value;
            Characters.ModuleInstance.Data.Specializations.TryGetValue(key, out value);

            return value;
        }

        public static Race GetData(this Gw2Sharp.Models.RaceType key)
        {
            Race value;
            Characters.ModuleInstance.Data.Races.TryGetValue(key, out value);

            return value;
        }
    }
}
