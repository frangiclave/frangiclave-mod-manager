using System;
using System.Collections.Generic;
using System.Linq;
using Assets.CS.TabletopUI;
using Frangiclave.Modding;
using MonoMod;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0626

namespace Frangiclave.Patches.Assets.CS.TabletopUI
{
    [MonoModPatch("Assets.CS.TabletopUI.MapTokenContainer")]
    public class MapTokenContainer : global::Assets.CS.TabletopUI.MapTokenContainer
    {
        private Map _defaultMap;

        [MonoModIgnore]
        private DoorSlot[] allSlots;

        private DoorSlot _doorSlot;

        public extern void orig_Initialise();

        public new void Initialise()
        {
            orig_Initialise();
            var portals = new List<MapPortal>();
            foreach (var doorSlot in allSlots)
            {
                var cardPositions = doorSlot.cardPositions.Select(cardPosition => cardPosition.transform.localPosition).ToList();

                // This is a hack because I haven't found where the image component is actually stored yet
                var icon = doorSlot.GetComponentsInChildren<Image>().FirstOrDefault(child => child.name == "Icon");

                if (icon == null)
                {
                    throw new Exception("Portal is missing an icon");
                }

                // Save the door slot for later
                _doorSlot = Instantiate(doorSlot);

                portals.Add(new MapPortal(
                    doorSlot.portalType,
                    null,
                    doorSlot.GetComponent<Transform>().localPosition,
                    cardPositions,
                    icon.sprite,
                    doorSlot.defaultBackgroundColor));
            }
            _defaultMap = new Map(Map.DefaultMapId, portals);
        }

        public void SetCurrentMap(string mapId)
        {
            Logging.Info("Loading " + (mapId == null ? "default map" : $"custom map '{mapId}'"));
            var mapController = (Compendium) Registry.Retrieve<ICompendium>();
            var map = mapId == null ? _defaultMap : mapController.GetMapById(mapId);

            foreach (var doorSlot in allSlots)
            {
                Destroy(doorSlot.gameObject);
            }

            allSlots = new DoorSlot[map.Portals.Count];
            for (var i = 0; i < map.Portals.Count; i++)
            {
                var portal = map.Portals[i];
                Logging.Info($"\tLoading portal '{portal.Effect}'");

                // Create the new door slot by copying the template
                var doorSlot = Instantiate(_doorSlot, transform);
                doorSlot.portalType = portal.Effect;
                doorSlot.DeckBase = portal.DeckBase;
                Logging.Info($"\t\tDeck: {portal.DeckBase}");

                // Set the door's appearance
                var icon = doorSlot.GetComponentsInChildren<Image>().FirstOrDefault(child => child.name == "Icon");
                var iconTransform = doorSlot.GetComponentsInChildren<RectTransform>().FirstOrDefault(child => child.name == "Icon");
                if (icon == null || iconTransform == null)
                {
                    throw new Exception("Portal is missing an icon");
                }
                icon.sprite = portal.Icon;
                iconTransform.sizeDelta = new Vector2(portal.Icon.rect.width, portal.Icon.rect.height);
                doorSlot.defaultBackgroundColor = portal.Color;
                Logging.Info($"\t\tColor: {portal.Color}");

                // Set the door's position
                doorSlot.transform.localPosition = portal.Position;
                Logging.Info($"\t\tPosition: {portal.Position}");
                for(var j = 0; j < portal.CardPositions.Count; j++)
                {
                    doorSlot.cardPositions[j].localPosition = portal.CardPositions[j];
                    Logging.Info($"\t\tCards {j}: {portal.CardPositions[j]}");
                }

                doorSlot.Initialise();
                allSlots[i] = doorSlot;
            }
        }
    }
}
