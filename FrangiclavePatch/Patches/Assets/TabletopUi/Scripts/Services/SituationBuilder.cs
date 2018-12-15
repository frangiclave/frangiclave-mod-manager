using System.Diagnostics.CodeAnalysis;
using Assets.Core.Commands;
using Frangiclave.Patches.Assets.CS.TabletopUI;
using MonoMod;
using UnityEngine;

#pragma warning disable CS0626

namespace Frangiclave.Patches.Assets.TabletopUi.Scripts.Services
{
    [MonoModPatch("global::Assets.TabletopUi.Scripts.Services.SituationBuilder")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class SituationBuilder : global::Assets.TabletopUi.Scripts.Services.SituationBuilder
    {
        public SituationBuilder(
            Transform tableLevel,
            Transform windowLevel,
            Heart heart) : base(tableLevel, windowLevel, heart)
        {
        }

        public void CreateReceivedRemoteToken(
            SituationCreationCommand situationCreationCommand,
            string locatorInfo = null)
        {
            var newToken =
                CreateTokenWithAttachedControllerAndSituation(situationCreationCommand, locatorInfo) as SituationToken;
            // ReSharper disable once PossibleNullReferenceException
            newToken.SetRemote(false);
        }
    }
}

