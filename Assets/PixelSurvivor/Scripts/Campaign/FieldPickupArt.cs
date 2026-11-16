using UnityEngine;
namespace PixelSurvivor
{
    // Shared, cached pixel icons: shape as well as color identifies every pickup.
    public static class FieldPickupArt
    {
        static readonly Sprite[] icons=new Sprite[7];
        static readonly string[] shapes={
            "................ /......++++....../....++++++++..../...++######++.../...+##++++###+.../..+##++##++##+../..+##++#+++##+../..+##++##++##+../..+##+++##+##+../...+##++++###+.../...++######++.../....++++++++..../......++++....../................",
            "................ /..+++......+++../..+#+......+#+../..+#+......+#+../..+#+......+#+../..+#+......+#+../..+#+......+#+../..+##+....+##+../...+##++++##+.../...+########+.../....+######+..../.....++++++...../................ /................",
            "..........+...../.........+.+..../.......##.+...../......#++#....../.....######...../...##########.../..###++++#####../..##+#########../..############../..############../...##########.../....########..../......####....../................",
            ".....######...../....#++++++#..../....#+####+#..../.....######...../......####....../....########..../...##++++++##.../...##++##++##.../...#++####++#.../...#+++##+++#.../...##++++++##.../....########..../................ /................",
            "...........+..../.........+.+..../..........+...../......++++....../.....+####+...../....+######+..../....+##++##+..../....+##++##+..../....+######+..../.....+####+...../.....+####+...../....++++++++..../................ /................",
            "................ /................ /...##########.../..#++++++++++#../..#+########+#../..############../..+++++##+++++../..#####++#####../..#####++#####../..############../...##########.../................ /................ /................",
            ".....+....+...../....+++..+++..../...++++++++++.../..############../..#++++++++++#../..#+########+#../..############../..+++++##+++++../..#####++#####../..#####++#####../..############../...##########.../................ /................"
        };
        public static Color Accent(FieldItemKind kind)
        {
            switch(kind)
            {
                case FieldItemKind.Gold:return new Color(1f,.77f,.2f);
                case FieldItemKind.Magnet:return new Color(.2f,.85f,1f);
                case FieldItemKind.Bomb:return new Color(1f,.4f,.18f);
                case FieldItemKind.Heal:return new Color(.35f,1f,.6f);
                case FieldItemKind.Support:return new Color(.8f,.58f,1f);
                case FieldItemKind.EliteChest:return new Color(.4f,1f,.8f);
                default:return new Color(1f,.7f,.2f);
            }
        }
        public static string Label(FieldItemKind kind)
        {
            switch(kind)
            {
                case FieldItemKind.Gold:return "골드";
                case FieldItemKind.Magnet:return "자석";
                case FieldItemKind.Bomb:return "폭탄";
                case FieldItemKind.Heal:return "회복";
                case FieldItemKind.Support:return "지원 호출";
                case FieldItemKind.EliteChest:return "정예 상자";
                default:return "보스 상자";
            }
        }
        public static string Description(FieldItemKind kind)
        {
            switch(kind)
            {
                case FieldItemKind.Gold:return "영구 강화에 사용하는 골드";
                case FieldItemKind.Magnet:return "필드의 경험치와 아이템 끌어오기";
                case FieldItemKind.Bomb:return "일반 몬스터 처치 · 정예와 보스에 피해";
                case FieldItemKind.Heal:return "체력 즉시 회복";
                case FieldItemKind.Support:return "공격·회복·방어 지원군 중 하나 소환";
                case FieldItemKind.EliteChest:return "장비 강화 또는 진화 · 잭팟 가능";
                default:return "장비 강화 또는 진화 · 보상 3회";
            }
        }
        public static Sprite Icon(FieldItemKind kind)
        {
            int index=(int)kind;
            if(icons[index]==null)icons[index]=Pixel(shapes[index],Accent(kind),Label(kind));
            return icons[index];
        }
        internal static Sprite Pixel(string pattern,Color color,string name)
        {
            string[] rows=pattern.Replace(" ","").Split('/');
            int width=0;foreach(string row in rows)width=Mathf.Max(width,row.Length);
            var texture=new Texture2D(width+2,rows.Length+2,TextureFormat.RGBA32,false);
            texture.name=name;texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;
            var pixels=new Color[(width+2)*(rows.Length+2)];
            for(int y=0;y<rows.Length;y++)for(int x=0;x<rows[y].Length;x++)
            {
                char ch=rows[y][x];if(ch=='.')continue;
                int px=x+1,py=rows.Length-y;
                for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)
                    pixels[(py+dy)*(width+2)+px+dx]=new Color(.035f,.05f,.075f,1);
            }
            for(int y=0;y<rows.Length;y++)for(int x=0;x<rows[y].Length;x++)
            {
                char ch=rows[y][x];if(ch=='.')continue;
                pixels[(rows.Length-y)*(width+2)+x+1]=ch=='+'?Color.Lerp(color,Color.white,.65f):ch=='o'?new Color(.06f,.08f,.12f):color;
            }
            texture.SetPixels(pixels);texture.Apply(false,false);
            return Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),18);
        }
    }
}
