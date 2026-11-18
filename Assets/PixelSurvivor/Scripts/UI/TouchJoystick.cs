using UnityEngine;
using UnityEngine.EventSystems;

namespace PixelSurvivor
{
    public sealed class TouchJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        RectTransform pad, knob;
        int pointerId=int.MinValue;
        static Vector2 direction;
        public static Vector2 Direction => direction;
        public void Configure(RectTransform padTransform,RectTransform knobTransform)
        { pad=padTransform;knob=knobTransform; }
        public void OnPointerDown(PointerEventData eventData)
        { if(pointerId!=int.MinValue)return;pointerId=eventData.pointerId;OnDrag(eventData); }
        public void OnDrag(PointerEventData eventData)
        {
            if(pointerId!=eventData.pointerId || pad==null || knob==null)return;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(pad,eventData.position,eventData.pressEventCamera,out Vector2 local))return;
            float radius=pad.rect.width*.35f;
            direction=Vector2.ClampMagnitude(local/Mathf.Max(1,radius),1);
            knob.anchoredPosition=direction*radius;
        }
        public void OnPointerUp(PointerEventData eventData)
        { if(pointerId==eventData.pointerId)ResetStick(); }
        void OnDisable(){ResetStick();}
        void ResetStick(){pointerId=int.MinValue;direction=Vector2.zero;if(knob!=null)knob.anchoredPosition=Vector2.zero;}
    }
}
