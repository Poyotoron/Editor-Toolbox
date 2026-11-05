using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Poyo.EditorToolbox.Editor.EditorToolboxToggleGUI;

namespace Poyo.EditorToolbox.Editor
{
    internal static class EditorToolboxSettingsProvider
    {
        private const string ColorizerSettingsPath = "Project/Poyo's Editor Toolbox/Irodori Colorizer";

        private static readonly GUIContent HeaderContent =
            new GUIContent("バージョン " + EditorToolboxInfo.Version);
        private static readonly GUIContent FeaturesContent = new GUIContent("エディタ機能");
        private static readonly GUIContent HierarchyOffsetContent = new GUIContent(
            "右端からの距離",
            "行の右端からチェックボックスまでの距離（px）。ほかの拡張のボタンと重なるときは大きくします。");
        private static readonly GUIContent ApplySceneContent = new GUIContent("今のシーンに適用");
        private static readonly GUIContent ResetIgnoredScenesContent = new GUIContent("確認しないシーンをリセット");
        private static readonly GUIContent BlendShapeTitle = new GUIContent("ブレンドシェイプのスライダー");
        private static readonly GUIContent BlendShapeDescription = new GUIContent(
            "SkinnedMeshRenderer のブレンドシェイプを、下限と上限を決めたスライダーで動かすウィンドウです。範囲の外の値は入力できません。");
        private static readonly GUIContent OpenBlendShapeContent = new GUIContent("ウィンドウを開く");
        private static readonly GUIContent ClippingCancellerTitle = new GUIContent("距離クリッピングキャンセラー");
        private static readonly GUIContent ClippingCancellerDescription = new GUIContent(
            "lilToon と Poiyomi の距離クリッピングキャンセラー（カメラが近づいたときにメッシュが欠けて見えるのを抑える設定）を、まとめて切り替えます。");
        private static readonly GUIContent OpenClippingCancellerContent = new GUIContent("距離クリッピングキャンセラーの設定を開く");
        private static readonly GUIContent ColorizerTitle = new GUIContent("Irodori Colorizer");
        private static readonly GUIContent ColorizerSettingsContent = new GUIContent("Irodori Colorizer の設定を開く");

        private static readonly GUIContent GlobalDescription = new GUIContent("ON / OFF は全体設定（Preferences > Poyo's Editor Toolbox）に従います。ここで切り替えると、このプロジェクトだけ上書きします。");
        private static readonly GUIContent OpenGlobalContent = new GUIContent("全体設定を開く");

        [SettingsProvider]
        private static SettingsProvider Create()
        {
            return new SettingsProvider(EditorToolboxInfo.SettingsPath, SettingsScope.Project)
            {
                label = EditorToolboxInfo.DisplayName,
                guiHandler = OnGUI,
                activateHandler = OnActivate,
                keywords = new HashSet<string>(new[]
                {
                    "toolbox", "hierarchy", "editoronly", "variant", "prefab",
                    "lighting", "scene", "vrchat", "thumbnail", "inspector", "blendshape", "irodori", "notification", "webhook", "discord", "sound",
                    "clipping", "liltoon", "poiyomi"
                }),
            };
        }

        [MenuItem(EditorToolboxInfo.MenuPath, false, EditorToolboxInfo.MenuPriority)]
        private static void OpenSettings()
        {
            SettingsService.OpenProjectSettings(EditorToolboxInfo.SettingsPath);
        }

        // NOTE: 色付けの項目と離して区切り線を入れる。GameObject 側は 49 以下で Hierarchy の右クリックにも表示される。
        [MenuItem("Assets/" + EditorToolboxInfo.DisplayName + "/Project Settings…", false, 1130)]
        private static void OpenSettingsFromAssets()
        {
            OpenSettings();
        }

