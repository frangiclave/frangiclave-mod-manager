using System.Diagnostics.CodeAnalysis;
using Frangiclave.Modding;
using MonoMod;

namespace Frangiclave.Patches
{
    [MonoModPatch("global::Recipe")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class Recipe : global::Recipe
    {
        public string MapId { get; set; }

        public Recipe()
        {
            MapId = Map.DefaultMapId;
        }
    }
}
