using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Extensions;
using Kenedia.Modules.Characters.Interfaces;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Patagames.Ocr;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kenedia.Modules.Characters.Services.SettingsModel;

namespace Kenedia.Modules.Characters.Views
{
    public class SettingsWindow : StandardWindow, ILocalizable
    {
        private readonly AsyncTexture2D _subWindowEmblem = AsyncTexture2D.FromAssetId(156027);
        private readonly AsyncTexture2D _mainWindowEmblem = AsyncTexture2D.FromAssetId(156015);
        private Rectangle _mainEmblemRectangle;
        private Rectangle _subEmblemRectangle;
        private Rectangle _titleRectangle;
        private BitmapFont _titleFont = GameService.Content.DefaultFont32;

        private readonly KeybindingAssigner _logoutButton;
        private readonly KeybindingAssigner _toggleWindowButton;
        private readonly Checkbox _showCornerIcon;
        private readonly Checkbox _closeOnSwap;
        private readonly Checkbox _ignoreDiacritics;
        private readonly Checkbox _showStatusPopup;
        private readonly Checkbox _loginAfterSelect;
        private readonly Checkbox _doubleClickLogin;
        private readonly Checkbox _enterLogin;
        private readonly Checkbox _autoFix;
        private readonly Checkbox _useOCR;
        private readonly Checkbox _windowMode;
        private readonly Checkbox _showRandomButton;
        private readonly Checkbox _showTooltip;

        private readonly Dropdown _panelSize;
        private readonly Dropdown _panelAppearance;

        private readonly Label _keyDelayLabel;
        private readonly TrackBar _keyDelay;
        private readonly Label _filterDelayLabel;
        private readonly TrackBar _filterDelay;
        private readonly Label _loadingDelayLabel;
        private readonly TrackBar _loadingDelay;

        private readonly Panel _contentPanel;
        private readonly List<Action> _languageActions = new();

        public SettingsWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {
            Label label;
            Panel p;
            Panel subP;

            _contentPanel = new()
            {
                Parent = this,
                Width = ContentRegion.Width,
                Height = ContentRegion.Height,                
                CanScroll = true,
            };

            #region Keybinds
            p = new Panel()
            {
                Parent = _contentPanel,
                Width = ContentRegion.Width - 20,
                Height = 100,
                ShowBorder = true,
            };

            _ = new Image()
            {
                Texture = AsyncTexture2D.FromAssetId(156734),
                Parent = p,
                Size = new(30, 30),
            };

            var label1 = new Label()
            {
                Parent = p,
                AutoSizeWidth = true,
                Location = new(35, 0),
                Height = 30,
            };
            _languageActions.Add(() => label1.Text = Strings.common.Keybinds);

            var cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5, 35),
                HeightSizingMode = SizingMode.Fill,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            _logoutButton = new()
            {
                Parent = cP,
                Width = ContentRegion.Width - 35,
                KeyBinding = Settings.LogoutKey.Value,
            };
            _logoutButton.BindingChanged += LogoutButton_BindingChanged;
            _languageActions.Add(() =>
            {
                _logoutButton.KeyBindingName = Strings.common.Logout;
                _logoutButton.BasicTooltipText = Strings.common.LogoutDescription;
            });

            _toggleWindowButton = new()
            {
                Parent = cP,
                Width = ContentRegion.Width - 35,
                KeyBinding = Settings.ShortcutKey.Value,
            };
            _toggleWindowButton.BindingChanged += ToggleWindowButton_BindingChanged;
            _languageActions.Add(() => 
            { 
                _toggleWindowButton.KeyBindingName = Strings.common.ShortcutToggle_DisplayName;
                _toggleWindowButton.BasicTooltipText = Strings.common.ShortcutToggle_Description; 
            });
            #endregion Keybinds

            #region Behaviour
            p = new Panel()
            {
                Parent = _contentPanel,
                Location = new(0, p.LocalBounds.Bottom + 5),
                Width = ContentRegion.Width - 20,
                Height = 190,
                ShowBorder = true,
            };

