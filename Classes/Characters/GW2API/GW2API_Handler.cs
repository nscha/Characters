namespace Kenedia.Modules.Characters.Classes
{
    using System;
    using System.IO;
    using System.Linq;
    using Blish_HUD.Controls;
    using Gw2Sharp.Models;
    using Gw2Sharp.WebApi.V2.Models;

    public class GW2API_Handler
    {
        private Account account;

        public Account Account
        {
            get => this.account;
            set
            {
                if (value != null && (this.account == null || this.account.Name != value.Name))
                {
                    this.UpdateFolderPaths(value.Name);
                }

                this.account = value;
            }
        }

        public async void CheckAPI()
        {
            Characters.ModuleInstance.APISpinner?.Show();

            try
            {
                var gw2ApiManager = Characters.ModuleInstance.Gw2ApiManager;

                if (gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Characters }))
                {
                    var account = await gw2ApiManager.Gw2ApiClient.V2.Account.GetAsync();
                    this.Account = account;

                    var characters = await gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync();
                    var character_Models = Characters.ModuleInstance.CharacterModels;
                    var pos = 0;

                    // Cleanup
                    for (int i = character_Models.Count - 1; i >= 0; i--)
                    {
                        var c = character_Models[i];
                        var character = characters.ToList().Find(e => e.Name == c.Name);
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
                            var cModel = new Character_Model()
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
                    ScreenNotification.ShowNotification("API Key has invalid Permissions!", ScreenNotification.NotificationType.Error);
                    Characters.Logger.Error("This API Token has not the required permissions!");
                }
            }
            catch (Exception ex)
            {
                Characters.Logger.Warn(ex, "Failed to fetch characters from the API.");
            }

            Characters.ModuleInstance.APISpinner?.Hide();
        }

        private void UpdateFolderPaths(string accountName)
        {
            var mIns = Characters.ModuleInstance;
            var b = mIns.BasePath;

            mIns.AccountPath = b + @"\" + accountName;
            mIns.CharactersPath = b + @"\" + accountName + @"\characters.json";
            mIns.AccountInfoPath = b + @"\" + accountName + @"\account.json";
            mIns.AccountImagesPath = b + @"\" + accountName + @"\images\";

            if (!Directory.Exists(mIns.AccountPath))
            {
                Directory.CreateDirectory(mIns.AccountPath);
            }

            if (!Directory.Exists(mIns.AccountImagesPath))
            {
                Directory.CreateDirectory(mIns.AccountImagesPath);
            }

            if (Characters.ModuleInstance.CharacterModels.Count == 0)
            {
                Characters.ModuleInstance.LoadCharacterList();
            }
        }
    }
}
