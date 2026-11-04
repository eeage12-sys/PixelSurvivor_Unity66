using UnityEngine;
namespace PixelSurvivor
{
    public sealed class ExperienceGem : MonoBehaviour
    {
        [SerializeField, Min(1)] int experience = 1;
        [SerializeField, Min(0.1f)] float pickupRadius = 0.65f;
        [SerializeField, Min(0.1f)] float magnetRadius = 1.65f;
        [SerializeField, Min(0.1f)] float magnetSpeed = 9;
        GameManager game;
        bool collected;
        bool globalMagnet;
        public void Attract() => globalMagnet=true;
        public void SetExperience(int amount) {experience=Mathf.Max(1,amount);}
        public void Initialize(GameManager owner) { game = owner; collected = false; }
        void Update()
        {
            if (game == null || !game.IsPlaying || collected) return;
            Vector2 target = game.Player.transform.position;
            float distance = Vector2.Distance(transform.position, target);
            if (distance <= pickupRadius)
            {
                collected = true;
                gameObject.SetActive(false);
                game.Level.AddExperience(experience);
                Destroy(gameObject);
            }
            else if (globalMagnet || distance <= magnetRadius*(game.Campaign!=null ? game.Campaign.PickupFactor : 1))
                transform.position = Vector2.MoveTowards(transform.position, target, magnetSpeed * Time.deltaTime);
        }
    }
}