            _ = new Image()
            {
                Texture = AsyncTexture2D.FromAssetId(157092),
                Parent = p,
                Size = new(30, 30),
            };

            var label2 = new Label()
            {
                Parent = p,
                AutoSizeWidth = true,
                Location = new(35, 0),
                Height = 30,
            };
            _languageActions.Add(() => label2.Text = Strings.common.SwapBehaviour);

            cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5, 35),
                HeightSizingMode = SizingMode.Fill,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            _closeOnSwap = new()
            {
                Parent = cP,
                Checked = Settings.CloseWindowOnSwap.Value,
            };
            _closeOnSwap.CheckedChanged += CloseOnSwap_CheckedChanged;
            _languageActions.Add(() => 
            {
                _closeOnSwap.Text = Strings.common.CloseWindowOnSwap_DisplayName;
                _closeOnSwap.BasicTooltipText = Strings.common.CloseWindowOnSwap_Description;
            });

            _loginAfterSelect = new()
            {
                Parent = cP,
                Checked = Settings.EnterOnSwap.Value,
            };
            _loginAfterSelect.CheckedChanged += LoginAfterSelect_CheckedChanged;
            _languageActions.Add(() => 
            {
                _loginAfterSelect.Text = Strings.common.EnterOnSwap_DisplayName;
                _loginAfterSelect.BasicTooltipText = Strings.common.EnterOnSwap_Description;
            });

            _doubleClickLogin = new()
            {
                Parent = cP,
                Checked = Settings.DoubleClickToEnter.Value,
            };
            _doubleClickLogin.CheckedChanged += DoubleClickLogin_CheckedChanged;
            _languageActions.Add(() => 
            {
                _doubleClickLogin.Text = Strings.common.DoubleClickToEnter_DisplayName;
                _doubleClickLogin.BasicTooltipText = Strings.common.DoubleClickToEnter_Description;
            });

            _enterLogin = new()
            {
                Parent = cP,
                Checked = Settings.EnterToLogin.Value,
            };
            _enterLogin.CheckedChanged += EnterLogin_CheckedChanged;
            _languageActions.Add(() => 
            {
                _enterLogin.Text = Strings.common.EnterToLogin_DisplayName;
                _enterLogin.BasicTooltipText = Strings.common.EnterToLogin_Description;
            });

            _useOCR = new()
            {
                Parent = cP,
                Checked = Settings.UseOCR.Value
            };
            _useOCR.CheckedChanged += UseOCR_CheckedChanged;
            _languageActions.Add(() => 
            {
                _useOCR.Text = Strings.common.UseOCR;
                _useOCR.BasicTooltipText = Strings.common.UseOCR_Tooltip;
            });

            _autoFix = new()
            {
                Parent = cP,
                Checked = Settings.AutoSortCharacters.Value
            };
            _autoFix.CheckedChanged += AutoFix_CheckedChanged;
            _languageActions.Add(() => 
            {
                _autoFix.Text = Strings.common.AutoFix;
                _autoFix.BasicTooltipText = Strings.common.AutoFix_Tooltip;
            });

            _ignoreDiacritics = new()
            {
                Parent = cP,
                Checked = Settings.FilterDiacriticsInsensitive.Value
            };
            _ignoreDiacritics.CheckedChanged += IgnoreDiacritics_CheckedChanged;
            _languageActions.Add(() => 
            {
                _ignoreDiacritics.Text = Strings.common.FilterDiacriticsInsensitive_DisplayName;
                _ignoreDiacritics.BasicTooltipText = Strings.common.FilterDiacriticsInsensitive_Description;
            });
            #endregion Behaviour

            #region Appearance
            p = new Panel()
            {
                Parent = _contentPanel,
                Location = new(0, p.LocalBounds.Bottom + 5),
                Width = ContentRegion.Width - 20,
                Height = 210,
                ShowBorder = true,
            };

