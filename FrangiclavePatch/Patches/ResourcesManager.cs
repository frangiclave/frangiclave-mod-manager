using System.Diagnostics.CodeAnalysis;
using Assets.CS.TabletopUI;
using Frangiclave.Modding;
using MonoMod;
using UnityEngine;

#pragma warning disable CS0626

namespace Frangiclave.Patches
{
    [MonoModPatch("global::ResourcesManager")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class ResourcesManager
    {
        private const string PlaceholderImageName = "_x";

        public static extern Sprite orig_GetSpriteForVerbLarge(string verbId);

        public static Sprite GetSpriteForVerbLarge(string verbId)
        {
            return GetSprite("icons100/verbs/", verbId);
        }

        public static extern Sprite orig_GetSpriteForElement(string imageName);

        public static Sprite GetSpriteForElement(string imageName)
        {
            return GetSprite("elementArt/", imageName);
        }

        public static extern Sprite orig_GetSpriteForElement(string imageName, int animFrame);

        public static Sprite GetSpriteForElement(string imageName, int animFrame)
        {
            return GetSprite("elementArt/anim/", string.Concat(imageName, "_", animFrame));
        }

        public static extern Sprite orig_GetSpriteForCardBack(string backId);

        public static Sprite GetSpriteForCardBack(string backId)
        {
            return GetSprite("cardBacks/", backId);
        }

        public static extern Sprite orig_GetSpriteForAspect(string aspectId);

        public static Sprite GetSpriteForAspect(string aspectId)
        {
            return GetSprite("icons40/aspects/", aspectId);
        }

        public static extern Sprite orig_GetSpriteForLegacy(string legacyImage);

        public static Sprite GetSpriteForLegacy(string legacyImage)
        {
            return GetSprite("icons100/legacies/", legacyImage);
        }

        public static extern Sprite orig_GetSpriteForEnding(string endingImage);

        public static Sprite GetSpriteForEnding(string endingImage)
        {
            return GetSprite("endingArt/", endingImage);
        }

        public static Sprite GetSpriteForMap(string map)
        {
            return GetSprite("maps/", map);
        }

        public static Sprite GetSpriteForMapPortal(string mapPortal)
        {
            return GetSprite("maps/portals/", mapPortal);
        }

        public static Sprite GetSprite(string folder, string file)
        {
            // Try to find the image in a mod first, in case it overrides an existing one
            var modManager = Registry.Retrieve<ModManager>();
            var modSprite = modManager.GetSprite(folder + file);
            if (modSprite != null)
            {
                return modSprite;
            }

            // Try to load the image from the packed resources next, and show the placeholder if not found
            var sprite = Resources.Load<Sprite>(folder + file);
            return sprite != null ? sprite : Resources.Load<Sprite>(folder + PlaceholderImageName);
        }
    }
}
