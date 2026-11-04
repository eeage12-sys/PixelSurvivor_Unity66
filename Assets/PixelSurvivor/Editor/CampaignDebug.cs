using UnityEditor;
using UnityEngine;
namespace PixelSurvivor.Editor
{
    public static class CampaignDebug
    {
        static CampaignController Get()
        {
            var game=Object.FindAnyObjectByType<GameManager>();
            if(!EditorApplication.isPlaying || game==null || game.Campaign==null || !game.Campaign.Practice || !game.IsPlaying)
            {Debug.LogWarning("QA commands require FAST TEST mode and an active, unpaused run.");return null;}
            return game.Campaign;
        }
        [MenuItem("Tools/Pixel Survivor/QA Fast Test/Grant 100 EXP")]
        static void Experience(){var c=Get();if(c!=null)c.Game.Level.AddExperience(100);}
        [MenuItem("Tools/Pixel Survivor/QA Fast Test/Max starter and give evolution partner")]
        static void Evolution()
        {
            var c=Get();if(c==null)return;var w=c.Equipment.Weapons[0];
            while(w.level<w.Maximum)c.Equipment.Add(w.data);
            c.Equipment.Add(w.data.evolutionPartner);c.RefreshStats(true);
            c.Field.DropChest(c.Game.Player.transform.position,false,false);
        }
        [MenuItem("Tools/Pixel Survivor/QA Fast Test/Drop all field items")]
        static void Items()
        {
            var c=Get();if(c==null)return;
            for(int i=0;i<5;i++)c.Field.Drop((Vector2)c.Game.Player.transform.position+Vector2.right*(2+i*2),(FieldItemKind)i,c.Data.fieldGold);
        }
        [MenuItem("Tools/Pixel Survivor/QA Fast Test/Spawn elite")]
        static void Elite(){var c=Get();if(c!=null)c.Stage.SpawnEnemy(true);}
        [MenuItem("Tools/Pixel Survivor/QA Fast Test/Defeat current boss")]
        static void Boss()
        {var c=Get();if(c!=null && c.Stage.CurrentBoss!=null)c.Stage.CurrentBoss.TakeDamage(c.Stage.CurrentBoss.CurrentHP+1);}
        [MenuItem("Tools/Pixel Survivor/QA Fast Test/Advance to next boss")]
        static void NextBoss()
        {var c=Get();if(c==null)return;float next=(c.Stage.BossWaves+1)*c.StageData.bossInterval;c.Game.Timer.AdvanceCampaign(Mathf.Max(0,next-c.Stage.Elapsed)/c.TimelineScale);c.Stage.Tick(0);}
        [MenuItem("Tools/Pixel Survivor/QA Fast Test/End test run")]
        static void EndTest(){var c=Get();if(c!=null)c.Game.EndRun(false);}
        [MenuItem("Tools/Pixel Survivor/QA Fast Test/Summon barrier")]
        static void Barrier(){var c=Get();if(c!=null)c.Supports.Summon(2);}
    }
}
