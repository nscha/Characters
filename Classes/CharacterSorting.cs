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
            this.tick += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (this.tick < 0)
            {
                return;
            }

            if (this.currentIndex == this.models.Count && this.subState == SortingState.NameFetched)
            {
                ScreenNotification.ShowNotification("Characters fixed!", ScreenNotification.NotificationType.Warning);
                this.AdjustCharacterLogins();
                this.Finished?.Invoke(null, null);
            }

            switch (this.state)
            {
                case SortingState.None:
                    for (int i = 0; i < this.models.Count; i++)
                    {
                        Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
                    }

                    this.state = SortingState.MovedToFirst;
                    this.tick -= 250;
                    break;

                case SortingState.MovedToFirst:
                    if (this.subState == SortingState.Selected)
                    {
                        var name = Characters.ModuleInstance.OCR.Read();

                        if (name != null)
                        {
                            var c = this.models.Find(e => e.Name == name);

                            if (c != null)
                            {
                                c.OrderIndex = this.currentIndex;
                            }
                            else
                            {
                                ScreenNotification.ShowNotification("Could not find any Character with name: " + name);
                            }

                            this.subState = SortingState.NameFetched;
                        }
                    }
                    else
                    {
                        Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
                        this.subState = SortingState.Selected;
                        this.tick -= 250;
                        this.currentIndex++;
                    }

                    break;

                case SortingState.Done:
                    break;
            }
        }

        private void AdjustCharacterLogins()
        {
            this.models = this.models.OrderBy(e => e.OrderIndex).ToList();

            var messedUp = true;

            while (messedUp)
            {
                messedUp = false;

                for (int i = 0; i < this.models.Count; i++)
                {
                    var next = this.models.Count > i + 1 ? this.models[i + 1] : null;
                    var current = this.models[i];

                    var nCurr = string.Format("Current: {0} | LastLogin: {1} | More Recent: {2}", current.Name, current.LastLogin, next != null && current.LastLogin <= next.LastLogin);
                    var nNext = string.Format("Next: {0} | LastLogin: {1} | More Recent: {2}", next != null ? next.Name : "No Next", next != null ? next.LastLogin : "No Next", next != null && current.LastLogin <= next.LastLogin);

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
    }
}
