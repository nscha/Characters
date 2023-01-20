using Blish_HUD.Controls;
using Blish_HUD.Controls.Extern;
using Kenedia.Modules.Characters.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kenedia.Modules.Characters.Services
{
    public enum SortingState
    {
        None,
        MovedToFirst,
        Selected,
        FirstNameFetched,
        Done,
        Canceled,
    }

    public static class CharacterSorting
    {
        private static CancellationTokenSource s_cancellationTokenSource;
        private static List<Character_Model> s_models;
        private static SortingState s_state;
        private static string s_lastName;
        private static int s_noNameChange = 0;

        private static int NoNameChange
        {
            get => s_noNameChange;
            set
            {
                s_noNameChange = value;
                if (s_noNameChange > 0)
                {
                    Status = $"Character name did not change {s_noNameChange}/2 ...";
                }

                if (s_noNameChange >= 2)
                {
                    s_state = SortingState.Done;
                    Status = "Done!";
                    AdjustCharacterLogins();
                    Completed?.Invoke(null, null);
                }
            }
        }

        private static int s_currentIndex = 0;

        public static event EventHandler Started;
        public static event EventHandler Completed;
        public static event EventHandler Finished;
        public static event EventHandler StatusChanged;

        private static string s_status;
        public static string Status
        {
            set
            {
                s_status = value;
                StatusChanged?.Invoke(null, EventArgs.Empty);
            }
            get => s_status;
        }

        public static void Cancel()
        {
            s_state = SortingState.Canceled;
            s_cancellationTokenSource?.Cancel();
            s_cancellationTokenSource = null;
        }

        public static async void Start(List<Character_Model> models)
        {
            s_state = SortingState.Canceled;
            s_cancellationTokenSource?.Cancel();
            s_cancellationTokenSource = new();
            s_cancellationTokenSource.CancelAfter(180000);

            s_models = models.OrderByDescending(e => e.LastLogin).ToList();
            s_lastName = string.Empty;
            s_state = SortingState.None;

            Started?.Invoke(null, null);
            Status = "Fixing characters ...";
            int i = 0;
            while (s_state is not SortingState.Done and not SortingState.Canceled && !s_cancellationTokenSource.Token.IsCancellationRequested)
            {
                i++;

                try
                {
                    await Run(s_cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {

                }
            }

            Finished?.Invoke(null, null);
        }


        private static async Task Delay(CancellationToken cancellationToken, int? delay = null)
        {
            delay ??= Characters.ModuleInstance.Settings.KeyDelay.Value;

            if (delay > 0)
            {
                await Task.Delay(delay.Value, cancellationToken);
            }
        }

        public static async Task Run(CancellationToken cancellationToken)
        {
            string name;

            if (cancellationToken.IsCancellationRequested) return;

            switch (s_state)
            {
                case SortingState.None:
                    await MoveFirst(cancellationToken);
                    break;

                case SortingState.MovedToFirst:
                    name = await FetchName(cancellationToken);
                    if (name == s_lastName)
                    {
                        NoNameChange++;
                    }
                    else
                    {
                        NoNameChange = 0;
                    }

                    s_lastName = name;
                    break;

                case SortingState.FirstNameFetched:
                    await MoveNext(cancellationToken);

                    name = await FetchName(cancellationToken);
                    if (name == s_lastName)
                    {
                        NoNameChange++;
                    }
                    else
                    {
                        NoNameChange = 0;
                    }

                    s_lastName = name;

                    await Delay(cancellationToken, 250);
                    break;
            }
        }

        private static async Task MoveFirst(CancellationToken cancellationToken)
        {
            Status = "Move to first character ...";
            if (IsTaskCanceled(cancellationToken)) { return; }

            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < s_models.Count; i++)
            {
                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
                await Delay(cancellationToken);

                if (IsTaskCanceled(cancellationToken)) { return; }

                if (stopwatch.ElapsedMilliseconds > 5000)
                {
                    InputService.MouseWiggle();
                    stopwatch.Restart();
                }
            }

            s_state = SortingState.MovedToFirst;
            s_currentIndex = 0;
        }

        private static async Task MoveNext(CancellationToken cancellationToken)
        {
            Status = "Move to next character ...";
            InputService.MouseWiggle();
            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
            await Delay(cancellationToken);
            s_currentIndex++;
        }

        private static async Task<string> FetchName(CancellationToken cancellationToken)
        {
            string name = Characters.ModuleInstance.OCR.Read();

            Status = "Fetch the character name ..." + Environment.NewLine + $"{name}";

            if (name != null)
            {
                Character_Model c = s_models.Find(e => e.Name == name);

                if (c != null)
                {
                    c.OrderIndex = s_currentIndex;
                }
                else
                {
                    Status = string.Format(Strings.common.CouldNotFindNamedItem, Strings.common.Character, name);
                }

                await Delay(cancellationToken);
            }

            if (s_state == SortingState.MovedToFirst)
            {
                s_state = SortingState.FirstNameFetched;
            }

            return name;
        }

        private static bool IsTaskCanceled(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                s_state = SortingState.Canceled;
                return true;
            }

            return false;
        }

        private static void AdjustCharacterLogins()
        {
            s_models = s_models.OrderBy(e => e.OrderIndex).ToList();

            bool messedUp = true;

            while (messedUp)
            {
                messedUp = false;

                for (int i = 0; i < s_models.Count; i++)
                {
                    Character_Model next = s_models.Count > i + 1 ? s_models[i + 1] : null;
                    Character_Model current = s_models[i];

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
