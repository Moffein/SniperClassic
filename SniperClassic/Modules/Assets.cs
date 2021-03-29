using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SniperClassic.Modules
{
    internal class Assets
    {
        internal static GameObject snipeTracer;
        internal static GameObject heavySnipeTracer;
        internal static GameObject markTracer;

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
            GameObject newTracer = EnigmaticThunder.Modules.Prefabs.InstantiateClone(Resources.Load<GameObject>("Prefabs/Effects/Tracers/" + originalTracerName), newTracerName, true);

            if (!newTracer.GetComponent<EffectComponent>()) newTracer.AddComponent<EffectComponent>();
            if (!newTracer.GetComponent<VFXAttributes>()) newTracer.AddComponent<VFXAttributes>();
            if (!newTracer.GetComponent<NetworkIdentity>()) newTracer.AddComponent<NetworkIdentity>();

            newTracer.GetComponent<Tracer>().speed = 250f;
            newTracer.GetComponent<Tracer>().length = 50f;

            SniperContent.effectDefs.Add(new EffectDef(newTracer));

            return newTracer;
        }
    }
}