using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Extensions;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using static Kenedia.Modules.Characters.Services.TextureManager;
using Point = Microsoft.Xna.Framework.Point;

namespace Kenedia.Modules.Characters.Controls
{
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
            FlowDirection = ControlFlowDirection.TopToBottom;
            WidthSizingMode = SizingMode.Fill;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.AutoSize;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 3);
            Location = new Point(0, 25);

            Services.TextureManager tM = Characters.ModuleInstance.TextureManager;
            contentPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = this,
                Location = new Point(0, 25),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            checkboxPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                OuterControlPadding = new Vector2(3, 3),
                ControlPadding = new Vector2(2, 4),
                AutoSizePadding = new Point(0, 4),
            };

            buttonPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(2, 2),
            };

            windowMode = new Checkbox()
            {
                Parent = checkboxPanel,
                Text = Strings.common.WindowedMode,
                BasicTooltipText = Strings.common.WindowedMode_Tooltip,
                Checked = Characters.ModuleInstance.Settings.WindowedMode.Value,
                Width = 200,
            };
            windowMode.Click += WindowMode_Click;
            Characters.ModuleInstance.Settings.WindowedMode.SettingChanged += (s, e) => { windowMode.Checked = Characters.ModuleInstance.Settings.WindowedMode.Value; };

            autoFix = new Checkbox()
            {
                Parent = checkboxPanel,
                Text = Strings.common.AutoFix,
                BasicTooltipText = Strings.common.AutoFix_Tooltip,
                Checked = Characters.ModuleInstance.Settings.AutoSortCharacters.Value,
                Width = 200,
            };
            autoFix.Click += AutoFix_Click;

            ocrCheckbox = new Checkbox()
            {
                Parent = checkboxPanel,
                Text = Strings.common.UseOCR,
                BasicTooltipText = Strings.common.UseOCR_Tooltip,
                Checked = Characters.ModuleInstance.Settings.UseOCR.Value,
                Width = 200,
            };
            ocrCheckbox.Click += OcrCheckbox_Click;

            ocrContainerButton = new StandardButton()
            {
                Parent = buttonPanel,
                Text = Strings.common.EditOCR,
                ResizeIcon = true,
                Icon = tM.GetIcon(Icons.Camera),
                BasicTooltipText = Strings.common.EditOCR_Tooltip,
            };
            ocrContainerButton.Click += OcrContainerButton_Click;

            potraitCaptureButton = new StandardButton()
            {
                Parent = buttonPanel,
                Text = Strings.common.TogglePortraitCapture,
                BasicTooltipText = Strings.common.TogglePortraitCapture_Tooltip,
                ResizeIcon = true,
                Icon = AsyncTexture2D.FromAssetId(358353),
            };
            potraitCaptureButton.Click += PotraitCaptureButton_Click;

            potraitFolderButton = new StandardButton()
            {
                Parent = buttonPanel,
                Text = Strings.common.OpenPortraitFolder,
                ResizeIcon = true,
                Icon = tM.GetIcon(Icons.Folder),
            };
            potraitFolderButton.Click += PotraitFolderButton_Click;

            fixCharacterOrder = new StandardButton()
            {
                Parent = buttonPanel,
                Text = Strings.common.FixCharacters,
                BasicTooltipText = Strings.common.FixCharacters_Tooltip,
                ResizeIcon = true,
                Icon = AsyncTexture2D.FromAssetId(156760),
            };
            fixCharacterOrder.Click += FixCharacterOrder_Click;

            refreshAPI = new StandardButton()
            {
                Parent = buttonPanel,
                Text = Strings.common.RefreshAPI,
                ResizeIcon = true,
                Icon = AsyncTexture2D.FromAssetId(156749),
            };
            refreshAPI.Click += RefreshAPI_Click;
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            if (windowMode != null)
            {
                windowMode.Width = contentPanel.Width - 5;
            }

            if (autoFix != null)
            {
                autoFix.Width = contentPanel.Width - 5;
            }

            if (ocrCheckbox != null)
            {
                ocrCheckbox.Width = contentPanel.Width - 5;
            }

            if (potraitCaptureButton != null)
            {
                potraitCaptureButton.Width = contentPanel.Width - 5;
            }

            if (potraitFolderButton != null)
            {
                potraitFolderButton.Width = contentPanel.Width - 5;
            }

            if (ocrContainerButton != null)
            {
                ocrContainerButton.Width = contentPanel.Width - 5;
            }

            if (fixCharacterOrder != null)
            {
                fixCharacterOrder.Width = contentPanel.Width - 5;
            }

            if (refreshAPI != null)
            {
                refreshAPI.Width = contentPanel.Width - 5;
            }
        }

        private void RefreshAPI_Click(object sender, MouseEventArgs e) => Characters.ModuleInstance.GW2APIHandler.CheckAPI();

        private void WindowMode_Click(object sender, MouseEventArgs e) => Characters.ModuleInstance.Settings.WindowedMode.Value = windowMode.Checked;

        private void AutoFix_Click(object sender, MouseEventArgs e) => Characters.ModuleInstance.Settings.AutoSortCharacters.Value = autoFix.Checked;

        private void FixCharacterOrder_Click(object sender, MouseEventArgs e)
        {
            if (!GameService.GameIntegration.Gw2Instance.IsInGame)
            {
                Characters.ModuleInstance.FixCharacterOrder();
            }
        }

        private void OcrCheckbox_Click(object sender, MouseEventArgs e) => Characters.ModuleInstance.Settings.UseOCR.Value = ocrCheckbox.Checked;

        private void OcrContainerButton_Click(object sender, MouseEventArgs e) => Characters.ModuleInstance.OCR?.ToggleContainer();

        private void PotraitFolderButton_Click(object sender, MouseEventArgs e)
        {
            ProcessStartInfo startInfo = new()
            {
                Arguments = Characters.ModuleInstance.AccountImagesPath,
                FileName = "explorer.exe",
            };

            _ = Process.Start(startInfo);
        }

        private void PotraitCaptureButton_Click(object sender, MouseEventArgs e) => Characters.ModuleInstance.PotraitCapture.ToggleVisibility();
    }
}
