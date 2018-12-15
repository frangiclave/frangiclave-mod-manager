using UnityEngine.Networking;

namespace Frangiclave.Multiplayer.Messages
{
    public class RoomJoinMessage : MessageBase
    {
        public bool Success;

        public override void Deserialize(NetworkReader reader)
        {
            Success = reader.ReadBoolean();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Success);
        }
    }
}
