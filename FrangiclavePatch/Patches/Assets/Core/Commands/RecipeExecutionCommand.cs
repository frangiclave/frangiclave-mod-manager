using MonoMod;

namespace Frangiclave.Patches.Assets.Core.Commands
{
    [MonoModPatch("global::Assets.Core.Commands.RecipeExecutionCommand")]
    public class RecipeExecutionCommand : global::Assets.Core.Commands.RecipeExecutionCommand
    {
        [MonoModIgnore] public new Recipe Recipe { get; set; }

        public bool SendAway { get; set; }

        public RecipeExecutionCommand(global::Recipe recipe, Expulsion expulsion) : base(recipe, expulsion)
        {
        }
    }
}