        [MenuItem("GameObject/" + EditorToolboxInfo.DisplayName + "/Project Settings…", false, 42)]
        private static void OpenSettingsFromGameObject()
        {
            OpenSettings();
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
            EditorGUILayout.LabelField(GlobalDescription, DescriptionStyle);
            if (GUILayout.Button(OpenGlobalContent))
            {
                SettingsService.OpenUserPreferences(EditorToolboxInfo.PreferencesPath);
            }
            if (HasAnyOverride())
            {
                EditorGUILayout.HelpBox(ProjectOverrideNotice, MessageType.Info);
            }
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(FeaturesContent, EditorStyles.boldLabel);

            EditorToolboxSettings settings = EditorToolboxSettings.Instance;
            DrawProjectFeature(EditorToolboxToggle.HierarchyToggles, HierarchyTitle, HierarchyDescription, true);
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(!EditorToolboxToggles.IsOn(EditorToolboxToggle.HierarchyToggles));
            EditorGUI.indentLevel++;
            float offset = EditorGUILayout.Slider(HierarchyOffsetContent, settings.hierarchyToggleRightOffset,
                0f, EditorToolboxSettings.MaxHierarchyToggleRightOffset);
            settings.hierarchyToggleRightOffset = Mathf.Round(offset);
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space(4f);
            if (EditorGUI.EndChangeCheck())
            {
                settings.SaveChanges();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
            DrawProjectFeature(EditorToolboxToggle.StripVariantSuffix, VariantTitle, VariantDescription, true);
            DrawProjectFeature(EditorToolboxToggle.SceneSetup, SceneTitle, SceneDescription, true);
            DrawSceneButtons(settings);
            DrawProjectFeature(EditorToolboxToggle.VrcSdkAutoFill, SdkTitle,
                HasVrcSdk ? SdkDescription : SdkUnavailableDescription, HasVrcSdk);
            DrawProjectFeature(EditorToolboxToggle.InspectorNewWindowButton, InspectorTitle, InspectorDescription, true);
            DrawProjectFeature(EditorToolboxToggle.InspectorCopyToButton, CopyToTitle, CopyToDescription, true);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(BlendShapeTitle);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(BlendShapeDescription, DescriptionStyle);
            BlendShapeSliderWindow.DrawRangeFields(settings);
            EditorGUI.indentLevel--;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15f);
            if (GUILayout.Button(OpenBlendShapeContent))
            {
                BlendShapeSliderWindow.Open(null);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(ClippingCancellerTitle);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(ClippingCancellerDescription, DescriptionStyle);
            EditorGUI.indentLevel--;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15f);
            if (GUILayout.Button(OpenClippingCancellerContent))
            {
                SettingsService.OpenProjectSettings(EditorToolboxInfo.ClippingCancellerSettingsPath);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField(ColorizerTitle, EditorStyles.boldLabel);
            bool installed = StandaloneColorizerNotice.IsStandaloneInstalled;
            DrawProjectFeature(EditorToolboxToggle.IrodoriColorizer, ColorizerToggleTitle, ColorizerDescription, !installed);
            if (installed)
            {
                EditorGUILayout.HelpBox(StandaloneColorizerNotice.WindowWarning, MessageType.Warning);
            }
            else
            {
                if (GUILayout.Button(ColorizerSettingsContent))
                {
                    SettingsService.OpenProjectSettings(ColorizerSettingsPath);
                }
            }
            NotificationSettingsGUI.DrawProject();

        }

        private static void DrawSceneButtons(EditorToolboxSettings settings)
        {
            EditorGUI.BeginDisabledGroup(!EditorToolboxToggles.IsOn(EditorToolboxToggle.SceneSetup) || EditorApplication.isPlayingOrWillChangePlaymode);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15f);
            if (GUILayout.Button(ApplySceneContent))
            {
                SceneSetupWatcher.ConvertActiveScene();
            }

            EditorGUI.BeginDisabledGroup(settings.GetIgnoredSceneGuids().Count == 0);
            if (GUILayout.Button(ResetIgnoredScenesContent))
            {
                settings.GetIgnoredSceneGuids().Clear();
                settings.SaveChanges();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space(4f);
        }
    }
}
