using Blish_HUD.Controls;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Enums;
using Kenedia.Modules.Characters.Models;
using System;
using System.IO;
using System.Linq;

namespace Kenedia.Modules.Characters.Services
{
    public class GW2API_Handler
    {
        private Account account;

        public Account Account
        {
            get => account;
            set
            {
                if (value != null && (account == null || account.Name != value.Name))
                {
                    UpdateFolderPaths(value.Name);
                }

                account = value;
            }
        }

        public async void CheckAPI()
        {
            Characters.ModuleInstance.APISpinner?.Show();

            try
            {
                Blish_HUD.Modules.Managers.Gw2ApiManager gw2ApiManager = Characters.ModuleInstance.Gw2ApiManager;

                if (gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Characters }))
                {
                    Account account = await gw2ApiManager.Gw2ApiClient.V2.Account.GetAsync();
                    Account = account;

                    Gw2Sharp.WebApi.V2.IApiV2ObjectList<Character> characters = await gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync();
                    System.Collections.Generic.List<Character_Model> character_Models = Characters.ModuleInstance.CharacterModels;
                    int pos = 0;

                    // Cleanup
                    for (int i = character_Models.Count - 1; i >= 0; i--)
                    {
                        Character_Model c = character_Models[i];
                        Character character = characters.ToList().Find(e => e.Name == c.Name);
                        if (character == null || character.Created != c.Created)
                        {
                            character_Models[i].Delete();
                        }
                    }

                    foreach (Character c in characters)
                    {
                        Character_Model character_Model = character_Models.Find(e => e.Name == c.Name);
                        if (character_Model == null || character_Model.Created != c.Created)
                        {
                            if (character_Model != null && character_Model.Created != c.Created)
                            {
                                // Delete the old model!
                                character_Model.Delete();
                            }

                            // Create New Entry
                            Character_Model cModel = new()
                            {
                                Name = c.Name,
                                Level = c.Level,
                                Race = (RaceType)Enum.Parse(typeof(RaceType), c.Race),
                                Profession = (ProfessionType)Enum.Parse(typeof(ProfessionType), c.Profession),
                                Specialization = SpecializationType.None,
                                Created = c.Created,
                                LastModified = c.LastModified.UtcDateTime,
                                LastLogin = c.LastModified.UtcDateTime,
                                Position = pos,
                            };

                            foreach (CharacterCraftingDiscipline disc in c.Crafting.ToList())
                            {
                                cModel.Crafting.Add(new CharacterCrafting()
                                {
                                    Id = (int)disc.Discipline.Value,
                                    Rating = disc.Rating,
                                    Active = disc.Active,
                                });
                            }

                            cModel.Initialize();
                            character_Models.Add(cModel);
                        }
                        else
                        {
                            // Update Character
                            character_Model.Position = pos;
                            character_Model.LastModified = c.LastModified.UtcDateTime;
                            character_Model.LastLogin = c.LastModified.UtcDateTime > character_Model.LastLogin ? c.LastModified.UtcDateTime : character_Model.LastLogin;
                        }

                        pos++;
                    }

                    Characters.ModuleInstance.CreateCharacterControls();
                }
                else
                {
                    ScreenNotification.ShowNotification(Strings.common.Error_InvalidPermissions, ScreenNotification.NotificationType.Error);
                    Characters.Logger.Error(Strings.common.Error_InvalidPermissions);
                }
            }
            catch (Exception ex)
            {
                Characters.Logger.Warn(ex, Strings.common.Error_FailedAPIFetch);
            }

            Characters.ModuleInstance.APISpinner?.Hide();
        }

        private void UpdateFolderPaths(string accountName)
        {
            Characters mIns = Characters.ModuleInstance;
            string b = mIns.BasePath;

            mIns.AccountPath = b + @"\" + accountName;
            mIns.CharactersPath = b + @"\" + accountName + @"\characters.json";
            mIns.AccountInfoPath = b + @"\" + accountName + @"\account.json";
            mIns.AccountImagesPath = b + @"\" + accountName + @"\images\";

            if (!Directory.Exists(mIns.AccountPath))
            {
                _ = Directory.CreateDirectory(mIns.AccountPath);
            }

            if (!Directory.Exists(mIns.AccountImagesPath))
            {
                _ = Directory.CreateDirectory(mIns.AccountImagesPath);
            }

            if (Characters.ModuleInstance.CharacterModels.Count == 0)
            {
                _ = Characters.ModuleInstance.LoadCharacterList();
            }
        }
    }
}
