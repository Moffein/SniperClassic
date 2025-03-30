using UnityEngine;

namespace EntityStates
{
    public class SniperMain : GenericCharacterMain
    {
        Animator cachedAnimator;

        public override void OnEnter()
        {
            base.OnEnter();
            cachedAnimator = base.GetModelAnimator();

            //base.smoothingParameters.forwardSpeedSmoothDamp = 0.0f;
            //base.smoothingParameters.rightSpeedSmoothDamp = 0.0f;

            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (scene == "moon" || scene == "moon2")
            {
                cachedAnimator.SetFloat("MoonMan", 1);
            }
        }
    }
}
