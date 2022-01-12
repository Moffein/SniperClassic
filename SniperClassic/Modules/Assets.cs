using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SniperClassic.Modules
{
    public class Assets
    {
        internal static GameObject snipeTracer;
        internal static GameObject heavySnipeTracer;
        internal static GameObject markTracer;

        private static Material commandoMat;

        internal static void InitializeAssets()
        {
            snipeTracer = CreateTracer("TracerHuntressSnipe", "ClassicSniperDefaultTracer");
            markTracer = snipeTracer;
            heavySnipeTracer = snipeTracer;

            LineRenderer line = snipeTracer.transform.Find("TracerHead").GetComponent<LineRenderer>();
            Material tracerMat = UnityEngine.Object.Instantiate<Material>(line.material);
            line.startWidth *= 0.25f;
            line.endWidth *= 0.25f;
            // this did not work.
            tracerMat.SetColor("_TintColor", new Color(78f / 255f, 80f / 255f, 111f / 255f));
            line.material = tracerMat;
        }

        private static GameObject CreateTracer(string originalTracerName, string newTracerName)
        {
            GameObject newTracer = R2API.PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Effects/Tracers/" + originalTracerName), newTracerName, true);

            if (!newTracer.GetComponent<EffectComponent>()) newTracer.AddComponent<EffectComponent>();
            if (!newTracer.GetComponent<VFXAttributes>()) newTracer.AddComponent<VFXAttributes>();
            if (!newTracer.GetComponent<NetworkIdentity>()) newTracer.AddComponent<NetworkIdentity>();

            newTracer.GetComponent<Tracer>().speed = 250f;
            newTracer.GetComponent<Tracer>().length = 50f;

            SniperContent.effectDefs.Add(new EffectDef(newTracer));

            return newTracer;
        }


        public static Material CreateMaterial(string materialName)
        {
            return CreateMaterial(materialName, 0f, Color.black, 0f);
        }

        public static Material CreateMaterial(string materialName, float emission, Color emissionColor)
        {
            return CreateMaterial(materialName, emission, emissionColor, 0f);
        }

        public static Material CreateMaterial(string materialName, float emission, Color emissionColor, float normalStrength)
        {
            if (!commandoMat) commandoMat = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial;

            Material mat = UnityEngine.Object.Instantiate<Material>(commandoMat);
            Material tempMat = SniperContent.assetBundle.LoadAsset<Material>(materialName);
            if (!tempMat)
            {
                return commandoMat;
            }

            mat.name = materialName;
            mat.SetColor("_Color", tempMat.GetColor("_Color"));
            mat.SetTexture("_MainTex", tempMat.GetTexture("_MainTex"));
            mat.SetColor("_EmColor", emissionColor);
            mat.SetFloat("_EmPower", emission);
            mat.SetTexture("_EmTex", tempMat.GetTexture("_EmissionMap"));
            mat.SetFloat("_NormalStrength", normalStrength);

            return mat;
        }
    }
}