using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using MonoMod;

namespace Frangiclave.Patches.Assets.Core.Commands
{
    [MonoModPatch("global::Assets.Core.Commands.SituationCreationCommand")]
    public class SituationCreationCommand : global::Assets.Core.Commands.SituationCreationCommand
    {
        public bool SendAway { get; set; }

        public SituationCreationCommand(
            IVerb verb,
            global::Recipe recipe,
            SituationState situationState,
            DraggableToken sourceToken = null) : base(verb, recipe, situationState, sourceToken)
        {
        }
    }
}

