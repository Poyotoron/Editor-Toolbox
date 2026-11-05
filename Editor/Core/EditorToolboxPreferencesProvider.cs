using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Poyo.EditorToolbox.Editor.EditorToolboxToggleGUI;

namespace Poyo.EditorToolbox.Editor
{
    internal static class EditorToolboxPreferencesProvider
    {
        private static readonly GUIContent HeaderContent = new GUIContent("バージョン " + EditorToolboxInfo.Version);
        private static readonly GUIContent DescriptionContent = new GUIContent("全プロジェクトに共通の設定です。プロジェクトごとに変えたいときは Project Settings > Poyo's Editor Toolbox で上書きします。");
        private static readonly GUIContent OpenProjectContent = new GUIContent("このプロジェクトの設定を開く");
        private static readonly GUIContent FeaturesContent = new GUIContent("エディタ機能");

        [SettingsProvider]
        private static SettingsProvider Create()
        {
            return new SettingsProvider(EditorToolboxInfo.PreferencesPath, SettingsScope.User)
            {
                label = EditorToolboxInfo.DisplayName,
                guiHandler = OnGUI,
                activateHandler = OnActivate,
                keywords = new HashSet<string>(new[]
                {
                    "toolbox", "hierarchy", "editoronly", "variant", "prefab", "lighting",
                    "scene", "vrchat", "thumbnail", "inspector", "irodori", "notification", "webhook", "discord", "sound"
                }),
            };
        }

        [MenuItem(EditorToolboxInfo.PreferencesMenuPath, false, EditorToolboxInfo.PreferencesMenuPriority)]
        private static void OpenPreferences()
        {
            SettingsService.OpenUserPreferences(EditorToolboxInfo.PreferencesPath);
        }

        [MenuItem("Assets/" + EditorToolboxInfo.DisplayName + "/Preferences…", false, 1131)]
        private static void OpenPreferencesFromAssets()
        {
            OpenPreferences();
        }

        [MenuItem("GameObject/" + EditorToolboxInfo.DisplayName + "/Preferences…", false, 43)]
        private static void OpenPreferencesFromGameObject()
        {
            OpenPreferences();
        }

        private static void OnActivate(string searchContext, VisualElement rootElement)
        {
            EditorToolboxGlobalSettings.Reload();
        }

        private static void OnGUI(string searchContext)
        {
            if (EditorToolboxLegacy.DrawWarningIfInstalled())
            {
                return;
            }
            EnsureStyles();
            EditorGUILayout.LabelField(HeaderContent, EditorStyles.miniLabel);
            EditorGUILayout.LabelField(DescriptionContent, DescriptionStyle);
            if (GUILayout.Button(OpenProjectContent))
            {
                SettingsService.OpenProjectSettings(EditorToolboxInfo.SettingsPath);
            }
            if (HasAnyOverride())
            {
                EditorGUILayout.HelpBox(GlobalOverrideNotice, MessageType.Info);
            }
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(FeaturesContent, EditorStyles.boldLabel);
            DrawGlobalFeature(EditorToolboxToggle.HierarchyToggles, HierarchyTitle, HierarchyDescription, true);
            DrawGlobalFeature(EditorToolboxToggle.StripVariantSuffix, VariantTitle, VariantDescription, true);
            DrawGlobalFeature(EditorToolboxToggle.SceneSetup, SceneTitle, SceneDescription, true);
            DrawGlobalFeature(EditorToolboxToggle.VrcSdkAutoFill, SdkTitle,
                HasVrcSdk ? SdkDescription : SdkUnavailableDescription, HasVrcSdk);
            DrawGlobalFeature(EditorToolboxToggle.InspectorNewWindowButton, InspectorTitle, InspectorDescription, true);
            DrawGlobalFeature(EditorToolboxToggle.InspectorCopyToButton, CopyToTitle, CopyToDescription, true);
            DrawGlobalFeature(EditorToolboxToggle.IrodoriColorizer, ColorizerToggleTitle, ColorizerDescription,
                !StandaloneColorizerNotice.IsStandaloneInstalled);
            NotificationSettingsGUI.DrawGlobal();
        }
    }
}
