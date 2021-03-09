using UnityEngine;

namespace Code.Styling
{
    /// <summary>
    /// Component that squash building with respect to the distance from the target <see cref="Transform"/>
    /// </summary>
    public class Squasher : MonoBehaviour
    {
        /// <summary>
        /// The target from which the distance is computed
        /// </summary>
        private Transform _target;
        /// <summary>
        /// Distance from the target where buildings start squashing
        /// </summary>
        private float _farDistance;
        /// <summary>
        /// Distance from the target where buildings stop squashing
        /// </summary>
        private float _nearDistance;
        /// <summary>
        /// Maximal squash scale of the building
        /// </summary>
        private float _maxSquashScale;

        private void Update()
        {
            var distance = Vector3.Distance(_target.position, transform.position);

            if (!(distance >= _nearDistance) || !(distance <= _farDistance)) return;
            
            var normalizedDistance = (distance - _nearDistance) / (_farDistance - _nearDistance);
            var clampedNormalizedDistance = Mathf.Clamp01(normalizedDistance);
            var scale = Mathf.Lerp(_maxSquashScale, 1, clampedNormalizedDistance);

            transform.localScale = new Vector3(1, scale, 1);
        }

        /// <summary>
        /// Initialize squasher settings
        /// </summary>
        /// <param name="target">The target from which the distance is computed</param>
        /// <param name="nearDistance">Distance from the target where buildings start squashing</param>
        /// <param name="farDistance">Distance from the target where buildings stop squashing</param>
        /// <param name="maxSquashScale">Maximal squash scale of the building</param>
        public void Initialize(Transform target, float nearDistance, float farDistance, float maxSquashScale)
        {
            _target = target;
            _farDistance = farDistance;
            _nearDistance = nearDistance;
            _maxSquashScale = maxSquashScale;
        }
    }
}