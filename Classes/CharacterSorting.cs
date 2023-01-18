namespace Kenedia.Modules.Characters.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Blish_HUD.Controls;
    using Blish_HUD.Controls.Extern;
    using Microsoft.Xna.Framework;

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
        private List<Character_Model> _models;
        private SortingState State;
        private SortingState SubState = SortingState.Selected;

        private int _currentIndex = 0;

        public event EventHandler Finished;

        public CharacterSorting(List<Character_Model> models)
        {
            this._models = models.OrderByDescending(e => e.LastLogin).ToList();
        }

        private void AdjustCharacterLogins()
        {
            this._models = this._models.OrderBy(e => e.OrderIndex).ToList();

            var messedUp = true;

            while (messedUp)
            {
                messedUp = false;

                for (int i = 0; i < this._models.Count; i++)
                {
                    var next = this._models.Count > i + 1 ? this._models[i + 1] : null;
                    var current = this._models[i];

                    var nCurr = String.Format("Current: {0} | LastLogin: {1} | More Recent: {2}", current.Name, current.LastLogin, next != null && current.LastLogin <= next.LastLogin);
                    var nNext = String.Format("Next: {0} | LastLogin: {1} | More Recent: {2}", next != null ? next.Name : "No Next", next != null ? next.LastLogin : "No Next", next != null && current.LastLogin <= next.LastLogin);

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
            this.tick += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (this.tick < 0)
            {
                return;
            }

            if (this._currentIndex == this._models.Count && this.SubState == SortingState.NameFetched)
            {
                ScreenNotification.ShowNotification("Characters fixed!", ScreenNotification.NotificationType.Warning);
                this.AdjustCharacterLogins();
                this.Finished?.Invoke(null, null);
            }

            switch (this.State)
            {
                case SortingState.None:
                    for (int i = 0; i < this._models.Count; i++)
                    {
                        Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
                    }
                    this.State = SortingState.MovedToFirst;
                    this.tick -= 250;
                    break;

                case SortingState.MovedToFirst:
                    if (this.SubState == SortingState.Selected)
                    {
                        var name = Characters.ModuleInstance.OCR.Read();

                        if (name != null)
                        {
                            var c = this._models.Find(e => e.Name == name);

                            if (c != null)
                            {
                                c.OrderIndex = this._currentIndex;
                            }
                            else
                            {
                                ScreenNotification.ShowNotification("Could not find any Character with name: " + name);
                            }

                            this.SubState = SortingState.NameFetched;
                        }
                    }
                    else
                    {
                        Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
                        this.SubState = SortingState.Selected;
                        this.tick -= 250;
                        this._currentIndex++;
                    }
                    break;

                case SortingState.Done:
                    break;
            }
        }
    }
}
