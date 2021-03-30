using System;
using System.Collections;
using Cinemachine;
using GUTGuide.Utilities;
using UnityEngine;

namespace GUTGuide.Components
{
    /// <summary>
    /// Controller of the camera pointing always to the <see cref="PointingArrow"/> object
    /// </summary>
    public class PointingArrowCameraController : MonoBehaviour
    {
        [Tooltip("Reference to the object holding the arrow virtual camera")]
        [SerializeField] private GameObject arrowVirtualCamera;
        [Tooltip("Time in seconds in which the camera pointing to the arrow is watching it")]
        [SerializeField] [Min(0.1f)] private float lookingAtArrowTime = 1;
        
        /// <summary>
        /// Reference to the <see cref="CinemachineBrain"/> component stored on the main <see cref="Camera"/>
        /// </summary>
        private CinemachineBrain _cinemachineBrain;
        /// <summary>
        /// Time in seconds in which the <see cref="Camera"/> blends position from the user camera
        /// to the <see cref="PointingArrow"/>
        /// </summary>
        private float _blendTime;

        private void Awake()
        {
            if (!(Camera.main is null)) _cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
            _blendTime = _cinemachineBrain.m_DefaultBlend.BlendTime;
        }

        private void Start()
        {
            arrowVirtualCamera.SetActive(false);
        }

        private void OnDisable()
        {
            if (!(PointingArrow.Instance is null))
                PointingArrow.Instance.onPointingArrowSpawn.RemoveListener(OnPointingArrowSpawnCallback);
        }

        private void OnEnable()
        {
            StartCoroutine(SetPointingArrowListeners());
        }

        /// <summary>
        /// Start the <see cref="Coroutine"/> for transition the <see cref="Camera"/> to the position
        /// of the <see cref="PointingArrow"/>
        /// </summary>
        private IEnumerator EnableArrowLookCoroutine()
        {
            arrowVirtualCamera.SetActive(true);

            yield return new WaitForSeconds(_blendTime + lookingAtArrowTime);

            arrowVirtualCamera.SetActive(false);
        }

        private void OnPointingArrowSpawnCallback()
        {
            StartCoroutine(EnableArrowLookCoroutine());
        }

        /// <summary>
        /// Start the coroutine to wait for the <see cref="PointingArrow"/> being active and then initialize its listeners
        /// </summary>
        private IEnumerator SetPointingArrowListeners()
        {
            yield return new WaitUntil(() => PointingArrow.Instance != null);
            
            PointingArrow.Instance.onPointingArrowSpawn.AddListener(OnPointingArrowSpawnCallback);
        }
    }
}