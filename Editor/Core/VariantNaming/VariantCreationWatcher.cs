using UnityEditor;

namespace Poyo.EditorToolbox.Editor
{
    internal sealed class VariantCreationWatcher : AssetModificationProcessor
    {
        private static void OnWillCreateAsset(string path)
        {
            VariantNameProcessor.NotifyCreating(path);
        }
    }
}
