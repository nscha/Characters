using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kenedia.Modules.Characters
{
    public static class ExtsionMethods {
        public static bool IsValidJson(this string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = Newtonsoft.Json.Linq.JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
    public partial class Module : Blish_HUD.Modules.Module
    {
        private string LoadFile(string path, List<string> filesToDelete)
        {
            var txt = "";

            {
                try
                {
                    txt = System.IO.File.ReadAllText(path);
                }
                catch (InvalidOperationException)
                {
                    if (System.IO.File.Exists(path)) filesToDelete.Add(path);
                }
                catch (UnauthorizedAccessException)
                {

                }
                catch (FileNotFoundException)
                {
                }
                catch (FileLoadException)
                {

                }
            }

            return txt;
        }

        public async void FetchAPI(bool force = false)
        {
            try
            {
                if (Gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Characters }) && API_Account != null && userAccount != null)
                {
                    //ScreenNotification.ShowNotification("Updating Account ....", ScreenNotification.NotificationType.Warning);
                    var account = await Gw2ApiManager.Gw2ApiClient.V2.Account.GetAsync();

                    userAccount.LastModified = account.LastModified;
                    userAccount.Save();

                    ///character.LastModified older than account.LastModified --> character.LastModified = account.LastModified.UtcDateTime.AddSeconds(-j)
                    ///character.LastModified younger than account.LastModified --> character.LastModified = character.LastModified

                    if (userAccount.CharacterUpdateNeeded() || force)
                    {
                        userAccount.LastBlishUpdate = userAccount.LastBlishUpdate > account.LastModified ? userAccount.LastBlishUpdate : account.LastModified;
                        userAccount.Save();

                        var characters = await Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync();
                        Logger.Debug("Updating Characters ....");
                        //ScreenNotification.ShowNotification("Updating Characters ....", ScreenNotification.NotificationType.Warning);
                        Character last = null;
                        int j = 0;

                        foreach (Gw2Sharp.WebApi.V2.Models.Character c in characters)
                        {
                            Character character = getCharacter(c.Name);

                            character.Name = character.Name ?? c.Name;
                            character.Race = (RaceType)Enum.Parse(typeof(RaceType), c.Race);
                            character._Profession = (int)Enum.Parse(typeof(Professions), c.Profession.ToString());
                            character.Profession = (ProfessionType)Enum.Parse(typeof(ProfessionType), c.Profession.ToString());
                            character._Specialization = character._Specialization > -1 ? character._Specialization : -1;
                            character.Level = c.Level;
                            character.Created = c.Created;
                            character.contentsManager = ContentsManager;
                            character.apiManager = Gw2ApiManager;

                            character.Crafting = new List<CharacterCrafting>();

                            foreach (CharacterCraftingDiscipline disc in c.Crafting.ToList())
                            {
                                character.Crafting.Add(new CharacterCrafting()
                                {
                                    Id = (int)disc.Discipline.Value,
                                    Rating = disc.Rating,
                                    Active = disc.Active,
                                });
                            }
                            character.apiIndex = j;

                            if (character.LastModified == dateZero || character.LastModified < account.LastModified.UtcDateTime)
                            {
                                character.LastModified = account.LastModified.UtcDateTime.AddSeconds(-j);
                            }

                            if (character.lastLogin == dateZero)
                            {
                                character.lastLogin = c.LastModified.UtcDateTime;
                            }

                            last = character;
                            j++;
                        }

                        if (last != null) last.Save();

                        filterCharacterPanel = true;
                        //ScreenNotification.ShowNotification("Characters Updated!", ScreenNotification.NotificationType.Warning);
                        Logger.Debug("Characters Updated!");
                    }

                    double lastModified = DateTimeOffset.UtcNow.Subtract(userAccount.LastModified).TotalSeconds;
                    double lastUpdate = DateTimeOffset.UtcNow.Subtract(userAccount.LastBlishUpdate).TotalSeconds;
                    infoImage.BasicTooltipText = "Last Modified: " + Math.Round(lastModified) + Environment.NewLine + "Last Blish Login: " + Math.Round(lastUpdate);
                }
                else
                {
                    ScreenNotification.ShowNotification(Strings.common.Error_Competivive, ScreenNotification.NotificationType.Error);
                    Logger.Error("This API Token has not the required permissions!");
                }
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to fetch characters from the API.");
            }
        }
        private async void Gw2ApiManager_SubtokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            try
            {
                Logger.Debug("API Subtoken Updated!");
                if (Gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Characters }))
                {
                    var account = await Gw2ApiManager.Gw2ApiClient.V2.Account.GetAsync();

                    API_Account = account;
                    string path = DirectoriesManager.GetFullDirectoryPath("characters") + @"\" + API_Account.Name;
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    AccountPath = path;
                    CharactersPath = path + @"\characters.json";
                    AccountInfoPath = path + @"\account.json";
                    AccountImagesPath = path + @"\images\";
                    if (!Directory.Exists(AccountImagesPath))
                    {
                        Directory.CreateDirectory(AccountImagesPath);
                    }

                    if (userAccount == null)
                    {
                        userAccount = new AccountInfo()
                        {
                            Name = account.Name,
                            LastModified = account.LastModified,
                        };
                    }

                    List<string> filesToDelete = new List<string>();

                    if (System.IO.File.Exists(AccountInfoPath))
                    {
                        requestAPI = false;

                        var text = LoadFile(AccountInfoPath, filesToDelete);
                        if(text == "" || !text.IsValidJson())
                        {
                            ScreenNotification.ShowNotification("Failed to load the account information file. Retry in 1 minute!", ScreenNotification.NotificationType.Error);
                            Logger.Warn("The file '{0}' could not be loaded. Deleting it.", AccountInfoPath);

                            try
                            {
                                System.IO.File.Delete(AccountInfoPath);
                            }
                            catch (IOException)
                            {
                            }
                            charactersLoaded_Failed = true;
                            return;
                        }

                        List<AccountInfo> accountInfos = new List<AccountInfo>();

                        try
                        {
                           accountInfos = JsonConvert.DeserializeObject<List<AccountInfo>>(text);
                        }
                        catch(Exception ex)
                        {
                            ScreenNotification.ShowNotification("Failed to load the account information file. Retry in 1 minute!", ScreenNotification.NotificationType.Error);
                            Logger.Warn(ex, "The file '{0}' could not be loaded. Deleting it.");
                            try
                            {
                                System.IO.File.Delete(AccountInfoPath);
                            }
                            catch (IOException)
                            {
                            }
                            charactersLoaded_Failed = true;
                            return;
                        }

                        foreach (AccountInfo acc in accountInfos)
                        {
                            if (acc.Name == account.Name)
                            {
                                userAccount.LastBlishUpdate = acc.LastBlishUpdate;
                                break;
                            }
                        }
                    }

                    LoadCharacterList();

                    if (userAccount.CharacterUpdateNeeded())
                    {
                        userAccount.LastBlishUpdate = account.LastModified;
                        userAccount.Save();

                        Logger.Debug("Updating Characters ....");
                        Logger.Debug("The last API modification is more recent than our last local data track.");
                        var characters = await Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync();
                        Character last = null;
                        int j = 0;

                        foreach (Gw2Sharp.WebApi.V2.Models.Character c in characters)
                        {
                            Character character = getCharacter(c.Name);
                            character.Name = character.Name ?? c.Name;
                            character.Race = (RaceType)Enum.Parse(typeof(RaceType), c.Race);
                            character._Profession = (int)Enum.Parse(typeof(Professions), c.Profession.ToString());
                            character.Profession = (ProfessionType)Enum.Parse(typeof(ProfessionType), c.Profession.ToString());
                            character._Specialization = character._Specialization > -1 ? character._Specialization : -1;
                            character.Level = c.Level;
                            character.Created = c.Created;
                            character.apiIndex = j;

                            if (character.LastModified == dateZero || character.LastModified < account.LastModified.UtcDateTime)
                            {
                                character.LastModified = account.LastModified.UtcDateTime.AddSeconds(-j);
                            }

                            if (character.lastLogin == dateZero)
                            {
                                character.lastLogin = c.LastModified.UtcDateTime;
                            }

                            character.contentsManager = ContentsManager;
                            character.apiManager = Gw2ApiManager;

                            character.Crafting = new List<CharacterCrafting>();

                            foreach (CharacterCraftingDiscipline disc in c.Crafting.ToList())
                            {
                                character.Crafting.Add(new CharacterCrafting()
                                {
                                    Id = (int)disc.Discipline.Value,
                                    Rating = disc.Rating,
                                    Active = disc.Active,
                                });
                            }

                            if (!CharacterNames.Contains(c.Name))
                            {
                                CharacterNames.Add(c.Name);
                                Characters.Add(character);
                            }

                            last = character;
                            j++;
                        }

                        if (last != null) last.Save();
                    }

                    var player = GameService.Gw2Mumble.PlayerCharacter;
                    foreach (Character character in Characters)
                    {
                        character.Create_UI_Elements();
                        if (player != null && player.Name == character.Name) Current.character = character;
                    }

                    charactersLoaded = true;
                    filterCharacterPanel = true;

                    double lastModified = DateTimeOffset.UtcNow.Subtract(userAccount.LastModified).TotalSeconds;
                    double lastUpdate = DateTimeOffset.UtcNow.Subtract(userAccount.LastBlishUpdate).TotalSeconds;
                    infoImage.BasicTooltipText = "Last Modified: " + Math.Round(lastModified) + Environment.NewLine + "Last Blish Login: " + Math.Round(lastUpdate);
                }
                else
                {
                    ScreenNotification.ShowNotification(Strings.common.Error_InvalidPermissions, ScreenNotification.NotificationType.Error);
                    Logger.Error("This API Token has not the required permissions!");
                    // You don't actually have permission
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to fetch characters from the API.");

            }
        }
    }
}
