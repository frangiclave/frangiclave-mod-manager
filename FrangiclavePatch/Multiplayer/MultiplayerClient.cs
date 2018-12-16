using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Frangiclave.Multiplayer.Messages;
using TabletopUi.Scripts.Interfaces;
using UnityEngine.Networking;
using TabletopManager = Frangiclave.Patches.Assets.CS.TabletopUI.TabletopManager;

namespace Frangiclave.Multiplayer
{
    public class MultiplayerClient
    {
        public const string PresenceId = "mp.presence";

        public bool IsConnected => _client.isConnected && _isInRoom;

        private const short Port = 4987;

        private readonly NetworkClient _client;

        private readonly string _server;

        private readonly string _roomId;

        private bool _isInRoom;

        public MultiplayerClient(string server, string roomId)
        {
            _server = server;
            _roomId = roomId;
            _client = new NetworkClient();
            _client.RegisterHandler(MsgType.Connect, OnConnected);
            _client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
            _client.RegisterHandler(NoonMsgType.PartnerJoin, OnPartnerJoin);
            _client.RegisterHandler(NoonMsgType.PartnerLeave, OnPartnerLeave);
            _client.RegisterHandler(NoonMsgType.Situation, OnSituationReceived);
            _client.RegisterHandler(NoonMsgType.RoomJoin, OnRoomJoin);
            _client.Connect(_server, Port);
            _isInRoom = false;
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }

        public void SendSituation(string verbId, string recipeId)
        {
            var situationMessage = new SituationMessage()
            {
                VerbId = verbId,
                RecipeId = recipeId
            };
            _client.Send(NoonMsgType.Situation, situationMessage);
        }

        private void OnConnected(NetworkMessage message)
        {
            Logging.Info($"Connected to server '{_server}'");
            var enterRoomMessage = new RoomEnterMessage()
            {
                RoomId = _roomId
            };
            _client.Send(NoonMsgType.RoomEnter, enterRoomMessage);
        }

        private void OnDisconnected(NetworkMessage message)
        {
            Logging.Info($"Disconnected from server '{_server}'");
            _isInRoom = false;
        }

        private void OnRoomJoin(NetworkMessage message)
        {
            var roomJoinMessage = message.ReadMessage<RoomJoinMessage>();
            _isInRoom = roomJoinMessage.Success;
            if (_isInRoom)
            {
                Logging.Info($"Joined room '{_roomId}'");
                ShowNotification("The Way is Opened", $"I have entered room '{_roomId}'.");
            }
            else
            {
                Logging.Info($"Failed to join room '{_roomId}'");
                ShowNotification("The Way is Shut", $"I have been denied access to room '{_roomId}'.");
            }
        }

        private void OnPartnerJoin(NetworkMessage message)
        {
            Logging.Info("Partner joined");
            ShowNotification("A Presence Nears", "Foe? Friend? Time will tell.");
            AddPresenceCard();
        }

        private void OnPartnerLeave(NetworkMessage message)
        {
            Logging.Info("Partner left");
            ShowNotification("A Presence Departs", "Whoever they were, they are gone now.");
            RemovePresenceCard();
        }

        private void OnSituationReceived(NetworkMessage message)
        {
            var situationMessage = message.ReadMessage<SituationMessage>();
            Logging.Info(
                "New situation received: verb '" + situationMessage.VerbId +
                "' with recipe '" + situationMessage.RecipeId + "'");
            var tabletopManager = Registry.Retrieve<ITabletopManager>() as TabletopManager;
            if (tabletopManager == null)
                return;
            tabletopManager.ReceiveRemoteSituation(
                situationMessage.VerbId, situationMessage.RecipeId);
        }

        private static void ShowNotification(string title, string message)
        {
            Registry.Retrieve<INotifier>().ShowNotificationWindow(title, message);
        }

        void AddPresenceCard()
        {
            var tabletopManager = Registry.Retrieve<ITabletopManager>() as TabletopManager;
            if (tabletopManager == null)
                return;
            var tabletop = tabletopManager._tabletop;
            var stackManager = tabletop.GetElementStacksManager();

            var element = Registry.Retrieve<ICompendium>().GetElementById(PresenceId);
            if (element == null)
                return;
            stackManager.ModifyElementQuantity(
                PresenceId, 1, Source.Existing(), new Context(Context.ActionSource.Debug));
        }

        void RemovePresenceCard()
        {
            var tabletopManager = Registry.Retrieve<ITabletopManager>() as TabletopManager;
            if (tabletopManager == null)
                return;
            var tabletop = tabletopManager._tabletop;
            tabletop.GetElementStacksManager().ModifyElementQuantity(
                PresenceId, -1, Source.Existing(), new Context(Context.ActionSource.Debug));
        }
    }
}
