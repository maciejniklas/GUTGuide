using System.Collections;
using Google.Maps;
using Google.Maps.Coord;
using GUTGuide.DataStructures;
using GUTGuide.UI;
using GUTGuide.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace GUTGuide.Core
{
    /// <summary>
    /// Responsible for map objects loading and unloading
    /// </summary>
    [RequireComponent(typeof(MapsService))]
    public class MapLoader : MonoBehaviour
    {
        [Header("General settings")]
        
        [Tooltip("Global coordinates of Gdańsk University of Technology")]
        [SerializeField] private LatLng gutCoordinates = new LatLng(54.3714279,18.6169474);
        [Tooltip("Distance in which map objects will be rendered")]
        [SerializeField] [Min(10)] private float renderDistance = 1000f;
        [Tooltip("Distance after which map objects will be updated")]
        [SerializeField] [Min(10)] private float updateDistance = 100f;
        [Tooltip("Angle after which map objects will be updated")]
        [SerializeField] [Range(1, 90)] private float updateAngle = 10f;
        [Tooltip("Interval in seconds at which unseen geometry is detected and unloaded")]
        [SerializeField] [Min(0)] private float unloadUnseenDelay = 5f;

        [Header("References")]
        
        [SerializeField] private Transform groundPlaneTransform;
        
        /// <summary>
        /// Was the map initialized?
        /// </summary>
        public bool IsInitialized { get; private set; }
        /// <summary>
        /// Is the map currently loading?
        /// </summary>
        public bool IsLoading { get; private set; }

        /// <summary>
        /// Event called just before map starts loading
        /// </summary>
        public UnityEvent onMapLoadingStarted;
        /// <summary>
        /// Event called just before map starts loading
        /// </summary>
        public UnityEvent<Vector3, float> onMapUnloaded;

        /// <summary>
        /// Reference to the map service
        /// </summary>
        private MapsService _mapsService;
        /// <summary>
        /// Reference to the main camera of the game
        /// </summary>
        private Camera _mainCamera;
        /// <summary>
        /// Default map objects styling
        /// </summary>
        private GameObjectOptions _mapDefaultStyle;
        /// <summary>
        /// Has the application quit?
        /// </summary>
        private bool _hasQuit;
        /// <summary>
        /// Used to indicate that we need to load the map using the current viewport
        /// </summary>
        private bool _needLoading;
        /// <summary>
        /// Used to let the unload co-routine that we have loaded additional geometry and that older data could be unloaded
        /// </summary>
        private bool _needUnloading;
        /// <summary>
        /// Position of the camera the last time the map was loaded
        /// </summary>
        private Vector3 _lastCameraPosition;
        /// <summary>
        /// Rotation of the camera the last time the map was loaded
        /// </summary>
        private Quaternion _lastCameraRotation;
        /// <summary>
        /// Handle to coroutine used to remove unneeded areas of the map
        /// </summary>
        private Coroutine _unloadUnseenCoroutine;

        private void Awake()
        {
            _mapsService = GetComponent<MapsService>();
            IsLoading = true;
            _mainCamera = Camera.main;
        }

        private void Start()
        {
            // Define the origin point of the loaded map
            _mapsService.InitFloatingOrigin(gutCoordinates);

            // Obtain default map objects styling
            _mapDefaultStyle = MapStyleProvider.Instance.GetMapStyle(MapStyleData.Type.Default);
            
            // Initialize events listeners
            _mapsService.Events.MapEvents.Loaded.AddListener((_) => IsLoading = false);
            
            // Initialize error handler
            _mapsService.Events.MapEvents.LoadError.AddListener(ErrorHandler.MapLoadingError);

            IsInitialized = true;
            LoadMap();
        }

        private void Update()
        {
            // Prepare necessary variables
            var mainCameraTransform = _mainCamera.transform;
            var mainCameraPosition = mainCameraTransform.position;
            var cameraDisplacementSqr = (mainCameraPosition - _lastCameraPosition).sqrMagnitude;
            var cameraMovedAngle = Quaternion.Angle(mainCameraTransform.rotation, _lastCameraRotation);

            // Check if loading is needed
            if (cameraDisplacementSqr > Mathf.Pow(updateDistance, 2) || cameraMovedAngle > updateAngle)
            {
                _needLoading = true;
            }

            if (!_needLoading) return;
            
            // Update ground plane position
            groundPlaneTransform.position = new Vector3(mainCameraPosition.x,
                groundPlaneTransform.position.y, mainCameraPosition.z);
                
            LoadMap();

            // Update fields connected with map loading
            _lastCameraPosition = mainCameraPosition;
            _lastCameraRotation = mainCameraTransform.rotation;
            _needLoading = false;
            _needUnloading = true;
        }

        private void OnApplicationQuit()
        {
            _hasQuit = true;
        }

        private void OnDestroy()
        {
            onMapLoadingStarted.RemoveAllListeners();
            onMapUnloaded.RemoveAllListeners();
        }

        private void OnDisable()
        {
            // Stop detecting if something is to unload
            if (_unloadUnseenCoroutine == null) return;
            
            StopCoroutine(_unloadUnseenCoroutine);
            _unloadUnseenCoroutine = null;
        }

        private void OnEnable()
        {
            // Start detecting if something is to unload
            _unloadUnseenCoroutine = StartCoroutine(UnloadUnseenCoroutine());
        }

        /// <summary>
        /// Loads objects in the camera viewport
        /// </summary>
        private void LoadMap()
        {
            // If app is closed do nothing
            if (_hasQuit) return;

            // Initial steps
            IsLoading = true;
            onMapLoadingStarted?.Invoke();

            // Load objects in viewport
            _mapsService.MakeMapLoadRegion().AddViewport(_mainCamera, renderDistance).Load(_mapDefaultStyle);
        }

        /// <summary>
        /// Periodically remove unneeded areas of the map
        /// </summary>
        private IEnumerator UnloadUnseenCoroutine()
        {
            while (true)
            {
                if (IsInitialized && _needUnloading)
                {
                    var mainCameraPosition = _mainCamera.transform.position;
                    
                    // Unload map regions that are not in viewport and are outside a radius around the camera
                    _mapsService.MakeMapLoadRegion().AddCircle(mainCameraPosition, renderDistance)
                        .UnloadOutside();
                    onMapUnloaded?.Invoke(mainCameraPosition, renderDistance);

                    _needUnloading = false;
                }

                yield return new WaitForSeconds(unloadUnseenDelay);
            }
        }
    }
}