using UnityEngine;

namespace EntityStates
{
    class SniperMain : GenericCharacterMain
    {

        Animator cachedAnimator;

        public override void OnEnter()
        {
            base.OnEnter();
            cachedAnimator = base.GetModelAnimator();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (scene == "moon" || scene == "moon2")
            {
                cachedAnimator.SetFloat("MoonMan", 1);
            }
        }
    }
}
