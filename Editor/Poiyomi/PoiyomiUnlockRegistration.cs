using System.Collections.Generic;
using Poyo.EditorToolbox.Editor;
using Thry.ThryEditor;
using UnityEditor;
using UnityEngine;

namespace Poyo.EditorToolbox.Poiyomi.Editor
{
    [InitializeOnLoad]
    internal static class PoiyomiUnlockRegistration
    {
        static PoiyomiUnlockRegistration()
        {
            PoiyomiLockBridge.UnlockMaterials = Unlock;
        }

        private static bool Unlock(IReadOnlyList<Material> materials)
        {
            // NOTE: ロックし直すとコンパイルに時間がかかるため解除だけ行い、アップロード時の自動ロックに任せる。
            return ShaderOptimizer.UnlockMaterials(materials, ShaderOptimizer.ProgressBar.Uncancellable);
        }
    }
}
