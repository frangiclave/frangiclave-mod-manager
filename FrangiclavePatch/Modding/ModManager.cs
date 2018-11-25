using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Frangiclave.Modding
{
    public class ModManager
    {
        private const string ModsFolderName = "mods";

        private const string ModManifestFileName = "manifest.json";

        private readonly HashSet<string> _entityCategories = new HashSet<string>
        {
            "decks",
            "elements",
            "endings",
            "legacies",
            "maps",
            "recipes",
            "verbs"
        };

        private readonly HashSet<string> _imagesDirectories = new HashSet<string>
        {
            "cardBacks/",
            "elementArt/",
            "elementArt/anim/",
            "endingArt/",
            "icons40/aspects/",
            "icons100/legacies/",
            "icons100/verbs/",
            "maps/",
            "maps/portals/"
        };

        private readonly string _modsFolder;

        public Dictionary<string, Mod> Mods { get; }

        public ModManager()
        {
            Mods = new Dictionary<string, Mod>();
            _modsFolder = Path.Combine(Application.streamingAssetsPath, ModsFolderName);
        }

        public void LoadAll()
        {
            Logging.Info("Loading all mods");
            Mods.Clear();

            // Check if the mods folder exists
            if (!Directory.Exists(_modsFolder))
            {
                Directory.CreateDirectory(_modsFolder);
                Logging.Warn("Mods folder not found, creating");
            }

            // Load the mod data from the file system
            foreach (var modFolder in Directory.GetDirectories(_modsFolder))
            {
                var modId = Path.GetFileName(modFolder);
                if (modId == null)
                {
                    Logging.Info("Unexpected null directory name for mod");
                    continue;
                }
                Logging.Info("Loading mod " + modId);

                // Find the mod's manifest and load its data
                var manifestPath = Path.Combine(modFolder, ModManifestFileName);
                if (!File.Exists(manifestPath))
                {
                    Logging.Error("Mod manifest not found, skipping mod");
                    continue;
                }
                var manifestData = SimpleJsonImporter.Import(File.ReadAllText(manifestPath));
                if (manifestData == null)
                {
                    Logging.Error("Invalid mod manifest JSON, skipping mod");
                    continue;
                }

                // Initialize the mod with its manifest information
                var mod = new Mod(modId);
                var errors = mod.FromManifest(manifestData);
                if (errors.Count > 0)
                {
                    foreach (var error in errors)
                    {
                        Logging.Error(error);
                    }
                    Logging.Error("Encountered errors in manifest, skipping mod");
                    continue;
                }

                // Collect the mod's content files
                // If an error occurs in the process, discard the mod
                if (!LoadContentDirectory(mod, Path.Combine(modFolder, "content")))
                {
                    Logging.Error("Encountered errors in content, skipping mod");
                    continue;
                }

                // Collect the mod's images
                // If an error occurs in the process, discard the mod
                if (!LoadAllImagesDirectory(mod, Path.Combine(modFolder, "images")))
                {
                    Logging.Error("Encountered errors in images, skipping mod");
                    continue;
                }

                // Add the mod to the collection
                Logging.Info($"Loaded mod '{modId}'");
                Mods.Add(modId, mod);
            }
            Logging.Info("Loaded all mods");

            // Check the dependencies to see if there are any missing or invalid ones
            foreach (var mod in Mods)
            {
                foreach (var dependency in mod.Value.Dependencies)
                {
                    if (!Mods.ContainsKey(dependency.ModId))
                    {
                        Logging.Warn($"Dependency '{dependency.ModId}' for '{mod.Key}' not found ");
                    }
                    else
                    {
                        var availableVersion = Mods[dependency.ModId].Version;
                        bool isVersionValid;
                        switch (dependency.VersionOperator)
                        {
                            case DependencyOperator.LessThan:
                                isVersionValid = availableVersion < dependency.Version;
                                break;
                            case DependencyOperator.LessThanOrEqual:
                                isVersionValid = availableVersion <= dependency.Version;
                                break;
                            case DependencyOperator.GreaterThan:
                                isVersionValid = availableVersion > dependency.Version;
                                break;
                            case DependencyOperator.GreaterThanOrEqual:
                                isVersionValid = availableVersion >= dependency.Version;
                                break;
                            case DependencyOperator.Equal:
                                isVersionValid = availableVersion == dependency.Version;
                                break;
                            default:
                                isVersionValid = true;
                                break;
                        }

                        if (!isVersionValid)
                        {
                            Logging.Warn($"Dependency '{dependency.ModId}' for '{mod.Key}' has incompatible version");
                        }
                    }
                }
            }
        }

        public IEnumerable<Hashtable> GetContentForCategory(string category)
        {
            var categoryContent = new List<Hashtable>();
            foreach (var mod in Mods)
            {
                if (mod.Value.Contents.ContainsKey(category))
                {
                    categoryContent.AddRange(mod.Value.Contents[category]);
                }
            }

            return categoryContent;
        }

        public Sprite GetSprite(string spriteResourceName)
        {
            foreach (var mod in Mods.Values)
            {
                if (mod.Images.ContainsKey(spriteResourceName))
                {
                    return mod.Images[spriteResourceName];
                }
            }

            return null;
        }

        private bool LoadContentDirectory(Mod mod, string contentDirectoryPath)
        {
            // Check if there is a `content` directory first, but don't require one for the mod to be valid
            if (!Directory.Exists(contentDirectoryPath))
            {
                Logging.Warn("No content directory found; content files must be placed in a 'content' subdirectory");
                return true;
            }

            // Search the directory for content files
            foreach (var contentFileName in Directory.GetFiles(contentDirectoryPath, "*.json"))
            {
                var contentFileData = SimpleJsonImporter.Import(File.ReadAllText(contentFileName));
                if (contentFileData == null)
                {
                    Logging.Error("Invalid content file JSON '" + Path.GetFileName(contentFileName) + "'");
                    return false;
                }

                foreach (DictionaryEntry contentEntry in contentFileData)
                {
                    var category = contentEntry.Key as string;
                    if (!(contentEntry.Value is ArrayList items))
                    {
                        Logging.Warn($"Unexpected type for items in category '{category}', should be array");
                        continue;
                    }
                    if (!_entityCategories.Contains(category))
                    {
                        Logging.Warn($"Invalid content category '{category}', ignoring");
                        continue;
                    }
                    mod.AddContent(category, items);
                }
            }

            // Search all subdirectories for more content files
            return Directory.GetDirectories(contentDirectoryPath).All(
                contentSubDirectoryPath => LoadContentDirectory(mod, contentSubDirectoryPath));
        }

        private bool LoadAllImagesDirectory(Mod mod, string imagesDirectoryPath)
        {
            // Search all subdirectories for more image files
            return _imagesDirectories.All(
                imageSubDirectoryPath => LoadImagesDirectory(mod, imagesDirectoryPath, imageSubDirectoryPath));
        }

        private static bool LoadImagesDirectory(Mod mod, string imagesDirectoryPath, string imagesSubdirectory)
        {
            // Check if the directory exists, otherwise don't try looking for images in it
            var imagesSubdirectoryPath = Path.Combine(imagesDirectoryPath, imagesSubdirectory);
            if (!Directory.Exists(imagesSubdirectoryPath))
            {
                return true;
            }

            // Load all PNG images into memory
            // This may incur a performance hit - a better system may be needed later
            foreach (var imagePath in Directory.GetFiles(imagesSubdirectoryPath, "*.png"))
            {
                var fileResourceName = imagesSubdirectory + Path.GetFileNameWithoutExtension(imagePath);
                Logging.Info($"Loading image '{fileResourceName}'");
                var fileData = File.ReadAllBytes(imagePath);

                // Try to load the image data into a sprite
                Sprite sprite;
                try
                {
                    var texture = new Texture2D(2, 2);
                    texture.LoadImage(fileData);
                    sprite = Sprite.Create(
                        texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
                catch
                {
                    Logging.Error($"Invalid image file '{fileResourceName}'");
                    return false;
                }

                mod.Images.Add(fileResourceName, sprite);
            }
            return true;
        }
    }
}
