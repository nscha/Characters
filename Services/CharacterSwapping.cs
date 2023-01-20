using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Extern;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;

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
        Canceled,
    }

    public static class CharacterSwapping
    {
        private static CancellationTokenSource s_cancellationTokenSource;

        private static int s_movedLeft;

        public static event EventHandler Succeeded;
        public static event EventHandler Failed;

        public static event EventHandler Started;
        public static event EventHandler Finished;

        public static event EventHandler StatusChanged;

        private static string s_status;

        private static SwappingState s_state = SwappingState.None;

        public static string Status
        {
            set
            {
                s_status = value;
                StatusChanged?.Invoke(null, EventArgs.Empty);
            }
            get => s_status;
        }

        public static Character_Model Character { get; set; }

        private static bool IsTaskCanceled(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (s_state is not SwappingState.LoggedOut) { s_movedLeft = 0; };
                if (s_state is SwappingState.MovedToStart) { s_movedLeft = Characters.ModuleInstance.CharacterModels.Count; };

                s_state = SwappingState.Canceled;
                return true;
            }

            return false;
        }

        public static async Task MoveRight(CancellationToken cancellationToken)
        {
            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
            await Delay(cancellationToken);
            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
            await Delay(cancellationToken);
        }

        public static async Task MoveLeft(CancellationToken cancellationToken)
        {
            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
            await Delay(cancellationToken);
        }

        public static async Task Run(CancellationToken cancellationToken)
        {
            if (IsTaskCanceled(cancellationToken)) { return; }

            Debug.WriteLine($"s_state: {s_state}");

            switch (s_state)
            {
                case SwappingState.None:
                    if (await LoggingOut(cancellationToken))
                    {
                        s_state = SwappingState.LoggedOut;
                    }

                    await Delay(cancellationToken, Characters.ModuleInstance.Settings.SwapDelay.Value);
                    await Delay(cancellationToken);
                    break;

                case SwappingState.LoggedOut:
                    await MoveToFirstCharacter(cancellationToken);
                    s_state = SwappingState.MovedToStart;
                    s_movedLeft = 0;
                    await Delay(cancellationToken, 250);
                    break;

                case SwappingState.MovedToStart:
                    await MoveToCharacter(cancellationToken);
                    s_state = SwappingState.MovedToCharacter;
                    await Delay(cancellationToken, 250);

                    break;

                case SwappingState.MovedToCharacter:
                    if (ConfirmName())
                    {
                        s_state = SwappingState.CharacterFound;
                    }
                    else
                    {
                        s_state = SwappingState.CharacterLost;

                        await MoveLeft(cancellationToken);
                        await Delay(cancellationToken, 250);
                        if (ConfirmName())
                        {
                            s_state = SwappingState.CharacterFound;
                            return;
                        }

                        await MoveRight(cancellationToken);
                        await Delay(cancellationToken, 250);
                        if (ConfirmName())
                        {
                            s_state = SwappingState.CharacterFound;
                            return;
                        }

                        s_state = SwappingState.CharacterFullyLost;
                    }

                    break;

                case SwappingState.CharacterFound:
                    await Login(cancellationToken);
                    s_state = SwappingState.LoggingIn;
                    break;

                case SwappingState.LoggingIn:
                    if (IsLoaded())
                    {
                        s_state = SwappingState.Done;
                        return;
                    }

                    await Delay(cancellationToken, 500);
                    break;
            }
        }

        public static void Reset()
        {
            s_state = SwappingState.None;
        }

        public static void Cancel()
        {
            s_state = SwappingState.Canceled;
            s_cancellationTokenSource?.Cancel();
            s_cancellationTokenSource = null;
        }

        public static async void Start(Character_Model character)
        {
            s_cancellationTokenSource?.Cancel();
            s_cancellationTokenSource = new();
            s_cancellationTokenSource.CancelAfter(60000);

            Character = character;
            s_state = GameService.GameIntegration.Gw2Instance.IsInGame ? SwappingState.None : SwappingState.LoggedOut;

            Started?.Invoke(null, null);
            Status = $"Switching to {character.Name}";
            while (s_state is not SwappingState.Done and not SwappingState.CharacterFullyLost and not SwappingState.Canceled && !s_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await Run(s_cancellationTokenSource.Token);

                    switch (s_state)
                    {
                        case SwappingState.Done:
                            Status = $"Done!";
                            Succeeded?.Invoke(null, null);
                            break;

                        case SwappingState.CharacterFullyLost:
                            Status = $"Failed to swap to {Character.Name}!";
                            ScreenNotification.ShowNotification(Status);
                            Failed?.Invoke(null, null);

                            if (Characters.ModuleInstance.Settings.AutoSortCharacters.Value)
                            {
                                ScreenNotification.ShowNotification("Fixing Characters!");
                                CharacterSorting.Start(Characters.ModuleInstance.CharacterModels);
                            }

                            break;
                    }
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

        private static async Task<bool> LoggingOut(CancellationToken cancellationToken)
        {
            if (IsTaskCanceled(cancellationToken)) { return false; }

            if (GameService.GameIntegration.Gw2Instance.IsInGame)
            {
                Status = $"Logging out ...";
                ModifierKeys mods = ModifierKeys.None;
                VirtualKeyShort primary = (VirtualKeyShort)Characters.ModuleInstance.Settings.LogoutKey.Value.PrimaryKey;

                foreach (ModifierKeys mod in Enum.GetValues(typeof(ModifierKeys)))
                {
                    if (mod != ModifierKeys.None && mods.HasFlag(mod))
                    {
                        Blish_HUD.Controls.Intern.Keyboard.Press(Characters.ModKeyMapping[(int)mod], false);
                        if (IsTaskCanceled(cancellationToken)) { return false; }
                    }
                }

                Blish_HUD.Controls.Intern.Keyboard.Stroke(primary, false);
                await Delay(cancellationToken);

                // Trigger other Modules such as GatherTools
                Blish_HUD.Controls.Intern.Keyboard.Stroke(primary, true);

                foreach (ModifierKeys mod in Enum.GetValues(typeof(ModifierKeys)))
                {
                    if (mod != ModifierKeys.None && mods.HasFlag(mod))
                    {
                        Blish_HUD.Controls.Intern.Keyboard.Release(Characters.ModKeyMapping[(int)mod], false);
                        if (IsTaskCanceled(cancellationToken)) { return false; }
                    }
                }

                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RETURN, false);
                await Delay(cancellationToken);
            }

            return !GameService.GameIntegration.Gw2Instance.IsInGame;
        }

        private static async Task MoveToFirstCharacter(CancellationToken cancellationToken)
        {
            Status = $"Move to first character ...";
            if (IsTaskCanceled(cancellationToken)) { return; }

            Stopwatch stopwatch = Stopwatch.StartNew();
            int moves = Characters.ModuleInstance.CharacterModels.Count - s_movedLeft;
            for (int i = 0; i < moves; i++)
            {
                if (stopwatch.ElapsedMilliseconds > 5000)
                {
                    InputService.MouseWiggle();
                    stopwatch.Restart();
                }

                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
                s_movedLeft++;
                await Delay(cancellationToken);
                if (IsTaskCanceled(cancellationToken)) { return; }
            }

            return;
        }

        private static async Task MoveToCharacter(CancellationToken cancellationToken)
        {
            Status = $"Try to move to {Character.Name}";
            if (IsTaskCanceled(cancellationToken)) { return; }

            List<Character_Model> order = Characters.ModuleInstance.CharacterModels.OrderByDescending(e => e.LastLogin).ToList();

            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (Character_Model character in order)
            {
                if (character == Character)
                {
                    break;
                }

                if (stopwatch.ElapsedMilliseconds > 5000)
                {
                    InputService.MouseWiggle();
                    stopwatch.Restart();
                }

                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
                await Delay(cancellationToken);
                if (IsTaskCanceled(cancellationToken)) { return; }
            }

            return;
        }

        private static bool ConfirmName()
        {
            string ocr_result = Characters.ModuleInstance.Settings.UseOCR.Value ? Characters.ModuleInstance.OCR.Read() : "No OCR";

            if (Characters.ModuleInstance.Settings.UseOCR.Value)
            {
                Status = $"Confirm name ..." + Environment.NewLine + $"{ocr_result}";
                Characters.Logger.Debug($"OCR Result: {ocr_result}.");
            }

            return !Characters.ModuleInstance.Settings.UseOCR.Value || Character.Name.ToLower() == ocr_result.ToLower();
        }

        private static async Task Login(CancellationToken cancellationToken)
        {
            if (IsTaskCanceled(cancellationToken)) { return; }

            if (Characters.ModuleInstance.Settings.EnterOnSwap.Value)
            {
                Status = $"Login to {Character.Name}";
                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RETURN, false);
                await Delay(cancellationToken);
                await Delay(cancellationToken, 1000);
            }

            return;
        }

        private static bool IsLoaded()
        {
            return !Characters.ModuleInstance.Settings.EnterOnSwap.Value || GameService.GameIntegration.Gw2Instance.IsInGame;
        }
    }
}
