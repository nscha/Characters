using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Point = Microsoft.Xna.Framework.Point;

namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    public class SettingsAndShortcuts : FlowTab
    {
        FlowPanel _checkboxPanel;
        FlowPanel _buttonPanel;
        FlowPanel _contentPanel;
        StandardButton _potraitCaptureButton;
        StandardButton _potraitFolderButton;
        StandardButton _fixCharacterOrder;
        StandardButton _refreshAPI;
        Checkbox _ocrCheckbox;
        Checkbox _autoFix;
        Checkbox _windowMode;
        StandardButton _ocrContainerButton;
        public SettingsAndShortcuts()
        {
            FlowDirection = ControlFlowDirection.TopToBottom;
            WidthSizingMode = SizingMode.Fill;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.AutoSize;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 3);
            Location = new Point(0, 25);

            var tM = Characters.ModuleInstance.TextureManager;
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
                ControlPadding = new Vector2(2,4),
                AutoSizePadding = new Point(0, 4),
            };

            _buttonPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = _contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(2,2),
            };

            _windowMode = new Checkbox()
            {
                Parent = _checkboxPanel,
                Text = "Windowed Mode",
                BasicTooltipText = "Is the game set to Window Mode?",
                Checked = Characters.ModuleInstance.Settings._WindowedMode.Value,
                Width = 200,
            };
            _windowMode.Click += _windowMode_Click;
            Characters.ModuleInstance.Settings._WindowedMode.SettingChanged += (s, e) => { _windowMode.Checked = Characters.ModuleInstance.Settings._WindowedMode.Value; };

            _autoFix = new Checkbox()
            {
                Parent = _checkboxPanel,
                Text = "Auto Fix",
                BasicTooltipText = "When failing to swap to a character automatically go through the characters to fix their order.",
                Checked = Characters.ModuleInstance.Settings._AutoSortCharacters.Value,
                Width = 200,
            };
            _autoFix.Click += _autoFix_Click;

            _ocrCheckbox = new Checkbox()
            {
                Parent = _checkboxPanel,
                Text = "Use OCR",
                BasicTooltipText = "Use 'Optical Character Recognition' (OCR) to confirm the character name.",
                Checked = Characters.ModuleInstance.Settings._UseOCR.Value,
                Width = 200,
            };
            _ocrCheckbox.Click += _ocrCheckbox_Click;

            _ocrContainerButton = new StandardButton()
            {
                Parent = _buttonPanel,
                Text = "Edit OCR Region",
                ResizeIcon = true,
                Icon = tM.getIcon(_Icons.Camera),
            };
            _ocrContainerButton.Click += _ocrContainerButton_Click;

            _potraitCaptureButton = new StandardButton()
            {
                Parent = _buttonPanel,
                Text = "Toggle Potrait Capture",
                ResizeIcon = true,
                Icon = AsyncTexture2D.FromAssetId(358353),
            };
            _potraitCaptureButton.Click += _potraitCaptureButton_Click;

            _potraitFolderButton = new StandardButton()
            {
                Parent = _buttonPanel,
                Text = "Open Potrait Folder",
                ResizeIcon = true,
                Icon = tM.getIcon(_Icons.Folder),
            };
            _potraitFolderButton.Click += _potraitFolderButton_Click;

            _fixCharacterOrder = new StandardButton()
            {
                Parent = _buttonPanel,
                Text = "Fix Characters",
                ResizeIcon = true,
                Icon = AsyncTexture2D.FromAssetId(156760),
            };
            _fixCharacterOrder.Click += _fixCharacterOrder_Click;

            _refreshAPI = new StandardButton()
            {
                Parent = _buttonPanel,
                Text = "Refresh API",
                ResizeIcon = true,
                Icon = AsyncTexture2D.FromAssetId(156749),
            };
            _refreshAPI.Click += _refreshAPI_Click;

        }

        private void _refreshAPI_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.GW2API_Handler.CheckAPI();
        }

        private void _windowMode_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.Settings._WindowedMode.Value = _windowMode.Checked;
        }

        private void _autoFix_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.Settings._AutoSortCharacters.Value = _autoFix.Checked;
        }

        private void _fixCharacterOrder_Click(object sender, MouseEventArgs e)
        {
            if (!GameService.GameIntegration.Gw2Instance.IsInGame)
            {
                Characters.ModuleInstance.FixCharacterOrder();
            }
        }

        private void _ocrCheckbox_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.Settings._UseOCR.Value = _ocrCheckbox.Checked;
        }

        private void _ocrContainerButton_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.OCR?.ToggleContainer();
        }

        private void _potraitFolderButton_Click(object sender, MouseEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = Characters.ModuleInstance.AccountImagesPath,
                FileName = "explorer.exe",
            };

            Process.Start(startInfo);
        }

        private void _potraitCaptureButton_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.PotraitCapture.ToggleVisibility();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            if (_windowMode != null) _windowMode.Width = _contentPanel.Width - 5;
            if (_autoFix != null) _autoFix.Width = _contentPanel.Width - 5;
            if (_ocrCheckbox != null) _ocrCheckbox.Width = _contentPanel.Width - 5;

            if (_potraitCaptureButton != null) _potraitCaptureButton.Width = _contentPanel.Width - 5;
            if (_potraitFolderButton != null) _potraitFolderButton.Width = _contentPanel.Width - 5;
            if (_ocrContainerButton != null) _ocrContainerButton.Width = _contentPanel.Width - 5;
            if (_fixCharacterOrder != null) _fixCharacterOrder.Width = _contentPanel.Width - 5;
            if (_refreshAPI != null) _refreshAPI.Width = _contentPanel.Width - 5;
        }
    }
}
