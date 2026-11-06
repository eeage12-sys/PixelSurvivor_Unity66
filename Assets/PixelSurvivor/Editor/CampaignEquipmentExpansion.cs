using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PixelSurvivor.Editor
{
    public static class CampaignEquipmentExpansion
    {
        const string Root = "Assets/PixelSurvivor/Data/";

        public static void Ensure(CampaignData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            string[] passiveNames = { "철벽 문장", "매의 눈", "영혼 병", "별빛 서책" };
            string[] passiveDescriptions = {
                "받는 피해를 레벨마다 1 줄입니다.", "치명타 확률이 레벨마다 4% 증가합니다.",
                "적을 처치할 때 체력을 레벨마다 0.4 회복합니다.", "획득 경험치가 레벨마다 8% 증가합니다."
            };
            StatKind[] kinds = { StatKind.Armor, StatKind.Critical, StatKind.Leech, StatKind.Experience };
            float[] amounts = { 1, .04f, .4f, .08f };
            var passives = new List<PassiveEquipmentData>(data.passives ?? Array.Empty<PassiveEquipmentData>());
            for (int n = 0; n < 4; n++)
            {
                int i = n + 8;
                string path = Root + "Passive_" + i + ".asset";
                var passive = AssetDatabase.LoadAssetAtPath<PassiveEquipmentData>(path);
                if (passive == null)
                {
                    passive = ScriptableObject.CreateInstance<PassiveEquipmentData>();
                    passive.id = "p" + i; passive.title = passiveNames[n]; passive.description = passiveDescriptions[n];
                    passive.bonus = new StatBonus(kinds[n], amounts[n]); passive.maxLevel = 5;
                    AssetDatabase.CreateAsset(passive, path);
                }
                if (!passives.Contains(passive)) passives.Add(passive);
            }

            string[] names = { "잿불 부채", "서리 십자", "가시 덫", "달빛 창" };
            string[] evolved = { "불꽃 폭풍", "빙결 성좌", "가시 정원", "만월 관통" };
            string[] descriptions = {
                "적을 향해 부채꼴로 불씨를 발사합니다.", "사방으로 얼음 결정을 발사합니다.",
                "목표 지점에 지속 피해를 주는 덫을 만듭니다.", "바라보는 방향으로 적을 꿰뚫는 창을 발사합니다."
            };
            float[] damage = { 7, 9, 6, 18 }, cooldown = { 1.25f, 2.2f, 3.6f, 1.3f };
            float[] area = { .85f, .9f, 1.7f, 1.1f }, duration = { 2.2f, 2.5f, 2.8f, 2.4f };
            Color[] colors = { new Color(1,.42f,.23f), new Color(.42f,.86f,1), new Color(.56f,.95f,.43f), new Color(.88f,.76f,1) };
            var weapons = new List<WeaponData>(data.weapons ?? Array.Empty<WeaponData>());
            for (int n = 0; n < 4; n++)
            {
                int i = n + 8;
                string path = Root + "Weapon_" + i + ".asset";
                var weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
                if (weapon == null)
                {
                    weapon = ScriptableObject.CreateInstance<WeaponData>();
                    weapon.id = "w" + i; weapon.title = names[n]; weapon.evolvedTitle = evolved[n];
                    weapon.description = descriptions[n]; weapon.pattern = (WeaponPattern)i;
                    weapon.evolutionPartner = passives[i]; weapon.color = colors[n];
                    weapon.damage = damage[n]; weapon.cooldown = cooldown[n]; weapon.area = area[n]; weapon.duration = duration[n];
                    weapon.projectileCount = n == 0 ? 3 : n == 1 ? 4 : 1;
                    weapon.extraProjectileEvery = n == 1 ? 1 : 2;
                    weapon.penetration = n == 3 ? 4 : 0;
                    weapon.projectileSpeed = n == 3 ? 14 : 9;
                    weapon.knockback = n == 3 ? .55f : .18f;
                    AssetDatabase.CreateAsset(weapon, path);
                }
                if (!weapons.Contains(weapon)) weapons.Add(weapon);
            }
            data.passives = passives.ToArray(); data.weapons = weapons.ToArray();
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }
    }
}
