using System.Diagnostics.CodeAnalysis;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using MonoMod;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0626
#pragma warning disable CS0649

namespace Frangiclave.Patches.Assets.CS.TabletopUI
{
    [MonoModPatch("global::Assets.CS.TabletopUI.SituationToken")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SituationToken : global::Assets.CS.TabletopUI.SituationToken
    {
        public bool SendingAway { get; private set; }

        [MonoModIgnore] private Image countdownBar;

        private CanvasGroupFader _fader;

        public extern void orig_Initialise(IVerb verb, SituationController sc, Heart heart);

        public new void Initialise(IVerb verb, SituationController sc, Heart heart)
        {
            orig_Initialise(verb, sc, heart);
            _fader = gameObject.AddComponent<CanvasGroupFader>();
            SendingAway = false;
        }

        private extern bool orig_Retire();

        public override bool Retire()
        {
            return Retire(true);
        }

        public bool Retire(bool withEffect)
        {
            return withEffect ? orig_Retire() : base.Retire();
        }

        public void SetRemote(bool isSending)
        {
            SendingAway = isSending;
            var countdownImage = countdownBar.GetComponent<Image>();
            countdownImage.color = new Color(161, 151, 151);
            _fader.durationTurnOn = 3.0f;
            _fader.durationTurnOff = 3.0f;
            _fader.destroyOnHide = false;
            if (isSending)
            {
                _fader.keepActiveOnHide = false;
                _fader.Hide();
            }
            else
            {
                _fader.keepActiveOnHide = true;
                _fader.SetAlpha(0.0f);
                _fader.Show();
            }
        }
    }
}
