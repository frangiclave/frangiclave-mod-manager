using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Assets.Core.Entities;
using Frangiclave.Modding;
using MonoMod;
using OrbCreationExtensions;
using UnityEngine;

#pragma warning disable CS0436
#pragma warning disable CS0626

namespace Frangiclave.Patches
{
    [MonoModPatch("global::Compendium")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Compendium : global::Compendium
    {
        private Dictionary<string, Ending> _endings = new Dictionary<string, Ending>();

        private Dictionary<string, Map> _maps = new Dictionary<string, Map>();

        public void UpdateEndings(IEnumerable<Hashtable> endingsData)
        {
            _endings = new Dictionary<string, Ending>();
            foreach (var endingData in endingsData)
            {
                _endings.Add(
                    endingData.GetString("id"),
                    new Ending(
                        endingData.GetString("id"),
                        endingData.GetString("title"),
                        endingData.GetString("description"),
                        endingData.GetString("image"),
                        (EndingFlavour) Enum.Parse(typeof(EndingFlavour), endingData.GetString("flavour")),
                        endingData.GetString("anim"),
                        null));
            }
        }

        private extern Ending orig_GetEndingById(string endingFlag);

        public new Ending GetEndingById(string endingFlag)
        {
            return _endings.ContainsKey(endingFlag) ? _endings[endingFlag] : orig_GetEndingById(endingFlag);
        }

        public void UpdateMaps(IEnumerable<Hashtable> mapsData)
        {
            _maps = new Dictionary<string, Map>();
            foreach (var mapData in mapsData)
            {
                var id = mapData.GetString("id");
                var portals = new List<MapPortal>();
                var portalsData = mapData.GetArrayList("portals");
                for (var i = 0; i < portalsData.Count; i++)
                {
                    var portalData = portalsData.GetHashtable(i);

                    // Get the portal effect
                    var effect = (PortalEffect) Enum.Parse(
                        typeof(PortalEffect),
                        portalData["effect"].ToString(),
                        true);

                    // Get the portal's position
                    var positionData = portalData.GetArrayList("position");
                    var position = new Vector3(positionData.GetFloat(0), positionData.GetFloat(1), 0);

                    // Get the positions of the cards
                    var cardPositions = new List<Vector3>();
                    var cardPositionsData = portalData.GetArrayList("cards");
                    for (var j = 0; j < cardPositionsData.Count; j++)
                    {
                        var cardPositionData = cardPositionsData.GetArrayList(j);
                        cardPositions.Add(new Vector3(cardPositionData.GetFloat(0), cardPositionData.GetFloat(1), 0));
                    }

                    // Get the portal color
                    var colorData = portalData.GetArrayList("color");
                    var color = new Color(colorData.GetFloat(0), colorData.GetFloat(1), colorData.GetFloat(2));

                    var deck = portalData["deck"].ToString();
                    var icon = ResourcesManager.GetSpriteForMapPortal(portalData["icon"].ToString());

                    portals.Add(new MapPortal(effect, deck, position, cardPositions, icon, color));
                }

                _maps.Add(
                    id,
                    new Map(id, portals));
            }
        }

        public Map GetMapById(string mapId)
        {
            return _maps.TryGetValue(mapId, out var value) ? value : null;
        }

        private extern Recipe orig_GetRecipeById(string recipeId);

        public new Recipe GetRecipeById(string recipeId)
        {
            return orig_GetRecipeById(recipeId);
        }
    }
}
