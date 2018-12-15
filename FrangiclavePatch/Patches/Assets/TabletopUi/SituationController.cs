using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.Logic;
using Assets.TabletopUi.Scripts.Interfaces;
using Frangiclave.Modding;
using Frangiclave.Patches.Assets.Core;
using Frangiclave.Patches.Assets.Core.Commands;
using MonoMod;
using TabletopUi.Scripts.Interfaces;
using SituationToken = Frangiclave.Patches.Assets.CS.TabletopUI.SituationToken;
using TabletopManager = Frangiclave.Patches.Assets.CS.TabletopUI.TabletopManager;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace Frangiclave.Patches.Assets.TabletopUi
{
    [MonoModPatch("Assets.TabletopUi.SituationController")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnassignedField.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class SituationController : global::Assets.TabletopUi.SituationController
    {
        [MonoModIgnore] private readonly ICompendium compendium;

        [MonoModIgnore] private readonly Character currentCharacter;

        [MonoModIgnore] public new ISituationAnchor situationToken;

        public bool SendingAway { get; set; }

        public SituationController(ICompendium co, Character ch) : base(co, ch)
        {
        }

        private extern void orig_SituationComplete();

        public new void SituationComplete()
        {
            if (SendingAway)
            {
                Retire();
                return;
            }

            var recipeById = (Recipe) compendium.GetRecipeById(SituationClock.RecipeId);
            var tabletopManager = Registry.Retrieve<ITabletopManager>() as TabletopManager;
            if (tabletopManager != null)
                tabletopManager.SetMap(Map.DefaultMapId.Equals(recipeById.MapId) ? null : recipeById.MapId);
            orig_SituationComplete();
        }

        private extern void orig_InitialiseActiveSituation(SituationCreationCommand command);

        private void InitialiseActiveSituation(SituationCreationCommand command)
        {
            orig_InitialiseActiveSituation(command);

            SendingAway = command.SendAway;
            if (!SendingAway)
                return;
            SituationToken token = situationToken as SituationToken;
            if (token != null)
                token.SetRemote(true);
            var tabletopManager = Registry.Retrieve<ITabletopManager>() as TabletopManager;
            if (tabletopManager != null)
                tabletopManager.SendRemoteSituation(command.Verb.Id, command.Recipe.Id);
        }

        public new void Retire()
        {
            (situationToken as SituationToken)?.Retire(!SendingAway);
            situationWindow.Retire();
            Registry.Retrieve<SituationsCatalogue>().DeregisterSituation(this);
        }

        public new void SituationExecutingRecipe(ISituationEffectCommand command)
        {
            var tabletopManager = Registry.Retrieve<ITabletopManager>();
            situationWindow.SetSlotConsumptions();
            StoreStacks(situationWindow.GetOngoingStacks());

            if (command.AsNewSituation)
            {
                List<IElementStack> stacksToAddToNewSituation = new List<IElementStack>();
                if (command.Expulsion != null)
                {
                    AspectMatchFilter filter = new AspectMatchFilter(command.Expulsion.Filter);
                    var filteredStacks = filter.FilterElementStacks(situationWindow.GetStoredStacks()).ToList();
                    if (filteredStacks.Any() && command.Expulsion.Limit > 0)
                    {
                        while (filteredStacks.Count > command.Expulsion.Limit)
                        {
                            filteredStacks.RemoveAt(filteredStacks.Count - 1);
                        }

                        stacksToAddToNewSituation = filteredStacks;
                    }
                }

                IVerb verbForNewSituation = compendium.GetOrCreateVerbForCommand(command);
                var scc = new SituationCreationCommand(
                    verbForNewSituation,
                    command.Recipe,
                    SituationState.FreshlyStarted,
                    situationToken as DraggableToken);
                if (command is SituationEffectCommand seCommand)
                    scc.SendAway = seCommand.SendAway;
                tabletopManager.BeginNewSituation(scc, stacksToAddToNewSituation);
                situationWindow.DisplayStoredElements();
                return;
            }

            currentCharacter.AddExecutionsToHistory(command.Recipe.Id, 1);
            var executor = new SituationEffectExecutor();
            executor.RunEffects(command, situationWindow.GetStorageStacksManager(), currentCharacter);

            if (command.Recipe.EndingFlag != null)
            {
                var ending = compendium.GetEndingById(command.Recipe.EndingFlag);
                tabletopManager.EndGame(ending, this);
            }

            situationWindow.DisplayStoredElements();
        }
    }
}
