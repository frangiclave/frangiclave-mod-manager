using System.Diagnostics.CodeAnalysis;
using Assets.CS.TabletopUI;
using Frangiclave.Modding;
using MonoMod;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0626

namespace Frangiclave.Patches
{
    [MonoModPatch("global::MenuScreenController")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnassignedField.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class MenuScreenController : global::MenuScreenController
    {
        [MonoModIgnore] public new TextMeshProUGUI VersionNumber;

        [MonoModIgnore] public new MenuSubtitle Subtitle;

        [MonoModIgnore] public new Button purgeButton;

        private ModManager _modManager;
        private CanvasGroupFader _modsPanel;

        private extern void orig_InitialiseServices();

        private void InitialiseServices()
        {
            // Load all mods and add the manager to the registry for easier access
            var registry = new Registry();
            _modManager = new ModManager();
            _modManager.LoadAll();
            registry.Register(_modManager);
            orig_InitialiseServices();

            // Change the version number and subtitle to indicate the game has been modded
            VersionNumber.text += " [M]";
            Subtitle.SubtitleText.text += " [M]";
            Subtitle.SubtitleTextShadow.text += " [M]";

            // Create the Mods panel
            BuildModsButton();
            _modsPanel = BuildModsPanel();
        }

        private void SetupSkin()
        {
            GUI.skin.scrollView.normal.background = purgeButton.GetComponent<Texture2D>();
        }

        private void ShowModsPanel()
        {
            ShowOverlay(_modsPanel);
        }

        private void BuildModsButton()
        {
            Button modsButton = Instantiate(purgeButton);
            modsButton.transform.SetParent(purgeButton.transform.parent);
            modsButton.transform.localScale = new Vector3(1, 1, 1);
            modsButton.transform.SetSiblingIndex(4);
            modsButton.GetComponentInChildren<TextMeshProUGUI>().text = "MODS";
            foreach (var image in modsButton.GetComponentsInChildren<Image>())
                if (image.name == "TokenArt")
                    image.sprite = ResourcesManager.GetSpriteForAspect("knock");
            modsButton.onClick = new Button.ButtonClickedEvent();
            modsButton.onClick.AddListener(ShowModsPanel);
            modsButton.gameObject.SetActive(true);
        }

        private CanvasGroupFader BuildModsPanel()
        {
            CanvasGroupFader modsPanel = Instantiate(credits, credits.transform.parent, false);
            modsPanel.name = "OverlayWindow_Mods";

            // Set the title
            foreach (var text in modsPanel.GetComponentsInChildren<TextMeshProUGUI>())
                if (text.name == "TitleText")
                {
                    // Disable the Babelfish localization
                    text.GetComponent<Babelfish>().enabled = false;
                    text.text = "Mods";
                }

            // Set the icon
            foreach (var image in modsPanel.GetComponentsInChildren<Image>())
                if (image.name == "TitleArtwork")
                    image.sprite = ResourcesManager.GetSpriteForAspect("knock");

            // Clear the old content
            VerticalLayoutGroup content = modsPanel.GetComponentInChildren<VerticalLayoutGroup>();
            foreach (Transform child in content.transform)
                Destroy(child.gameObject);

            // Get the font to use for the contents
            TMP_FontAsset font = LanguageManager.Instance.GetFont(
                LanguageManager.eFontStyle.BodyText, LanguageTable.targetCulture);

            // Add the intro text
            TextMeshProUGUI modText = new GameObject("ModsIntro").AddComponent<TextMeshProUGUI>();
            modText.transform.SetParent(content.transform, false);
            modText.font = font;
            modText.fontSize = 24;
            modText.fontStyle = FontStyles.Bold;
            modText.alignment = TextAlignmentOptions.Center;
            modText.color = modText.faceColor = new Color32(193, 136, 232, 255);
            modText.margin = new Vector4(20, 20, 20, 10);
            modText.enableKerning = true;
            modText.text = "Currently active mods";

            // Add entries for each mod
            foreach (var mod in _modManager.Mods.Values)
                AddModEntry(mod.Id, $"{mod.Name} (v{mod.Version}), by {mod.Author}", font, content.transform);

            return modsPanel;
        }

        private static void AddModEntry(
            string id, string text, TMP_FontAsset font, Transform parent, bool withTopMargin = false)
        {
            // Display the mod's name
            TextMeshProUGUI modText = new GameObject("Mod_" + id).AddComponent<TextMeshProUGUI>();
            modText.transform.SetParent(parent, false);
            modText.font = font;
            modText.fontSize = 18;
            modText.color = modText.faceColor = new Color32(147, 225, 239, 255);
            modText.margin = new Vector4(25, withTopMargin ? 10 : 0, 20, 0);
            modText.enableKerning = true;
            modText.text = "- " + text;
        }

        private extern void orig_ShowOverlay(CanvasGroupFader overlay);

        private void ShowOverlay(CanvasGroupFader overlay)
        {
            orig_ShowOverlay(overlay);
        }
    }
}
