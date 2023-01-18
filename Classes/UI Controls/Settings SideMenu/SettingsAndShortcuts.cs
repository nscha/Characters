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
        private readonly FlowPanel checkboxPanel;
        private readonly FlowPanel buttonPanel;
        private readonly FlowPanel contentPanel;
        private readonly StandardButton potraitCaptureButton;
        private readonly StandardButton potraitFolderButton;
        private readonly StandardButton fixCharacterOrder;
        private readonly StandardButton refreshAPI;
        private readonly Checkbox ocrCheckbox;
        private readonly Checkbox autoFix;
        private readonly Checkbox windowMode;
        private readonly StandardButton ocrContainerButton;

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
            this.contentPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = this,
                Location = new Point(0, 25),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            this.checkboxPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = this.contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                OuterControlPadding = new Vector2(3, 3),
                ControlPadding = new Vector2(2, 4),
                AutoSizePadding = new Point(0, 4),
            };

            this.buttonPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = this.contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(2, 2),
            };

            this.windowMode = new Checkbox()
            {
                Parent = this.checkboxPanel,
                Text = "Windowed Mode",
                BasicTooltipText = "Is the game set to Window Mode?",
                Checked = Characters.ModuleInstance.Settings.WindowedMode.Value,
                Width = 200,
            };
            this.windowMode.Click += this.WindowMode_Click;
            Characters.ModuleInstance.Settings.WindowedMode.SettingChanged += (s, e) => { this.windowMode.Checked = Characters.ModuleInstance.Settings.WindowedMode.Value; };

            this.autoFix = new Checkbox()
            {
                Parent = this.checkboxPanel,
                Text = "Auto Fix",
                BasicTooltipText = "When failing to swap to a character automatically go through the characters to fix their order.",
                Checked = Characters.ModuleInstance.Settings.AutoSortCharacters.Value,
                Width = 200,
            };
            this.autoFix.Click += this.AutoFix_Click;

            this.ocrCheckbox = new Checkbox()
            {
                Parent = this.checkboxPanel,
                Text = "Use OCR",
                BasicTooltipText = "Use 'Optical Character Recognition' (OCR) to confirm the character name.",
                Checked = Characters.ModuleInstance.Settings.UseOCR.Value,
                Width = 200,
            };
            this.ocrCheckbox.Click += this.OcrCheckbox_Click;

            this.ocrContainerButton = new StandardButton()
            {
                Parent = this.buttonPanel,
                Text = "Edit OCR Region",
                ResizeIcon = true,
                Icon = tM.GetIcon(Icons.Camera),
            };
            this.ocrContainerButton.Click += this.OcrContainerButton_Click;

            this.potraitCaptureButton = new StandardButton()
            {
                Parent = this.buttonPanel,
                Text = "Toggle Potrait Capture",
                ResizeIcon = true,
                Icon = AsyncTexture2D.FromAssetId(358353),
            };
            this.potraitCaptureButton.Click += this.PotraitCaptureButton_Click;

            this.potraitFolderButton = new StandardButton()
            {
                Parent = this.buttonPanel,
                Text = "Open Potrait Folder",
                ResizeIcon = true,
                Icon = tM.GetIcon(Icons.Folder),
            };
            this.potraitFolderButton.Click += this.PotraitFolderButton_Click;

            this.fixCharacterOrder = new StandardButton()
            {
                Parent = this.buttonPanel,
                Text = "Fix Characters",
                ResizeIcon = true,
                Icon = AsyncTexture2D.FromAssetId(156760),
            };
            this.fixCharacterOrder.Click += this.FixCharacterOrder_Click;

            this.refreshAPI = new StandardButton()
            {
                Parent = this.buttonPanel,
                Text = "Refresh API",
                ResizeIcon = true,
                Icon = AsyncTexture2D.FromAssetId(156749),
            };
            this.refreshAPI.Click += this.RefreshAPI_Click;
        }

        private void RefreshAPI_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.GW2APIHandler.CheckAPI();
        }

        private void WindowMode_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.Settings.WindowedMode.Value = this.windowMode.Checked;
        }

        private void AutoFix_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.Settings.AutoSortCharacters.Value = this.autoFix.Checked;
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
            Characters.ModuleInstance.Settings.UseOCR.Value = this.ocrCheckbox.Checked;
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

            if (this.windowMode != null)
            {
                this.windowMode.Width = this.contentPanel.Width - 5;
            }

            if (this.autoFix != null)
            {
                this.autoFix.Width = this.contentPanel.Width - 5;
            }

            if (this.ocrCheckbox != null)
            {
                this.ocrCheckbox.Width = this.contentPanel.Width - 5;
            }

            if (this.potraitCaptureButton != null)
            {
                this.potraitCaptureButton.Width = this.contentPanel.Width - 5;
            }

            if (this.potraitFolderButton != null)
            {
                this.potraitFolderButton.Width = this.contentPanel.Width - 5;
            }

            if (this.ocrContainerButton != null)
            {
                this.ocrContainerButton.Width = this.contentPanel.Width - 5;
            }

            if (this.fixCharacterOrder != null)
            {
                this.fixCharacterOrder.Width = this.contentPanel.Width - 5;
            }

            if (this.refreshAPI != null)
            {
                this.refreshAPI.Width = this.contentPanel.Width - 5;
            }
        }
    }
}
