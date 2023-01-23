using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace Kenedia.Modules.Characters.Controls.SideMenu
{
    public class DisplayCheckToggle : FlowPanel
    {
        private readonly Checkbox _checkBox;
        private readonly ImageToggle _showButton;
        private readonly string _key;
        private readonly SettingsModel _settings;

        private bool _checkChecked;
        private bool _showChecked;

        public event EventHandler<Tuple<bool, bool>> Changed;
        public event EventHandler<bool> ShowChanged;
        public event EventHandler<bool> CheckChanged;

        public DisplayCheckToggle(bool displayButton_Checked = true, bool checkbox_Checked = true)
        {
            WidthSizingMode = SizingMode.Fill;
            HeightSizingMode = SizingMode.AutoSize;

            FlowDirection = ControlFlowDirection.SingleLeftToRight;
            ControlPadding = new(5, 0);

            _showButton = new()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(605021),
                Size = new Point(20, 20),
                TextureRectangle = new Rectangle(4, 4, 24, 24),
                ColorActive = Color.White,
                ColorHovered = Color.White * 0.5f,
                Checked = displayButton_Checked,
            };
            ShowChecked = _showButton.Checked;
            _showButton.CheckedChanged += SettingChanged;

            _checkBox = new()
            {
                Parent = this,
                Height = 20,
                Checked = checkbox_Checked,
            };
            CheckChecked = _checkBox.Checked;
            _checkBox.CheckedChanged += SettingChanged;
        }

        public DisplayCheckToggle(SettingsModel settings, string key) : this()
        {
            _key = key;

            if (!settings.DisplayToggles.Value.ContainsKey(_key))
            {
                settings.DisplayToggles.Value.Add(_key, new(true, true));
            }

            _showButton.Checked = settings.DisplayToggles.Value[_key].Show;
            ShowChecked = _showButton.Checked;

            _checkBox.Checked = settings.DisplayToggles.Value[_key].Check;
            CheckChecked = _checkBox.Checked;

            _settings = settings;
        }

        public string Text
        {
            get => _checkBox.Text;
            set => _checkBox.Text = value;
        }

        public string CheckTooltip
        {
            get => _checkBox.BasicTooltipText;
            set => _checkBox.BasicTooltipText = value;
        }

        public string DisplayTooltip
        {
            get => _showButton.BasicTooltipText;
            set => _showButton.BasicTooltipText = value;
        }

        public bool CheckChecked { get => _checkChecked; set { _checkChecked = value; _checkBox.Checked = value; } }
        public bool ShowChecked { get => _showChecked; set { _showChecked = value; _showButton.Checked = value; } }

        private void SettingChanged(object sender, EventArgs e)
        {
            if (_settings != null)
            {
                _settings.DisplayToggles.Value = new(_settings.DisplayToggles.Value)
                {
                    [_key] = new(_showButton.Checked, _checkBox.Checked)
                };
            }

            if (_checkChecked != _checkBox.Checked)
            {
                _checkChecked = _checkBox.Checked;
                CheckChanged?.Invoke(this, _checkBox.Checked);
            }

            if (_showChecked != _showButton.Checked)
            {
                _showChecked = _showButton.Checked;
                ShowChanged?.Invoke(this, _showButton.Checked);
            }

            Changed?.Invoke(this, new(_showButton.Checked, _checkBox.Checked));
        }
    }
}
