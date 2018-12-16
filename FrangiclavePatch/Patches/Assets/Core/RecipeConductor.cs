using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Assets.Core;
using Assets.Core.Commands;
using MonoMod;
using Noon;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace Frangiclave.Patches.Assets.Core
{
    [MonoModPatch("global::Assets.Core.RecipeConductor")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class RecipeConductor : global::Assets.Core.RecipeConductor
    {
        [MonoModIgnore] private ICompendium compendium;

        [MonoModIgnore] private IAspectsDictionary aspectsToConsider;

        [MonoModIgnore] private IDice dice;

        [MonoModIgnore] private Character currentCharacter;

        public RecipeConductor(ICompendium c, IAspectsDictionary a, IDice d, Character character) : base(c, a, d,
            character)
        {
        }

        public IList<RecipeExecutionCommand> GetActualRecipesToExecute(Recipe recipe)
        {
            IList<RecipeExecutionCommand> recipeExecutionCommands = new List<RecipeExecutionCommand>()
                {new RecipeExecutionCommand(recipe, null)};
            if (recipe.AlternativeRecipes.Count == 0)
                return recipeExecutionCommands;


            foreach (var ar in recipe.AlternativeRecipes)
            {
                int diceResult = dice.Rolld100(recipe);
                if (diceResult > ar.Chance)
                {
                    NoonUtility.Log(recipe.Id + " says: " + "Dice result " + diceResult + ", against chance " +
                                    ar.Chance +
                                    " for alternative recipe " + ar.Id +
                                    "; will try to execute next alternative recipe");
                }
                else
                {
                    // ReSharper disable once PossibleNullReferenceException
                    Recipe candidateRecipe = (compendium as Compendium).GetRecipeById(ar.Id);

                    if (!candidateRecipe.RequirementsSatisfiedBy(aspectsToConsider))
                    {
                        NoonUtility.Log(recipe.Id + " says: couldn't satisfy requirements for " + ar.Id, 5);
                        continue;
                    }

                    if (currentCharacter.HasExhaustedRecipe(candidateRecipe))
                    {
                        NoonUtility.Log(recipe.Id + " says: already exhausted " + ar.Id, 5);
                        continue;
                    }

                    if (ar.Additional)
                    {
                        var command = new Frangiclave.Patches.Assets.Core.Commands.RecipeExecutionCommand(
                            candidateRecipe, ar.Expulsion) {SendAway = ar.Remote};
                        recipeExecutionCommands.Add(command);
                        NoonUtility.Log(recipe.Id + " says: Found additional recipe " + ar.Id +
                                        " to execute - adding it to execution list and looking for more");
                    }
                    else
                    {
                        IList<RecipeExecutionCommand> recursiveRange = GetActualRecipesToExecute(candidateRecipe);

                        string logMessage = recipe.Id + " says: reached the bottom of the execution list: returning ";
                        logMessage =
                            recursiveRange.Aggregate(logMessage, (current, r) => current + r.Recipe.Id + "; ");
                        NoonUtility.Log(logMessage);

                        return recursiveRange;
                    }
                }
            }

            return recipeExecutionCommands;
        }
    }
}
