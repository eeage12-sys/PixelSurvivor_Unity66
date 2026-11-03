using UnityEngine;
using UnityEngine.EventSystems;
namespace PixelSurvivor
{
    public sealed class LevelUpUI : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] UpgradeButton[] cards;
        GameManager game;
        public bool IsConfigured => panel != null && cards != null && cards.Length == 3 &&
            cards[0] != null && cards[1] != null && cards[2] != null && cards[0].IsConfigured && cards[1].IsConfigured && cards[2].IsConfigured;
        public void Configure(GameObject p, UpgradeButton[] buttons) { panel = p; cards = buttons; }
        public void Bind(GameManager owner) => game = owner;
        public void Show(UpgradeKind[] choices, PlayerStats stats, int level)
        {
            panel.SetActive(true);
            for (int i = 0; i < 3; i++) cards[i].Present(choices[i], stats, i, game);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(cards[0].gameObject);
        }
        public void Hide() => panel.SetActive(false);
    }
}
