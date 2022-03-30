using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kenedia.Modules.Characters
{
    public partial class Module : Blish_HUD.Modules.Module
    {
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

                    CharactersPath = path + @"\characters.json";
                    AccountPath = path + @"\account.json";

                    if (userAccount == null)
                    {
                        userAccount = new AccountInfo()
                        {
                            Name = account.Name,
                            LastModified = account.LastModified,
                        };
                    }

                    if (System.IO.File.Exists(AccountPath))
                    {
                        requestAPI = false;
                        List<AccountInfo> accountInfos = JsonConvert.DeserializeObject<List<AccountInfo>>(System.IO.File.ReadAllText(AccountPath));

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
                        //ScreenNotification.ShowNotification("Updating Characters!", ScreenNotification.NotificationType.Warning);
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
                        //ScreenNotification.ShowNotification("Characters Updated!", ScreenNotification.NotificationType.Warning);
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
