using Blish_HUD.Input;

namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    class ImageColorToggle : ImageGrayScaled
    {
        public object FilterObject;
        public FilterCategory FilterCategory;
        public Gw2Sharp.Models.ProfessionType Profession;

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            Active = !Active;

            if (FilterObject != null)
            {
                if (Active) Characters.ModuleInstance.MainWindow.CategoryFilters[FilterCategory].Add(FilterObject);
                else Characters.ModuleInstance.MainWindow.CategoryFilters[FilterCategory].Remove(FilterObject);

                Characters.ModuleInstance.MainWindow.FilterCharacters(null, null);
            }
        }
    }
}
