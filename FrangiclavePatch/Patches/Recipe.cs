using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Frangiclave.Modding;
using MonoMod;

namespace Frangiclave.Patches
{
    [MonoModPatch("global::Recipe")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class Recipe : global::Recipe
    {
        [MonoModIgnore]
        // ReSharper disable once CollectionNeverUpdated.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public new List<LinkedRecipeDetails> AlternativeRecipes { get; set; }

        public string MapId { get; set; }

        public Recipe()
        {
            MapId = Map.DefaultMapId;
        }
    }


    [MonoModPatch("global::LinkedRecipeDetails")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class LinkedRecipeDetails : global::LinkedRecipeDetails
    {
        public bool Remote { get; set; }

        public LinkedRecipeDetails(string id, int chance, bool additional, Expulsion expulsion, Dictionary<string, string> challenges) : base(id, chance, additional, expulsion, challenges)
        {
        }
    }
}
