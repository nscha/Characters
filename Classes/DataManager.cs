using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kenedia.Modules.Characters
{
    public static class DataManager
    {
        public static Blish_HUD.Modules.Managers.ContentsManager ContentsManager;
        private static readonly Blish_HUD.Logger Logger = Blish_HUD.Logger.GetLogger<Module>();
        public static void Load()
        {
            _Professions = new _jsonProfession().Load();
            _Specializations = new _jsonSpecialization().Load();
            _Races = new _jsonRace().Load();
            _Maps = new _jsonMap().Load();
        }
        public static _Profession[] _Professions { get; set; }
        public static _Specialization[] _Specializations { get; set; }
        public static _Race[] _Races { get; set; }
        public static _Map[] _Maps { get; set; }                
        public class _Names
        {
            public string de { get; set; }
            public string en { get; set; }
            public string es { get; set; }
            public string fr { get; set; }
        }
        public class _jsonProfession
        {
            public _Profession[] Load()
            {
                var path = @"data\professions.json";
                _Profession[] professions = new _Profession[1];

                string jsonString = new StreamReader(ContentsManager.GetFileStream(path)).ReadToEnd();

                if (jsonString != null && jsonString != "")
                {
                    List<_jsonProfession> localData = JsonConvert.DeserializeObject<List<_jsonProfession>>(jsonString);
                    _jsonProfession biggest = localData.Aggregate((i1, i2) => i1.Id > i2.Id ? i1 : i2);
                    professions = new _Profession[biggest.Id + 1];

                    foreach (_jsonProfession entry in localData)
                    {
                        professions[entry.Id] = new _Profession() { _Names = entry._Names, API_Id = entry.API_Id, Id = entry.Id};
                    }
                }

                return professions;
            }
            public int Id { get; set; }
            public string API_Id { get; set; }
            public _Names _Names = new _Names();
        }
        public class _jsonSpecialization
        {
            public _Specialization[] Load()
            {
                var path = @"data\specialization.json";
                _Specialization[] specializations = new _Specialization[1];
                string jsonString = new StreamReader(ContentsManager.GetFileStream(path)).ReadToEnd();

                if (jsonString != null && jsonString != "")
                {
                    List<_jsonSpecialization> localData = JsonConvert.DeserializeObject<List<_jsonSpecialization>>(jsonString);
                    _jsonSpecialization biggest = localData.Aggregate((i1, i2) => i1.Id > i2.Id ? i1 : i2);
                    specializations = new _Specialization[biggest.Id + 1];

                    foreach (_jsonSpecialization entry in localData)
                    {
                        specializations[entry.Id] = new _Specialization() { _Names = entry._Names, API_Id = entry.API_Id, Id = entry.Id };
                    }
                }

                return specializations;
            }
            public int Id { get; set; }
            public int API_Id { get; set; }
            public _Names _Names = new _Names();
        }
        public class _jsonRace
        {
            public _Race[] Load()
            {
                var path = @"data\races.json";
                _Race[] races = new _Race[1];

                string jsonString = new StreamReader(ContentsManager.GetFileStream(path)).ReadToEnd();

                if (jsonString != null && jsonString != "")
                {
                    List<_jsonRace> localData = JsonConvert.DeserializeObject<List<_jsonRace>>(jsonString);
                    _jsonRace biggest = localData.Aggregate((i1, i2) => i1.Id > i2.Id ? i1 : i2);
                    races = new _Race[biggest.Id + 1];

                    foreach (_jsonRace entry in localData)
                    {
                        races[entry.Id] = new _Race() { _Names = entry._Names, API_Id = entry.API_Id, Id = entry.Id };
                    }
                }

                return races;
            }
            public int Id { get; set; }
            public string API_Id { get; set; }
            public _Names _Names = new _Names();
        }
        public class _jsonMap
        {
            public _Map[] Load()
            {
                var path = @"data\maps.json";
                _Map[] maps = new _Map[1];

                string jsonString = new StreamReader(ContentsManager.GetFileStream(path)).ReadToEnd();

                if (jsonString != null && jsonString != "")
                {
                    List<_jsonMap> localData = JsonConvert.DeserializeObject<List<_jsonMap>>(jsonString);
                    _jsonMap biggest = localData.Aggregate((i1, i2) => i1.Id > i2.Id ? i1 : i2);
                    maps = new _Map[biggest.Id + 1];

                    foreach (_jsonMap entry in localData)
                    {
                        maps[entry.Id] = new _Map() { _Names = entry._Names, API_Id = entry.Id, Id = entry.Id };
                    }
                }

                return maps;
            }
            public _Names _Names = new _Names();
            public int Id;
            public int API_Id;
            public IReadOnlyList<int> Floors;
            public int DefaultFloor;
            public int ContinentId;
        }

        public class _Profession : _jsonProfession
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
        public class _Specialization : _jsonSpecialization
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
        public class _Race : _jsonRace
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
        public class _Map : _jsonMap
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

         public static string getMapName(int id)
        {
            return _Maps.Length > id && _Maps[id] != null ? _Maps[id].Name : Strings.common.Unkown + " " + Strings.common.Map;
        } 
         public static string getMapName(string id)
        {
            foreach (_Map map in _Maps)
            {
                if (map != null && (map._Names.de == id || map._Names.en == id || map._Names.es == id || map._Names.fr == id)) return map.Name;
            }

            return Strings.common.Unkown + Strings.common.Map;
        } 
         public static string getRaceName(int id)
        {
            return _Races.Length > id && _Races[id] != null ? _Races[id].Name : Strings.common.Unkown + " " + Strings.common.Race;
        } 
         public static string getRaceName(string id)
        {
            foreach (_Race race in _Races)
            {
                if (race != null && (race.API_Id == id)) return race.Name;
            }

            return Strings.common.Unkown + " " + Strings.common.Race;
        }         
        public static string getSpecName(int id)
        {
            return _Specializations.Length > id && _Specializations[id] != null ? _Specializations[id].Name : Strings.common.Unkown + " " + Strings.common.Specialization;
        } 
         public static string getSpecName(string id)
        {
            foreach (_Specialization spec in _Specializations)
            {
                if (spec != null && (spec._Names.de == id || spec._Names.en == id || spec._Names.es == id || spec._Names.fr == id)) return spec.Name;
            }

            return Strings.common.Unkown + " " + Strings.common.Specialization;
        } 
         public static string getProfessionName(int id)
        {
            return _Professions.Length > id && _Professions[id] != null ? _Professions[id].Name : Strings.common.Unkown + " " + Strings.common.Profession;
        } 
         public static string getProfessionName(string id)
        {
            foreach (_Profession spec in _Professions)
            {
                if (spec != null && (spec.API_Id == id)) return spec.Name;
            }

            return Strings.common.Unkown + " " +Strings.common.Profession;
        } 

        public static string getCraftingName(string id)
        {
            switch (id)
            {
                case "Armorsmith":
                    return Strings.common.Armorsmith;
                case "Artificer":
                    return Strings.common.Artificer;
                case "Chef":
                    return Strings.common.Chef;
                case "Huntsman":
                    return Strings.common.Huntsman;
                case "Jeweler":
                    return Strings.common.Jeweler;
                case "Leatherworker":
                    return Strings.common.Leatherworker;
                case "Scribe":
                    return Strings.common.Scribe;
                case "Tailor":
                    return Strings.common.Tailor;
                case "Weaponsmith":
                    return Strings.common.Weaponsmith;
                default:
                    return Strings.common.Unkown + " " + Strings.common.CraftingProfession;
            }
        }
    }
}
