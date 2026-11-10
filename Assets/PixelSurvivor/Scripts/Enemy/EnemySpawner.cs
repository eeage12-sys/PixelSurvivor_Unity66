using System.Collections.Generic;
using UnityEngine;
namespace PixelSurvivor
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] Enemy enemyPrefab;
        [SerializeField] Enemy mothPrefab;
        [SerializeField] Enemy beetlePrefab;
        [SerializeField] Enemy boneCrawlerPrefab;
        [SerializeField] Enemy emberWraithPrefab;
        [SerializeField, Min(0)] float mothUnlockSeconds = 30;
        [SerializeField, Min(0)] float beetleUnlockSeconds = 90;
        public Enemy MothPrefab => mothPrefab;
        public Enemy BeetlePrefab => beetlePrefab;
        public Enemy BoneCrawlerPrefab => boneCrawlerPrefab;
        public Enemy EmberWraithPrefab => emberWraithPrefab;
        public bool HasExpandedRoster => enemyPrefab != null && mothPrefab != null && beetlePrefab != null;
        public bool HasFullRoster => HasExpandedRoster && boneCrawlerPrefab != null && emberWraithPrefab != null;
        public void SetVariants(Enemy moth, Enemy beetle) { mothPrefab = moth; beetlePrefab = beetle; }
        public void SetAdditionalVariants(Enemy boneCrawler, Enemy emberWraith) { boneCrawlerPrefab=boneCrawler; emberWraithPrefab=emberWraith; }
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
        public int MaxEnemyCount => maxEnemyCount;
        public void SetMaxEnemyCount(int value) => maxEnemyCount = Mathf.Max(1, value);
        readonly List<Enemy> enemies = new List<Enemy>();
        const float SeparationCellSize=0.8f;
        readonly Dictionary<long,List<Enemy>> nearby = new Dictionary<long,List<Enemy>>();
        readonly Stack<List<Enemy>> cellPool = new Stack<List<Enemy>>();
        int nearbyFrame=-1;
        GameManager game;
        float countdown;
        public IReadOnlyList<Enemy> ActiveEnemies => enemies;
        public Enemy EnemyPrefab => enemyPrefab;
        public float CurrentInterval => GameRules.SpawnInterval(game != null ? game.Timer.Elapsed : 0, spawnIntervals);
        public void SetEnemy(Enemy prefab) => enemyPrefab = prefab;
        public void Initialize(GameManager owner) => game = owner;
        public void ResetSpawner() { enemies.Clear(); countdown = 0.5f; nearbyFrame=-1; }
        public void Unregister(Enemy enemy) => enemies.Remove(enemy);
        static long CellKey(int x,int y) => ((long)x<<32)|(uint)y;
        public int SeparationCell(float coordinate) => Mathf.FloorToInt(coordinate/SeparationCellSize);
        public IReadOnlyList<Enemy> NearbyCell(int x,int y)
        {
            if(nearbyFrame!=Time.frameCount)
            {
                foreach(var cell in nearby.Values){cell.Clear();cellPool.Push(cell);}
                nearby.Clear();
                foreach(var enemy in enemies)
                {
                    if(enemy==null || !enemy.IsAlive)continue;
                    Vector2 position=enemy.transform.position;
                    long key=CellKey(SeparationCell(position.x),SeparationCell(position.y));
                    if(!nearby.TryGetValue(key,out var cell))
                    {cell=cellPool.Count>0?cellPool.Pop():new List<Enemy>(4);nearby.Add(key,cell);}
                    cell.Add(enemy);
                }
                nearbyFrame=Time.frameCount;
            }
            nearby.TryGetValue(CellKey(x,y),out var result);
            return result;
        }
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
            for(int attempt=0;attempt<20;attempt++)
            {
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
                if(game.Campaign==null || !game.Campaign.Terrain.IsBlocked(position,.5f))return position;
            }
            return game.Bounds.Clamp((Vector2)game.Player.transform.position+Vector2.right*(spawnDistance+4),1);
        }
        public Enemy SpawnSpecial(Vector2 position,Enemy prefab,bool boss=false)
        {
            if(game==null || prefab==null)return null;
            if(enemies.Count>=maxEnemyCount)
            {
                Enemy retire=null;
                foreach(var candidate in enemies)
                    if(candidate!=null && !candidate.IsBoss && !candidate.IsElite){retire=candidate;break;}
                if(retire==null && boss)
                    foreach(var candidate in enemies)
                        if(candidate!=null && !candidate.IsFinalBoss){retire=candidate;break;}
                if(retire==null)return null;
                retire.gameObject.SetActive(false);
                Destroy(retire.gameObject);
            }
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
