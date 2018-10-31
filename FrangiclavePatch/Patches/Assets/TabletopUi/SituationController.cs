using System.Diagnostics.CodeAnalysis;
using Assets.CS.TabletopUI;
using Frangiclave.Modding;
using MonoMod;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace Frangiclave.Patches.Assets.TabletopUi
{
    [MonoModPatch("Assets.TabletopUi.SituationController")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class SituationController : global::Assets.TabletopUi.SituationController
    {
        [MonoModIgnore]
        private readonly ICompendium compendium;

        public SituationController(ICompendium co, Character ch) : base(co, ch)
        {
        }

        private extern void orig_SituationComplete();

        public new void SituationComplete()
        {
            var recipeById = (Recipe) compendium.GetRecipeById(SituationClock.RecipeId);
            var tabletopManager = (Frangiclave.Patches.Assets.CS.TabletopUI.TabletopManager) Registry.Retrieve<TabletopManager>();
            tabletopManager.SetMap(Map.DefaultMapId.Equals(recipeById.MapId) ? null : recipeById.MapId);
            orig_SituationComplete();
        }
    }
}
