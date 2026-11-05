using System;
using System.Collections.Generic;
using UnityEngine;

namespace Poyo.EditorToolbox.Editor
{
    public static class PoiyomiLockBridge
    {
        // NOTE: Core が ThryEditor なしでも動くよう、解除の関数を別アセンブリから差し込ませる。
        public static Func<IReadOnlyList<Material>, bool> UnlockMaterials;
    }
}
