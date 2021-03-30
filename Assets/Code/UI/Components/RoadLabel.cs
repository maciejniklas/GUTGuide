using System.Collections;
using System.Collections.Generic;
using Google.Maps.Feature.Shape;
using UnityEngine;
using UnityEngine.UI;

namespace GUTGuide.UI.Components
{
    /// <summary>
    /// Controller of in-scene showing road label
    /// </summary>
    public class RoadLabel : MonoBehaviour
    {
        [Tooltip("Text element to show this Label's text in")]
        [SerializeField] private Text label;

        /// <summary>
        /// The midpoints of all individual lines making up this road
        /// </summary>
        private readonly List<Vector3> _lineMidPoints = new List<Vector3>(); 

        /// <summary>
        /// Is this label currently tracking main camera? i.e. is this label continually making sure it is aligned
        /// to main camera?
        /// </summary>
        private bool _isTrackingCamera;
        /// <summary>
        /// Reference to the transform of main camera
        /// </summary>
        private Transform _mainCameraTransform;

        private void Awake()
        {
            if (!(Camera.main is null)) _mainCameraTransform = Camera.main.transform;
        }

        /// <summary>
        /// Add a new chunk to this road, repositioning label to new, collective center
        /// </summary>
        /// <param name="roadLinePosition">New piece of this road</param>
        /// <param name="roadLine">Line defining this new chunk's shape</param>
        public void SetLine(Vector3 roadLinePosition, Line roadLine)
        {
            // Store the midpoints of the individual straight lines making up this new chunk of the road
            for (var index = 0; index < roadLine.Vertices.Length - 1; index++)
            {
                // Line vertices need to be converted from 2D coordinates (x and y) to 3D coordinates
                var startPoint = new Vector3(roadLine.Vertices[index].x, 0, roadLine.Vertices[index].y);
                var endPoint = new Vector3(roadLine.Vertices[index + 1].x, 0, roadLine.Vertices[index + 1].y);

                // Convert from local space to world space by adding the world space position of the GameObject
                // containing these lines
                startPoint += roadLinePosition;
                endPoint += roadLinePosition;

                // Store the midpoint of this start and end as the midpoint of this line
                var midPoint = (startPoint + endPoint) / 2f;
                _lineMidPoints.Add(midPoint);
            }

            // Calculate collective center of all road lines
            var centerPoint = Vector3.zero;
            _lineMidPoints.ForEach(midPoint => centerPoint += midPoint);
            centerPoint /= (float) _lineMidPoints.Count;

            // Determine which line is closest to the collective center. This is so we can place the label over this
            // center-most line (rather than at the exact collective center, which may not be over any individual
            // part of this road)
            var closestMidPointIndex = 0;
            var closestDistance = float.MaxValue;

            for (var index = 0; index < _lineMidPoints.Count; index++)
            {
                var currentDistance = Vector3.Distance(centerPoint, _lineMidPoints[index]);

                if (!(currentDistance < closestDistance)) continue;
                
                closestMidPointIndex = index;
                closestDistance = currentDistance;
            }

            // Set the position of the road label
            var roadLabelTransform = transform;
            var closestMidPointPosition = _lineMidPoints[closestMidPointIndex];
            roadLabelTransform.position = new Vector3(closestMidPointPosition.x, roadLabelTransform.position.y,
                closestMidPointPosition.z);
        }

        /// <summary>
        /// Set the specific text to display on this label
        /// </summary>
        /// <param name="text">Text to show on this label</param>
        public void SetText(string text)
        {
            // Name this GameObject based on given text (to make debugging easier) and display the text
            gameObject.name = $"Label: {text}";
            label.text = text;

            // Start this Label tracking the Camera (unless already doing so)
            if (!_isTrackingCamera) StartCoroutine(TrackCamera());
        }

        /// <summary>
        /// Turn to face the main camera every frame
        /// </summary>
        /// <returns></returns>
        private IEnumerator TrackCamera()
        {
            // Flag that this coroutine has started (so it will not be redundantly restarted later)
            _isTrackingCamera = true;

            // Start facing Camera
            while (true)
            {
                // Get the current rotation of the Camera and apply it to the label
                transform.rotation = _mainCameraTransform.rotation;
                // Wait for next frame
                yield return null;
            }
        }
    }
}