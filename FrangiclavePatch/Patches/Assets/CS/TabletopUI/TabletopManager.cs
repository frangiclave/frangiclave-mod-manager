using System.Diagnostics.CodeAnalysis;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;
using MonoMod;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace Frangiclave.Patches.Assets.CS.TabletopUI
{
    [MonoModPatch("Assets.CS.TabletopUI.TabletopManager")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class TabletopManager : global::Assets.CS.TabletopUI.TabletopManager
    {
        [MonoModIgnore]
        public new MapTokenContainer mapTokenContainer;

        [MonoModIgnore]
        private TabletopBackground mapBackground;

        private Sprite _defaultMapSprite;

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
    }
}
