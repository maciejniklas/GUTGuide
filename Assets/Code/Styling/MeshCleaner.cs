using UnityEngine;

namespace Code.Styling
{
    /// <summary>
    /// This component is used to clean up the mesh to prevent a memory leak that happens when decorations, such as
    /// parapets, are dynamically loaded and unloaded
    /// </summary>
    public class MeshCleaner : MonoBehaviour
    {
        private void OnDestroy()
        {
            var meshFilter = gameObject.GetComponent<MeshFilter>();

            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                Destroy(meshFilter.sharedMesh);
            }
        }
    }
}