using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SniperClassic
{
    public class whythefuckarethetoesnotconnectedtothefoot : MonoBehaviour
    {
        [SerializeField]
        private Transform foot;

        private void LateUpdate()
        {
            transform.position = foot.position;
        }

        private void OnDisable()
        {
            if (gameObject.activeInHierarchy)
            {
                transform.parent = foot;
            }
        }
    }
}
