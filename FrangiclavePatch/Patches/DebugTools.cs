using System.Diagnostics.CodeAnalysis;
using Assets.CS.TabletopUI;
using Frangiclave.Modding;
using MonoMod;
using TabletopUi.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using TabletopManager = Frangiclave.Patches.Assets.CS.TabletopUI.TabletopManager;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace Frangiclave.Patches
{
    [MonoModPatch("global::DebugTools")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class DebugTools : global::DebugTools
    {
        [MonoModIgnore] private InputField input;

        [MonoModIgnore] private Button btnResetAchivement;

        private Button _btnJoinServer;

        public extern void orig_Awake();

        public new void Awake()
        {
            orig_Awake();

            // Add a Join Server button
            _btnJoinServer = Instantiate(btnResetAchivement, btnResetAchivement.transform.parent, false);
            var oldBtnPosition = _btnJoinServer.GetComponent<RectTransform>().localPosition;
            _btnJoinServer.GetComponent<RectTransform>().localPosition = new Vector2(oldBtnPosition.x, oldBtnPosition.y + 32);
            _btnJoinServer.GetComponentInChildren<Text>().text = "Join Server";
            _btnJoinServer.onClick.AddListener(() => JoinServer(input.text));

        }

        private extern void orig_UpdateCompendiumContent();

        private void UpdateCompendiumContent()
        {
            Registry.Retrieve<ModManager>().LoadAll();
            orig_UpdateCompendiumContent();
        }

        private void JoinServer(string serverUriString)
        {
            string[] serverUri = serverUriString.Split('/');
            if (serverUri.Length != 2)
            {
                Logging.Info($"Invalid server URI: '{serverUriString}'");
                return;
            }

            var tabletopManager = Registry.Retrieve<ITabletopManager>() as TabletopManager;
            if (tabletopManager != null)
                tabletopManager.JoinServer(serverUri[0], serverUri[1]);
        }

    }
}
