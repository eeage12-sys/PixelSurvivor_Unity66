using UnityEngine;
using UnityEngine.UI;

namespace PixelSurvivor
{
    public sealed partial class CampaignUI
    {
        void BuildTouchControls()
        {
            if(!Application.isMobilePlatform)return;
            var pad=Box("Touch movement pad",hud.transform,Vector2.zero,new Vector2(190,190),new Color(.04f,.14f,.18f,.62f));
            RectTransform padRect=pad.rectTransform;
            padRect.anchorMin=padRect.anchorMax=Vector2.zero;
            padRect.pivot=new Vector2(.5f,.5f);
            padRect.anchoredPosition=new Vector2(130,135);
            var knob=Box("Touch movement knob",pad.transform,Vector2.zero,new Vector2(68,68),new Color(.42f,.91f,.82f,.86f));
            knob.raycastTarget=false;
            pad.gameObject.AddComponent<TouchJoystick>().Configure(padRect,knob.rectTransform);
        }
    }
}
