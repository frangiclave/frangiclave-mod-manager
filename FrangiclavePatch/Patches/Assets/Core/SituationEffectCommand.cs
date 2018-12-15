using MonoMod;

namespace Frangiclave.Patches.Assets.Core
{
    [MonoModPatch("global::Assets.Core.SituationEffectCommand")]
    public class SituationEffectCommand : global::Assets.Core.SituationEffectCommand
    {
        public bool SendAway { get; set;  }

        public SituationEffectCommand(
            global::Recipe recipe,
            bool asNewSituation,
            Expulsion expulsion) : base(recipe, asNewSituation, expulsion)
        {
        }
    }
}
