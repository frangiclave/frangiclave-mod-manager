using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Assets.Core;
using Assets.Core.Commands;
using Assets.TabletopUi.Scripts.Interfaces;
using Frangiclave.Patches.Assets.TabletopUi;
using MonoMod;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace Frangiclave.Patches.Assets.Core.Entities
{
    [MonoModPatch("global::Assets.Core.Entities.SituationClock")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class SituationClock : global::Assets.Core.Entities.SituationClock
    {
        [MonoModIgnore] private new SituationState State { get; set; }

        [MonoModIgnore] private Recipe currentPrimaryRecipe { get; set; }

        [MonoModIgnore] private ISituationSubscriber subscriber;

        public SituationClock(ISituationSubscriber s) : base(s)
        {
        }

        public SituationClock(
            float? timeRemaining,
            SituationState state,
            global::Recipe withPrimaryRecipe,
            ISituationSubscriber s) :
            base(timeRemaining, state, withPrimaryRecipe, s)
        {
        }

        private void RequireExecution(IRecipeConductor rc)
        {
            State = SituationState.RequiringExecution;
            // ReSharper disable once PossibleNullReferenceException
            IList<RecipeExecutionCommand> recipeExecutionCommands = (rc as RecipeConductor).GetActualRecipesToExecute(currentPrimaryRecipe);
            if (recipeExecutionCommands.First().Recipe.Id != currentPrimaryRecipe.Id)
                currentPrimaryRecipe = ((Commands.RecipeExecutionCommand) recipeExecutionCommands.First()).Recipe;

            foreach (Commands.RecipeExecutionCommand c in recipeExecutionCommands)
            {
                SituationEffectCommand ec = new SituationEffectCommand(
                    c.Recipe, c.Recipe.ActionId != currentPrimaryRecipe.ActionId, c.Expulsion)
                {
                    SendAway = c.SendAway
                };
                subscriber.SituationExecutingRecipe(ec);
            }
        }

        private extern void orig_End(IRecipeConductor rc);

        private void End(IRecipeConductor rc)
        {
            if (subscriber is SituationController controller && controller.SendingAway)
            {
                Complete();
                return;
            }
            orig_End(rc);
        }

        private void Complete()
        {
            State = SituationState.Complete;
            subscriber.SituationComplete();
            if (!(subscriber is SituationController controller) || !controller.SendingAway)
                SoundManager.PlaySfx("SituationComplete");
        }
    }
}

