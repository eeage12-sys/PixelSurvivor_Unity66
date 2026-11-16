using UnityEngine;
namespace PixelSurvivor
{
    // Reinforcements join the existing roster; no scene/prefab regeneration is needed.
    public static class StageReinforcements
    {
        static readonly string[] names={"가시 버섯","들판 멧돼지","호박 말벌","수정 달팽이","유령 거미","폐허 석상"};
        static readonly float[] health={24,85,22,95,30,125};
        static readonly float[] speed={2.25f,1.65f,3.6f,1.25f,3.2f,1.35f};
        static readonly float[] damage={10,18,12,19,14,23};
        static readonly Color[] colors={new Color(.88f,.42f,.52f),new Color(.72f,.51f,.3f),new Color(1f,.72f,.22f),new Color(.35f,.85f,.9f),new Color(.68f,.52f,.94f),new Color(.65f,.76f,.82f)};
        static readonly Sprite[] sprites=new Sprite[6];
        static readonly string[] shapes={
            ".......+......../...+..###..+..../....#######...../...#########..../..##++###++##.../.#############../.#############../....+#####+...../.....#####....../....##oo###...../....#######...../...##.....##..../................",
            "...##......##.../...###....###.../....########..../...##########.../..##+######+##../.##############./.###oo####oo###./..############../...##++++++##.../...##o####o##.../....########..../....##....##..../...###....###...",
            "..++........++../.++++......++++./.+++++.##.+++++./..++++####++++../.....######...../....##oooo##..../...##########.../....++++++++..../....########..../....++++++++..../.....######...../......####....../.......##.......",
            ".......++......./.....++++++...../....+######+..../...+##++++##+.../..+##+####+##+../..+##+#++#+##+../..+##+####+##+../...+##++++##+.../....+######+..../..############../.##oo##########./.##############./..############../................",
            "..##........##../...##......##.../.##.##.##.##.##./..############../....########..../...##+oooo+##.../...##########.../....########..../..############../.##.##.##.##.##./...##......##.../..##........##../................",
            ".....++++++...../....+######+..../....##oooo##..../....########..../.....######...../..############../.###++++++++###./.##############./.###.######.###./.###.######.###./.....######...../....###..###..../...####..####..."
        };
        public const float FirstUnlock=120, SecondUnlock=360;
        public static int Select(int stage,float elapsed,float sample)
        {
            if(stage<0 || stage>2 || elapsed<FirstUnlock)return -1;
            if(elapsed>=SecondUnlock && sample<.16f)return stage*2+1;
            if(sample<(elapsed>=SecondUnlock?.38f:.22f))return stage*2;
            return -1;
        }
        public static Sprite Icon(int index)
        {
            if(sprites[index]==null)sprites[index]=FieldPickupArt.Pixel(shapes[index],colors[index],names[index]);
            return sprites[index];
        }
        public static void Apply(Enemy enemy,int index)
        {
            enemy.name=names[index];
            enemy.ConfigureStats(health[index],speed[index],damage[index],index%2==0?.36f:.48f);
            enemy.SetAppearance(Icon(index));
            var feedback=enemy.GetComponent<HitFeedbackVisual>();
            if(feedback!=null)feedback.ConfigureMotion(index==2 || index==4);
        }
    }
}