            _ = new Image()
            {
                Texture = AsyncTexture2D.FromAssetId(156740),
                Parent = p,
                Size = new(30, 30),
            };

            var label3 = new Label()
            {
                Parent = p,
                AutoSizeWidth = true,
                Location = new(35, 0),
                Height = 30,
            };
            _languageActions.Add(() => label3.Text = Strings.common.Appearance);

            cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5, 35),
                HeightSizingMode = SizingMode.Fill,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };
            _windowMode = new()
            {
                Parent = cP,
                Checked = Settings.WindowedMode.Value,
            };
            _windowMode.CheckedChanged += WindowMode_CheckedChanged;
            _languageActions.Add(() =>
            {
                _windowMode.Text = Strings.common.WindowedMode;
                _windowMode.BasicTooltipText = Strings.common.WindowedMode_Tooltip;
            });

            _showCornerIcon = new()
            {
                Parent = cP,
                Checked = Settings.ShowCornerIcon.Value,
            };
            _showCornerIcon.CheckedChanged += ShowCornerIcon_CheckedChanged;
            _languageActions.Add(() => 
            {
                _showCornerIcon.Text = Strings.common.ShowCorner_Name;
                _showCornerIcon.BasicTooltipText = string.Format(Strings.common.ShowCorner_Tooltip, Characters.ModuleInstance.Name);
            });

            _showRandomButton = new()
            {
                Parent = cP,
                Checked = Settings.ShowRandomButton.Value,
            };
            _showRandomButton.CheckedChanged += ShowRandomButton_CheckedChanged;
            _languageActions.Add(() =>
            {
                _showRandomButton.Text = Strings.common.ShowRandomButton_Name;
                _showRandomButton.BasicTooltipText = Strings.common.ShowRandomButton_Description;
            });

            _showStatusPopup = new()
            {
                Parent = cP,
                Checked = Settings.ShowStatusWindow.Value
            };
            _showStatusPopup.CheckedChanged += ShowStatusPopup_CheckedChanged;
            _languageActions.Add(() =>
            {
                _showStatusPopup.Text = Strings.common.ShowStatusWindow_Name;
                _showStatusPopup.BasicTooltipText = Strings.common.ShowStatusWindow_Description;
            });

            _showTooltip = new()
            {
                Parent = cP,
                Checked = Settings.ShowStatusWindow.Value
            };
            _showTooltip.CheckedChanged += ShowTooltip_CheckedChanged;
            _languageActions.Add(() =>
            {
                _showTooltip.Text = string.Format(Strings.common.ShowItem, Strings.common.DetailedTooltip);
                _showTooltip.BasicTooltipText = Strings.common.DetailedTooltip_Description;
            });

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                Height = 30,
            };

            var label4 = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                Height = 30
            };
            _languageActions.Add(() => label4.Text = Strings.common.CharacterDisplayOption);

            _panelSize = new()
            {
                Location = new(250, 0),
                Parent = subP,
            };
            _panelSize.ValueChanged += PanelSize_ValueChanged;
            _languageActions.Add(() =>
            {
                _panelSize.Items.Clear();
                _panelSize.SelectedItem = Settings.PanelSize.Value.GetPanelSize();
                _panelSize.Items.Add(Strings.common.Small);
                _panelSize.Items.Add(Strings.common.Normal);
                _panelSize.Items.Add(Strings.common.Large);
            });

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                Height = 30,
            };

            var label5 = new Label()
            {
                Parent = subP,
                AutoSizeWidth = true,
                Height = 30
            };
            _languageActions.Add(() => label5.Text = Strings.common.CharacterDisplaySize);

            _panelAppearance = new()
            {
                Parent = subP,
                Location = new(250, 0),
            };
            _panelAppearance.ValueChanged += PanelAppearance_ValueChanged;
            _languageActions.Add(() =>
            {
                _panelAppearance.Items.Clear();
                _panelAppearance.SelectedItem = Settings.PanelLayout.Value.GetPanelLayout();
                _panelAppearance.Items.Add(Strings.common.OnlyText);
                _panelAppearance.Items.Add(Strings.common.OnlyIcons);
                _panelAppearance.Items.Add(Strings.common.TextAndIcon);
            });

            #endregion Appearance

            #region Delays
            p = new Panel()
            {
                Parent = _contentPanel,
                Location = new(0, p.LocalBounds.Bottom + 5),
                Width = ContentRegion.Width - 20,
                Height = 125,
                ShowBorder = true,
            };

            _ = new Image()
            {
                Texture = AsyncTexture2D.FromAssetId(155035),
                Parent = p,
                Size = new(30, 30),
            };

            var label6 = new Label()
            {
                Parent = p,
                AutoSizeWidth = true,
                Location = new(35, 0),
                Height = 30,
            };
            _languageActions.Add(() => label6.Text = Strings.common.Delays);

            cP = new FlowPanel()
            {
                Parent = p,
                Location = new(5, 35),
                HeightSizingMode = SizingMode.Fill,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new(3, 3),
            };

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            _keyDelayLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth= true,
                Height = 20,
            };
            _languageActions.Add(() =>
            {
                _keyDelayLabel.Text = string.Format(Strings.common.KeyDelay_DisplayName, Settings.KeyDelay.Value);
                _keyDelayLabel.BasicTooltipText = Strings.common.KeyDelay_Description;
            });
            _keyDelay = new()
            {
                Location = new(225, 2),
                Parent = subP,
                MinValue = 0,
                MaxValue = 500,
                Value = Settings.KeyDelay.Value,
                Width = ContentRegion.Width - 35 - 225,
            };
            _keyDelay.ValueChanged += KeyDelay_ValueChanged;

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            _filterDelayLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth= true,
                Height = 20,
            };
            _languageActions.Add(() =>
            {
                _filterDelayLabel.Text = string.Format(Strings.common.FilterDelay_DisplayName, Settings.FilterDelay.Value);
                _filterDelayLabel.BasicTooltipText = Strings.common.FilterDelay_Description;
            });
            _filterDelay = new()
            {                
                Location = new(225, 2),
                Parent = subP,
                MinValue = 0,
                MaxValue = 500,
                Value = Settings.FilterDelay.Value,
                Width = ContentRegion.Width - 35 - 225,
            };
            _filterDelay.ValueChanged += FilterDelay_ValueChanged;

            subP = new Panel()
            {
                Parent = cP,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            _loadingDelayLabel = new Label()
            {
                Parent = subP,
                AutoSizeWidth= true,
                Height = 20,
            };
            _languageActions.Add(() =>
            {
                _loadingDelayLabel.Text = string.Format(Strings.common.SwapDelay_DisplayName, Settings.SwapDelay.Value);
                _loadingDelayLabel.BasicTooltipText = Strings.common.SwapDelay_Description;
            });
            _loadingDelay = new()
            {
                Location = new(225, 2),
                Parent = subP,
                MinValue = 0,
                MaxValue = 5000,
                Value = Settings.SwapDelay.Value,
                Width = ContentRegion.Width - 35 - 225,
            };
            _loadingDelay.ValueChanged += LoadingDelay_ValueChanged;
            #endregion Delays

            Characters.ModuleInstance.LanguageChanged += OnLanguageChanged;
            OnLanguageChanged();
        }

        private void ShowTooltip_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.ShowDetailedTooltip.Value = _showTooltip.Checked;
        }

        private void WindowMode_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.WindowedMode.Value = _windowMode.Checked;
        }

        private void LoadingDelay_ValueChanged(object sender, ValueEventArgs<float> e)
        {
            Settings.SwapDelay.Value = (int)_loadingDelay.Value;
            _loadingDelayLabel.Text = string.Format(Strings.common.SwapDelay_DisplayName, Settings.SwapDelay.Value);
        }

        private void FilterDelay_ValueChanged(object sender, ValueEventArgs<float> e)
        {
            Settings.FilterDelay.Value = (int)_filterDelay.Value;
            _filterDelayLabel.Text = string.Format(Strings.common.FilterDelay_DisplayName, Settings.FilterDelay.Value);
        }

        private void KeyDelay_ValueChanged(object sender, ValueEventArgs<float> e)
        {
            Settings.KeyDelay.Value = (int)_keyDelay.Value;
            _keyDelayLabel.Text = string.Format(Strings.common.KeyDelay_DisplayName, Settings.KeyDelay.Value);
        }

        private void PanelAppearance_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Settings.PanelLayout.Value = _panelAppearance.SelectedItem.GetPanelLayout(); 
        }

        private void PanelSize_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Settings.PanelSize.Value = _panelSize.SelectedItem.GetPanelSize();
        }

        private void ShowStatusPopup_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.ShowStatusWindow.Value = _showStatusPopup.Checked;
        }

        private void ShowRandomButton_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.ShowRandomButton.Value = _showRandomButton.Checked;
        }

        private void ShowCornerIcon_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.ShowCornerIcon.Value = _showCornerIcon.Checked;
        }

        private void IgnoreDiacritics_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.FilterDiacriticsInsensitive.Value = _ignoreDiacritics.Checked;
        }

        private void AutoFix_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.AutoSortCharacters.Value = _autoFix.Checked;
        }

        private void UseOCR_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.UseOCR.Value = _useOCR.Checked;
        }

        private void EnterLogin_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.EnterToLogin.Value = _enterLogin.Checked;
        }

        private void DoubleClickLogin_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.DoubleClickToEnter.Value = _doubleClickLogin.Checked;
        }

        private void LoginAfterSelect_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.EnterOnSwap.Value = _loginAfterSelect.Checked;
        }

        private void CloseOnSwap_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Settings.CloseWindowOnSwap.Value = _closeOnSwap.Checked;
        }

        private void LogoutButton_BindingChanged(object sender, EventArgs e)
        {
            Settings.LogoutKey.Value = _logoutButton.KeyBinding;
        }

        private void ToggleWindowButton_BindingChanged(object sender, EventArgs e)
        {
            Settings.ShortcutKey.Value = _toggleWindowButton.KeyBinding;
        }

        private SettingsModel Settings => Characters.ModuleInstance.Settings;

        public void OnLanguageChanged(object s = null, EventArgs e = null)
        {
            foreach(var action in _languageActions)
            {
                action.Invoke();
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _subEmblemRectangle = new(-43 + 64, -58 + 64, 64, 64);
            _mainEmblemRectangle = new(-43, -58, 128, 128);

            var titleBounds = _titleFont.GetStringRectangle(string.Format(Strings.common.ItemSettings, $"{Characters.ModuleInstance.Name}"));
            _titleRectangle = new(80, 5, (int)titleBounds.Width, Math.Max(30, (int)titleBounds.Height));
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            spriteBatch.DrawOnCtrl(
                this,
                _mainWindowEmblem,
                _mainEmblemRectangle,
                _mainWindowEmblem.Bounds,
                Color.White,
                0f,
                default);

            spriteBatch.DrawOnCtrl(
                this,
                _subWindowEmblem,
                _subEmblemRectangle,
                _subWindowEmblem.Bounds,
                Color.White,
                0f,
                default);

            if (_titleRectangle.Width < bounds.Width - (_subEmblemRectangle.Width - 20))
            {
                spriteBatch.DrawStringOnCtrl(
                    this,
                    string.Format(Strings.common.ItemSettings, $"{Characters.ModuleInstance.Name}"),
                    _titleFont,
                    _titleRectangle,
                    ContentService.Colors.ColonialWhite, // new Color(247, 231, 182, 97),
                    false,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Bottom);
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Characters.ModuleInstance.LanguageChanged -= OnLanguageChanged;
        }
    }
}
