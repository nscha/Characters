namespace Kenedia.Modules.Characters
{
    using Blish_HUD.Controls;

    internal static class ContainerExtensions
    {
        public static void ToggleVisibility(this Container c)
        {
            c.Visible = !c.Visible;
        }
    }
}
