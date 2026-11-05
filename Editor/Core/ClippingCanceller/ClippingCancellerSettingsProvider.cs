using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Poyo.EditorToolbox.Editor.EditorToolboxToggleGUI;

namespace Poyo.EditorToolbox.Editor
{
    internal static class ClippingCancellerSettingsProvider
    {
        private static readonly GUIContent PageTitle = new GUIContent("距離クリッピングキャンセラー");
        private static readonly GUIContent PageDescription = new GUIContent("カメラがメッシュに近づいたときに、手前が切れて欠けて見えるのを抑えるシェーダーの設定を、まとめて切り替えます。切り替えたときだけ、シェーダーの設定やマテリアルを書き換えます。");
        private static readonly GUIContent PoiyomiTitle = new GUIContent("Poiyomi");
        private static readonly GUIContent LilToonTitle = new GUIContent("lilToon");
        private static readonly GUIContent LilToonToggle = new GUIContent("距離クリッピングキャンセラーを使う");
        private static readonly GUIContent LilToonDescription = new GUIContent("lilToon の設定はプロジェクトのすべてのマテリアルで共通です。lilToon の「シェーダー設定 (全マテリアル共通)」にある「距離クリッピングキャンセラー」と同じものを切り替えます。切り替えるとシェーダーを作り直すため、しばらくかかります。lilToonMulti のマテリアルは、マテリアルごとの設定に従います。");
        private static readonly GUIContent LilToonUnavailable = new GUIContent("lilToon 2.0.0 以上を VCC などで入れているときに使えます。");
        private static readonly string LilToonLocked = "lilToon の設定がロックされているため切り替えられません。lilToon の設定画面でロックを外してください。";
        private static readonly string LilToonConfirmOn = "lilToon の距離クリッピングキャンセラーを ON にします。\n\nlilToon のシェーダーを作り直し、そのあとスクリプトを再読み込みします。しばらくかかります。";
        private static readonly string LilToonConfirmOff = "lilToon の距離クリッピングキャンセラーを OFF にします。\n\nlilToon のシェーダーを作り直し、そのあとスクリプトを再読み込みします。しばらくかかります。";
        private static readonly GUIContent ReloadContent = new GUIContent("再読み込み");
        private static LilToonClippingCanceller.State _lilToonState;
        private static bool _lilToonLocked;
        private static string _lilToonMessage;
        private static long _lilToonStamp;
        private static SettingsProvider _provider;

        [SettingsProvider]
        private static SettingsProvider Create()
        {
            _provider = new SettingsProvider(EditorToolboxInfo.ClippingCancellerSettingsPath, SettingsScope.Project)
            {
                label = "Clipping Canceller",
                guiHandler = OnGUI,
                activateHandler = OnActivate,
                inspectorUpdateHandler = OnInspectorUpdate,
                keywords = new HashSet<string>(new[]
                {
                    "clipping", "canceller", "near", "clip", "liltoon", "poiyomi", "距離クリッピングキャンセラー",
                }),
            };
            return _provider;
        }

        private static void Refresh()
        {
            _lilToonState = LilToonClippingCanceller.ReadState();
            _lilToonLocked = LilToonClippingCanceller.IsLocked();
            _lilToonStamp = LilToonClippingCanceller.IsInstalled ? LilToonClippingCanceller.GetChangeStamp() : 0L;
        }

        private static void OnInspectorUpdate()
        {
            // NOTE: マテリアルの Inspector で切り替えたときも、開いたままのこのページに反映する。
            bool changed = false;
            if (LilToonClippingCanceller.IsInstalled && LilToonClippingCanceller.GetChangeStamp() != _lilToonStamp)
            {
                Refresh();
                changed = true;
            }
            if (PoiyomiClippingCancellerGUI.Recount())
            {
                changed = true;
            }
            if (changed && _provider != null)
            {
                _provider.Repaint();
            }
        }

        private static void OnActivate(string searchContext, VisualElement rootElement)
        {
            Refresh();
            _lilToonMessage = SessionState.GetString(LilToonClippingCanceller.PendingMessageSessionKey, null);
            SessionState.EraseString(LilToonClippingCanceller.PendingMessageSessionKey);
            if (string.IsNullOrEmpty(_lilToonMessage))
            {
                _lilToonMessage = null;
            }
            PoiyomiClippingCancellerGUI.ResetMessage();
            PoiyomiClippingCancellerGUI.Rescan();
        }

        private static void OnGUI(string searchContext)
        {
            if (EditorToolboxLegacy.DrawWarningIfInstalled())
            {
                return;
            }
            EnsureStyles();
            EditorGUILayout.LabelField(PageTitle, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(PageDescription, DescriptionStyle);
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(LilToonTitle, EditorStyles.boldLabel);
            DrawLilToon();
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField(PoiyomiTitle, EditorStyles.boldLabel);
            PoiyomiClippingCancellerGUI.Draw();
        }

        private static void DrawLilToon()
        {
            if (_lilToonState == LilToonClippingCanceller.State.NotInstalled)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ToggleLeft(LilToonToggle, false);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.LabelField(LilToonUnavailable, DescriptionStyle);
                return;
            }
            if (_lilToonState == LilToonClippingCanceller.State.Unreadable)
            {
                EditorGUILayout.HelpBox(LilToonClippingCanceller.UnreadableMessage, MessageType.Warning);
                if (GUILayout.Button(ReloadContent))
                {
                    Refresh();
                }
                return;
            }
            bool current = _lilToonState == LilToonClippingCanceller.State.On;
            EditorGUI.BeginDisabledGroup(_lilToonLocked || EditorApplication.isPlayingOrWillChangePlaymode);
            bool next = EditorGUILayout.ToggleLeft(LilToonToggle, current);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.LabelField(LilToonDescription, DescriptionStyle);
            if (_lilToonLocked)
            {
                EditorGUILayout.HelpBox(LilToonLocked, MessageType.Info);
            }
            if (next != current)
            {
                // NOTE: 時間がかかるため、利用者がチェックを押した直後にモーダルで確かめる。
                bool confirmed = EditorUtility.DisplayDialog("距離クリッピングキャンセラー",
                    next ? LilToonConfirmOn : LilToonConfirmOff, "切り替える", "キャンセル");
                if (confirmed)
                {
                    LilToonClippingCanceller.TrySetEnabled(next, out _lilToonMessage);
                }
                Refresh();
                GUIUtility.ExitGUI();
            }
            if (!string.IsNullOrEmpty(_lilToonMessage))
            {
                EditorGUILayout.HelpBox(_lilToonMessage, MessageType.Warning);
            }
        }
    }
}
