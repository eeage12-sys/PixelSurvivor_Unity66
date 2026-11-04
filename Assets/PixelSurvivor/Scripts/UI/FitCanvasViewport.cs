using UnityEngine;
using UnityEngine.UI;
namespace PixelSurvivor
{
    [RequireComponent(typeof(CanvasScaler))]
    public sealed class FitCanvasViewport : MonoBehaviour
    {
        CanvasScaler scaler;
        void Awake(){scaler=GetComponent<CanvasScaler>();Fit();}
        void Update(){Fit();}
        void Fit(){if(scaler!=null)scaler.matchWidthOrHeight=Screen.width/(float)Mathf.Max(1,Screen.height)<1600f/900f?0:1;}
    }
}
