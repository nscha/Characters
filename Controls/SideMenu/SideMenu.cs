using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Interfaces;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Characters.Views;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace Kenedia.Modules.Characters.Controls.SideMenu
{
    public class SideMenu : TabbedPanel, ILocalizable
    {
        private readonly SideMenuToggles _toggles;
        private readonly SideMenuBehaviours _behaviours;

        public SideMenu()
        {
            WidthSizingMode = SizingMode.Standard;
            HeightSizingMode = SizingMode.Standard;
            BackgroundColor = Color.Black * 0.4f;
            Background = AsyncTexture2D.FromAssetId(156003);
            Parent = GameService.Graphics.SpriteScreen;
            ZIndex = 100;

            Width = 250;
            //Height = 415;
            HeightSizingMode = SizingMode.AutoSize;

            TextureRectangle = new Rectangle(30, 30, Background.Width - 60, Background.Height - 60);

            AddTab(_toggles = new SideMenuToggles()
            {
                Icon = AsyncTexture2D.FromAssetId(440021),
                Width = Width,
            });

            AddTab(_behaviours = new SideMenuBehaviours()
            {
                Icon = AsyncTexture2D.FromAssetId(156909),
            });
            SwitchTab(_toggles);

            Characters.ModuleInstance.LanguageChanged += OnLanguageChanged;
            OnLanguageChanged();
        }

        private SettingsModel Settings => Characters.ModuleInstance.Settings;

        private MainWindow MainWindow => Characters.ModuleInstance.MainWindow;

        private Characters ModuleInstance => Characters.ModuleInstance;

        public void OnLanguageChanged(object s = null, EventArgs e = null)
        {
            _toggles.Name = Strings.common.FilterToggles;
            _behaviours.Name = Strings.common.AppearanceAndBehaviour;
        }

        protected override void SwitchTab(PanelTab tab = null)
        {
            base.SwitchTab(tab);

            foreach(var t in Tabs)
            {
                t.Height= 5;
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            ModuleInstance.LanguageChanged -= OnLanguageChanged;
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            TextureRectangle = new Rectangle(30, 30, Math.Min(Background.Width - 100, Width), Math.Min(Background.Height - 100, Height));
        }
    }
}
