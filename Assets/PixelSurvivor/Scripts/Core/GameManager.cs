using UnityEngine;
namespace PixelSurvivor
{
    public enum RunState { Menu, Playing, ChoosingUpgrade, GameOver, Treasure=5, Paused=6 }
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
        public CampaignController Campaign {get;private set;}
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
            { Debug.LogError("Pixel Survivor: missing reference. Run Tools > Pixel Survivor > Setup - Rebuild Game Scene.", this); enabled = false; return; }
            stats = player.GetComponent<PlayerStats>(); health = player.GetComponent<PlayerHealth>();
            level = player.GetComponent<PlayerLevel>(); weapon = player.GetComponent<AutoAttack>();
            player.Initialize(this); health.Initialize(this); level.Initialize(this); weapon.Initialize(this); spawner.Initialize(this);
            gameUI.Bind(this); levelUpUI.Bind(this); resultUI.Bind(this);
            CampaignController candidate=GetComponent<CampaignController>();
            if(candidate==null || !candidate.IsConfigured)
            {Debug.LogError("Campaign data is incomplete. Run Apply Infinite Survival Update and check Assets/PixelSurvivor/Data.",this);enabled=false;return;}
            if(candidate!=null){Campaign=candidate;Campaign.Initialize(this);}
            ResetRun();
            State = RunState.Menu; Time.timeScale = 0;
            gameUI.ShowMenu(); levelUpUI.Hide(); resultUI.Hide();
            if(Campaign!=null)Campaign.UI.ShowMenu();
        }
        void ResetRun()
        {
            // Disable first so no pending object can damage/award EXP during the next run.
            for (int i = runtimeRoot.childCount - 1; i >= 0; i--)
            { GameObject item = runtimeRoot.GetChild(i).gameObject; item.SetActive(false); Destroy(item); }
            stats.ResetStats(); health.ResetHealth(); level.ResetLevel(); weapon.ResetWeapon();
            timer.ResetTimer(); spawner.ResetSpawner(); KillCount = 0; choices = null;
            player.transform.position = bounds.transform.position;
            PlayerSpriteAnimator animation = player.GetComponent<PlayerSpriteAnimator>();
            if (animation != null) animation.ResetAnimation();
            CameraFollow follow = view.GetComponent<CameraFollow>();
            if (follow != null) follow.Snap();
        }
        public void BeginRun()
        {
            if (!enabled || stats == null || Campaign==null || !Campaign.HasCharacterSelection || (State!=RunState.Menu && State!=RunState.GameOver)) return;
            ResetRun(); State = RunState.Playing; Time.timeScale = 1;
            gameUI.ShowGameplay(); levelUpUI.Hide(); resultUI.Hide();
            if(Campaign!=null)Campaign.BeginRun();
        }
        void Update()
        {
            if(Campaign!=null)Campaign.UI.HandlePauseInput();
            if(IsPlaying)
            {
                if(Campaign!=null){timer.AdvanceCampaign(Time.deltaTime);Campaign.Tick(Time.deltaTime);}
            }
        }
        public void RegisterKill() { if (IsPlaying) {KillCount++;if(Campaign!=null)Campaign.OnKill();} }
        public void RequestUpgrade()
        {
            if (!IsPlaying || level.PendingChoices <= 0 || Campaign==null) return;
            if(!Campaign.HasEquipmentChoices)
            {
                while(level.PendingChoices>0)level.ConsumeChoice();
                Campaign.UI.Notify("모든 장비 최대 레벨 · 레벨은 계속 상승합니다. 진화는 상자에서 진행하세요.");
                return;
            }
            State=RunState.ChoosingUpgrade; Time.timeScale=0; Campaign.ShowChoices();
        }
        public void SelectUpgrade(int index)
        {
            if(State!=RunState.ChoosingUpgrade || Campaign==null || !Campaign.SelectChoice(index))return;
            level.ConsumeChoice(); State=RunState.Playing;Time.timeScale=1;
            if(level.PendingChoices>0)RequestUpgrade();
        }
        public void EndRun(bool survived)
        {
            if (!IsPlaying || survived) return;
            State = RunState.GameOver;
            Time.timeScale = 0; levelUpUI.Hide();
            if(Campaign!=null)Campaign.Finish(survived);else resultUI.Show(survived, timer.Elapsed, level.Level, KillCount);
        }
        public void PauseForChest(){if(IsPlaying){State=RunState.Treasure;Time.timeScale=0;}}
        public void ResumeFromChest(){if(State==RunState.Treasure){State=RunState.Playing;Time.timeScale=1;}}
        public void TogglePause()
        {
            if(IsPlaying){State=RunState.Paused;Time.timeScale=0;}
            else if(State==RunState.Paused){State=RunState.Playing;Time.timeScale=1;}
        }
        public void ReturnToMenu()
        {
            if(State!=RunState.GameOver)return;
            for(int i=runtimeRoot.childCount-1;i>=0;i--){var go=runtimeRoot.GetChild(i).gameObject;go.SetActive(false);Destroy(go);}
            State=RunState.Menu;Time.timeScale=0;resultUI.Hide();levelUpUI.Hide();gameUI.ShowMenu();
            if(Campaign!=null)Campaign.ReturnMenu();
        }
        void OnDestroy() => Time.timeScale = 1;
    }
}
