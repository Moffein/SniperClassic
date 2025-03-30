using UnityEngine;

namespace SniperClassic {

    public class whythefuckarethetoesnotconnectedtothefoot : MonoBehaviour
    {
        [SerializeField]
        private Transform foot;

        void LateUpdate()
        {
            transform.position = foot.position;
        }

        private void OnDisable()
        {
            transform.parent = foot;
        }
    }
}