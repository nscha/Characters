using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kenedia.Modules.Characters
{    public class AccountInfo
    {
        public string Name;
        public DateTimeOffset LastModified;
        public DateTimeOffset LastBlishUpdate;
        public void Save()
        {
            if (Module.API_Account != null)
            {
                List<AccountInfo> _data = new List<AccountInfo>();
                _data.Add(Module.userAccount);

                string json = JsonConvert.SerializeObject(_data.ToArray());

                //write string to file
                System.IO.File.WriteAllText(Module.AccountInfoPath, json);
            }
        }
        public bool CharacterUpdateNeeded()
        {
            double lastModified = DateTimeOffset.UtcNow.Subtract(LastModified).TotalSeconds;
            double lastUpdate = DateTimeOffset.UtcNow.Subtract(LastBlishUpdate).TotalSeconds;

            return lastModified > 800 || (lastUpdate) > lastModified;
        }
    }
}
