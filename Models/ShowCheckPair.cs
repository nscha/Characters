using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.Characters.Models
{
    public class ShowCheckPair
    {
        public ShowCheckPair(bool show, bool check)
        {
            Check = check;
            Show = show;
        }

        public bool Show { get; set; }

        public bool Check { get; set; }
    }
}
