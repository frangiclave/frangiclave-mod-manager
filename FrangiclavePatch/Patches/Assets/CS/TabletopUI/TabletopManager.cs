using System.Diagnostics.CodeAnalysis;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using Frangiclave.Patches.Assets.Core.Commands;
using Frangiclave.Patches.Assets.TabletopUi.Scripts.Services;
using Frangiclave.Multiplayer;
using MonoMod;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace Frangiclave.Patches.Assets.CS.TabletopUI
{
    [MonoModPatch("Assets.CS.TabletopUI.TabletopManager")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class TabletopManager : global::Assets.CS.TabletopUI.TabletopManager
    {
        [MonoModIgnore] public new MapTokenContainer mapTokenContainer;

        [MonoModIgnore] private TabletopBackground mapBackground;

        [MonoModIgnore] private SituationBuilder _situationBuilder;

        [MonoModIgnore] public new TabletopTokenContainer _tabletop;

        private Sprite _defaultMapSprite;

        private MultiplayerClient _client;

        public void JoinServer(string serverAddress, string roomId)
        {
            Logging.Info($"Attempting to join server '{serverAddress}' in room '{roomId}'");
            _client?.Disconnect();
            _client = new MultiplayerClient(serverAddress, roomId);
        }

        private extern void orig_InitialiseSubControllers(
            SpeedController speedController,
            HotkeyWatcher hotkeyWatcher,
            CardAnimationController cardAnimationController,
            MapController mapController,
            EndGameAnimController endGameAnimController,
            Notifier notifier,
            OptionsPanel optionsPanel);

        private void InitialiseSubControllers(
            SpeedController speedController,
            HotkeyWatcher hotkeyWatcher,
            CardAnimationController cardAnimationController,
            MapController mapController,
            EndGameAnimController endGameAnimController,
            Notifier notifier,
            OptionsPanel optionsPanel)
        {
            orig_InitialiseSubControllers(
                speedController,
                hotkeyWatcher,
                cardAnimationController,
                mapController,
                endGameAnimController,
                notifier,
                optionsPanel);

            if (_defaultMapSprite != null)
            {
                return;
            }

            var image = mapBackground.GetComponent<Image>();
            _defaultMapSprite = image.sprite;
        }

        public void SetMap(string mapId)
        {
            var image = mapBackground.GetComponent<Image>();
            image.sprite = mapId == null ? _defaultMapSprite : ResourcesManager.GetSpriteForMap(mapId);
            mapTokenContainer.SetCurrentMap(mapId);
        }

        public void SendRemoteSituation(string verbId, string recipeId)
        {
            if (_client == null || !_client.IsConnected)
            {
                Logging.Info("Failed to send situation, not connected to any server");
                return;
            }

            _client.SendSituation(verbId, recipeId);
        }

        public void ReceiveRemoteSituation(string verbId, string recipeId)
        {
            var compendium = Registry.Retrieve<ICompendium>();
            var recipe = compendium.GetRecipeById(recipeId);
            var command = new SituationCreationCommand(
                compendium.GetVerbById(verbId), compendium.GetRecipeById(recipeId), SituationState.Ongoing)
            {
                TimeRemaining = recipe.Warmup
            };
            _situationBuilder.CreateReceivedRemoteToken(command);
        }
    }
}
