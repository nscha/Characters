using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Extern;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.Characters.Classes
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
        double Tick;
        public SwappingState State = SwappingState.None;
        public SwappingState SubState = SwappingState.None;
        public Character_Model Character;
        public event EventHandler Succeeded;
        public event EventHandler Failed;
        
        public CharacterSwapping(Character_Model character_Model)
        {
            Character = character_Model;
        }
        public void Reset()
        {
            State = SwappingState.None;
            Tick = 0;
        }

        bool LoggingOut()
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

                //Trigger other Modules such as GatherTools
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

        bool MoveToFirstCharacter()
        {
            for (int i = 0; i < Characters.ModuleInstance.Character_Models.Count; i++)
            {
                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
            }

            return true;
        }

        bool MoveToCharacter()
        {
            var order = Characters.ModuleInstance.Character_Models.OrderByDescending(e => e.LastLogin);
            foreach (Character_Model c in order)
            {
                if (c == Character) break;
                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
            }

            return true;
        }

        bool ConfirmName()
        {
            var ocr_result = Characters.ModuleInstance.Settings._UseOCR.Value ? Characters.ModuleInstance.OCR.Read() : "No OCR";

            if(Characters.ModuleInstance.Settings._UseOCR.Value) Characters.Logger.Debug($"OCR Result: {ocr_result}.");

            return !Characters.ModuleInstance.Settings._UseOCR.Value || Character.Name.ToLower() == ocr_result.ToLower();
        }

        bool Login()
        {
            if (Characters.ModuleInstance.Settings.EnterOnSwap.Value) Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RETURN, false);
            return true; 
        }

        bool IsLoaded()
        {
            return GameService.GameIntegration.Gw2Instance.IsInGame;
        }

        public void Run(GameTime gameTime)
        {
            Tick += gameTime.ElapsedGameTime.TotalMilliseconds;
            //Characters.Logger.Debug(State.ToString());

            if (Tick < 0) return;

            switch (State)
            {
                case SwappingState.None:
                    if (LoggingOut())
                    {
                        State = SwappingState.LoggedOut;
                        Tick = -Characters.ModuleInstance.Settings.SwapDelay.Value;
                    }
                    else
                    {
                        Tick = -1000;
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
                        Tick = -750;
                    }
                    break;

                case SwappingState.MovedToCharacter:
                    if (ConfirmName())
                    {
                        State = SwappingState.CharacterFound;
                    }
                    else
                    {
                        State = SwappingState.CharacterLost;
                    }
                    break;

                case SwappingState.CharacterRead:
                    break;

                case SwappingState.CharacterFound:
                    if (Login())
                    {
                        State = SwappingState.LoggingIn;
                        Tick = -1000;
                    }
                    break;

                case SwappingState.CharacterLost:
                    switch (SubState)
                    {
                        case SwappingState.None:
                            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
                            SubState = SwappingState.MovedLeft;
                            Tick = -750;
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
                            Tick = -750;
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
                    this.Failed?.Invoke(null, null);
                    break;
                case SwappingState.LoggingIn:
                    if (IsLoaded())
                    {
                        State = SwappingState.Done;
                    }
                    break;
                case SwappingState.Done:
                    Character.LastLogin = DateTime.UtcNow;
                    this.Succeeded?.Invoke(null, null);
                    break;
            }
        }
    }
}
