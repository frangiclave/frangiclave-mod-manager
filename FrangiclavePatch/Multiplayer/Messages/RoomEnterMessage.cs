using UnityEngine.Networking;

namespace Frangiclave.Multiplayer.Messages
{
    public class RoomEnterMessage : MessageBase
    {
        public string RoomId;

        public override void Deserialize(NetworkReader reader)
        {
            RoomId = reader.ReadString();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(RoomId);
        }
    }
}
