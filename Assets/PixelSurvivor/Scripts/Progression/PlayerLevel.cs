using UnityEngine;
namespace PixelSurvivor
{
    public sealed class PlayerLevel : MonoBehaviour
    {
        GameManager game;
        public int Level { get; private set; } = 1;
        public int Experience { get; private set; }
        public int RequiredEXP => GameRules.RequiredExperience(Level);
        public int PendingChoices { get; private set; }
        public void Initialize(GameManager owner) => game = owner;
        public void ResetLevel() { Level = 1; Experience = 0; PendingChoices = 0; }
        public void AddExperience(int amount)
        {
            if (game == null || !game.IsPlaying || amount <= 0) return;
            Experience += amount;
            while (Experience >= RequiredEXP)
            { Experience -= RequiredEXP; Level++; PendingChoices++; }
            if (PendingChoices > 0) game.RequestUpgrade();
        }
        public void ConsumeChoice() { PendingChoices = Mathf.Max(0, PendingChoices - 1); }
    }
}
