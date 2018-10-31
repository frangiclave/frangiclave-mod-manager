using System.Diagnostics.CodeAnalysis;
using MonoMod;
using UnityEngine;

#pragma warning disable CS0626

namespace Frangiclave.Patches.Assets.CS.TabletopUI
{
    [MonoModPatch("Assets.CS.TabletopUI.TabletopImageBurner")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class TabletopImageBurner : global::Assets.CS.TabletopUI.TabletopImageBurner
    {
        private extern Sprite orig_LoadBurnSprite(string imageName);

        private Sprite LoadBurnSprite(string imageName)
        {
            return ResourcesManager.GetSprite("burnImages/", imageName);
        }
    }
}
