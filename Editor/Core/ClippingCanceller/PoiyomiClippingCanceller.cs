using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Poyo.EditorToolbox.Editor
{
    internal static class PoiyomiClippingCanceller
    {
        internal const string PropertyName = "_RenderingReduceClipDistance";
        internal const string LockedShaderPrefix = "Hidden/Locked/";
        private static readonly Regex VariantParentPattern = new Regex(@"(?m)^\s*m_Parent:\s*\{fileID:\s*(?!0\b)-?\d+");

        internal sealed class ExcludedEntry
        {
            internal string Guid;
            internal Material Material;
            internal bool IsOn;
            internal bool IsLocked;
            internal GUIContent MissingLabel;
        }

        internal sealed class ScanResult
        {
            internal readonly List<Material> Targets = new List<Material>();
            internal readonly List<ExcludedEntry> Excluded = new List<ExcludedEntry>();
            internal int OnCount;
            internal int OffCount;
            internal int LockedCount;
            internal int FollowingParentCount;
        }

        internal struct ApplyResult
        {
            internal int Changed;
            internal int Unlocked;
            internal int SkippedLocked;
        }

        internal static bool IsPoiyomi(Material material)
        {
            return material != null && material.HasProperty(PropertyName);
        }

        internal static bool IsOn(Material material)
        {
            return material.GetFloat(PropertyName) >= 0.5f;
        }

        internal static bool IsLocked(Material material)
        {
            return material.shader != null && material.shader.name.StartsWith(LockedShaderPrefix, StringComparison.Ordinal);
        }

        internal static ScanResult Scan(string[] folders, IReadOnlyList<string> excludedGuids)
        {
            var result = new ScanResult();
            var excluded = new HashSet<string>();
            if (excludedGuids != null)
            {
                for (int i = 0; i < excludedGuids.Count; i++)
                {
                    excluded.Add(excludedGuids[i]);
                }
            }
            string[] guids = AssetDatabase.FindAssets("t:Material", folders);
            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    if (i % 50 == 0)
                    {
                        EditorUtility.DisplayProgressBar("距離クリッピングキャンセラー",
                            "Poiyomi のマテリアルを探しています…", i / (float)guids.Length);
                    }
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (!path.EndsWith(".mat", StringComparison.OrdinalIgnoreCase) || excluded.Contains(guids[i]) ||
                        !MayBePoiyomi(path))
                    {
                        continue;
                    }
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (!IsPoiyomi(material))
                    {
                        continue;
                    }
                    // NOTE: 親に従う Variant に書くと上書きになるため、親と一緒に変わるものは書かない。
                    if (material.parent != null && !material.IsPropertyOverriden(PropertyName))
                    {
                        result.FollowingParentCount++;
                        continue;
                    }
                    result.Targets.Add(material);
                    if (IsOn(material))
                    {
                        result.OnCount++;
                    }
                    else
                    {
                        result.OffCount++;
                    }
                    if (IsLocked(material))
                    {
                        result.LockedCount++;
                    }
                }
                if (excludedGuids != null)
                {
                    for (int i = 0; i < excludedGuids.Count; i++)
                    {
                        string guid = excludedGuids[i];
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        Material material = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Material>(path);
                        var entry = new ExcludedEntry { Guid = guid };
                        if (IsPoiyomi(material))
                        {
                            entry.Material = material;
                            entry.IsOn = IsOn(material);
                            entry.IsLocked = IsLocked(material);
                        }
                        else
                        {
                            entry.MissingLabel = new GUIContent("（見つかりません） " + guid);
                        }
                        result.Excluded.Add(entry);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            return result;
        }

        private static bool MayBePoiyomi(string path)
        {
            // NOTE: 読み込みでテクスチャまで展開されるため、プロパティ名のあるテキストだけを先に選ぶ。
            try
            {
                string text = File.ReadAllText(path);
                // NOTE: Variant の YAML には継承したプロパティがないため、親のあるものも確認する。
                return !text.StartsWith("%YAML", StringComparison.Ordinal) ||
                    text.Contains(PropertyName, StringComparison.Ordinal) || VariantParentPattern.IsMatch(text);
            }
            catch (IOException exception)
            {
                Debug.LogException(exception);
            }
            catch (UnauthorizedAccessException exception)
            {
                Debug.LogException(exception);
            }
            return false;
        }

        internal static ApplyResult SetEnabled(IReadOnlyList<Material> materials, bool enabled)
        {
            var result = new ApplyResult();
            var toChange = new List<Material>();
            var locked = new HashSet<Material>();
            for (int i = 0; i < materials.Count; i++)
            {
                Material material = materials[i];
                if (material == null || IsOn(material) == enabled)
                {
                    continue;
                }
                toChange.Add(material);
                if (IsLocked(material))
                {
                    locked.Add(material);
                }
            }
            if (locked.Count > 0 && PoiyomiLockBridge.UnlockMaterials != null)
            {
                try
                {
                    PoiyomiLockBridge.UnlockMaterials(new List<Material>(locked));
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
            // NOTE: 多数の変更とロック解除をまとめて戻せないため、Undo には載せない。
            foreach (Material material in toChange)
            {
                if (IsLocked(material))
                {
                    result.SkippedLocked++;
                    continue;
                }
                material.SetFloat(PropertyName, enabled ? 1f : 0f);
                EditorUtility.SetDirty(material);
                AssetDatabase.SaveAssetIfDirty(material);
                result.Changed++;
                if (locked.Contains(material))
                {
                    result.Unlocked++;
                }
            }
            if (result.Changed > 0)
            {
                Debug.Log(EditorToolboxInfo.LogPrefix + "Poiyomi の距離クリッピングキャンセラーを " +
                    (enabled ? "ON" : "OFF") + " にしました（" + result.Changed + " 件。うちロックを解除したもの " +
                    result.Unlocked + " 件）。");
            }
            if (result.SkippedLocked > 0)
            {
                Debug.LogWarning(EditorToolboxInfo.LogPrefix + GetSkippedLockedMessage(result.SkippedLocked));
            }
            return result;
        }

        internal static string GetSkippedLockedMessage(int count)
        {
            return "ロック中のため変えなかった Poiyomi のマテリアルが " + count +
                " 件あります。Poiyomi のロックを解除してから、もう一度切り替えてください。";
        }
    }
}
