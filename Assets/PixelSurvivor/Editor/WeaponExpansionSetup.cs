using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PixelSurvivor.Editor
{
    public static class WeaponExpansionSetup
    {
        const string Root="Assets/PixelSurvivor";

        static Sprite Import(string name,int pixelsPerUnit)
        {
            string path=Root+"/Resources/PixelCombat/"+name+".png";
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null)throw new InvalidOperationException("Missing weapon sprite: "+path);
            importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=pixelsPerUnit;
            importer.filterMode=FilterMode.Point;
            importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency=true;
            importer.SaveAndReimport();
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(sprite==null)throw new InvalidOperationException("Cannot import sprite: "+path);
            return sprite;
        }

        static WeaponData Weapon(int index,string title,string evolved,string description,
            PassiveEquipmentData partner,Color color,float damage,float cooldown,float area,float duration,
            float speed,float knock,int evolvedExtra)
        {
            string path=Root+"/Data/Weapon_"+index+".asset";
            var weapon=AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if(weapon==null)
            {
                weapon=ScriptableObject.CreateInstance<WeaponData>();
                AssetDatabase.CreateAsset(weapon,path);
            }
            weapon.id="w"+index;weapon.title=title;weapon.evolvedTitle=evolved;
            weapon.description=description;weapon.pattern=(WeaponPattern)index;
            weapon.evolutionPartner=partner;weapon.icon=Import("Weapon"+index,100);
            weapon.evolvedIcon=Import("Evolved"+index,100);
            Import("Projectile"+index,64);Import("Flash"+index,64);
            weapon.color=color;weapon.maxLevel=5;weapon.damage=damage;weapon.cooldown=cooldown;
            weapon.area=area;weapon.duration=duration;weapon.projectileSpeed=speed;
            weapon.projectileCount=1;weapon.penetration=0;weapon.knockback=knock;
            weapon.damagePerLevel=.23f;weapon.areaPerLevel=.1f;weapon.cooldownPerLevel=.04f;
            weapon.extraProjectileEvery=2;weapon.evolvedDamage=1.75f;
            weapon.evolvedArea=1.3f;weapon.evolvedCooldown=.8f;
            weapon.evolvedExtraProjectiles=evolvedExtra;weapon.evolvedPenetration=1;
            EditorUtility.SetDirty(weapon);
            return weapon;
        }

        [MenuItem("Tools/Pixel Survivor/Apply Four New Weapons")]
        public static void Apply()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play mode first");
            AssetDatabase.Refresh();
            var campaign=AssetDatabase.LoadAssetAtPath<CampaignData>(Root+"/Data/Campaign.asset");
            if(campaign==null || campaign.weapons==null || campaign.weapons.Length<12 ||
                campaign.passives==null || campaign.passives.Length<8)
                throw new InvalidOperationException("Existing campaign weapons or passives are missing");
            var existing=campaign.weapons.Take(12).ToArray();
            if(existing.Any(w=>w==null))throw new InvalidOperationException("An existing weapon is missing");
            var added=new[]
            {
                Weapon(12,"프리즘 광선","일곱 빛의 굴절","적을 향해 직선 광선을 쏘아 경로의 몬스터를 타격합니다.",campaign.passives[7],new Color(.25f,.95f,1f),12,1.4f,1.4f,.2f,0,.12f,2),
                Weapon(13,"생명의 개화","만개하는 정원","목표 지점에 꽃을 피우고 꽃잎을 사방으로 흩뿌립니다.",campaign.passives[4],new Color(.45f,1f,.35f),12,2.1f,1.5f,.7f,6,.18f,3),
                Weapon(14,"중력 소용돌이","심연의 인력","적을 끌어당기며 지속 피해를 주는 중력장을 만듭니다.",campaign.passives[6],new Color(.7f,.4f,1f),7,3.5f,2.2f,2.8f,0,.35f,0),
                Weapon(15,"붉은 혜성","쌍성 낙하","충돌 지점에 폭발을 일으키는 혜성을 발사합니다.",campaign.passives[3],new Color(1f,.42f,.14f),19,1.8f,1.3f,1.3f,12,.5f,1)
            };
            campaign.weapons=existing.Concat(added).ToArray();
            EditorUtility.SetDirty(campaign);
            CampaignSetup.ValidateData(campaign);
            AssetDatabase.SaveAssets();
        }
    }
}
