using System;
using System.Collections;
using Cinemachine;
using GUTGuide.UI.Windows;
using UnityEngine;

namespace GUTGuide.Components
{
    /// <summary>
    /// Controls movement of the camera
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Parameters")]
        
        [Tooltip("Time after which the touches counter will reset")]
        [SerializeField] [Min(0.01f)] private float timeToResetTouches = 0.25f;
        [Tooltip("Swipe sensitivity along the X-axis")]
        [SerializeField] [Min(0.01f)] private float horizontalSensitivity = 1.5f;
        [Tooltip("Swipe sensitivity along the Y-axis")]
        [SerializeField] [Min(0.01f)] private float verticalSensitivity = 0.25f;
        
        [Header("References")]
        
        [Tooltip("Reference to the controlled virtual camera")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [Tooltip("Reference to the component where camera transition path is defined")]
        [SerializeField] private CinemachineSmoothPath path;
        [Tooltip("Reference to the transform on which Y-axis rotation will be done")]
        [SerializeField] private Transform targetForHorizontalRotation;

        /// <summary>
        /// Counter of the touches done in the given interval
        /// </summary>
        private int _touchCounter;
        /// <summary>
        /// Reference to the coroutine that reset touches counter
        /// </summary>
        private Coroutine _resetTouchesCoroutine;
        /// <summary>
        /// Flag indicating whether the coroutine that reset touches is running or not
        /// </summary>
        private bool _isResetTouchesCoroutineRunning;
        /// <summary>
        /// Flag indicating whether the double-tap was performed or not
        /// </summary>
        private bool _isSecondTouch;
        /// <summary>
        /// Amount of waypoints on the camera transition path
        /// </summary>
        private int _pathPointsAmount;
        /// <summary>
        /// Reference to the <see cref="CinemachineTrackedDolly"/> component that is responsible for the control of
        /// camera position on its transition path
        /// </summary>
        private CinemachineTrackedDolly _trackedDolly;
        /// <summary>
        /// Reference to the <see cref="SearchBuildingPanel"/> to be able to add corresponding events for its opening
        /// and close
        /// </summary>
        private SearchBuildingPanel _searchBuildingPanel;
        /// <summary>
        /// Flag indicating whether the camera controller should be locked or not
        /// </summary>
        private bool _isLocked;

        private void Awake()
        {
            _pathPointsAmount = path.m_Waypoints.Length;
            _trackedDolly = virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
            _searchBuildingPanel = FindObjectOfType<SearchBuildingPanel>();
        }

        private void Update()
        {
            // If the controller is locked, do nothing
            if (_isLocked) return;
            
            // Get delta of the position along the X-axis for target rotation around the Y-axis
            var yRotation = -Input.GetAxisRaw("Mouse X") * horizontalSensitivity;

            // If the mouse is not pressed or touch is not present, do nothing
            if (Input.GetMouseButton(0) && (yRotation > float.Epsilon || yRotation < -float.Epsilon))
            {
                // On Android devices, the touch phase should be checked first, then just rotate target transform
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (Input.touches[0].phase == TouchPhase.Moved)
                        targetForHorizontalRotation.Rotate(Vector3.up, yRotation);
                }
                else
                {
                    targetForHorizontalRotation.Rotate(Vector3.up, yRotation);
                }
            }
            
            // If the second touch was detected, enable control of the camera on its transition path
            if (_isSecondTouch)
            {
                var pathPosition = _trackedDolly.m_PathPosition - Input.GetAxis("Mouse Y") * verticalSensitivity;
                pathPosition = Mathf.Clamp(pathPosition, 0, _pathPointsAmount);
                _trackedDolly.m_PathPosition = pathPosition;
            }

            // If the second touch is released, unset the flag
            if (_isSecondTouch && Input.GetMouseButtonUp(0)) _isSecondTouch = false;

            // If this is not the frame when touch begin, do nothing
            if (!Input.GetMouseButtonDown(0)) return;

            // Increment the touch counter
            _touchCounter++;

            // Turn on the coroutine that reset the touch counter after the selected time
            if (_isResetTouchesCoroutineRunning)
            {
                StopCoroutine(_resetTouchesCoroutine);
            }
            _resetTouchesCoroutine = StartCoroutine(ResetTouches());

            // If the second touch was detected, set its flag
            if (_touchCounter == 2)
            {
                _isSecondTouch = true;
            }
        }

        private void OnDisable()
        {
            if (_searchBuildingPanel is null) return;
            
            _searchBuildingPanel.onOpen.RemoveListener(Lock);
            _searchBuildingPanel.onClose.RemoveListener(Unlock);
        }

        private void OnEnable()
        {
            if (_searchBuildingPanel is null) return;
            
            _searchBuildingPanel.onOpen.AddListener(Lock);
            _searchBuildingPanel.onClose.AddListener(Unlock);
        }

        /// <summary>
        /// Lock the capability to move the camera
        /// </summary>
        private void Lock()
        {
            _isLocked = true;
        }

        /// <summary>
        /// Coroutine that reset the touches counter after selected time
        /// </summary>
        private IEnumerator ResetTouches()
        {
            _isResetTouchesCoroutineRunning = true;
            
            yield return new WaitForSeconds(timeToResetTouches);
            _touchCounter = 0;

            _isResetTouchesCoroutineRunning = false;
        }

        // Unlock the capability to move the camera
        private void Unlock()
        {
            _isLocked = false;
        }
    }
}