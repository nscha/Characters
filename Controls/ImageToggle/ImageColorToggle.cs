using Blish_HUD.Input;
using System;

namespace Kenedia.Modules.Characters.Controls
{
    internal class ImageColorToggle : ImageGrayScaled
    {
        public object FilterObject { get; set; }

        public FilterCategory FilterCategory { get; set; }

        public Gw2Sharp.Models.ProfessionType Profession { get; set; }

        private readonly Action<bool> _onChanged;

        public ImageColorToggle()
        {

        }

        public ImageColorToggle(Action<bool> onChanged) 
            : this()
        {
            _onChanged= onChanged;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            Active = !Active;
            _onChanged?.Invoke(Active);
        }
    }
}
