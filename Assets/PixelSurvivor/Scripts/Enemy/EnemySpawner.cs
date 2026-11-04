using System.Collections.Generic;
using UnityEngine;
namespace PixelSurvivor
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] Enemy enemyPrefab;
        [SerializeField] Enemy mothPrefab;
        [SerializeField] Enemy beetlePrefab;
        [SerializeField, Min(0)] float mothUnlockSeconds = 30;
        [SerializeField, Min(0)] float beetleUnlockSeconds = 90;
        public Enemy MothPrefab => mothPrefab;
        public Enemy BeetlePrefab => beetlePrefab;
        public bool HasExpandedRoster => enemyPrefab != null && mothPrefab != null && beetlePrefab != null;
        public void SetVariants(Enemy moth, Enemy beetle) { mothPrefab = moth; beetlePrefab = beetle; }
        // One random sample per spawn. Missing optional prefabs fall back to the slime.
        public Enemy SelectPrefab(float elapsed, float sample)
        {
            sample = Mathf.Clamp01(sample);
            if (elapsed >= beetleUnlockSeconds && sample < 0.2f && beetlePrefab != null) return beetlePrefab;
            float mothStart = elapsed >= beetleUnlockSeconds ? 0.2f : 0;
            if (elapsed >= mothUnlockSeconds && sample >= mothStart && sample < mothStart + 0.3f && mothPrefab != null) return mothPrefab;
            return enemyPrefab;
        }
        [Tooltip("One interval per minute, in seconds.")]
        [SerializeField] float[] spawnIntervals = { 1.5f, 1.2f, 0.9f, 0.7f, 0.5f };
        [Tooltip("Minimum distance from player; also kept outside the actual camera viewport.")]
        [SerializeField, Min(1)] float spawnDistance = 12;
        [SerializeField, Min(0.5f)] float screenPadding = 1.5f;
        [SerializeField, Min(1)] int maxEnemyCount = 120;
        readonly List<Enemy> enemies = new List<Enemy>();
        GameManager game;
        float countdown;
        public IReadOnlyList<Enemy> ActiveEnemies => enemies;
        public Enemy EnemyPrefab => enemyPrefab;
        public float CurrentInterval => GameRules.SpawnInterval(game != null ? game.Timer.Elapsed : 0, spawnIntervals);
        public void SetEnemy(Enemy prefab) => enemyPrefab = prefab;
        public void Initialize(GameManager owner) => game = owner;
        public void ResetSpawner() { enemies.Clear(); countdown = 0.5f; }
        public void Unregister(Enemy enemy) => enemies.Remove(enemy);
        void Update()
        {
            if (game == null || !game.IsPlaying || enemyPrefab == null || game.Campaign!=null) return;
            countdown -= Time.deltaTime;
            if (countdown > 0) return;
            countdown = CurrentInterval;
            if (enemies.Count < maxEnemyCount) SpawnAt(ChoosePosition(), SelectPrefab(game.Timer.Elapsed, Random.value));
        }
        public Vector2 ChooseSpawnPosition() => ChoosePosition();
        Vector2 ChoosePosition()
        {
            Camera camera = game.View;
            Vector2 center = camera.transform.position;
            float h = camera.orthographicSize + screenPadding, w = camera.orthographicSize * camera.aspect + screenPadding;
            Vector2 position;
            switch (Random.Range(0, 4))
            {
                case 0: position = center + new Vector2(-w, Random.Range(-h, h)); break;
                case 1: position = center + new Vector2(w, Random.Range(-h, h)); break;
                case 2: position = center + new Vector2(Random.Range(-w, w), -h); break;
                default: position = center + new Vector2(Random.Range(-w, w), h); break;
            }
            Vector2 offset = position - (Vector2)game.Player.transform.position;
            if (offset.magnitude < spawnDistance) position = (Vector2)game.Player.transform.position + offset.normalized * spawnDistance;
            return position;
        }
        public Enemy SpawnSpecial(Vector2 position,Enemy prefab)
        {
            if(game==null || prefab==null)return null;
            Enemy enemy=Instantiate(prefab,position,Quaternion.identity,game.RuntimeRoot);
            enemy.Initialize(game,this);enemies.Add(enemy);return enemy;
        }
        public Enemy SpawnAt(Vector2 position) => SpawnAt(position, enemyPrefab);
        public Enemy SpawnAt(Vector2 position, Enemy prefab)
        {
            if (game == null || prefab == null || enemies.Count >= maxEnemyCount) return null;
            Enemy enemy = Instantiate(prefab, position, Quaternion.identity, game.RuntimeRoot);
            enemy.Initialize(game, this); enemies.Add(enemy); return enemy;
        }
    }
}
