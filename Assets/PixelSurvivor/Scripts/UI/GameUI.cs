using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace PixelSurvivor
{
    public sealed class GameUI : MonoBehaviour
    {
        [SerializeField] GameObject mainMenuPanel, gameplayPanel;
        [SerializeField] Button startButton;
        [SerializeField] Text levelText, timerText, hpText, expText, killsText;
        [SerializeField] Image hpFill, expFill;
        GameManager game;
        public bool IsConfigured => mainMenuPanel != null && gameplayPanel != null && startButton != null &&
            levelText != null && timerText != null && hpText != null && expText != null && killsText != null && hpFill != null && expFill != null;
        public void Configure(GameObject menu, GameObject hud, Button start, Text level, Text timer, Text hp,
            Text exp, Text kills, Image healthFill, Image experienceFill)
        { mainMenuPanel = menu; gameplayPanel = hud; startButton = start; levelText = level; timerText = timer;
          hpText = hp; expText = exp; killsText = kills; hpFill = healthFill; expFill = experienceFill; }
        // Restores the saved edit-mode menu, including projects whose old upgrade cleared labels.
        public void PrepareEditorPreview(Font font)
        {
            if(mainMenuPanel==null)return;
            mainMenuPanel.SetActive(true);gameplayPanel.SetActive(false);
            foreach(Transform child in transform)
                if(child.name=="LevelUpPanel" || child.name=="ResultPanel")child.gameObject.SetActive(false);
            foreach(var label in mainMenuPanel.GetComponentsInChildren<Text>(true))
            {
                label.font=font;label.material=null;label.color=Color.white;
                label.verticalOverflow=VerticalWrapMode.Overflow;
                switch(label.gameObject.name)
                {
                    case "Title":label.text="픽셀 서바이버";label.fontSize=64;break;
                    case "Eyebrow":label.text="무한 생존";break;
                    case "Subtitle":label.text="게임 미리보기";break;
                    case "Instructions":label.text="Unity 상단 ▶ Play를 누르면 게임 메뉴가 열립니다.\nWASD 이동 · 자동 공격 · ESC 캐릭터 정보 / 옵션";break;
                    case "Menu help":label.text="실행 전에는 안내 화면이 표시됩니다.";break;
                }
            }
            var caption=startButton.GetComponentInChildren<Text>(true);
            if(caption!=null)caption.text="▶ Play로 시작";
        }
        public void Bind(GameManager owner) { game = owner; foreach(var label in GetComponentsInChildren<Text>(true))label.font=KoreanText.Font; startButton.onClick.AddListener(game.BeginRun); }
        public void ShowMenu()
        {
            mainMenuPanel.SetActive(game.Campaign==null); gameplayPanel.SetActive(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }
        public void ShowGameplay()
        {
            mainMenuPanel.SetActive(false); gameplayPanel.SetActive(game.Campaign==null);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }
        void LateUpdate()
        {
            if (game == null || game.Stats == null || game.Campaign!=null) return;
            levelText.text = $"레벨 {game.Level.Level:00}";
            timerText.text = SurvivalRules.Clock(game.Campaign!=null ? game.Campaign.Stage.Elapsed : game.Timer.Elapsed);
            hpText.text = $"체력  {Mathf.CeilToInt(game.Health.CurrentHP)} / {game.Stats.MaxHP:0}";
            expText.text = $"경험치  {game.Level.Experience} / {game.Level.RequiredEXP}";
            killsText.text = $"처치  {game.KillCount:000}";
            hpFill.fillAmount = game.Health.CurrentHP / game.Stats.MaxHP;
            expFill.fillAmount = (float)game.Level.Experience / game.Level.RequiredEXP;
        }
    }
}
