using UnityEngine;
using UnityEngine.UI;
namespace PixelSurvivor
{
    public sealed partial class CampaignUI
    {
        Image pickupPanel,pickupIcon;
        Text pickupName,pickupDescription;
        void BuildPickupHint()
        {
            pickupPanel=Box("Nearby pickup",hud.transform,new Vector2(0,60),new Vector2(500,66),Ink);
            var rect=pickupPanel.rectTransform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,0);
            pickupPanel.raycastTarget=false;
            pickupIcon=Icon(pickupPanel.transform,null,-215,0,40,Color.white);
            pickupName=TextAt(pickupPanel.transform,"",20,15,400,28,22,Mint);
            pickupDescription=TextAt(pickupPanel.transform,"",20,-15,420,26,18,Color.white);
            pickupPanel.gameObject.SetActive(false);
        }
        void TickPickupHint()
        {
            if(pickupPanel==null)return;
            FieldItem nearest=null;float best=9;
            if(c.Game.IsPlaying && !c.Stage.DoomAppeared)
                foreach(var item in c.Field.Items)
                {
                    if(item==null)continue;
                    float distance=((Vector2)item.transform.position-(Vector2)c.Game.Player.transform.position).sqrMagnitude;
                    if(distance<best){best=distance;nearest=item;}
                }
            pickupPanel.gameObject.SetActive(nearest!=null);
            if(nearest==null)return;
            var kind=nearest.Kind;
            pickupIcon.sprite=FieldPickupArt.Icon(kind);
            pickupName.text=FieldPickupArt.Label(kind);pickupName.color=FieldPickupArt.Accent(kind);
            pickupDescription.text=FieldPickupArt.Description(kind);
        }
    }
}
