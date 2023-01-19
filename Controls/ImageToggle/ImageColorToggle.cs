namespace Kenedia.Modules.Characters.Controls
{
    using Blish_HUD.Input;

    internal class ImageColorToggle : ImageGrayScaled
    {
        public object FilterObject { get; set; }

        public FilterCategory FilterCategory { get; set; }

        public Gw2Sharp.Models.ProfessionType Profession { get; set; }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            this.Active = !this.Active;

            if (this.FilterObject != null)
            {
                if (this.Active)
                {
                    Characters.ModuleInstance.MainWindow.CategoryFilters[this.FilterCategory].Add(this.FilterObject);
                }
                else
                {
                    Characters.ModuleInstance.MainWindow.CategoryFilters[this.FilterCategory].Remove(this.FilterObject);
                }

                Characters.ModuleInstance.MainWindow.FilterCharacters(null, null);
            }
        }
    }
}
