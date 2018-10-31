using Assets.CS.TabletopUI;
using Frangiclave.Modding;
using MonoMod;

#pragma warning disable CS0626

namespace Frangiclave.Patches
{
    [MonoModPatch("global::DebugTools")]
    public class DebugTools : global::DebugTools
    {
        private extern void orig_UpdateCompendiumContent();

        private void UpdateCompendiumContent()
        {
            Registry.Retrieve<ModManager>().LoadAll();
            orig_UpdateCompendiumContent();
        }

    }
}
