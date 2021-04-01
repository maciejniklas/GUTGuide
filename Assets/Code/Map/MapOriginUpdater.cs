using System.Collections.Generic;
using Google.Maps;
using UnityEngine;
using UnityEngine.Events;

namespace Code.Map
{
    /// <summary>
    /// Updates the <see cref="MapsService"/> floating origin whenever the main <see cref="Camera"/> moves far enough
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class MapOriginUpdater : MonoBehaviour
    {
        [Tooltip("Distance in meters the Camera should move before the world's Floating Origin is reset")]
        [SerializeField] [Min(1)] private float updateRange = 200f;

        [SerializeField] private bool dynamicRecenter = false;
        
        /// <summary>
        /// Called whenever this script updates the maps' origin point. Uses recenter offset.
        /// </summary>
        public UnityEvent<Vector3> onMapOriginUpdate;

        /// <summary>
        /// Reference to the <see cref="MapsService"/> of the app
        /// </summary>
        private MapsService _mapsService;
        /// <summary>
        /// All <see cref="GameObject"/>s to be moved when the world's Floating Origin is moved
        /// </summary>
        private List<GameObject> _objectsToRecenter;
        /// <summary>
        /// The last set floating origin
        /// </summary>
        private Vector3 _floatingOrigin;

        /// <summary>
        /// Camera position only at the XZ plane
        /// </summary>
        private Vector3 CameraPositionOnGroundPlane
        {
            get
            {
                var position = transform.position;
                position.y = 0f;
                
                return position;
            }
        }

        private void Awake()
        {
            if (!tag.Equals("MainCamera"))
            {
                Destroy(this);
            }

            _mapsService = FindObjectOfType<MapsService>();
            _floatingOrigin = CameraPositionOnGroundPlane;
            _objectsToRecenter = new List<GameObject> { gameObject };
        }

        private void FixedUpdate()
        {
            if (!dynamicRecenter) return;
            
            // Collect necessary data
            var currentOriginPoint = CameraPositionOnGroundPlane;
            var distance = Vector3.Distance(_floatingOrigin, currentOriginPoint);

            // Check if the origin point update is required
            if (updateRange > distance) return;

            Recenter(currentOriginPoint);
        }

        public void Recenter(Vector3 newCenter)
        {
            // Update map origin and get the offset due to the previous one
            var originOffset = _mapsService.MoveFloatingOrigin(newCenter, _objectsToRecenter);

            // Notify listeners
            onMapOriginUpdate.Invoke(originOffset);
            // Remember the new origin point
            _floatingOrigin = newCenter;
        }

        /// <summary>
        /// Add the object to the list of moved ones whenever the world's Floating Origin  is recentered
        /// </summary>
        /// <param name="objectToRecenter">Object that will be recentered</param>
        public void AddGameObjectToRecenter(GameObject objectToRecenter)
        {
            if (!_objectsToRecenter.Contains(objectToRecenter))
            {
                _objectsToRecenter.Add(objectToRecenter);
            }
        }

        /// <summary>
        /// Remove the object from the list of moved ones whenever the world's Floating Origin  is recentered
        /// </summary>
        /// <param name="objectToRecenter">Object to remove from recenter process</param>
        public void StopRecenterizingGameObject(GameObject objectToRecenter)
        {
            if (_objectsToRecenter.Contains(objectToRecenter))
            {
                _objectsToRecenter.Remove(objectToRecenter);
            }
        }
    }
}