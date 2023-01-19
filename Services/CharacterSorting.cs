using Blish_HUD.Controls;
using Blish_HUD.Controls.Extern;
using Kenedia.Modules.Characters.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kenedia.Modules.Characters.Services
{
    public enum SortingState
    {
        None,
        MovedToFirst,
        Selected,
        NameFetched,
        Done,
    }

    public class CharacterSorting
    {
        private double _tick;
        private List<Character_Model> _models;
        private SortingState _state;
        private SortingState _subState = SortingState.Selected;

        private int _currentIndex = 0;

        public CharacterSorting(List<Character_Model> models)
        {
            this._models = models.OrderByDescending(e => e.LastLogin).ToList();
        }

        public event EventHandler Finished;

        public void Run(GameTime gameTime)
        {
            _tick += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_tick < 0)
            {
                return;
            }

            if (_currentIndex == _models.Count && _subState == SortingState.NameFetched)
            {
                ScreenNotification.ShowNotification(Strings.common.CharactersFixed, ScreenNotification.NotificationType.Warning);
                AdjustCharacterLogins();
                Finished?.Invoke(null, null);
            }

            switch (_state)
            {
                case SortingState.None:
                    for (int i = 0; i < _models.Count; i++)
                    {
                        Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
                    }

                    _state = SortingState.MovedToFirst;
                    _tick -= 250;
                    break;

                case SortingState.MovedToFirst:
                    if (_subState == SortingState.Selected)
                    {
                        string name = Characters.ModuleInstance.OCR.Read();

                        if (name != null)
                        {
                            Character_Model c = _models.Find(e => e.Name == name);

                            if (c != null)
                            {
                                c.OrderIndex = _currentIndex;
                            }
                            else
                            {
                                ScreenNotification.ShowNotification(string.Format(Strings.common.CouldNotFindNamedItem, Strings.common.Character, name));
                            }

                            _subState = SortingState.NameFetched;
                        }
                    }
                    else
                    {
                        Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
                        _subState = SortingState.Selected;
                        _tick -= 250;
                        _currentIndex++;
                    }

                    break;

                case SortingState.Done:
                    break;
            }
        }

        private void AdjustCharacterLogins()
        {
            _models = _models.OrderBy(e => e.OrderIndex).ToList();

            bool messedUp = true;

            while (messedUp)
            {
                messedUp = false;

                for (int i = 0; i < _models.Count; i++)
                {
                    Character_Model next = _models.Count > i + 1 ? _models[i + 1] : null;
                    Character_Model current = _models[i];

                    // var nCurr = string.Format("Current: {0} | LastLogin: {1} | More Recent: {2}", current.Name, current.LastLogin, next != null && current.LastLogin <= next.LastLogin);
                    // var nNext = string.Format("Next: {0} | LastLogin: {1} | More Recent: {2}", next != null ? next.Name : "No Next", next != null ? next.LastLogin : "No Next", next != null && current.LastLogin <= next.LastLogin);

                    // Characters.Logger.Debug(nCurr);
                    // Characters.Logger.Debug(nNext + Environment.NewLine);
                    if (next != null && current.LastLogin <= next.LastLogin)
                    {
                        current.LastLogin = next.LastLogin.AddSeconds(1);
                        messedUp = true;
                    }
                }
            }
        }
    }
}
