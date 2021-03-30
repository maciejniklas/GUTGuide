using System;
using UnityEngine;

namespace GUTGuide.UI.Components
{
    public class CompassNeedle : MonoBehaviour
    {
        private Transform _mainCameraTransform;

        private void Awake()
        {
            if (!(Camera.main is null)) _mainCameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            transform.localRotation = Quaternion.Euler(Vector3.forward * _mainCameraTransform.rotation.eulerAngles.y);
        }
    }
}