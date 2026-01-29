using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace WebViewToolkit.Editor.Cleanup
{
    [InitializeOnLoad]
    public static class FixWebViewScene
    {
        static FixWebViewScene()
        {
            EditorApplication.update += Cleanup;
        }

        private static void Cleanup()
        {
            EditorApplication.update -= Cleanup;

            // Find stale GameObject
            var go = GameObject.Find("[WebViewManager]");
            if (go != null)
            {
                Debug.Log("[FixWebViewScene] Found stale '[WebViewManager]' GameObject. Deleting...");
                GameObject.DestroyImmediate(go);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log("[FixWebViewScene] Deleted stale GameObject. Please save the scene.");
            }
        }
    }
}
