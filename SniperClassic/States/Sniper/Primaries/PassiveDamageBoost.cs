using UnityEngine;

namespace SniperClassic.Skills
{
    //Scrapped.
    public class PassiveDamageBoost
    {
        public static float CalcBoostedDamage(float damageStat, float attackSpeed, float baseDamage)
        {
            if (SniperClassic.enableAttackSpeedPassive)
            {
                return damageStat + baseDamage * Mathf.Max(0f, attackSpeed - 1f);
            }
            else
            {
                return damageStat;
            }
        }
    }
}
