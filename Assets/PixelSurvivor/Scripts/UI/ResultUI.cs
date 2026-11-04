using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace PixelSurvivor
{
    public sealed class ResultUI : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] Text title, summary;
        [SerializeField] Button restartButton;
        public bool IsConfigured => panel != null && title != null && summary != null && restartButton != null;
        public void Configure(GameObject p, Text t, Text s, Button restart) { panel = p; title = t; summary = s; restartButton = restart; }
        public void Bind(GameManager game) => restartButton.onClick.AddListener(game.BeginRun);
        public void Hide() => panel.SetActive(false);
        public void Show(bool won, float elapsed, int level, int kills)
        {
            panel.SetActive(true); title.text = "게임 종료";
            title.color = new Color(1, 0.45f, 0.5f);
            summary.text = $"생존 시간    {SurvivalRules.Clock(elapsed)}\n\n레벨    {level:00}         처치    {kills:000}";
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(restartButton.gameObject);
        }
    }
}
