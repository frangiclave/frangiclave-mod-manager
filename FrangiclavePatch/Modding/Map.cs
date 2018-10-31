using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0436

namespace Frangiclave.Modding
{
    public class Map
    {
        public const string DefaultMapId = "__default__";

        public string Id { get; }

        public List<MapPortal> Portals { get; }

        public Map(string id, List<MapPortal> portals)
        {
            Id = id;
            Portals = portals;
        }

        public override string ToString()
        {
            return $"Map(Id={Id})";
        }
    }

    public class MapPortal
    {
        public PortalEffect Effect { get; }

        public string DeckBase { get; }

        public Vector3 Position { get; }

        public List<Vector3> CardPositions { get; }

        public Sprite Icon { get; }

        public Color Color { get; }

        public MapPortal(
            PortalEffect effect,
            string deckBase,
            Vector3 position,
            List<Vector3> cardPositions,
            Sprite icon,
            Color color)
        {
            Effect = effect;
            DeckBase = deckBase;
            Position = position;
            CardPositions = cardPositions;
            Icon = icon;
            Color = color;
        }
    }
}
