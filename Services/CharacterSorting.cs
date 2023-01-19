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
        private double tick;
        private List<Character_Model> models;
        private SortingState state;
        private SortingState subState = SortingState.Selected;

        private int currentIndex = 0;

        public CharacterSorting(List<Character_Model> models)
        {
            this.models = models.OrderByDescending(e => e.LastLogin).ToList();
        }

        public event EventHandler Finished;

        public void Run(GameTime gameTime)
        {
            tick += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (tick < 0)
            {
                return;
            }

            if (currentIndex == models.Count && subState == SortingState.NameFetched)
            {
                ScreenNotification.ShowNotification(Strings.common.CharactersFixed, ScreenNotification.NotificationType.Warning);
                AdjustCharacterLogins();
                Finished?.Invoke(null, null);
            }

            switch (state)
            {
                case SortingState.None:
                    for (int i = 0; i < models.Count; i++)
                    {
                        Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
                    }

                    state = SortingState.MovedToFirst;
                    tick -= 250;
                    break;

                case SortingState.MovedToFirst:
                    if (subState == SortingState.Selected)
                    {
                        string name = Characters.ModuleInstance.OCR.Read();

                        if (name != null)
                        {
                            Character_Model c = models.Find(e => e.Name == name);

                            if (c != null)
                            {
                                c.OrderIndex = currentIndex;
                            }
                            else
                            {
                                ScreenNotification.ShowNotification(string.Format(Strings.common.CouldNotFindNamedItem, Strings.common.Character, name));
                            }

                            subState = SortingState.NameFetched;
                        }
                    }
                    else
                    {
                        Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
                        subState = SortingState.Selected;
                        tick -= 250;
                        currentIndex++;
                    }

                    break;

                case SortingState.Done:
                    break;
            }
        }

        private void AdjustCharacterLogins()
        {
            models = models.OrderBy(e => e.OrderIndex).ToList();

            bool messedUp = true;

            while (messedUp)
            {
                messedUp = false;

                for (int i = 0; i < models.Count; i++)
                {
                    Character_Model next = models.Count > i + 1 ? models[i + 1] : null;
                    Character_Model current = models[i];

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
