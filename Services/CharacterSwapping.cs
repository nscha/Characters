using Blish_HUD;
using Blish_HUD.Controls.Extern;
using Kenedia.Modules.Characters.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace Kenedia.Modules.Characters.Services
{
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
        private double _tick;

        public CharacterSwapping(Character_Model character_Model)
        {
            Character = character_Model;
        }

        public event EventHandler Succeeded;

        public event EventHandler Failed;

        public SwappingState State { get; set; } = SwappingState.None;

        public SwappingState SubState { get; set; } = SwappingState.None;

        public Character_Model Character { get; set; }

        public void Run(GameTime gameTime)
        {
            _tick += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_tick < 0)
            {
                return;
            }

            switch (State)
            {
                case SwappingState.None:
                    if (LoggingOut())
                    {
                        State = SwappingState.LoggedOut;
                        _tick = -Characters.ModuleInstance.Settings.SwapDelay.Value;
                    }
                    else
                    {
                        _tick = -1000;
                    }

                    break;

                case SwappingState.LoggedOut:
                    if (MoveToFirstCharacter())
                    {
                        State = SwappingState.MovedToStart;
                    }

                    break;

                case SwappingState.MovedToStart:
                    if (MoveToCharacter())
                    {
                        State = SwappingState.MovedToCharacter;
                        _tick = -750;
                    }

                    break;

                case SwappingState.MovedToCharacter:
                    State = ConfirmName() ? SwappingState.CharacterFound : SwappingState.CharacterLost;
                    break;

                case SwappingState.CharacterRead:
                    break;

                case SwappingState.CharacterFound:
                    if (Login())
                    {
                        State = SwappingState.LoggingIn;
                        _tick = -1000;
                    }

                    break;

                case SwappingState.CharacterLost:
                    switch (SubState)
                    {
                        case SwappingState.None:
                            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
                            SubState = SwappingState.MovedLeft;
                            _tick = -750;
                            break;

                        case SwappingState.MovedLeft:
                            if (ConfirmName())
                            {
                                State = SwappingState.CharacterFound;
                            }
                            else
                            {
                                SubState = SwappingState.CheckedLeft;
                            }

                            break;

                        case SwappingState.CheckedLeft:
                            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
                            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
                            _tick = -750;
                            SubState = SwappingState.MovedRight;
                            break;

                        case SwappingState.MovedRight:
                            if (ConfirmName())
                            {
                                State = SwappingState.CharacterFound;
                            }
                            else
                            {
                                SubState = SwappingState.CheckedRight;
                                State = SwappingState.CharacterFullyLost;
                            }

                            break;
                    }

                    break;

                case SwappingState.CharacterFullyLost:
                    Failed?.Invoke(null, null);
                    break;
                case SwappingState.LoggingIn:
                    if (IsLoaded())
                    {
                        State = SwappingState.Done;
                    }

                    break;
                case SwappingState.Done:
                    Character.LastLogin = DateTime.UtcNow;
                    Succeeded?.Invoke(null, null);
                    break;
            }
        }

        public void Reset()
        {
            State = SwappingState.None;
            _tick = 0;
        }

        private bool LoggingOut()
        {
            if (GameService.GameIntegration.Gw2Instance.IsInGame)
            {
                ModifierKeys mods = ModifierKeys.None;
                VirtualKeyShort primary = (VirtualKeyShort)Characters.ModuleInstance.Settings.LogoutKey.Value.PrimaryKey;

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
            IOrderedEnumerable<Character_Model> order = Characters.ModuleInstance.CharacterModels.OrderByDescending(e => e.LastLogin);
            foreach (Character_Model c in order)
            {
                if (c == Character)
                {
                    break;
                }

                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
            }

            return true;
        }

        private bool ConfirmName()
        {
            string ocr_result = Characters.ModuleInstance.Settings.UseOCR.Value ? Characters.ModuleInstance.OCR.Read() : "No OCR";

            if (Characters.ModuleInstance.Settings.UseOCR.Value)
            {
                Characters.Logger.Debug($"OCR Result: {ocr_result}.");
            }

            return !Characters.ModuleInstance.Settings.UseOCR.Value || Character.Name.ToLower() == ocr_result.ToLower();
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
