namespace Kenedia.Modules.Characters
{
    using Blish_HUD.Controls;

    internal static class ControlExtensions
    {
        public static void ToggleVisibility(this Control c)
        {
            c.Visible = !c.Visible;
        }
    }
}
