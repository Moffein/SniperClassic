using UnityEngine;

namespace SniperClassic {

    public class whythefuckarethetoesnotconnectedtothefoot : MonoBehaviour
    {
        [SerializeField]
        private Transform foot;

        private void OnDisable()
        {
            transform.parent = foot;
        }
    }
}