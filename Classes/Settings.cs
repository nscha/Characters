using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Input;

namespace Kenedia.Modules.Characters
{
    public class _Settings
    {
        public SettingEntry<Blish_HUD.Input.KeyBinding> LogoutKey;
        public SettingEntry<bool> EnterOnSwap;
        public SettingEntry<int> SwapDelay;
        public SettingEntry<int> FilterDelay;
        public int _FilterDelay = 75;
    }

    public partial class Module : Blish_HUD.Modules.Module
    {
        //Settings
        public static _Settings Settings = new _Settings();

        protected override void DefineSettings(SettingCollection settings)
        {
            Settings.LogoutKey = settings.DefineSetting(nameof(Settings.LogoutKey),
                                                     new Blish_HUD.Input.KeyBinding(Keys.F12),
                                                     () => Strings.common.Logout,
                                                     () => Strings.common.LogoutDescription);
            Settings.EnterOnSwap = settings.DefineSetting(nameof(Settings.EnterOnSwap),
                                                              false,
                                                              () => Strings.common.LoginAfterSelect,
                                                              () => Strings.common.LoginAfterSelect);

            Settings.SwapDelay = settings.DefineSetting(nameof(Settings.SwapDelay),
                                                              500,
                                                              () => string.Format(Strings.common.SwapDelay_DisplayName, Settings.SwapDelay.Value),
                                                              () => Strings.common.SwapDelay_Description);
            Settings.SwapDelay.SetRange(0, 5000);

            Settings.FilterDelay = settings.DefineSetting(nameof(Settings.FilterDelay),
                                                              150,
                                                              () => string.Format(Strings.common.FilterDelay_DisplayName, Settings.FilterDelay.Value),
                                                              () => Strings.common.FilterDelay_Description);
            Settings.FilterDelay.SetRange(0, 500);
            Settings.FilterDelay.SettingChanged += delegate { Settings._FilterDelay = Settings.FilterDelay.Value / 2; };
            Settings._FilterDelay = Settings.FilterDelay.Value / 2;
        }
    }
}
