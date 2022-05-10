using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Input;
using Blish_HUD.Controls;
using System;

namespace Kenedia.Modules.Characters
{
    public class _Settings
    {
        public SettingEntry<Blish_HUD.Input.KeyBinding> LogoutKey;
        public SettingEntry<Blish_HUD.Input.KeyBinding> ShortcutKey;
        public SettingEntry<Blish_HUD.Input.KeyBinding> SwapModifier;
        public SettingEntry<bool> ShowCornerIcon;
        public SettingEntry<bool> EnterOnSwap;
        public SettingEntry<bool> AutoLogin;
        public SettingEntry<bool> DoubleClickToEnter;
        public SettingEntry<bool> FadeSubWindows;
        public SettingEntry<bool> OnlyMaxCrafting;
        public SettingEntry<bool> FocusFilter;
        public SettingEntry<bool> EnterToLogin;
        public SettingEntry<int> SwapDelay;
        public SettingEntry<int> FilterDelay;
        public int _FilterDelay = 75;

        public DateTime SwapModifierPressed;
        public bool isSwapModifierPressed() {
            Module.Logger.Debug("Time Since Logout Mod Click: " + DateTime.Now.Subtract(SwapModifierPressed).TotalMilliseconds);
            return DateTime.Now.Subtract(SwapModifierPressed).TotalMilliseconds <= 2500;
        }
    }

    public partial class Module : Blish_HUD.Modules.Module
    {
        //Settings
        protected override void DefineSettings(SettingCollection settings)
        {
            Settings.AutoLogin = settings.DefineSetting(nameof(Settings.AutoLogin),
                                                              false,
                                                              () => Strings.common.AutoLogin_DisplayName,
                                                              () => Strings.common.AutoLogin_Description);

            Settings.EnterOnSwap = settings.DefineSetting(nameof(Settings.EnterOnSwap),
                                                              false,
                                                              () => Strings.common.EnterOnSwap_DisplayName,
                                                              () => Strings.common.EnterOnSwap_Description);

            Settings.DoubleClickToEnter = settings.DefineSetting(nameof(Settings.DoubleClickToEnter),
                                                              false,
                                                              () => Strings.common.DoubleClickToEnter_DisplayName,
                                                              () => Strings.common.DoubleClickToEnter_Description);

            Settings.EnterToLogin = settings.DefineSetting(nameof(Settings.EnterToLogin),
                                                              false,
                                                              () => Strings.common.EnterToLogin_DisplayName,
                                                              () => Strings.common.EnterToLogin_Description);


            Settings.FadeSubWindows = settings.DefineSetting(nameof(Settings.FadeSubWindows),
                                                              false,
                                                              () => Strings.common.FadeOut_DisplayName,
                                                              () => Strings.common.FadeOut_Description);
            
            Settings.OnlyMaxCrafting = settings.DefineSetting(nameof(Settings.OnlyMaxCrafting),
                                                              true,
                                                              () => Strings.common.OnlyMaxCrafting_DisplayName,
                                                              () => Strings.common.OnlyMaxCrafting_Description);
            Settings.OnlyMaxCrafting.SettingChanged += delegate {
                foreach (Character c in Module.Characters)
                {
                    if (c.loaded && c.Crafting.Count > 0)
                    {
                        foreach(DataImage image in c.characterControl.crafting_Images)
                        {
                            image.Visible = (!Module.Settings.OnlyMaxCrafting.Value) || ((image.Crafting.Id == 4 || image.Crafting.Id == 7) && image.Crafting.Rating == 400) || (image.Crafting.Rating == 500);
                        }
                        c.characterControl.crafting_Panel.Invalidate();
                    }
                }
            };

            Settings.FocusFilter = settings.DefineSetting(nameof(Settings.FocusFilter),
                                                              false,
                                                              () => Strings.common.FocusFilter_DisplayName,
                                                              () => Strings.common.FocusFilter_Description);

            Settings.ShowCornerIcon = settings.DefineSetting(nameof(Settings.ShowCornerIcon),
                                                              true,
                                                              () => Strings.common.ShowCornerIcon_DisplayName,
                                                              () => Strings.common.ShowCornerIcon_Description);

            Settings.FilterDelay = settings.DefineSetting(nameof(Settings.FilterDelay),
                                                              150,
                                                              () => string.Format(Strings.common.FilterDelay_DisplayName, Settings.FilterDelay.Value),
                                                              () => Strings.common.FilterDelay_Description);

            Settings.FilterDelay.SetRange(0, 500);
            Settings.FilterDelay.SettingChanged += delegate { Settings._FilterDelay = Settings.FilterDelay.Value / 2; };
            Settings._FilterDelay = Settings.FilterDelay.Value / 2;

            Settings.SwapDelay = settings.DefineSetting(nameof(Settings.SwapDelay),
                                                              500,
                                                              () => string.Format(Strings.common.SwapDelay_DisplayName, Settings.SwapDelay.Value),
                                                              () => Strings.common.SwapDelay_Description);
            Settings.SwapDelay.SetRange(0, 5000);

            Settings.LogoutKey = settings.DefineSetting(nameof(Settings.LogoutKey),
                                                     new Blish_HUD.Input.KeyBinding(Keys.F12),
                                                     () => Strings.common.Logout,
                                                     () => Strings.common.LogoutDescription);

            Settings.ShortcutKey = settings.DefineSetting(nameof(Settings.ShortcutKey),
                                                     new Blish_HUD.Input.KeyBinding(ModifierKeys.Shift, Keys.C),
                                                     () => Strings.common.ShortcutToggle_DisplayName,
                                                     () => Strings.common.ShortcutToggle_Description);

            Settings.SwapModifier = settings.DefineSetting(nameof(Settings.SwapModifier),
                                                     new Blish_HUD.Input.KeyBinding(Keys.None),
                                                     () => Strings.common.SwapModifier_DisplayName,
                                                     () => Strings.common.SwapModifier_Description);
        }
    }
}
