﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Extensions;
using Kenedia.Modules.Characters.Interfaces;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Characters.Views;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Kenedia.Modules.Characters.Controls.SideMenu
{
    public class SideMenu : TabbedPanel, ILocalizable
    {
        private readonly Panel _headerPanel;
        private readonly List<Control> _buttons = new();
        private ImageButton _closeButton;
        private ImageToggleButton _pinButton;
        private ImageButton _ocrButton;
        private ImageButton _potraitButton;
        private ImageButton _folderButton;
        private ImageButton _fixButton;
        private ImageButton _refreshButton;

        private readonly SideMenuToggles _toggles;
        private readonly SideMenuBehaviours _behaviours;
        private double _opacityTick;
        private DateTime _lastMouseOver;

        public SideMenu()
        {
            WidthSizingMode = SizingMode.Standard;
            HeightSizingMode = SizingMode.Standard;
            BackgroundColor = Color.Black * 0.4f;
            Background = AsyncTexture2D.FromAssetId(156003);
            Parent = GameService.Graphics.SpriteScreen;
            ZIndex = 100;

            _headerPanel = new()
            {
                Parent = this,
                BackgroundColor = Color.Black * 0.95f,
                WidthSizingMode = SizingMode.Fill,
                Height = 25,
            };

            _tabsButtonPanel.Location = new Point(0, _headerPanel.Bottom);
            CreateHeaderButtons();

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
            _ = SwitchTab(_toggles);

            Characters.ModuleInstance.LanguageChanged += OnLanguageChanged;
            OnLanguageChanged();
        }

        private void CloseButton_Click(object sender, MouseEventArgs e)
        {
            Hide();
        }

        private void CreateHeaderButtons()
        {
            var tm = Characters.ModuleInstance.TextureManager;

            _ocrButton = new()
            {
                Parent = _headerPanel,
                Texture = tm.GetIcon(TextureManager.Icons.Camera),
                HoveredTexture = tm.GetIcon(TextureManager.Icons.Camera_Hovered),
                Size = new Point(20, 20),
            };
            _ocrButton.Click += OCRButton_Click;
            _buttons.Add(_ocrButton);

            _potraitButton = new()
            {
                Parent = _headerPanel,
                Texture = AsyncTexture2D.FromAssetId(358353),
                HoveredTexture = tm.GetIcon(TextureManager.Icons.Portrait_Hovered),
                Size = new Point(20, 20),
                ColorHovered = Color.White,
            };
            _potraitButton.Click += PotraitButton_Click;
            _buttons.Add(_potraitButton);

            _folderButton = new()
            {
                Parent = _headerPanel,
                Texture = tm.GetIcon(TextureManager.Icons.Folder),
                HoveredTexture = tm.GetIcon(TextureManager.Icons.Folder_Hovered),
                Size = new Point(20, 20),
            };
            _folderButton.Click += FolderButton_Click;
            _buttons.Add(_folderButton);

            _fixButton = new()
            {
                Parent = _headerPanel,
                Texture = AsyncTexture2D.FromAssetId(156760),
                HoveredTexture = AsyncTexture2D.FromAssetId(156759),
                Size = new Point(20, 20),
            };
            _fixButton.Click += FixButton_Click;
            _buttons.Add(_fixButton);

            _refreshButton = new()
            {
                Parent = _headerPanel,
                Texture = AsyncTexture2D.FromAssetId(156749),
                HoveredTexture = AsyncTexture2D.FromAssetId(156750),
                Size = new Point(20, 20),
            };
            _refreshButton.Click += RefreshButton_Click;
            _buttons.Add(_refreshButton);

            _pinButton = new ImageToggleButton((b) => Settings.PinSideMenus.Value = b)
            {
                Parent = _headerPanel,
                Texture = Characters.ModuleInstance.TextureManager.GetIcon(TextureManager.Icons.Pin),
                HoveredTexture = Characters.ModuleInstance.TextureManager.GetIcon(TextureManager.Icons.Pin_Hovered),
                ActiveTexture = Characters.ModuleInstance.TextureManager.GetIcon(TextureManager.Icons.Pin_Hovered),
                ColorDefault = new Color(175, 175, 175),
                ColorActive = ContentService.Colors.ColonialWhite,
                Size = new(20, 20),
                Active = Settings.PinSideMenus.Value,
                BasicTooltipText = Strings.common.PinSideMenus_Description,
            };
            _buttons.Add(_pinButton);

            _closeButton = new()
            {
                Parent = _headerPanel,
                Texture = AsyncTexture2D.FromAssetId(156012),
                HoveredTexture = AsyncTexture2D.FromAssetId(156011),
                Size = new Point(20, 20),
                TextureRectangle = new Rectangle(7, 7, 20, 20),
            };
            _closeButton.Click += CloseButton_Click;
            _buttons.Add(_closeButton);
        }

        private void RefreshButton_Click(object sender, MouseEventArgs e)
        {
            ModuleInstance.GW2APIHandler.CheckAPI();
        }

        private void FixButton_Click(object sender, MouseEventArgs e)
        {
            if (!GameService.GameIntegration.Gw2Instance.IsInGame)
            {
                CharacterSorting.Start(ModuleInstance.CharacterModels);
            }
        }

        private void FolderButton_Click(object sender, MouseEventArgs e)
        {
            ProcessStartInfo startInfo = new()
            {
                Arguments = ModuleInstance.AccountImagesPath,
                FileName = "explorer.exe",
            };

            _ = Process.Start(startInfo);
        }

        private void PotraitButton_Click(object sender, MouseEventArgs e)
        {
            ModuleInstance.PotraitCapture.ToggleVisibility();
        }

        private void OCRButton_Click(object sender, MouseEventArgs e)
        {
            ModuleInstance.OCR?.ToggleContainer();
        }

        private SettingsModel Settings => Characters.ModuleInstance.Settings;

        private MainWindow MainWindow => Characters.ModuleInstance.MainWindow;

        private Characters ModuleInstance => Characters.ModuleInstance;

        public void ResetToggles()
        {
            _toggles.ResetToggles();
        }

        public void OnLanguageChanged(object s = null, EventArgs e = null)
        {
            _toggles.Name = Strings.common.FilterToggles;
            _behaviours.Name = Strings.common.AppearanceAndBehaviour;
            _closeButton.BasicTooltipText = Strings.common.Close;
            _pinButton.BasicTooltipText = Strings.common.PinSideMenus_Description;
            _refreshButton.BasicTooltipText = Strings.common.RefreshAPI;
            _fixButton.BasicTooltipText = Strings.common.FixCharacters_Tooltip;
            _folderButton.BasicTooltipText = Strings.common.OpenPortraitFolder;
            _potraitButton.BasicTooltipText = Strings.common.TogglePortraitCapture_Tooltip;
            _ocrButton.BasicTooltipText = Strings.common.EditOCR_Tooltip;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (gameTime.TotalGameTime.TotalMilliseconds - _opacityTick > 50)
            {
                _opacityTick = gameTime.TotalGameTime.TotalMilliseconds;

                if (!MouseOver && !Characters.ModuleInstance.Settings.PinSideMenus.Value && DateTime.Now.Subtract(_lastMouseOver).TotalMilliseconds >= 2500)
                {
                    Opacity -= 0.05F;
                    if (Opacity <= 0F)
                    {
                        Hide();
                    }
                }
            }
        }

        protected override bool SwitchTab(PanelTab tab = null)
        {
            bool result = base.SwitchTab(tab);

            foreach (var t in Tabs)
            {
               if(t != tab) t.Height = 5;
            }

            return result;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            ModuleInstance.LanguageChanged -= OnLanguageChanged;
            _closeButton.Click -= CloseButton_Click;
            _refreshButton.Click -= RefreshButton_Click;
            _fixButton.Click -= FixButton_Click;
            _folderButton.Click -= FolderButton_Click;
            _potraitButton.Click -= PotraitButton_Click;
            _ocrButton.Click -= OCRButton_Click;
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            TextureRectangle = new Rectangle(30, 30, Math.Min(Background.Width - 100, Width), Math.Min(Background.Height - 100, Height));

            var gap = (Width - 6 - (_buttons.Count * 20)) / (_buttons.Count - 1);
            for (int i = 0; i < _buttons.Count; i++)
            {
                Control b = _buttons[i];
                b.Location = new(6 + (i * gap) + (i * b.Width), 3);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _lastMouseOver = DateTime.Now;
            Opacity = 1f;
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            _lastMouseOver = DateTime.Now;
            Opacity = 1f;
        }
    }
}