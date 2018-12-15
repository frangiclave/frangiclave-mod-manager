using UnityEngine.Networking;

namespace Frangiclave.Multiplayer
{
    public class NoonMsgType
    {
        public const short PartnerJoin = MsgType.Highest + 1;
        public const short PartnerLeave = MsgType.Highest + 2;
        public const short Situation = MsgType.Highest + 3;
        public const short RoomEnter = MsgType.Highest + 4;
        public const short RoomJoin = MsgType.Highest + 5;
    }
}
