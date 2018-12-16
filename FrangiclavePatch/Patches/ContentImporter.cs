using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Assets.CS.TabletopUI;
using Frangiclave.Modding;
using Frangiclave.Multiplayer;
using MonoMod;
using OrbCreationExtensions;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace Frangiclave.Patches
{
    [MonoModPatch("global::ContentImporter")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ContentImporter : global::ContentImporter
    {
        [MonoModIgnore]
        [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
        private new List<Recipe> Recipes;

        private extern int orig_PopulateElements(ArrayList alElements);

        public new int PopulateElements(ArrayList alElements)
        {
            Hashtable mpMortal = new Hashtable
            {
                {"id", "mp.mortal"},
                {"label", "Mortal"},
                {"description", "'The long habit of living indisposeth us for dying.' - Thomas Browne"},
                {"isAspect", "true"},
                {"icon", "mortal"}
            };
            alElements.Insert(0, mpMortal);
            Hashtable mpPresence = new Hashtable
            {
                {"id", MultiplayerClient.PresenceId},
                {"label", "A Presence"},
                {"description", "Unnamed, unseen, but nonetheless, here they are."},
                {"unique", "true"},
                {
                    "aspects", new Hashtable()
                    {
                        {"mp.mortal", 1}
                    }
                },
                {"icon", "ritual_defense"}
            };
            alElements.Insert(1, mpPresence);
            return orig_PopulateElements(alElements);
        }

        private extern void orig_PopulateRecipeList(ArrayList importedRecipes);

        public new void PopulateRecipeList(ArrayList importedRecipes)
        {
            // Set up dictionaries to hold the additional properties for each recipe
            // This has to be done before because the original function will delete the ID first otherwise,
            // making it impossible to re-associate the property with the recipe.
            var recipeMaps = new Dictionary<string, string>();
            var remoteAlternatives = new Dictionary<string, List<bool>>();
            for (var i = 0; i < importedRecipes.Count; i++)
            {
                var recipeData = importedRecipes.GetHashtable(i);
                var id = recipeData["id"].ToString();
                if (recipeData.ContainsKey("map"))
                {
                    recipeMaps[id] = recipeData["map"].ToString();
                    recipeMaps.Remove("map");
                    recipeData.Remove("map");
                }

                if (recipeData.ContainsKey("alternativerecipes"))
                {
                    remoteAlternatives[id] = new List<bool>();
                    foreach (Hashtable ra in recipeData.GetArrayList("alternativerecipes"))
                        remoteAlternatives[id].Add(ra.GetBool("remote"));
                }
            }

            // Import all the recipes
            orig_PopulateRecipeList(importedRecipes);

            // Add the map IDs
            foreach (var recipe in Recipes)
            {
                if (recipeMaps.ContainsKey(recipe.Id))
                {
                    var moddedRecipe = recipe;
                    moddedRecipe.MapId = recipeMaps[recipe.Id];
                }

                if (remoteAlternatives.ContainsKey(recipe.Id))
                    for (int i = 0; i < remoteAlternatives[recipe.Id].Count; i++)
                        recipe.AlternativeRecipes[i].Remote = remoteAlternatives[recipe.Id][i];
            }
        }

        private extern void orig_PopulateCompendium(ICompendium compendium);

        public new void PopulateCompendium(ICompendium compendium)
        {
            // Populate the compendium with the original content data mixed in with the modded data
            // The mix will be performed by GetContentItems
            orig_PopulateCompendium(compendium);

            // Handle the endings and maps separately, since there is no built-in support for custom endings yet
            var modManager = Registry.Retrieve<ModManager>();
            var moddedCompendium = (Compendium) compendium;
            moddedCompendium.UpdateEndings(modManager.GetContentForCategory("endings"));
            moddedCompendium.UpdateMaps(modManager.GetContentForCategory("maps"));
        }

        private extern ArrayList orig_GetContentItems(string contentOfType);

        private ArrayList GetContentItems(string contentOfType)
        {
            var modManager = Registry.Retrieve<ModManager>();
            var items = orig_GetContentItems(contentOfType);
            var moddedItems = modManager.GetContentForCategory(contentOfType);
            foreach (var moddedItem in moddedItems)
            {
                var moddedItemId = moddedItem.GetString("id");

                // Check if this is deleting an existing item
                if (moddedItem.GetBool("deleted"))
                {
                    // Try to find an item with this ID
                    int foundIndex = -1;
                    for (int i = 0; i < items.Count; i++)
                        if (((Hashtable) items[i])["id"].ToString() == moddedItemId)
                        {
                            foundIndex = i;
                            break;
                        }
                    if (foundIndex < 0)
                        Logging.Warn($"Tried to delete '{moddedItemId}' but was not found");
                    else
                    {
                        Logging.Info($"Deleted '{moddedItemId}'");
                        items.RemoveAt(foundIndex);
                    }

                    break;
                }

                Hashtable originalItem = null;
                var parents = new Dictionary<string, Hashtable>();
                var parentsOrder = moddedItem.GetArrayList("extends") ?? new ArrayList();
                foreach (Hashtable item in items)
                {
                    // Check if this item is overwriting an existing item (this will consider only the first matching
                    // item - normally, there should only be one)
                    var itemId = item.GetString("id");
                    if (itemId == moddedItemId && originalItem == null)
                    {
                        originalItem = item;
                    }

                    // Collect all the parents of this modded item so that the full item can be built
                    if (parentsOrder.Contains(itemId))
                    {
                        parents[itemId] = item;
                    }
                }

                // Build the new item, first by copying its parents, then by applying its own specificities
                // If the new item should override an older one, replace that one too
                var newItem = new Hashtable();
                foreach (string parent in parentsOrder)
                {
                    if (!parents.ContainsKey(parent))
                    {
                        Logging.Error($"Unknown parent '{parent}' for '{moddedItemId}', skipping parent");
                        continue;
                    }
                    newItem.AddHashtable(parents[parent], false);
                }
                newItem.AddHashtable(moddedItem, true);

                // Run any property operations that are present
                ProcessPropertyOperations(newItem);

                if (originalItem != null)
                {
                    originalItem.Clear();
                    originalItem.AddHashtable(newItem, true);
                }
                else
                {
                    items.Add(newItem);
                }
            }
            return items;
        }

        private static void ProcessPropertyOperations(Hashtable item)
        {
            var itemId = item.GetString("id");
            var keys = new ArrayList(item.Keys);
            foreach (string property in keys)
            {
                var propertyWithOperation = property.Split('$');
                if (propertyWithOperation.Length < 2)
                {
                    continue;
                }
                if (propertyWithOperation.Length > 2)
                {
                    Logging.Warn($"Property '{property}' in '{itemId}' contains too many '$', skipping");
                    continue;
                }

                var originalProperty = propertyWithOperation[0];
                if (!item.ContainsKey(originalProperty))
                {
                    Logging.Warn($"Unknown property '{originalProperty}' for property '{property}' in '{itemId}', skipping");
                    continue;
                }
                var operation = propertyWithOperation[1];
                switch (operation)
                {
                    // append: append values to a list
                    // prepend: prepend values to a list
                    case "append":
                    case "prepend":
                    {
                        var value = item.GetArrayList(originalProperty);
                        var newValue = item.GetArrayList(property);
                        if (value == null || newValue == null)
                        {
                            Logging.Warn(
                                $"Cannot apply '{operation}' to '{originalProperty}' in '{itemId}': invalid type, must be a list");
                            continue;
                        }

                        if (operation == "append")
                        {
                            value.AddRange(newValue);
                        }
                        else
                        {
                            value.InsertRange(0, newValue);
                        }

                        break;
                    }

                    // plus: Adds a numerical value to another.
                    // minus: Subtracts a numerical value from another.
                    case "plus":
                    case "minus":
                    {
                        var value = item.GetFloat(originalProperty);
                        var newValue = item.GetFloat(property);

                        var modifier = operation == "plus" ? 1 : -1;
                        item[originalProperty] = value + newValue * modifier;
                        break;
                    }

                    // extend: add or replace keys in a dictionary
                    case "extend":
                    {
                        var value = item.GetHashtable(originalProperty);
                        var newValue = item.GetHashtable(property);
                        if (value == null || newValue == null)
                        {
                            Logging.Warn(
                                $"Cannot apply '{operation}' to '{originalProperty}' in '{itemId}': invalid type, must be a dictionary");
                            continue;
                        }

                        value.AddHashtable(newValue, true);

                        break;
                    }

                    // remove: removes items from a dictionary or a list
                    case "remove":
                    {
                        var newValue = item.GetArrayList(property);
                        if (newValue == null)
                        {
                            Logging.Warn(
                                $"Invalid value for '{property}' in '{itemId}': invalid type, must be a list");
                            continue;
                        }

                        if (!item.ContainsKey(originalProperty))
                        {
                            Logging.Warn(
                                $"Cannot apply '{operation}' to '{originalProperty}' in '{itemId}': failed to find '{originalProperty}'");
                            continue;
                        }

                        object originalPropertyValue = item[originalProperty];
                        Type propType = originalPropertyValue.GetType();
                        if (propType == typeof(Hashtable))
                        {
                            var value = item.GetHashtable(originalProperty);
                            foreach (string toDelete in newValue)
                            {
                                if (value.ContainsKey(toDelete))
                                    value.Remove(toDelete);
                                else
                                    Logging.Warn($"Failed to delete '{toDelete}' from '{originalProperty}' in '{itemId}'");
                            }
                        }
                        else if (propType == typeof(ArrayList))
                        {
                            var value = item.GetArrayList(originalProperty);
                            foreach (string toDelete in newValue)
                            {
                                if (value.Contains(toDelete))
                                    value.Remove(toDelete);
                                else
                                    Logging.Warn($"Failed to delete '{toDelete}' from '{originalProperty}' in '{itemId}'");
                            }
                        }
                        else
                        {
                            Logging.Warn($"Cannot apply '{operation}' to '{originalProperty}' in '{itemId}': invalid type, must be a dictionary or a list");
                        }

                        break;
                    }
                    default:
                        Logging.Warn($"Unknown operation '{operation}' for property '{property}' in '{itemId}', skipping");
                        continue;
                }

                // Remove the property once it has been processed, to avoid warnings from the content importer
                item.Remove(property);
            }
        }
    }
}
