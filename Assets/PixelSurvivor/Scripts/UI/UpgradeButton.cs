using UnityEngine;
using UnityEngine.UI;
namespace PixelSurvivor
{
    public sealed class UpgradeButton : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] Text title, description, number;
        public Button Button => button;
        public bool IsConfigured => button != null && title != null && description != null && number != null;
        public void Configure(Button b, Text t, Text d, Text n) { button = b; title = t; description = d; number = n; }
        public void Present(UpgradeKind kind, PlayerStats stats, int index, GameManager game)
        {
            title.text = UpgradeSystem.Title(kind); description.text = UpgradeSystem.Description(kind, stats);
            number.text = $"0{index + 1}";
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => game.SelectUpgrade(index));
        }
    }
}
