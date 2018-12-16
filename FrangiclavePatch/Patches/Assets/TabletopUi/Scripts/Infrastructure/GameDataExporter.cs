using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Assets.Core.Interfaces;
using Frangiclave.Multiplayer;
using Frangiclave.Patches.Assets.CS.TabletopUI;
using MonoMod;

namespace Frangiclave.Patches.Assets.TabletopUi.Scripts.Infrastructure
{
    [MonoModPatch("Assets.TabletopUi.Scripts.Infrastructure.GameDataExporter")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class GameDataExporter : global::Assets.TabletopUi.Scripts.Infrastructure.GameDataExporter
    {
        private Hashtable GetHashTableForSituations(IEnumerable<global::Assets.TabletopUi.SituationController> situationControllers)
        {
            var htSituations = new Hashtable();
            foreach (var s in situationControllers)
            {
                if (s.situationToken == null || !(s.situationToken is SituationToken token) ||
                    token.SaveLocationInfo == null || token.SendingAway)
                    continue;
                var htSituationProperties = s.GetSaveData();
                htSituations.Add(s.situationToken.SaveLocationInfo, htSituationProperties);
            }
            return htSituations;
        }

        public new Hashtable GetHashTableForStacks(IEnumerable<IElementStack> stacks)
        {
            var htElementStacks = new Hashtable();
            foreach (var e in stacks)
            {
                if (e?.SaveLocationInfo == null || e.EntityId == MultiplayerClient.PresenceId)
                    continue;
                var stackHashtable = GetHashtableForThisStack(e);
                htElementStacks.Add(e.SaveLocationInfo, stackHashtable);
            }

            return htElementStacks;
        }

        [MonoModIgnore]
        private extern Hashtable GetHashtableForThisStack(IElementStack stack);
    }
}
