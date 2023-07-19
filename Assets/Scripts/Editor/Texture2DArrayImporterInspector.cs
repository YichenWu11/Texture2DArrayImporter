using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.AssetImporters;

namespace Util
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Texture2DArrayImporter), true)]
    public class Texture2DArrayImporterInspector : ScriptedImporterEditor
    {
        private class Styles
        {
            public readonly GUIStyle preButton = "RL FooterButton";
            public readonly GUIContent textureTypeLabel = new("Texture Type");
            public readonly GUIContent textureTypeValue = new("Texture Array");
            public readonly GUIContent textureShapeLabel = new("Texture Shape");
            public readonly GUIContent textureShapeValue = new("2D");
            public readonly GUIContent wrapModeLabel = new("Wrap Mode", "Select how the Texture behaves when tiled.");

            public readonly GUIContent filterModeLabel = new("Filter Mode",
                "Select how the Texture is filtered when it gets stretched by 3D transformations.");

            public readonly GUIContent anisoLevelLabel = new("Aniso Level",
                "Increases Texture quality when viewing the Texture at a steep angle. Good for floor and ground Textures.");

            public readonly GUIContent readableLabel = new("Read/Write",
                "Enable to be able to access the texture data from scripts.");

            public readonly GUIContent srgbLabel = new("sRGB (Color Texture)",
                "Texture2DArray contents is stored in gamma space.");

            public readonly GUIContent texture2DArrayLabel = new("Texture2DArray",
                "The initial Texture2DArray asset.");

            public readonly GUIContent anisotropicFilteringDisable =
                new("Anisotropic filtering is disabled for all textures in Quality Settings.");

            public readonly GUIContent anisotropicFilteringForceEnable =
                new("Anisotropic filtering is enabled for all textures in Quality Settings.");

            public readonly GUIContent maxSizeLabel = new("Max Size");
            public readonly GUIContent resizeAlgorithmLabel = new("Resize Algorithm");
            public readonly GUIContent formatLabel = new("Format");
            public readonly GUIContent compressionLabel = new("Compression");

            public readonly GUIContent overrideLabelStandalone = new("Override For Windows, Mac, Linux");
            public readonly GUIContent overrideLabelAndroid = new("Override For Android");
            public readonly GUIContent overrideLabelIOS = new("Override For iOS");
        }

        private static Styles s_Styles;

        private Styles styles
        {
            get
            {
                s_Styles = s_Styles ?? new Styles();
                return s_Styles;
            }
        }

        private SerializedProperty m_WrapMode = null;
        private SerializedProperty m_FilterMode = null;
        private SerializedProperty m_AnisoLevel = null;
        private SerializedProperty m_IsReadable = null;
        private SerializedProperty m_SRGB = null;
        private SerializedProperty m_Tex2DArray = null;

        private SerializedProperty maxSizeDefault = null;
        private SerializedProperty resizeAlgorithmDefault = null;
        private SerializedProperty formatDefault = null;
        private SerializedProperty compressionDefault = null;

        private SerializedProperty maxSizeStandalone = null;
        private SerializedProperty resizeAlgorithmStandalone = null;
        private SerializedProperty formatStandalone = null;
        private SerializedProperty compressionStandalone = null;
        private SerializedProperty isOverrideEnabledStandalone = null;

        private SerializedProperty maxSizeAndroid = null;
        private SerializedProperty resizeAlgorithmAndroid = null;
        private SerializedProperty formatAndroid = null;
        private SerializedProperty compressionAndroid = null;
        private SerializedProperty isOverrideEnabledAndroid = null;

        private SerializedProperty maxSizeIOS = null;
        private SerializedProperty resizeAlgorithmIOS = null;
        private SerializedProperty formatIOS = null;
        private SerializedProperty compressionIOS = null;
        private SerializedProperty isOverrideEnabledIOS = null;

        private enum Tab
        {
            Default,
            Standalone,
            Android,
            IOS
        }

        private Tab selectedTab = Tab.Default;

        public override bool showImportedObject => true;

        public override void OnEnable()
        {
            base.OnEnable();

            m_WrapMode = serializedObject.FindProperty("m_WrapMode");
            m_FilterMode = serializedObject.FindProperty("m_FilterMode");
            m_AnisoLevel = serializedObject.FindProperty("m_AnisoLevel");
            m_IsReadable = serializedObject.FindProperty("m_IsReadable");
            m_SRGB = serializedObject.FindProperty("m_sRGB");
            m_Tex2DArray = serializedObject.FindProperty("m_Tex2DArray");

            maxSizeDefault = serializedObject.FindProperty("maxSizeDefault");
            resizeAlgorithmDefault = serializedObject.FindProperty("resizeAlgorithmDefault");
            formatDefault = serializedObject.FindProperty("formatDefault");
            compressionDefault = serializedObject.FindProperty("compressionDefault");

            maxSizeStandalone = serializedObject.FindProperty("maxSizeStandalone");
            resizeAlgorithmStandalone = serializedObject.FindProperty("resizeAlgorithmStandalone");
            formatStandalone = serializedObject.FindProperty("formatStandalone");
            compressionStandalone = serializedObject.FindProperty("compressionStandalone");
            isOverrideEnabledStandalone = serializedObject.FindProperty("isOverrideEnabledStandalone");

            maxSizeAndroid = serializedObject.FindProperty("maxSizeAndroid");
            resizeAlgorithmAndroid = serializedObject.FindProperty("resizeAlgorithmAndroid");
            formatAndroid = serializedObject.FindProperty("formatAndroid");
            compressionAndroid = serializedObject.FindProperty("compressionAndroid");
            isOverrideEnabledAndroid = serializedObject.FindProperty("isOverrideEnabledAndroid");

            maxSizeIOS = serializedObject.FindProperty("maxSizeIOS");
            resizeAlgorithmIOS = serializedObject.FindProperty("resizeAlgorithmIOS");
            formatIOS = serializedObject.FindProperty("formatIOS");
            compressionIOS = serializedObject.FindProperty("compressionIOS");
            isOverrideEnabledIOS = serializedObject.FindProperty("isOverrideEnabledIOS");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.LabelField(styles.textureTypeLabel, styles.textureTypeValue, EditorStyles.popup);
                EditorGUILayout.LabelField(styles.textureShapeLabel, styles.textureShapeValue, EditorStyles.popup);
                EditorGUILayout.Separator();
            }

            EditorGUILayout.LabelField("Texture2DArray asset", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_Tex2DArray, styles.texture2DArrayLabel);
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Texture2DArray settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_WrapMode, styles.wrapModeLabel);
            EditorGUILayout.PropertyField(m_FilterMode, styles.filterModeLabel);
            EditorGUILayout.PropertyField(m_AnisoLevel, styles.anisoLevelLabel);
            EditorGUILayout.PropertyField(m_IsReadable, styles.readableLabel);
            EditorGUILayout.PropertyField(m_SRGB, styles.srgbLabel);
            EditorGUILayout.Separator();

            // If Aniso is used, check quality settings and displays some info.
            // I've only added this, because Unity is doing it in the Texture Inspector as well.
            if (m_AnisoLevel.intValue > 1)
            {
                if (QualitySettings.anisotropicFiltering == AnisotropicFiltering.Disable)
                    EditorGUILayout.HelpBox(styles.anisotropicFilteringDisable.text, MessageType.Info);

                if (QualitySettings.anisotropicFiltering == AnisotropicFiltering.ForceEnable)
                    EditorGUILayout.HelpBox(styles.anisotropicFilteringForceEnable.text, MessageType.Info);
            }

            selectedTab = (Tab)GUILayout.Toolbar((int)selectedTab,
                new[] { "Default", "Standalone", "Android", "IOS" });

            switch (selectedTab)
            {
                case Tab.Default:
                    EditorGUILayout.PropertyField(maxSizeDefault, styles.maxSizeLabel);
                    EditorGUILayout.PropertyField(resizeAlgorithmDefault, styles.resizeAlgorithmLabel);
                    EditorGUILayout.PropertyField(formatDefault, styles.formatLabel);
                    EditorGUILayout.PropertyField(compressionDefault, styles.compressionLabel);

                    break;

                case Tab.Standalone:
                    EditorGUILayout.PropertyField(isOverrideEnabledStandalone, styles.overrideLabelStandalone);
                    EditorGUILayout.PropertyField(maxSizeStandalone, styles.maxSizeLabel);
                    EditorGUILayout.PropertyField(resizeAlgorithmStandalone, styles.resizeAlgorithmLabel);
                    EditorGUILayout.PropertyField(formatStandalone, styles.formatLabel);
                    EditorGUILayout.PropertyField(compressionStandalone, styles.compressionLabel);

                    break;

                case Tab.Android:
                    EditorGUILayout.PropertyField(isOverrideEnabledAndroid, styles.overrideLabelAndroid);
                    EditorGUILayout.PropertyField(maxSizeAndroid, styles.maxSizeLabel);
                    EditorGUILayout.PropertyField(resizeAlgorithmAndroid, styles.resizeAlgorithmLabel);
                    EditorGUILayout.PropertyField(formatAndroid, styles.formatLabel);
                    EditorGUILayout.PropertyField(compressionAndroid, styles.compressionLabel);

                    break;

                case Tab.IOS:
                    EditorGUILayout.PropertyField(isOverrideEnabledIOS, styles.overrideLabelIOS);
                    EditorGUILayout.PropertyField(maxSizeIOS, styles.maxSizeLabel);
                    EditorGUILayout.PropertyField(resizeAlgorithmIOS, styles.resizeAlgorithmLabel);
                    EditorGUILayout.PropertyField(formatIOS, styles.formatLabel);
                    EditorGUILayout.PropertyField(compressionIOS, styles.compressionLabel);

                    break;
            }

            serializedObject.ApplyModifiedProperties();
            ApplyRevertGUI();
        }
    }
}