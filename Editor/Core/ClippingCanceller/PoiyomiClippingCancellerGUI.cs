using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Poyo.EditorToolbox.Editor.EditorToolboxToggleGUI;

namespace Poyo.EditorToolbox.Editor
{
    internal static class PoiyomiClippingCancellerGUI
    {
        private static readonly GUIContent PoiyomiDescription = new GUIContent("Poiyomi の設定はマテリアルごとです（Rendering の Reduce Clip Distance）。チェックを切り替えると、Assets にある Poiyomi のマテリアルを、除外したものを除いてまとめて ON / OFF します。新しく入れたマテリアルには自動では効かないので、混在の表示になったら切り替え直してください。");
        private static readonly GUIContent PoiyomiToggle = new GUIContent("距離クリッピングキャンセラーを使う");
        private static readonly string NoMaterials = "Assets に Poiyomi のマテリアルがありません。";
        private static readonly string PoiyomiNoThry = "ThryEditor 2.74.3 以上が VCC などで入っていないため、ロック中のマテリアルは切り替えません。Poiyomi のロックを解除してから切り替えてください。";
        private static readonly GUIContent ExcludedTitle = new GUIContent("一括切り替えから除外するマテリアル");
        private static readonly GUIContent ExcludedDescription = new GUIContent("ここに入れたマテリアルは、上のチェックで切り替えません。行の「ON」で、そのマテリアルだけ切り替えられます。");
        private static readonly GUIContent RowToggle = new GUIContent("ON", "このマテリアルだけ、距離クリッピングキャンセラーを切り替えます。");
        private static readonly GUIContent RowLocked = new GUIContent("ロック中");
        private static readonly GUIContent AddContent = new GUIContent("除外に追加");
        private static readonly GUIContent AddSelectionContent = new GUIContent("選択中のマテリアルを追加");
        private static readonly GUIContent RemoveContent = new GUIContent("除外をやめる");
        private static readonly GUIContent RescanContent = new GUIContent("再スキャン");
        private static readonly string[] ScanFolders = { "Assets" };
        private static readonly List<Material> SingleBuffer = new List<Material>(1);
        private static PoiyomiClippingCanceller.ScanResult _scan;
        private static GUIContent _status;
        private static string _message;
        private static GUILayoutOption _rowToggleWidth;
        private static GUILayoutOption _rowLockedWidth;
        private static GUILayoutOption _removeWidth;

        internal static void Rescan()
        {
            _scan = PoiyomiClippingCanceller.Scan(ScanFolders, EditorToolboxSettings.Instance.GetClippingCancellerExcludedGuids());
            UpdateStatus();
        }

        internal static bool Recount()
        {
            // NOTE: マテリアルの Inspector で変えた値を反映する。ファイルは読まず、読み込み済みのマテリアルだけを数え直す。
            if (_scan == null)
            {
                return false;
            }
            int on = 0;
            int off = 0;
            int locked = 0;
            foreach (Material material in _scan.Targets)
            {
                if (material == null)
                {
                    continue;
                }
                if (PoiyomiClippingCanceller.IsOn(material))
                {
                    on++;
                }
                else
                {
                    off++;
                }
                if (PoiyomiClippingCanceller.IsLocked(material))
                {
                    locked++;
                }
            }
            bool changed = on != _scan.OnCount || off != _scan.OffCount || locked != _scan.LockedCount;
            _scan.OnCount = on;
            _scan.OffCount = off;
            _scan.LockedCount = locked;
            foreach (PoiyomiClippingCanceller.ExcludedEntry entry in _scan.Excluded)
            {
                if (entry.Material == null)
                {
                    continue;
                }
                bool isOn = PoiyomiClippingCanceller.IsOn(entry.Material);
                bool isLocked = PoiyomiClippingCanceller.IsLocked(entry.Material);
                if (isOn != entry.IsOn || isLocked != entry.IsLocked)
                {
                    entry.IsOn = isOn;
                    entry.IsLocked = isLocked;
                    changed = true;
                }
            }
            if (changed)
            {
                UpdateStatus();
            }
            return changed;
        }

