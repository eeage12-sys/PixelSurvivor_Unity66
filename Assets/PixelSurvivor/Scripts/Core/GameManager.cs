using UnityEngine;
namespace PixelSurvivor
{
    public enum RunState { Menu, Playing, ChoosingUpgrade, GameOver, Victory }
    [DefaultExecutionOrder(-100)]
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] PlayerMovement player;
        [SerializeField] EnemySpawner spawner;
        [SerializeField] GameTimer timer;
        [SerializeField] MapBounds bounds;
        [SerializeField] Camera view;
        [SerializeField] Transform runtimeRoot;
        [SerializeField] GameUI gameUI;
        [SerializeField] LevelUpUI levelUpUI;
        [SerializeField] ResultUI resultUI;
        PlayerStats stats;
        PlayerHealth health;
        PlayerLevel level;
        AutoAttack weapon;
        UpgradeKind[] choices;
        public RunState State { get; private set; } = RunState.Menu;
        public bool IsPlaying => State == RunState.Playing;
        public int KillCount { get; private set; }
        public PlayerMovement Player => player;
        public PlayerStats Stats => stats;
        public PlayerHealth Health => health;
        public PlayerLevel Level => level;
        public EnemySpawner Spawner => spawner;
        public GameTimer Timer => timer;
        public MapBounds Bounds => bounds;
        public Camera View => view;
        public Transform RuntimeRoot => runtimeRoot;
        public UpgradeKind[] Choices => choices == null ? new UpgradeKind[0] : (UpgradeKind[])choices.Clone();
        public void Configure(PlayerMovement p, EnemySpawner s, GameTimer t, MapBounds b, Camera c,
            Transform root, GameUI ui, LevelUpUI up, ResultUI result)
        { player = p; spawner = s; timer = t; bounds = b; view = c; runtimeRoot = root; gameUI = ui; levelUpUI = up; resultUI = result; }
        public bool HasRequiredReferences()
        {
            return player != null && player.MoveAction != null && player.GetComponent<PlayerStats>() != null &&
                player.GetComponent<PlayerHealth>() != null && player.GetComponent<PlayerLevel>() != null &&
                player.GetComponent<AutoAttack>() != null && player.GetComponent<AutoAttack>().ProjectilePrefab != null &&
                spawner != null && spawner.EnemyPrefab != null && spawner.EnemyPrefab.GemPrefab != null &&
                timer != null && bounds != null && view != null && runtimeRoot != null &&
                gameUI != null && levelUpUI != null && resultUI != null &&
                gameUI.IsConfigured && levelUpUI.IsConfigured && resultUI.IsConfigured;
        }
        void Awake()
        {
            Time.timeScale = 1;
            Application.targetFrameRate = 60;
            if (!HasRequiredReferences())
            { Debug.LogError("Pixel Survivor: missing reference. Run Tools > Pixel Survivor > Setup / Rebuild MVP Scene.", this); enabled = false; return; }
            stats = player.GetComponent<PlayerStats>(); health = player.GetComponent<PlayerHealth>();
            level = player.GetComponent<PlayerLevel>(); weapon = player.GetComponent<AutoAttack>();
            player.Initialize(this); health.Initialize(this); level.Initialize(this); weapon.Initialize(this); spawner.Initialize(this);
            gameUI.Bind(this); levelUpUI.Bind(this); resultUI.Bind(this);
            ResetRun();
            State = RunState.Menu; Time.timeScale = 0;
            gameUI.ShowMenu(); levelUpUI.Hide(); resultUI.Hide();
        }
        void ResetRun()
        {
            // Disable first so no pending object can damage/award EXP during the next run.
            for (int i = runtimeRoot.childCount - 1; i >= 0; i--)
            { GameObject item = runtimeRoot.GetChild(i).gameObject; item.SetActive(false); Destroy(item); }
            stats.ResetStats(); health.ResetHealth(); level.ResetLevel(); weapon.ResetWeapon();
            timer.ResetTimer(); spawner.ResetSpawner(); KillCount = 0; choices = null;
            player.transform.position = bounds.transform.position;
            CameraFollow follow = view.GetComponent<CameraFollow>();
            if (follow != null) follow.Snap();
        }
        public void BeginRun()
        {
            if (!enabled || stats == null) return;
            ResetRun(); State = RunState.Playing; Time.timeScale = 1;
            gameUI.ShowGameplay(); levelUpUI.Hide(); resultUI.Hide();
        }
        void Update()
        {
            if (IsPlaying && timer.Tick(Time.deltaTime)) EndRun(true);
        }
        public void RegisterKill() { if (IsPlaying) KillCount++; }
        public void RequestUpgrade()
        {
            if (!IsPlaying || level.PendingChoices <= 0) return;
            State = RunState.ChoosingUpgrade; Time.timeScale = 0;
            choices = UpgradeSystem.Roll(stats); levelUpUI.Show(choices, stats, level.Level);
        }
        public void SelectUpgrade(int index)
        {
            if (State != RunState.ChoosingUpgrade || choices == null || index < 0 || index >= choices.Length) return;
            UpgradeKind chosen = choices[index];
            stats.Apply(chosen);
            if (chosen == UpgradeKind.MaxHP) health.Heal(20);
            level.ConsumeChoice(); choices = null;
            levelUpUI.Hide(); State = RunState.Playing;
            if (level.PendingChoices > 0) RequestUpgrade();
            else Time.timeScale = 1;
        }
        public void EndRun(bool survived)
        {
            if (!IsPlaying) return;
            State = survived ? RunState.Victory : RunState.GameOver;
            Time.timeScale = 0; levelUpUI.Hide(); resultUI.Show(survived, timer.Elapsed, level.Level, KillCount);
        }
        void OnDestroy() => Time.timeScale = 1;
    }
}
