using Blish_HUD.Controls;
using Blish_HUD.Controls.Extern;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.Characters.Classes
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
        double Tick;
        List<Character_Model> _models;
        SortingState State;
        SortingState SubState = SortingState.Selected;

        int _currentIndex = 0;
        DateTime _curDateTime;

        public event EventHandler Finished;
        public CharacterSorting(List<Character_Model> models)
        {
            _models = models.OrderByDescending(e => e.LastLogin).ToList();
        }
        void AdjustCharacterLogins()
        {
            _models = _models.OrderBy(e => e.OrderIndex).ToList();

            var messedUp = true;

            while (messedUp)
            {
                messedUp = false;

                for (int i = 0; i < _models.Count; i++)
                {
                    var next = _models.Count > i + 1 ? _models[i + 1] : null;
                    var current = _models[i];

                    var nCurr = String.Format("Current: {0} | LastLogin: {1} | More Recent: {2}", current.Name, current.LastLogin, (next != null && current.LastLogin <= next.LastLogin));
                    var nNext = String.Format("Next: {0} | LastLogin: {1} | More Recent: {2}", next != null ? next.Name : "No Next", next != null ? next.LastLogin : "No Next", (next != null && current.LastLogin <= next.LastLogin));

                    Characters.Logger.Debug(nCurr);
                    Characters.Logger.Debug(nNext + Environment.NewLine);

                    if (next != null && current.LastLogin <= next.LastLogin)
                    {
                        current.LastLogin = next.LastLogin.AddSeconds(1);
                        messedUp = true;
                    }
                }
            }
        }

        public void Run(GameTime gameTime)
        {
            Tick += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (Tick < 0) return;
            if (_currentIndex == _models.Count && SubState == SortingState.NameFetched)
            {
                ScreenNotification.ShowNotification("Characters fixed!", ScreenNotification.NotificationType.Warning);
                AdjustCharacterLogins();
                this.Finished?.Invoke(null, null);
            }

            switch (State)
            {
                case SortingState.None:
                    for (int i = 0; i < _models.Count; i++)
                    {
                        Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
                    }
                    State = SortingState.MovedToFirst;
                    Tick -= 250;
                    break;

                case SortingState.MovedToFirst:
                    if (SubState == SortingState.Selected)
                    {
                        var name = Characters.ModuleInstance.OCR.Read();

                        if (name != null)
                        {
                            var c = _models.Find(e => e.Name == name);

                            if (c != null)
                            {
                                c.OrderIndex = _currentIndex;
                            }
                            else
                            {
                                ScreenNotification.ShowNotification("Could not find any Character with name: " + name);
                            }

                            SubState = SortingState.NameFetched;
                        }
                    }
                    else
                    {
                        Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
                        SubState = SortingState.Selected;
                        Tick -= 250;
                        _currentIndex++;
                    }
                    break;

                case SortingState.Done:
                    break;
            }
        }
    }
}