        private static void UpdateStatus()
        {
            string text = "対象 " + _scan.Targets.Count + " 件（ON " + _scan.OnCount + " / OFF " + _scan.OffCount +
                "）・除外 " + _scan.Excluded.Count + " 件・ロック中 " + _scan.LockedCount + " 件";
            if (_scan.FollowingParentCount > 0)
            {
                text += "・親の値に従う Variant " + _scan.FollowingParentCount + " 件";
            }
            _status = new GUIContent(text);
        }

        internal static void ResetMessage()
        {
            _message = null;
        }

        private static void EnsureWidths()
        {
            if (_rowToggleWidth != null)
            {
                return;
            }
            _rowToggleWidth = GUILayout.Width(GUI.skin.toggle.CalcSize(RowToggle).x + 4f);
            _rowLockedWidth = GUILayout.Width(EditorStyles.miniLabel.CalcSize(RowLocked).x + 4f);
            _removeWidth = GUILayout.Width(EditorStyles.miniButton.CalcSize(RemoveContent).x + 8f);
        }

        internal static void Draw()
        {
            if (_scan == null)
            {
                Rescan();
            }
            EnsureWidths();
            EditorGUILayout.LabelField(PoiyomiDescription, DescriptionStyle);
            if (_scan.Targets.Count == 0 && _scan.Excluded.Count == 0 && _scan.FollowingParentCount == 0)
            {
                EditorGUILayout.HelpBox(NoMaterials, MessageType.Info);
                DrawRescan();
                return;
            }
            bool allOn = _scan.OnCount > 0 && _scan.OffCount == 0;
            EditorGUI.showMixedValue = _scan.OnCount > 0 && _scan.OffCount > 0;
            EditorGUI.BeginDisabledGroup(_scan.Targets.Count == 0 || EditorApplication.isPlayingOrWillChangePlaymode);
            bool next = EditorGUILayout.ToggleLeft(PoiyomiToggle, allOn);
            EditorGUI.EndDisabledGroup();
            EditorGUI.showMixedValue = false;
            if (next != allOn)
            {
                ConfirmAndApply(!allOn);
            }
            EditorGUILayout.LabelField(_status, EditorStyles.miniLabel);
            if (_scan.LockedCount > 0 && PoiyomiLockBridge.UnlockMaterials == null)
            {
                EditorGUILayout.HelpBox(PoiyomiNoThry, MessageType.Info);
            }
            if (!string.IsNullOrEmpty(_message))
            {
                EditorGUILayout.HelpBox(_message, MessageType.Warning);
            }
            DrawRescan();
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(ExcludedTitle);
            EditorGUILayout.LabelField(ExcludedDescription, DescriptionStyle);
            DrawExcluded();
            DrawAdd();
        }

        private static void ConfirmAndApply(bool enabled)
        {
            int count = 0;
            int locked = 0;
            foreach (Material material in _scan.Targets)
            {
                if (material != null && PoiyomiClippingCanceller.IsOn(material) != enabled)
                {
                    count++;
                    if (PoiyomiClippingCanceller.IsLocked(material))
                    {
                        locked++;
                    }
                }
            }
            string text = "Poiyomi のマテリアル " + count + " 件で、距離クリッピングキャンセラーを " +
                (enabled ? "ON" : "OFF") + " にします（除外したもの・親の値に従う Variant は変えません）。";
            if (locked > 0)
            {
                text += PoiyomiLockBridge.UnlockMaterials != null
                    ? "\n\nロック中の " + locked + " 件は、ロックを解除してから変えます。解除したままになりますが、アップロード時に Poiyomi が自動でロックし直します。"
                    : "\n\nロック中の " + locked + " 件は変えません。";
            }
            text += "\n\nUndo では戻せません。";
            // NOTE: チェックを押した直後に多数のマテリアルを書き換えるため、モーダルで確かめる。
            if (EditorUtility.DisplayDialog("距離クリッピングキャンセラー", text, "切り替える", "キャンセル"))
            {
                PoiyomiClippingCanceller.ApplyResult result = PoiyomiClippingCanceller.SetEnabled(_scan.Targets, enabled);
                _message = result.SkippedLocked > 0 ? PoiyomiClippingCanceller.GetSkippedLockedMessage(result.SkippedLocked) : null;
            }
            Rescan();
            GUIUtility.ExitGUI();
        }

