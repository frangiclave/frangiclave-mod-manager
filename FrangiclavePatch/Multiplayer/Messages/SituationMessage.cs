using UnityEngine.Networking;

namespace Frangiclave.Multiplayer.Messages
{
    public class SituationMessage : MessageBase
    {
        public string VerbId;
        public string RecipeId;

        public override void Deserialize(NetworkReader reader)
        {
            VerbId = reader.ReadString();
            RecipeId = reader.ReadString();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(VerbId);
            writer.Write(RecipeId);
        }
    }
}
