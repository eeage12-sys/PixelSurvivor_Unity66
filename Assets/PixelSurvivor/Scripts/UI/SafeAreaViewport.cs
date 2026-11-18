using UnityEngine;

namespace PixelSurvivor
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaViewport : MonoBehaviour
    {
        RectTransform rect;
        Rect lastArea;
        int lastWidth,lastHeight;
        void Awake(){rect=GetComponent<RectTransform>();Apply();}
        void Update(){if(lastArea!=Screen.safeArea || lastWidth!=Screen.width || lastHeight!=Screen.height)Apply();}
        void Apply()
        {
            lastArea=Screen.safeArea;lastWidth=Screen.width;lastHeight=Screen.height;
            if(rect==null || lastWidth<=0 || lastHeight<=0)return;
            rect.anchorMin=new Vector2(lastArea.xMin/lastWidth,lastArea.yMin/lastHeight);
            rect.anchorMax=new Vector2(lastArea.xMax/lastWidth,lastArea.yMax/lastHeight);
            rect.offsetMin=rect.offsetMax=Vector2.zero;
        }
    }
}
