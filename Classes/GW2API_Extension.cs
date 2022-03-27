using Blish_HUD.Modules.Managers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Globalization;
using MapModel = Gw2Sharp.WebApi.V2.Models.Map;
using Newtonsoft.Json;

namespace Kenedia.Modules.Characters
{
    public class _Professions {
        public int Id { get; set; } 
        public string API_Id { get; set; }
        public _Names _Names = new _Names();
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
    public class _Specializations
    {
        public int Id { get; set; }
        public int API_Id { get; set; }
        public _Names _Names = new _Names();
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
    public class _Race
    {
        public int Id { get; set; }
        public string API_Id { get; set; }
        public _Names _Names = new _Names();
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
    public class _Names
    {
        public string de { get; set; }
        public string en { get; set; }
        public string es { get; set; }
        public string fr { get; set; }
    }
    public class localMapModel
    {
        public string Name { get {
                
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
        public _Names _Names = new _Names();
        public int Id;
        public IReadOnlyList<int>Floors;
        public int DefaultFloor;
        public int ContinentId;
    }

    public partial class Module
    {
        public localMapModel[] __Maps;
        public List<string> __ProfessionNames;
        public List<string> __SpecializationNames;

        public localMapModel[] _Maps
        {            
            get
            {
                return __Maps;
            }
            set { _Maps = value; }
        }

        public List<string> _ProfessionNames;
        public List<string> _SpecializationNames;

        async public Task _Fetch_ApiData()
        {
           await _Fetch_Maps();
            await _Fetch_Professions();
            await _Fetch_Specializations();
            await _Fetch_Races();
        }
        async public Task _Fetch_Maps()
        {
            var maps = await Gw2ApiManager.Gw2ApiClient.V2.Maps.AllAsync();
            var max = maps[maps.Count - 1].Id;
            var mapArray = new localMapModel[max + 1];

            string path = @"C:\maps.json";

            if (System.IO.File.Exists(path))
            {
                List<localMapModel> localMaps = JsonConvert.DeserializeObject<List<localMapModel>>(System.IO.File.ReadAllText(path));

                foreach (localMapModel map in localMaps)
                {
                    mapArray[map.Id] = map;
                }
            }

            foreach (MapModel map in maps)
            {
                if (mapArray[map.Id] == null)
                {
                    mapArray[map.Id] = new localMapModel() { Id = map.Id, Name = map.Name, Floors = map.Floors, DefaultFloor = map.DefaultFloor, ContinentId = map.ContinentId };
                }
                else
                {
                    mapArray[map.Id].Name = map.Name;
                }
            }

            List<localMapModel> _data = new List<localMapModel>();
            foreach (localMapModel map in mapArray)
            {
                if (map != null)
                {
                    _data.Add(map);
                }
            }

            string json = JsonConvert.SerializeObject(_data.ToArray());

            //write string to file
            System.IO.File.WriteAllText(path, json);
        }
        async public Task _Fetch_Professions()
        {
            var professions = await Gw2ApiManager.Gw2ApiClient.V2.Professions.AllAsync();
            var resultArray = new _Professions[Enum.GetValues(typeof(Professions)).Cast<int>().Max() + 1];
            string path = @"C:\professions.json";

            if (System.IO.File.Exists(path))
            {
                List<_Professions> localData = JsonConvert.DeserializeObject<List<_Professions>>(System.IO.File.ReadAllText(path));
                foreach (_Professions profession in localData)
                {
                    resultArray[profession.Id] = profession;
                }
            }

            foreach (Gw2Sharp.WebApi.V2.Models.Profession profession in professions)
            {
                int id = (int)Enum.Parse(typeof(Professions), profession.Id);
                if (resultArray[id] == null)
                {
                    resultArray[id] = new _Professions() { Id = id, Name = profession.Name, API_Id = profession.Id };
                }
                else
                {
                    resultArray[id].Name = profession.Name;
                }
            }

            List<_Professions> _data = new List<_Professions>();
            foreach (_Professions profession in resultArray)
            {
                if (profession != null)
                {
                    _data.Add(profession);
                }
            }

            string json = JsonConvert.SerializeObject(_data.ToArray());

            //write string to file
            System.IO.File.WriteAllText(path, json);
        }
        async public Task _Fetch_Specializations()
        {
            var specs = await Gw2ApiManager.Gw2ApiClient.V2.Specializations.AllAsync();
            var resultArray = new _Specializations[Enum.GetValues(typeof(Specializations)).Cast<int>().Max() + 1];
            string path = @"C:\specialization.json";

            if (System.IO.File.Exists(path))
            {
                List<_Specializations> localData = JsonConvert.DeserializeObject<List<_Specializations>>(System.IO.File.ReadAllText(path));
                foreach (_Specializations spec in localData)
                {
                    resultArray[spec.Id] = spec;
                }
            }

            foreach (Gw2Sharp.WebApi.V2.Models.Specialization spec in specs)
            {
                if (spec.Elite)
                {
                    int id = spec.Id;
                    if (resultArray[id] == null)
                    {
                        resultArray[id] = new _Specializations() { Id = id, Name = spec.Name, API_Id = spec.Id };
                    }
                    else
                    {
                        resultArray[id].Name = spec.Name;
                    }
                }
            }

            List<_Specializations> _data = new List<_Specializations>();
            foreach (_Specializations spec in resultArray)
            {
                if (spec != null)
                {
                    _data.Add(spec);
                }
            }

            string json = JsonConvert.SerializeObject(_data.ToArray());

            //write string to file
            System.IO.File.WriteAllText(path, json);
        }
        async public Task _Fetch_Races()
        {
            var races = await Gw2ApiManager.Gw2ApiClient.V2.Races.AllAsync();
            var resultArray = new _Race[Enum.GetValues(typeof(RaceEnum)).Cast<int>().Max() + 1];
            string path = @"C:\races.json";

            if (System.IO.File.Exists(path))
            {
                List<_Race> localData = JsonConvert.DeserializeObject<List<_Race>>(System.IO.File.ReadAllText(path));
                foreach (_Race race in localData)
                {
                    resultArray[race.Id] = race;
                }
            }

            foreach (Gw2Sharp.WebApi.V2.Models.Race race in races)
            {
                int id = (int)Enum.Parse(typeof(RaceEnum), race.Id);
                if (resultArray[id] == null)
                {
                    resultArray[id] = new _Race() { Id = id, Name = race.Name, API_Id = race.Id };
                }
                else
                {
                    resultArray[id].Name = race.Name;
                }
            }

            List<_Race> _data = new List<_Race>();
            foreach (_Race race in resultArray)
            {
                if (race != null)
                {
                    _data.Add(race);
                }
            }

            string json = JsonConvert.SerializeObject(_data.ToArray());

            //write string to file
            System.IO.File.WriteAllText(path, json);
        }

        async public Task<string> getMapNameAsync(int id)
        {
            var map = _Maps[id];

            if (map != null)
            {
                if (map.Name != null && map.Name != "")
                {
                    return map.Name;
                }
                else
                {
                    return map.Name;
                }
            }

            return "Unkown Map ID";
        }
        async public Task<string> getRace(string id)
        {
            var result = await Gw2ApiManager.Gw2ApiClient.V2.Races.GetAsync(id);
            if (result != null)
            {
                return result.Name;
            }

            return "Unkown Race";
        }
        async public Task<string> getProfession(string id)
        {
            var result = await Gw2ApiManager.Gw2ApiClient.V2.Professions.GetAsync(id);
            if (result != null)
            {
                return result.Name;
            }

            return "Unkown Race";
        }
        async public Task<string> getSpecialization(int id)
        {
            var result = await Gw2ApiManager.Gw2ApiClient.V2.Specializations.GetAsync(id);
            if (result != null)
            {
                return result.Name;
            }

            return "Unkown Race";
        }
    }
}
