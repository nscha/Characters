using Blish_HUD.Input;

namespace Kenedia.Modules.Characters.Controls
{
    internal class ImageColorToggle : ImageGrayScaled
    {
        public object FilterObject { get; set; }

        public FilterCategory FilterCategory { get; set; }

        public Gw2Sharp.Models.ProfessionType Profession { get; set; }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            Active = !Active;

            if (FilterObject != null)
            {
                if (Active)
                {
                    Characters.ModuleInstance.MainWindow.CategoryFilters[FilterCategory].Add(FilterObject);
                }
                else
                {
                    _ = Characters.ModuleInstance.MainWindow.CategoryFilters[FilterCategory].Remove(FilterObject);
                }

                Characters.ModuleInstance.MainWindow.FilterCharacters(null, null);
            }
        }
    }
}
