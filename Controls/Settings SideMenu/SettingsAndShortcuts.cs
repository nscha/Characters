using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Extensions;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using static Kenedia.Modules.Characters.Services.TextureManager;
using Point = Microsoft.Xna.Framework.Point;

namespace Kenedia.Modules.Characters.Controls
{
    public class SettingsAndShortcuts : FlowTab
    {
        private readonly FlowPanel _checkboxPanel;
        private readonly FlowPanel _buttonPanel;
        private readonly FlowPanel _contentPanel;
        private readonly StandardButton _potraitCaptureButton;
        private readonly StandardButton _potraitFolderButton;
        private readonly StandardButton _fixCharacterOrder;
        private readonly StandardButton _refreshAPI;
        private readonly Checkbox _fadeSideMenus;
        private readonly Checkbox _ocrCheckbox;
        private readonly Checkbox _autoFix;
        private readonly Checkbox _windowMode;
        private readonly StandardButton _ocrContainerButton;

        public SettingsAndShortcuts()
        {
            FlowDirection = ControlFlowDirection.TopToBottom;
            WidthSizingMode = SizingMode.Fill;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.AutoSize;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 3);
            Location = new Point(0, 25);

            Services.TextureManager tM = Characters.ModuleInstance.TextureManager;
            _contentPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = this,
                Location = new Point(0, 25),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            _checkboxPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = _contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                OuterControlPadding = new Vector2(3, 3),
                ControlPadding = new Vector2(2, 4),
                AutoSizePadding = new Point(0, 4),
            };

            _buttonPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = _contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(2, 2),
            };

            _fadeSideMenus = new Checkbox()
            {
                Parent = _checkboxPanel,
                Text = Strings.common.FadeOut_DisplayName,
                BasicTooltipText = Strings.common.FadeOut_Description,
                Checked = Characters.ModuleInstance.Settings.FadeOut.Value,
                Width = 200,
            };
            _fadeSideMenus.Click += FadeSideMenus_Click;

            _windowMode = new Checkbox()
            {
                Parent = _checkboxPanel,
                Text = Strings.common.WindowedMode,
                BasicTooltipText = Strings.common.WindowedMode_Tooltip,
                Checked = Characters.ModuleInstance.Settings.WindowedMode.Value,
                Width = 200,
            };
            _windowMode.Click += WindowMode_Click;
            Characters.ModuleInstance.Settings.WindowedMode.SettingChanged += (s, e) => _windowMode.Checked = Characters.ModuleInstance.Settings.WindowedMode.Value;

            _autoFix = new Checkbox()
            {
                Parent = _checkboxPanel,
                Text = Strings.common.AutoFix,
                BasicTooltipText = Strings.common.AutoFix_Tooltip,
                Checked = Characters.ModuleInstance.Settings.AutoSortCharacters.Value,
                Width = 200,
            };
            _autoFix.Click += AutoFix_Click;

            _ocrCheckbox = new Checkbox()
            {
                Parent = _checkboxPanel,
                Text = Strings.common.UseOCR,
                BasicTooltipText = Strings.common.UseOCR_Tooltip,
                Checked = Characters.ModuleInstance.Settings.UseOCR.Value,
                Width = 200,
            };
            _ocrCheckbox.Click += OcrCheckbox_Click;

            _ocrContainerButton = new StandardButton()
            {
                Parent = _buttonPanel,
                Text = Strings.common.EditOCR,
                ResizeIcon = true,
                Icon = tM.GetIcon(Icons.Camera),
                BasicTooltipText = Strings.common.EditOCR_Tooltip,
            };
            _ocrContainerButton.Click += OcrContainerButton_Click;

            _potraitCaptureButton = new StandardButton()
            {
                Parent = _buttonPanel,
                Text = Strings.common.TogglePortraitCapture,
                BasicTooltipText = Strings.common.TogglePortraitCapture_Tooltip,
                ResizeIcon = true,
                Icon = AsyncTexture2D.FromAssetId(358353),
            };
            _potraitCaptureButton.Click += PotraitCaptureButton_Click;

            _potraitFolderButton = new StandardButton()
            {
                Parent = _buttonPanel,
                Text = Strings.common.OpenPortraitFolder,
                ResizeIcon = true,
                Icon = tM.GetIcon(Icons.Folder),
            };
            _potraitFolderButton.Click += PotraitFolderButton_Click;

            _fixCharacterOrder = new StandardButton()
            {
                Parent = _buttonPanel,
                Text = Strings.common.FixCharacters,
                BasicTooltipText = Strings.common.FixCharacters_Tooltip,
                ResizeIcon = true,
                Icon = AsyncTexture2D.FromAssetId(156760),
            };
            _fixCharacterOrder.Click += FixCharacterOrder_Click;

            _refreshAPI = new StandardButton()
            {
                Parent = _buttonPanel,
                Text = Strings.common.RefreshAPI,
                ResizeIcon = true,
                Icon = AsyncTexture2D.FromAssetId(156749),
            };
            _refreshAPI.Click += RefreshAPI_Click;
        }

        private void FadeSideMenus_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.Settings.FadeOut.Value = _fadeSideMenus.Checked;
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            if (_windowMode != null)
            {
                _windowMode.Width = _contentPanel.Width - 5;
            }

            if (_autoFix != null)
            {
                _autoFix.Width = _contentPanel.Width - 5;
            }

            if (_ocrCheckbox != null)
            {
                _ocrCheckbox.Width = _contentPanel.Width - 5;
            }

            if (_potraitCaptureButton != null)
            {
                _potraitCaptureButton.Width = _contentPanel.Width - 5;
            }

            if (_potraitFolderButton != null)
            {
                _potraitFolderButton.Width = _contentPanel.Width - 5;
            }

            if (_ocrContainerButton != null)
            {
                _ocrContainerButton.Width = _contentPanel.Width - 5;
            }

            if (_fixCharacterOrder != null)
            {
                _fixCharacterOrder.Width = _contentPanel.Width - 5;
            }

            if (_refreshAPI != null)
            {
                _refreshAPI.Width = _contentPanel.Width - 5;
            }
        }

        private void RefreshAPI_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.GW2APIHandler.CheckAPI();
        }

        private void WindowMode_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.Settings.WindowedMode.Value = _windowMode.Checked;
        }

        private void AutoFix_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.Settings.AutoSortCharacters.Value = _autoFix.Checked;
        }

        private void FixCharacterOrder_Click(object sender, MouseEventArgs e)
        {
            if (!GameService.GameIntegration.Gw2Instance.IsInGame)
            {
                CharacterSorting.Start(Characters.ModuleInstance.CharacterModels);
            }
        }

        private void OcrCheckbox_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.Settings.UseOCR.Value = _ocrCheckbox.Checked;
        }

        private void OcrContainerButton_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.OCR?.ToggleContainer();
        }

        private void PotraitFolderButton_Click(object sender, MouseEventArgs e)
        {
            ProcessStartInfo startInfo = new()
            {
                Arguments = Characters.ModuleInstance.AccountImagesPath,
                FileName = "explorer.exe",
            };

            _ = Process.Start(startInfo);
        }

        private void PotraitCaptureButton_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.PotraitCapture.ToggleVisibility();
        }
    }
}
