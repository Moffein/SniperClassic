using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HenryTools.Editor
{
    public class EditorRecordAndSetTransforms
    {

        public static List<Transform> _transforms;

        public static Vector3[] _storedPositions;
        public static Quaternion[] _storedRotations;
        public static Vector3[] _storedScales;

        [MenuItem("CONTEXT/Transform/Record Local/Record Child Transforms")]
        public static void getAllTransformPositions()
        {

            FillTransformList();

            _storedPositions = new Vector3[_transforms.Count];
            _storedRotations = new Quaternion[_transforms.Count];
            _storedScales = new Vector3[_transforms.Count];

            int aye = 0;
            for (int i = 0; i < _transforms.Count; i++)
            {

                if (_transforms[i])
                {
                    _storedPositions[i] = _transforms[i].position;
                    _storedRotations[i] = _transforms[i].rotation;
                    _storedScales[i] = _transforms[i].localScale;

                    aye++;
                }
            }

            Debug.Log($"{aye}/{_transforms.Count} transforms have been recorded. Turn off animation Preview and click 'Set ALL Recorded Transforms'");
        }

        private static void FillTransformList()
        {

            if (_transforms == null)
                _transforms = new List<Transform>();

            Transform[] children;
            for (int select = 0; select < Selection.transforms.Length; select++)
            {

                //NEVER do GetComponentsInChildren at runtime, unless you don't care about FPS in which case you're not a true gamer -ts
                children = Selection.transforms[select].GetComponentsInChildren<Transform>(true);

                for (int i = 0; i < children.Length || i < _transforms.Count; i++)
                {

                    if (i < children.Length)
                    {

                        if (_transforms.Count <= i)
                        {
                            _transforms.Add(children[i]);
                        }
                        else
                        {
                            _transforms[i] = children[i];
                        }
                    }
                    else
                    {
                        _transforms[i] = null;
                    }
                }
            }
        }

        [CanEditMultipleObjects]
        [MenuItem("CONTEXT/Transform/Record Local/Set Recorded Transforms To Recorded Positions")]
        public static void setAllTransformPositions()
        {

            if (_transforms == null)
            {
                Debug.LogError("no transforms are recorded. use Record Transforms first");
                return;
            }

            for (int i = 0; i < _transforms.Count; i++)
            {

                if (_transforms[i] != null)
                {
                    Undo.RecordObject(_transforms[i], "setting transforms");
                    _transforms[i].position = _storedPositions[i];
                    _transforms[i].rotation = _storedRotations[i];
                    _transforms[i].localScale = _storedScales[i];
                }
            }
        }
    }


    public class EditorRecordAndSetTransformsAbsolute
    {
        public class StoredTransformInfo
        {
            public string transformName;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 localScale;
        }

        public static List<StoredTransformInfo> _storedTransformInfos;

        [MenuItem("CONTEXT/Transform/Record World/Record Child Transforms")]
        public static void getAllTransformPositions()
        {

            if (_storedTransformInfos == null)
                _storedTransformInfos = new List<StoredTransformInfo>();

            _storedTransformInfos.Clear();

            Transform[] children;
            Transform child;
            for (int select = 0; select < Selection.transforms.Length; select++)
            {

                children = Selection.transforms[select].GetComponentsInChildren<Transform>();

                for (int i = 0; i < children.Length; i++)
                {
                    child = children[i];
                    _storedTransformInfos.Add(new StoredTransformInfo
                    {
                        transformName = child.name,
                        position = child.position,
                        rotation = child.rotation,
                        localScale = child.localScale
                    });
                }
            }

            Debug.Log(_storedTransformInfos.Count + " transforms recorded");
        }

        [CanEditMultipleObjects]
        [MenuItem("CONTEXT/Transform/Record World/Set Child Transforms To Recorded World Positions")]
        public static void setAllTransformPositions()
        {

            if (_storedTransformInfos == null)
            {
                Debug.LogError("no transforms are recorded. Record Transforms first");
                return;
            }

            Transform[] children;
            Transform selectedChild;
            StoredTransformInfo foundInfo;
            for (int select = 0; select < Selection.transforms.Length; select++)
            {

                children = Selection.transforms[select].GetComponentsInChildren<Transform>();

                for (int i = 0; i < children.Length; i++)
                {
                    selectedChild = children[i];
                    Undo.RecordObject(selectedChild, "set transforms absolute");
                    foundInfo = _storedTransformInfos.Find((info) => info.transformName == selectedChild.name);
                    if (foundInfo != null)
                    {
                        selectedChild.position = foundInfo.position;
                        selectedChild.rotation = foundInfo.rotation;
                        selectedChild.localScale = foundInfo.localScale;
                    }
                }
            }
        }
    }
}