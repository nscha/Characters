namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    using System.Diagnostics;
    using Blish_HUD;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Microsoft.Xna.Framework;
    using Point = Microsoft.Xna.Framework.Point;

    public class SettingsAndShortcuts : FlowTab
    {
        private readonly FlowPanel _checkboxPanel;
        private readonly FlowPanel _buttonPanel;
        private readonly FlowPanel _contentPanel;
        private readonly StandardButton _potraitCaptureButton;
        private readonly StandardButton _potraitFolderButton;
        private readonly StandardButton _fixCharacterOrder;
        private readonly StandardButton _refreshAPI;
        private readonly Checkbox _ocrCheckbox;
        private readonly Checkbox _autoFix;
        private readonly Checkbox _windowMode;
        private readonly StandardButton _ocrContainerButton;

        public SettingsAndShortcuts()
        {
            this.FlowDirection = ControlFlowDirection.TopToBottom;
            this.WidthSizingMode = SizingMode.Fill;
            this.AutoSizePadding = new Point(5, 5);
            this.HeightSizingMode = SizingMode.AutoSize;
            this.OuterControlPadding = new Vector2(5, 5);
            this.ControlPadding = new Vector2(5, 3);
            this.Location = new Point(0, 25);

            var tM = Characters.ModuleInstance.TextureManager;
            this._contentPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = this,
                Location = new Point(0, 25),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            this._checkboxPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = this._contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                OuterControlPadding = new Vector2(3, 3),
                ControlPadding = new Vector2(2, 4),
                AutoSizePadding = new Point(0, 4),
            };

            this._buttonPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = this._contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(2, 2),
            };

            this._windowMode = new Checkbox()
            {
                Parent = this._checkboxPanel,
                Text = "Windowed Mode",
                BasicTooltipText = "Is the game set to Window Mode?",
                Checked = Characters.ModuleInstance.Settings.WindowedMode.Value,
                Width = 200,
            };
            this._windowMode.Click += this.WindowMode_Click;
            Characters.ModuleInstance.Settings.WindowedMode.SettingChanged += (s, e) => { this._windowMode.Checked = Characters.ModuleInstance.Settings.WindowedMode.Value; };

            this._autoFix = new Checkbox()
            {
                Parent = this._checkboxPanel,
                Text = "Auto Fix",
                BasicTooltipText = "When failing to swap to a character automatically go through the characters to fix their order.",
                Checked = Characters.ModuleInstance.Settings.AutoSortCharacters.Value,
                Width = 200,
            };
            this._autoFix.Click += this.AutoFix_Click;

            this._ocrCheckbox = new Checkbox()
            {
                Parent = this._checkboxPanel,
                Text = "Use OCR",
                BasicTooltipText = "Use 'Optical Character Recognition' (OCR) to confirm the character name.",
                Checked = Characters.ModuleInstance.Settings.UseOCR.Value,
                Width = 200,
            };
            this._ocrCheckbox.Click += this.OcrCheckbox_Click;

            this._ocrContainerButton = new StandardButton()
            {
                Parent = this._buttonPanel,
                Text = "Edit OCR Region",
                ResizeIcon = true,
                Icon = tM.GetIcon(Icons.Camera),
            };
            this._ocrContainerButton.Click += this.OcrContainerButton_Click;

            this._potraitCaptureButton = new StandardButton()
            {
                Parent = this._buttonPanel,
                Text = "Toggle Potrait Capture",
                ResizeIcon = true,
                Icon = AsyncTexture2D.FromAssetId(358353),
            };
            this._potraitCaptureButton.Click += this.PotraitCaptureButton_Click;

            this._potraitFolderButton = new StandardButton()
            {
                Parent = this._buttonPanel,
                Text = "Open Potrait Folder",
                ResizeIcon = true,
                Icon = tM.GetIcon(Icons.Folder),
            };
            this._potraitFolderButton.Click += this.PotraitFolderButton_Click;

            this._fixCharacterOrder = new StandardButton()
            {
                Parent = this._buttonPanel,
                Text = "Fix Characters",
                ResizeIcon = true,
                Icon = AsyncTexture2D.FromAssetId(156760),
            };
            this._fixCharacterOrder.Click += this.FixCharacterOrder_Click;

            this._refreshAPI = new StandardButton()
            {
                Parent = this._buttonPanel,
                Text = "Refresh API",
                ResizeIcon = true,
                Icon = AsyncTexture2D.FromAssetId(156749),
            };
            this._refreshAPI.Click += this.RefreshAPI_Click;
        }

        private void RefreshAPI_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.GW2APIHandler.CheckAPI();
        }

        private void WindowMode_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.Settings.WindowedMode.Value = this._windowMode.Checked;
        }

        private void AutoFix_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.Settings.AutoSortCharacters.Value = this._autoFix.Checked;
        }

        private void FixCharacterOrder_Click(object sender, MouseEventArgs e)
        {
            if (!GameService.GameIntegration.Gw2Instance.IsInGame)
            {
                Characters.ModuleInstance.FixCharacterOrder();
            }
        }

        private void OcrCheckbox_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.Settings.UseOCR.Value = this._ocrCheckbox.Checked;
        }

        private void OcrContainerButton_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.OCR?.ToggleContainer();
        }

        private void PotraitFolderButton_Click(object sender, MouseEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = Characters.ModuleInstance.AccountImagesPath,
                FileName = "explorer.exe",
            };

            Process.Start(startInfo);
        }

        private void PotraitCaptureButton_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.PotraitCapture.ToggleVisibility();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            if (this._windowMode != null)
            {
                this._windowMode.Width = this._contentPanel.Width - 5;
            }

            if (this._autoFix != null)
            {
                this._autoFix.Width = this._contentPanel.Width - 5;
            }

            if (this._ocrCheckbox != null)
            {
                this._ocrCheckbox.Width = this._contentPanel.Width - 5;
            }

            if (this._potraitCaptureButton != null)
            {
                this._potraitCaptureButton.Width = this._contentPanel.Width - 5;
            }

            if (this._potraitFolderButton != null)
            {
                this._potraitFolderButton.Width = this._contentPanel.Width - 5;
            }

            if (this._ocrContainerButton != null)
            {
                this._ocrContainerButton.Width = this._contentPanel.Width - 5;
            }

            if (this._fixCharacterOrder != null)
            {
                this._fixCharacterOrder.Width = this._contentPanel.Width - 5;
            }

            if (this._refreshAPI != null)
            {
                this._refreshAPI.Width = this._contentPanel.Width - 5;
            }
        }
    }
}
