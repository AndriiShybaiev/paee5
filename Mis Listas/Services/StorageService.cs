using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Newtonsoft.Json;
using Mis_Listas.Models;

namespace Mis_Listas.Services
{
    public static class StorageService
    {
        private const string FileName = "data.json";
        private const string BackupFileName = "datos.json.old";

        public static async Task<List<cList>> LoadDataAsync()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            // 1. Try reading from LocalFolder
            try
            {
                IStorageItem item = await localFolder.TryGetItemAsync(FileName);
                if (item is StorageFile localFile)
                {
                    string json = await FileIO.ReadTextAsync(localFile);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var result = JsonConvert.DeserializeObject<List<cList>>(json);
                        if (result != null && result.Count > 0)
                        {
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StorageService] Error reading local data.json: {ex.Message}");
            }

            // 2. Try reading from backup file
            try
            {
                IStorageItem backupItem = await localFolder.TryGetItemAsync(BackupFileName);
                if (backupItem is StorageFile backupFile)
                {
                    string json = await FileIO.ReadTextAsync(backupFile);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var result = JsonConvert.DeserializeObject<List<cList>>(json);
                        if (result != null && result.Count > 0)
                        {
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StorageService] Error reading backup file: {ex.Message}");
            }

            // 3. Fallback to bundled seed in Assets/Data/data.json
            try
            {
                StorageFile seedFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Data/data.json"));
                string json = await FileIO.ReadTextAsync(seedFile);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var result = JsonConvert.DeserializeObject<List<cList>>(json);
                    if (result != null)
                    {
                        // Save initial copy to LocalFolder
                        await SaveDataAsync(result);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StorageService] Error reading bundled seed data: {ex.Message}");
            }

            // 4. Default in-memory seed
            return GetDefaultLists();
        }

        public static async Task<bool> SaveDataAsync(List<cList> lists)
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                string serialized = JsonConvert.SerializeObject(lists, Formatting.Indented);

                // Create backup of existing data file before overwriting
                try
                {
                    IStorageItem existingItem = await localFolder.TryGetItemAsync(FileName);
                    if (existingItem is StorageFile existingFile)
                    {
                        await existingFile.CopyAsync(localFolder, BackupFileName, NameCollisionOption.ReplaceExisting);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[StorageService] Warning creating backup: {ex.Message}");
                }

                // Write new data
                StorageFile dataFile = await localFolder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(dataFile, serialized);

                // Attempt to update package directory if available (as shown in course slides)
                try
                {
                    StorageFolder installedFolder = Package.Current.InstalledLocation;
                    StorageFolder assetsFolder = await installedFolder.GetFolderAsync(@"Assets\Data");
                    await dataFile.CopyAsync(assetsFolder, FileName, NameCollisionOption.ReplaceExisting);
                }
                catch
                {
                    // In packaged UWP, installed directory is typically read-only at runtime
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StorageService] Error saving data: {ex.Message}");
                return false;
            }
        }

        private static List<cList> GetDefaultLists()
        {
            return new List<cList>
            {
                new cList
                {
                    id = 1,
                    name = "Lista de tareas",
                    text = "Lista de tareas",
                    color = "Orange",
                    visible = true,
                    date = DateTime.Now,
                    tasks = new List<cTask>
                    {
                        new cTask
                        {
                            text = "Tarea de bienvenida",
                            name = "Tarea de bienvenida",
                            date = DateTime.Now,
                            finished = false,
                            visible = true
                        }
                    }
                }
            };
        }
    }
}
