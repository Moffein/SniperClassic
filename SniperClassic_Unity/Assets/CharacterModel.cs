using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CharacterModel : MonoBehaviour
{
    [System.Serializable]
    public struct RendererInfo
    {
        public Renderer renderer;
        public Material defaultMaterial;
        public ShadowCastingMode defaultShadowCastingMode;
        public bool ignoreOverlays;
    }

    public RendererInfo[] baseRendererInfos;
}
