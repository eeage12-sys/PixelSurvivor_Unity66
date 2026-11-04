using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
namespace PixelSurvivor.Editor
{
    public static class KoreanSetup
    {
        public const string FontPath=SurvivorSetup.Root+"/Resources/Fonts/NotoSansCJKkr-Regular.otf";
        public static Font EnsureFont()
        {
            AssetDatabase.ImportAsset(FontPath);
            var importer=AssetImporter.GetAtPath(FontPath) as TrueTypeFontImporter;
            if(importer==null)throw new System.InvalidOperationException("한글 폰트가 없습니다. Resources/Fonts 폴더까지 모두 복사하세요.");
            if(importer.fontTextureCase!=FontTextureCase.Dynamic || !importer.includeFontData)
            {importer.fontTextureCase=FontTextureCase.Dynamic;importer.includeFontData=true;importer.SaveAndReimport();}
            var font=AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if(font==null)throw new System.InvalidOperationException("한글 폰트 임포트가 끝난 뒤 다시 실행하세요.");
            return font;
        }
        public static string Legacy(string text)
        {
            switch(text)
            {
                case "TIME REMAINING":return "생존 시간";
                case "LEVEL 01":return "레벨 01";
                case "HP 100 / 100":return "체력 100 / 100";
                case "KILLS 000":return "처치 000";
                case "EXP 0 / 5":return "경험치 0 / 5";
                case "A FIVE-MINUTE SURVIVAL RUN":case "INFINITE SURVIVAL":return "무한 생존";
                case "PIXEL SURVIVOR":return "픽셀 서바이버";
                case "L U M E N   F I E L D":return "빛의 들판";
                case "START":return "시작";
                case "RESTART":return "다시 시작";
                case "SURVIVED!":case "GAME OVER":return "게임 종료";
                case "05:00":return "00:00:00";
                case "LEVEL UP":return "레벨 상승";
                case "CHOOSE ONE UPGRADE  /  TIME IS PAUSED":return "장비 하나를 선택하세요 · 전투 일시정지 중";
                case "Click a card, or use Left / Right + Enter":return "카드를 클릭하거나 좌우 방향키와 Enter로 선택하세요";
                case "Click a button, or use arrow keys + Enter":return "버튼을 클릭하거나 방향키와 Enter로 선택하세요";
                case "POWER CORE":return "힘의 문장";
                case "Damage +20%\n10 > 12":return "공격력 +20%\n10 > 12";
            }
            if(text!=null && (text.StartsWith("WASD to move") || text.StartsWith("WASD or arrow keys")))
                return "WASD 또는 방향키로 이동 · 자동 공격\n경험치를 모으고 장비를 조합해 오래 생존하세요.";
            return text;
        }
        public static void ApplyScene(GameManager game)
        {
            var font=EnsureFont();
            foreach(var ui in Object.FindObjectsByType<GameUI>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                if(ui.gameObject.scene!=game.gameObject.scene)continue;
                foreach(var label in ui.GetComponentsInChildren<Text>(true))
                {
                    label.font=font;label.text=Legacy(label.text);
                    label.resizeTextForBestFit=true;label.resizeTextMinSize=Mathf.Max(14,label.fontSize-4);label.resizeTextMaxSize=label.fontSize;
                    EditorUtility.SetDirty(label);
                    if(PrefabUtility.IsPartOfPrefabInstance(label))PrefabUtility.RecordPrefabInstancePropertyModifications(label);
                }
            }
        }
        [MenuItem("Tools/Pixel Survivor/한국어 적용 (Apply Korean)",false,13)]
        public static void ApplyMenu()
        {
            CampaignSetup.ApplyMenu();
        }
    }
}