        private static void DrawRescan()
        {
            if (GUILayout.Button(RescanContent))
            {
                Rescan();
                GUIUtility.ExitGUI();
            }
        }

        private static void DrawExcluded()
        {
            foreach (PoiyomiClippingCanceller.ExcludedEntry entry in _scan.Excluded)
            {
                bool next = entry.IsOn;
                EditorGUILayout.BeginHorizontal();
                if (entry.Material == null)
                {
                    EditorGUILayout.LabelField(entry.MissingLabel);
                }
                else
                {
                    EditorGUILayout.ObjectField(entry.Material, typeof(Material), false);
                    bool disabled = (entry.IsLocked && PoiyomiLockBridge.UnlockMaterials == null) ||
                        EditorApplication.isPlayingOrWillChangePlaymode;
                    EditorGUI.BeginDisabledGroup(disabled);
                    next = GUILayout.Toggle(entry.IsOn, RowToggle, _rowToggleWidth);
                    EditorGUI.EndDisabledGroup();
                    if (disabled)
                    {
                        EditorGUILayout.LabelField(RowLocked, EditorStyles.miniLabel, _rowLockedWidth);
                    }
                }
                bool remove = GUILayout.Button(RemoveContent, EditorStyles.miniButton, _removeWidth);
                EditorGUILayout.EndHorizontal();
                if (next != entry.IsOn)
                {
                    SingleBuffer.Clear();
                    SingleBuffer.Add(entry.Material);
                    PoiyomiClippingCanceller.ApplyResult result = PoiyomiClippingCanceller.SetEnabled(SingleBuffer, next);
                    _message = result.SkippedLocked > 0 ? PoiyomiClippingCanceller.GetSkippedLockedMessage(result.SkippedLocked) : null;
                    Rescan();
                    GUIUtility.ExitGUI();
                }
                if (remove)
                {
                    EditorToolboxSettings.Instance.GetClippingCancellerExcludedGuids().Remove(entry.Guid);
                    EditorToolboxSettings.Instance.SaveChanges();
                    Rescan();
                    GUIUtility.ExitGUI();
                }
            }
        }

        private static void DrawAdd()
        {
            Material added = (Material)EditorGUILayout.ObjectField(AddContent, null, typeof(Material), false);
            if (added != null)
            {
                var reasons = new List<string>();
                TryAddExcluded(added, reasons);
                FinishAdd(reasons);
            }
            if (GUILayout.Button(AddSelectionContent))
            {
                var reasons = new List<string>();
                bool found = false;
                foreach (UnityEngine.Object obj in Selection.objects)
                {
                    Material material = obj as Material;
                    if (material != null)
                    {
                        found = true;
                        TryAddExcluded(material, reasons);
                    }
                }
                if (!found)
                {
                    reasons.Add("マテリアルが選択されていません。");
                }
                FinishAdd(reasons);
            }
        }

        private static void FinishAdd(List<string> reasons)
        {
            _message = reasons.Count > 0 ? string.Join("\n", reasons) : null;
            EditorToolboxSettings.Instance.SaveChanges();
            Rescan();
            GUIUtility.ExitGUI();
        }

        private static bool TryAddExcluded(Material material, List<string> reasons)
        {
            string path = AssetDatabase.GetAssetPath(material);
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) || !path.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
            {
                reasons.Add("「" + material.name + "」は Assets にある .mat ではないため、追加しませんでした。");
                return false;
            }
            if (!PoiyomiClippingCanceller.IsPoiyomi(material))
            {
                reasons.Add("「" + material.name + "」は Poiyomi のマテリアルではないため、追加しませんでした。");
                return false;
            }
            string guid = AssetDatabase.AssetPathToGUID(path);
            List<string> excluded = EditorToolboxSettings.Instance.GetClippingCancellerExcludedGuids();
            if (!excluded.Contains(guid))
            {
                excluded.Add(guid);
            }
            return true;
        }
    }
}
