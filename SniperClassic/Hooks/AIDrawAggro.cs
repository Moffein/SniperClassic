using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoR2;
using RoR2.CharacterAI;
using SniperClassic.Controllers;
using UnityEngine;

namespace SniperClassic.Hooks
{
    //Based on https://github.com/DestroyedClone/PoseHelper/blob/master/HighPriorityAggroTest/HPATPlugin.cs
    public class AIDrawAggro
    {
        public AIDrawAggro()
        {
            On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += BaseAI_FindEnemyHurtBox;
            On.RoR2.CharacterAI.BaseAI.OnBodyDamaged += BaseAI_OnBodyDamaged;
        }

        private static void BaseAI_OnBodyDamaged(On.RoR2.CharacterAI.BaseAI.orig_OnBodyDamaged orig, BaseAI self, DamageReport damageReport)
        {
            if (self.currentEnemy.gameObject != null && self.currentEnemy.gameObject.GetComponent<EnemyDisruptComponent>())
            {
                return;
            }
            orig(self, damageReport);
        }

        private static HurtBox BaseAI_FindEnemyHurtBox(On.RoR2.CharacterAI.BaseAI.orig_FindEnemyHurtBox orig, BaseAI self, float maxDistance, bool full360Vision, bool filterByLoS)
        {
            if (self.currentEnemy.gameObject && self.currentEnemy.gameObject.GetComponent<EnemyDisruptComponent>())
            {
                return self.currentEnemy.bestHurtBox;
            }
            return orig(self, maxDistance, full360Vision, filterByLoS);
        }
    }
}
