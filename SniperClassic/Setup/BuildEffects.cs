using SniperClassic.Controllers;
using RoR2;
using UnityEngine;
using SniperClassic.Hooks;
using R2API;
using SniperClassic.Modules;
using EntityStates.SniperClassicSkills;
using UnityEngine.AddressableAssets;

namespace SniperClassic.Setup
{
    public static class BuildEffects
    {
        private static bool initialized = false;
        public static void Init()
        {
            if (initialized) return;
            initialized = true;
            BuildDisruptEffect();
            CreateSpotterLightningEffect();
            FixTracer();
            CreateBackflipStunEffect();
            CreateSpotterTazeEffect();
            CreateHeadshotEffect();
            CreateSoundEffects();
        }

        private static void CreateSpotterLightningEffect()
        {
            OnHitEnemy.shockExplosionEffect = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/lightningstakenova"), "MoffeinSniperClassicSpotterLightningExplosion", false);
            EffectComponent ec = OnHitEnemy.shockExplosionEffect.GetComponent<EffectComponent>();
            ec.applyScale = true;
            ec.soundName = "Play_mage_m2_impact";
            SniperContent.effectDefs.Add(new EffectDef(OnHitEnemy.shockExplosionEffect));
        }

        private static void FixTracer()
        {
            GameObject sniperTracerObject = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/tracers/tracersmokechase");
            DestroyOnTimer destroyTimer = sniperTracerObject.AddComponent<DestroyOnTimer>();
            destroyTimer.duration = 0.42f;

            //Snipe.tracerEffectPrefab = sniperTracerObject;
            //HeavySnipe.tracerEffectPrefab = sniperTracerObject;
            FireBattleRifle.tracerEffectPrefab = sniperTracerObject;
        }

        private static void CreateBackflipStunEffect()
        {
            GameObject backflipEffect = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/muzzleflashes/Bandit2SmokeBomb").InstantiateClone("MoffeinSniperClassicBackflipStun", false);
            EffectComponent ec = backflipEffect.GetComponent<EffectComponent>();
            ec.soundName = "Play_commando_M2_grenade_explo";
            SniperContent.effectDefs.Add(new EffectDef(backflipEffect));
            Backflip.stunEffectPrefab = backflipEffect;
        }

        private static void CreateSpotterTazeEffect()
        {
            GameObject effect = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/omnieffect/omniimpactvfxloader").InstantiateClone("MoffeinSniperClassicBackflipTaze", false);
            EffectComponent ec = effect.GetComponent<EffectComponent>();
            ec.soundName = "Play_captain_m2_tazer_shoot";
            SniperContent.effectDefs.Add(new EffectDef(effect));
            Backflip.shockEffectPrefab = effect;
        }

        private static void BuildDisruptEffect()
        {
            GameObject effect = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/smokescreeneffect").InstantiateClone("MoffeinSniperClassicDisruptEffect", false);
            EffectComponent ec = effect.GetComponent<EffectComponent>();
            ec.soundName = "Play_SniperClassic_pipebomb";
            ec.applyScale = false;

            SniperContent.effectDefs.Add(new EffectDef(effect));
            EnemyDisruptComponent.effectPrefab = effect;
        }

        private static void CreateHeadshotEffect()
        {
            GameObject effect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/SniperTargetHitEffect.prefab").WaitForCompletion().InstantiateClone("MoffeinSniperClassicHeadshotEffect", false);
            EffectComponent ec = effect.GetComponent<EffectComponent>();
            ec.soundName = "";//"Play_SniperClassic_headshot";
            SniperContent.effectDefs.Add(new EffectDef(effect));
            BaseSnipeState.headshotEffectPrefab = effect;
            FireBattleRifle.headshotEffectPrefab = effect;
        }

        private static void CreateSoundEffects()
        {
            FireBattleRifle.pingSound = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            FireBattleRifle.pingSound.eventName = "Play_SniperClassic_m1_br_ping";
            FireBattleRifle.pingSound.akId = AkSoundEngine.GetIDFromString("Play_SniperClassic_m1_br_ping");
            SniperContent.networkSoundEventDefs.Add(FireBattleRifle.pingSound);
        }
    }
}
