namespace Kenedia.Modules.Characters.Extensions
{
    using Kenedia.Modules.Characters.Enums;
    using static Kenedia.Modules.Characters.Services.Data;

    public static class Gw2SharpExtension
    {
        public static CrafingProfession GetData(this int key)
        {
            Characters.ModuleInstance.Data.CrafingProfessions.TryGetValue(key, out CrafingProfession value);

            return value;
        }

        public static Profession GetData(this Gw2Sharp.Models.ProfessionType key)
        {
            Characters.ModuleInstance.Data.Professions.TryGetValue(key, out Profession value);

            return value;
        }

        public static Specialization GetData(this SpecializationType key)
        {
            Characters.ModuleInstance.Data.Specializations.TryGetValue(key, out Specialization value);

            return value;
        }

        public static Race GetData(this Gw2Sharp.Models.RaceType key)
        {
            Characters.ModuleInstance.Data.Races.TryGetValue(key, out Race value);

            return value;
        }
    }
}
