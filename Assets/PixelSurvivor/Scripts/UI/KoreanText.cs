using System.Collections.Generic;
using UnityEngine;
namespace PixelSurvivor
{
    public static class KoreanText
    {
        public const string FontResource="Fonts/NotoSansCJKkr-Regular";
        static Font cached;
        public static Font Font
        {
            get
            {
                if(cached==null)cached=Resources.Load<Font>(FontResource);
                if(cached==null)throw new System.InvalidOperationException("한글 폰트 파일이 없습니다. 새 Assets 폴더 전체를 복사한 뒤 한국어 적용 메뉴를 실행하세요.");
                return cached;
            }
        }
        static readonly Dictionary<string,string> DefaultText=new Dictionary<string,string>
        {
            {"Might Sigil","힘의 문장"},
            {"Life Seed","생명의 씨앗"},
            {"Clockwork","태엽 장치"},
            {"Wide Lens","확장 렌즈"},
            {"Long Candle","영원의 촛불"},
            {"Wind Boots","바람 장화"},
            {"Magnet Core","자석 핵"},
            {"Twin Prism","쌍둥이 프리즘"},
            {"All weapon damage +10% per level.","레벨당 모든 무기 피해량 +10%."},
            {"Maximum HP +20 per level; heal the added amount.","레벨당 최대 체력 +20. 증가한 만큼 체력을 회복합니다."},
            {"Weapon cooldown -6% per level.","레벨당 무기 재사용 대기시간 6% 감소."},
            {"Attack area +12% per level.","레벨당 공격 범위 +12%."},
            {"Attack duration +20% per level.","레벨당 공격 지속시간 +20%."},
            {"Movement speed +8% per level.","레벨당 이동속도 +8%."},
            {"Pickup attraction range +25% per level.","레벨당 아이템 획득 범위 +25%."},
            {"Projectile count +1 per level.","레벨당 투사체 수 +1."},
            {"Energy Bolt","단검 투척"},
            {"Wind Dagger","바람 검기"},
            {"Light Whip","빛의 강타"},
            {"Throwing Daggers","단검 투척"},
            {"Wind Blade","바람 검기"},
            {"Radiant Slam","빛의 강타"},
            {"Orbit Orb","궤도 구체"},
            {"Crescent Scythe","초승달 낫"},
            {"Storm Mark","폭풍 표식"},
            {"Pulse Nova","펄스 노바"},
            {"Seeking Spark","추적 불꽃"},
            {"Prism Barrage","그림자 단검비"},
            {"Gale Knives","질풍의 칼날"},
            {"Lifeline Lash","수호의 강타"},
            {"Shadow Daggerstorm","그림자 단검비"},
            {"Guardian Slam","수호의 강타"},
            {"Solar Halo","태양의 고리"},
            {"Eclipse Reaper","월식의 낫"},
            {"Thunder Crown","뇌전의 왕관"},
            {"Supernova","초신성"},
            {"Star Swarm","별빛 군집"},
            {"Fires at the nearest target. Levels add damage and bolts.","가장 가까운 적에게 단검을 자동으로 던집니다. 성장하면 피해량과 투척 수가 증가합니다."},
            {"Throws knives in your facing direction; pierces targets.","바라보는 방향으로 바람 검기를 날려 적을 관통합니다."},
            {"Sweeps a wide arc toward enemies. Extra count adds a rear sweep.","방패로 전방을 강타해 적을 밀쳐냅니다. 공격 수가 늘면 후방에도 충격파를 냅니다."},
            {"Throws daggers at the nearest enemy. Levels add damage and blades.","가장 가까운 적에게 단검을 자동으로 던집니다. 성장하면 피해량과 투척 수가 증가합니다."},
            {"Sends a piercing wind blade in your facing direction.","바라보는 방향으로 바람 검기를 날려 적을 관통합니다."},
            {"Slams a shield forward and knocks enemies back. Extra count adds a rear shockwave.","방패로 전방을 강타해 적을 밀쳐냅니다. 공격 수가 늘면 후방에도 충격파를 냅니다."},
            {"A rotating ring damages enemies around you for several seconds.","주변을 도는 고리가 일정 시간 동안 적에게 피해를 줍니다."},
            {"A piercing blade travels outward then returns toward you.","적을 관통한 칼날이 플레이어에게 돌아옵니다."},
            {"Lightning strikes the target and nearby ground.","목표와 주변 지면에 번개를 떨어뜨립니다."},
            {"A circular shockwave damages enemies around you.","원형 충격파로 주변 적을 공격합니다."},
            {"Homing sparks continually track nearby enemies.","유도 불꽃이 가까운 적을 따라가 공격합니다."},
            {"Ari - Balanced","아리 · 밸런스형"},
            {"Vela - Swift","벨라 · 속도형"},
            {"Bram - Power","브람 · 파워형"},
            {"Mira - Mage","미라 · 메이지형"},
            {"Nox - Reaper","녹스 · 리퍼형"},
            {"BALANCED","밸런스형"},
            {"SWIFT","속도형"},
            {"POWER","파워형"},
            {"MAGE","메이지형"},
            {"REAPER","리퍼형"},
            {"Flexible mixed-weapon explorer.","다양한 무기 조합을 활용하는 탐험가."},
            {"Fragile, fast-moving knife specialist.","체력은 낮지만 빠르게 움직이는 단검 전문가."},
            {"Heavy close-range sweeper with high health.","높은 체력과 강한 근접 공격을 갖춘 전사."},
            {"Fragile mage with large persistent attacks.","넓고 오래 지속되는 공격을 다루는 마법사."},
            {"High-risk fighter who grows stronger as health falls.","체력이 줄어들수록 강해지는 위험한 전사."},
            {"Experience +10%","경험치 획득량 +10%"},
            {"Dodge chance +8%","회피 확률 +8%"},
            {"Knockback +40%, armor +2","넉백 +40%, 방어력 +2"},
            {"Area +20%, projectiles +1","공격 범위 +20%, 투사체 +1"},
            {"Up to +80% damage at low HP","잃은 체력에 비례해 피해량 최대 +80%"},
            {"Versatile power","다재다능"},
            {"Survival","생존 훈련"},
            {"Learning","학습 능력"},
            {"Gathering","수집 능력"},
            {"Quick step","빠른 발걸음"},
            {"Fast hands","민첩한 손"},
            {"Evasion","회피 훈련"},
            {"Knife training","단검 훈련"},
            {"Iron body","강철 체력"},
            {"Heavy impact","강력한 일격"},
            {"Plating","방어 강화"},
            {"Shove","밀어내기"},
            {"Wide magic","광역 마법"},
            {"Spell flow","마력 순환"},
            {"Split magic","분열 마법"},
            {"Lingering magic","지속 마법"},
            {"Precision","정밀 타격"},
            {"Reaping speed","수확의 속도"},
            {"Blood power","혈기의 힘"},
            {"Soul recovery","영혼 회복"},
            {"Attack","공격력"},
            {"Maximum HP","최대 체력"},
            {"Move speed","이동속도"},
            {"Armor","방어력"},
            {"Experience","경험치 획득"},
            {"Gold gain","골드 획득"},
            {"Pickup range","아이템 획득 범위"},
            {"Stage 1 - Lumen Field","1구역 · 빛의 들판"},
            {"Stage 2 - Amber Basin","2구역 · 호박빛 분지"},
            {"Stage 3 - Violet Ruins","3구역 · 보랏빛 폐허"},
            {"Infinite survival. Balanced monster mix.","다양한 몬스터가 등장하는 들판에서 20분을 버티세요."},
            {"Infinite survival. Wide basin; more fast moths.","넓은 분지입니다. 빠른 나방이 더 자주 등장합니다."},
            {"Infinite survival. Tall ruins; more armored beetles.","세로로 긴 폐허입니다. 단단한 딱정벌레가 더 자주 등장합니다."},
        };
        public static string Translate(string value)
        {if(string.IsNullOrEmpty(value))return value??"";return DefaultText.TryGetValue(value,out var ko)?ko:value;}
        public static void LocalizeData(CampaignData data)
        {
            foreach(var w in data.weapons){w.title=Translate(w.title);w.evolvedTitle=Translate(w.evolvedTitle);w.description=Translate(w.description);}
            foreach(var p in data.passives){p.title=Translate(p.title);p.description=Translate(p.description);}
            foreach(var ch in data.characters)
            {ch.title=Translate(ch.title);ch.role=Translate(ch.role);ch.description=Translate(ch.description);ch.passiveDescription=Translate(ch.passiveDescription);foreach(var u in ch.upgrades)u.title=Translate(u.title);}
            foreach(var stage in data.stages){stage.title=Translate(stage.title);stage.description=Translate(stage.description);}
            foreach(var u in data.commonUpgrades)u.title=Translate(u.title);
        }
        public static string StatEffect(StatBonus b)
        {
            string label;bool percent=true;string sign="+";
            switch(b.kind)
            {
                case StatKind.Damage:label="공격력";break;
                case StatKind.MaxHP:label="최대 체력";percent=false;break;
                case StatKind.MoveSpeed:label="이동속도";break;
                case StatKind.Cooldown:label="재사용 대기시간";sign="-";break;
                case StatKind.Area:label="공격 범위";break;
                case StatKind.Duration:label="지속시간";break;
                case StatKind.Projectiles:label="투사체 수";percent=false;break;
                case StatKind.Pickup:label="획득 범위";break;
                case StatKind.Armor:label="방어력";percent=false;break;
                case StatKind.Experience:label="경험치 획득량";break;
                case StatKind.Gold:label="골드 획득량";break;
                case StatKind.Critical:label="치명타 확률";break;
                case StatKind.Leech:label="처치 시 체력 회복";percent=false;break;
                case StatKind.LowHPDamage:label="체력 감소 시 최대 추가 피해";break;
                case StatKind.Dodge:label="회피 확률";break;
                case StatKind.Knockback:label="넉백";break;
                default:label="효과";break;
            }
            return "단계당 "+label+" "+sign+(b.amount*(percent?100:1)).ToString("0.##")+(percent?"%":"");
        }
    }
}
