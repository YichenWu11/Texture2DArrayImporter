using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

namespace Util
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Texture2DArrayImporter), true)]
    public class Texture2DArrayImporterInspector : ScriptedImporterEditor
    {
        // public override void OnInspectorGUI()
        // {
        //     serializedObject.Update();
        //     
        //     serializedObject.ApplyModifiedProperties();
        //     ApplyRevertGUI();
        // }
    }
   
}