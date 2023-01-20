using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.Characters.Services
{
    public static class InputService
    {
        public static void MouseWiggle()
        {
            System.Drawing.Point p = Blish_HUD.Controls.Intern.Mouse.GetPosition();

            Blish_HUD.Controls.Intern.Mouse.SetPosition(p.X, p.Y, false);
            Blish_HUD.Controls.Intern.Mouse.SetPosition(p.X, p.Y, true);
        }
    }
}
