using System;
using GUTGuide.Patterns;
using UnityEngine;
using UnityEngine.Events;

namespace GUTGuide.Utilities
{
    /// <summary>
    /// Controller of the pointing arrow
    /// </summary>
    public class PointingArrow : PersistentSingleton<PointingArrow>
    {
        [Tooltip("Reference to the game object of the 3D model of the pointing arrow")]
        [SerializeField] private GameObject model;

        /// <summary>
        /// The event called when the pointing arrow is spawned
        /// </summary>
        public UnityEvent onPointingArrowSpawn;

        private void Start()
        {
            Hide();
        }

        /// <summary>
        /// Hide the pointing arrow
        /// </summary>
        public void Hide()
        {
            model.SetActive(false);
        }
        
        /// <summary>
        /// Spawn the pointing arrow on the place of specified transform
        /// </summary>
        /// <param name="targetTransform">Target transform where the pointing arrow will spawn</param>
        public void SpawnAt(Transform targetTransform)
        {
            if (!model.activeSelf) model.SetActive(true);
            
            var targetTransformPosition = targetTransform.position;
            transform.position = new Vector3(targetTransformPosition.x, 0,  targetTransformPosition.z);
            
            onPointingArrowSpawn?.Invoke();
        }
    }
}