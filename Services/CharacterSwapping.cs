namespace Kenedia.Modules.Characters.Services
{
    using System;
    using System.Linq;
    using Blish_HUD;
    using Blish_HUD.Controls.Extern;
    using Kenedia.Modules.Characters.Models;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;

    public enum SwappingState
    {
        None,
        LoggedOut,
        MovedToStart,
        MovedToCharacter,
        CharacterRead,
        CharacterFound,
        CharacterLost,
        MovedLeft,
        CheckedLeft,
        MovedRight,
        CheckedRight,
        CharacterFullyLost,
        LoggingIn,
        Done,
    }

    public class CharacterSwapping
    {
        private double tick;

        public CharacterSwapping(Character_Model character_Model)
        {
            this.Character = character_Model;
        }

        public event EventHandler Succeeded;

        public event EventHandler Failed;

        public SwappingState State { get; set; } = SwappingState.None;

        public SwappingState SubState { get; set; } = SwappingState.None;

        public Character_Model Character { get; set; }

        public void Run(GameTime gameTime)
        {
            this.tick += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (this.tick < 0)
            {
                return;
            }

            switch (this.State)
            {
                case SwappingState.None:
                    if (this.LoggingOut())
                    {
                        this.State = SwappingState.LoggedOut;
                        this.tick = -Characters.ModuleInstance.Settings.SwapDelay.Value;
                    }
                    else
                    {
                        this.tick = -1000;
                    }

                    break;

                case SwappingState.LoggedOut:
                    if (this.MoveToFirstCharacter())
                    {
                        this.State = SwappingState.MovedToStart;
                    }

                    break;

                case SwappingState.MovedToStart:
                    if (this.MoveToCharacter())
                    {
                        this.State = SwappingState.MovedToCharacter;
                        this.tick = -750;
                    }

                    break;

                case SwappingState.MovedToCharacter:
                    if (this.ConfirmName())
                    {
                        this.State = SwappingState.CharacterFound;
                    }
                    else
                    {
                        this.State = SwappingState.CharacterLost;
                    }

                    break;

                case SwappingState.CharacterRead:
                    break;

                case SwappingState.CharacterFound:
                    if (this.Login())
                    {
                        this.State = SwappingState.LoggingIn;
                        this.tick = -1000;
                    }

                    break;

                case SwappingState.CharacterLost:
                    switch (this.SubState)
                    {
                        case SwappingState.None:
                            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
                            this.SubState = SwappingState.MovedLeft;
                            this.tick = -750;
                            break;

                        case SwappingState.MovedLeft:
                            if (this.ConfirmName())
                            {
                                this.State = SwappingState.CharacterFound;
                            }
                            else
                            {
                                this.SubState = SwappingState.CheckedLeft;
                            }

                            break;

                        case SwappingState.CheckedLeft:
                            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
                            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
                            this.tick = -750;
                            this.SubState = SwappingState.MovedRight;
                            break;

                        case SwappingState.MovedRight:
                            if (this.ConfirmName())
                            {
                                this.State = SwappingState.CharacterFound;
                            }
                            else
                            {
                                this.SubState = SwappingState.CheckedRight;
                                this.State = SwappingState.CharacterFullyLost;
                            }

                            break;
                    }

                    break;

                case SwappingState.CharacterFullyLost:
                    this.Failed?.Invoke(null, null);
                    break;
                case SwappingState.LoggingIn:
                    if (this.IsLoaded())
                    {
                        this.State = SwappingState.Done;
                    }

                    break;
                case SwappingState.Done:
                    this.Character.LastLogin = DateTime.UtcNow;
                    this.Succeeded?.Invoke(null, null);
                    break;
            }
        }

        public void Reset()
        {
            this.State = SwappingState.None;
            this.tick = 0;
        }

        private bool LoggingOut()
        {
            if (GameService.GameIntegration.Gw2Instance.IsInGame)
            {
                var mods = ModifierKeys.None;
                var primary = (VirtualKeyShort)Characters.ModuleInstance.Settings.LogoutKey.Value.PrimaryKey;

                foreach (ModifierKeys mod in Enum.GetValues(typeof(ModifierKeys)))
                {
                    if (mod != ModifierKeys.None && mods.HasFlag(mod))
                    {
                        Blish_HUD.Controls.Intern.Keyboard.Press(Characters.ModKeyMapping[(int)mod], false);
                    }
                }

                Blish_HUD.Controls.Intern.Keyboard.Stroke(primary, false);

                // Trigger other Modules such as GatherTools
                Blish_HUD.Controls.Intern.Keyboard.Stroke(primary, true);

                foreach (ModifierKeys mod in Enum.GetValues(typeof(ModifierKeys)))
                {
                    if (mod != ModifierKeys.None && mods.HasFlag(mod))
                    {
                        Blish_HUD.Controls.Intern.Keyboard.Release(Characters.ModKeyMapping[(int)mod], false);
                    }
                }

                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RETURN, false);
            }

            return !GameService.GameIntegration.Gw2Instance.IsInGame;
        }

        private bool MoveToFirstCharacter()
        {
            for (int i = 0; i < Characters.ModuleInstance.CharacterModels.Count; i++)
            {
                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
            }

            return true;
        }

        private bool MoveToCharacter()
        {
            var order = Characters.ModuleInstance.CharacterModels.OrderByDescending(e => e.LastLogin);
            foreach (Character_Model c in order)
            {
                if (c == this.Character)
                {
                    break;
                }

                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
            }

            return true;
        }

        private bool ConfirmName()
        {
            var ocr_result = Characters.ModuleInstance.Settings.UseOCR.Value ? Characters.ModuleInstance.OCR.Read() : "No OCR";

            if (Characters.ModuleInstance.Settings.UseOCR.Value)
            {
                Characters.Logger.Debug($"OCR Result: {ocr_result}.");
            }

            return !Characters.ModuleInstance.Settings.UseOCR.Value || this.Character.Name.ToLower() == ocr_result.ToLower();
        }

        private bool Login()
        {
            if (Characters.ModuleInstance.Settings.EnterOnSwap.Value)
            {
                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RETURN, false);
            }

            return true;
        }

        private bool IsLoaded()
        {
            return GameService.GameIntegration.Gw2Instance.IsInGame;
        }
    }
}
