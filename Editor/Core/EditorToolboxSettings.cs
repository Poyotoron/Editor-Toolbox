using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Poyo.EditorToolbox.Editor
{
    // NOTE: ProjectSettings に置くと Assets やビルド成果物を汚さず、プロジェクト単位で設定を持てる。
    [FilePath(EditorToolboxSettings.SettingsFilePath, FilePathAttribute.Location.ProjectFolder)]
    public sealed class EditorToolboxSettings : ScriptableSingleton<EditorToolboxSettings>
    {
        internal const string SettingsFilePath = "ProjectSettings/EditorToolbox.asset";
        private static readonly string[] LegacyPaths = { EditorToolboxLegacy.ProjectSettingsPath };
        private static bool _migrated;

        // NOTE: 引き継ぎより先に空の設定を読み込まないよう、入口を一本化する。
        internal static EditorToolboxSettings Instance
        {
            get
            {
                if (!_migrated)
                {
                    _migrated = EditorToolboxLegacy.TryCopySettingsFile(SettingsFilePath, LegacyPaths,
                        "Packages/net.maaaaa.editor-toolbox/Editor/Core/EditorToolboxSettings.cs", out string copiedFrom);
                    if (copiedFrom != null)
                    {
                        Debug.Log(EditorToolboxInfo.LogPrefix + "Marshmallow Jar の設定を引き継ぎました: " + copiedFrom + " → " + SettingsFilePath);
                    }
                }
                return instance;
            }
        }

        public int schemaVersion = 2;
        public List<EditorToolboxToggleValue> toggleOverrides = new List<EditorToolboxToggleValue>();
        public List<string> sceneSetupIgnoredSceneGuids = new List<string>();
        public List<string> clippingCancellerExcludedMaterialGuids = new List<string>();
        public float hierarchyToggleRightOffset = DefaultHierarchyToggleRightOffset;
        public float blendShapeSliderMin = DefaultBlendShapeSliderMin;
        public float blendShapeSliderMax = DefaultBlendShapeSliderMax;

        // NOTE: 行の右端には Prefab インスタンスを開く「>」ボタン（16px）と、他の拡張が置くボタンが並ぶ。
        //       既定ではそれらの分を空けて重ならない位置にする。
        public const float DefaultHierarchyToggleRightOffset = 56f;
        public const float MaxHierarchyToggleRightOffset = 200f;
        public const float DefaultBlendShapeSliderMin = 0f;
        public const float DefaultBlendShapeSliderMax = 100f;
        // NOTE: 下限と上限が近すぎるとスライダーが操作できないため、最低でもこの幅を保つ。
        public const float MinBlendShapeSliderSpan = 1f;

        [InitializeOnLoadMethod]
        private static void MigrateSchema()
        {
            EditorToolboxSettings settings = Instance;
            if (settings.schemaVersion >= 2)
            {
                return;
            }
            // NOTE: 旧版のプロジェクトごとの ON / OFF は全体設定に従わせるため読まずに捨て、距離などは残す。
            settings.schemaVersion = 2;
            settings.GetToggleOverrides().Clear();
            settings.SaveChanges();
            EditorToolboxGlobalSettings unused = EditorToolboxGlobalSettings.Instance;
            Debug.Log(EditorToolboxInfo.LogPrefix +
                (SessionState.GetBool(EditorToolboxGlobalSettings.CreatedSessionKey, false)
                    ? "各機能の ON / OFF は全体設定（Preferences > Poyo's Editor Toolbox）で決めます。最初はすべて OFF です。使う機能を ON にしてください。"
                    : "各機能の ON / OFF は全体設定（Preferences > Poyo's Editor Toolbox）に従います。このプロジェクトだけ変えたいときは Project Settings > Poyo's Editor Toolbox で上書きしてください。"));
            EditorToolboxToggles.Invalidate();
        }

        internal List<EditorToolboxToggleValue> GetToggleOverrides()
        {
            if (toggleOverrides == null)
            {
                toggleOverrides = new List<EditorToolboxToggleValue>();
            }
            return toggleOverrides;
        }

        internal void SetBlendShapeSliderRange(float min, float max, bool minEdited)
        {
            if (max - min < MinBlendShapeSliderSpan)
            {
                // NOTE: 入力した側を保ち、もう片方を動かすと範囲が逆転しても編集を続けられる。
                if (minEdited)
                {
                    max = min + MinBlendShapeSliderSpan;
                }
                else
                {
                    min = max - MinBlendShapeSliderSpan;
                }
            }

            blendShapeSliderMin = min;
            blendShapeSliderMax = max;
            SaveChanges();
        }

        public void SaveChanges()
        {
            // NOTE: 併存中に保存すると、旧版を削除したあとの引き継ぎを妨げる。
            if (EditorToolboxLegacy.IsInstalled)
            {
                return;
            }
            Save(true);
        }

        internal List<string> GetClippingCancellerExcludedGuids()
        {
            if (clippingCancellerExcludedMaterialGuids == null)
            {
                clippingCancellerExcludedMaterialGuids = new List<string>();
            }
            return clippingCancellerExcludedMaterialGuids;
        }

        internal List<string> GetIgnoredSceneGuids()
        {
            if (sceneSetupIgnoredSceneGuids == null)
            {
                sceneSetupIgnoredSceneGuids = new List<string>();
            }

            return sceneSetupIgnoredSceneGuids;
        }
    }
}
