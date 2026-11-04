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
        public void Bind(GameManager owner) { game = owner; foreach(var label in GetComponentsInChildren<Text>(true))label.font=KoreanText.Font; startButton.onClick.AddListener(game.BeginRun); }
        public void ShowMenu()
        {
            mainMenuPanel.SetActive(game.Campaign==null); gameplayPanel.SetActive(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }
        public void ShowGameplay()
        {
            mainMenuPanel.SetActive(false); gameplayPanel.SetActive(true);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }
        void LateUpdate()
        {
            if (game == null || game.Stats == null) return;
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
