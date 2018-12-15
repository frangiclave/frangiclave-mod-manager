using System.Diagnostics.CodeAnalysis;
using MonoMod;

#pragma warning disable CS0436
#pragma warning disable CS0626

namespace Frangiclave.Patches.Assets.CS.TabletopUI
{
    [MonoModPatch("Assets.CS.TabletopUI.DoorSlot")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class DoorSlot : global::Assets.CS.TabletopUI.DoorSlot
    {
        [MonoModIgnore]
        public new PortalEffect portalType;

        public string DeckBase { private get; set; }

        public extern string orig_GetDeckName(int cardPosition);

        public new string GetDeckName(int cardPosition)
        {
            var text = DeckBase ?? "mansus_" + portalType.ToString().ToLowerInvariant();
            if (cardPosition > 0)
            {
                text += cardPosition.ToString();
            }
            return text;
        }
    }
}
