using System;
using Google.Maps.Event;
using GUTGuide.Patterns;
using UnityEngine;
using UnityEngine.UI;

namespace GUTGuide.UI.Components
{
    /// <summary>
    /// Error handler with the possibility to expose it's in the notification bar
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class ErrorHandler : PersistentSingleton<ErrorHandler>
    {
        [Tooltip("Reference to text message object of the popup message window")]
        [SerializeField] private Text errorText;
        
        /// <summary>
        /// Hash code of hide animation trigger
        /// </summary>
        private static readonly int HideAnimationTrigger = Animator.StringToHash("Hide");
        /// <summary>
        /// Hash code of display animation trigger
        /// </summary>
        private static readonly int ShowAnimationTrigger = Animator.StringToHash("Show");

        /// <summary>
        /// Animator component attached to this game object
        /// </summary>
        private Animator _animator;
        /// <summary>
        /// Flag used to mark if error bar is currently visible at screen
        /// </summary>
        private bool _isVisible;

        protected override void Awake()
        {
            base.Awake();

            _animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Handle map loading error
        /// </summary>
        /// <param name="errorArgs"><see cref="MapLoadErrorArgs"/> that help identify the issue</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when error type (<see cref="ErrorArgs.DetailedErrorEnum"/>)
        /// is not recognized</exception>
        public static void MapLoadingError(MapLoadErrorArgs errorArgs)
        {
            switch (errorArgs.DetailedErrorCode)
            {
                case ErrorArgs.DetailedErrorEnum.None:
                    Instance.Hide();
                    break;
                
                case ErrorArgs.DetailedErrorEnum.UnsupportedClientVersion:
                    Instance.ExposeMessage($"The {Application.version} version of the GUT Guide is unsupported");
                    break;
                
                case ErrorArgs.DetailedErrorEnum.ClientError:
                    Instance.ExposeMessage("The request to the API succeeded, but an error occurred on the client");
                    break;
                
                case ErrorArgs.DetailedErrorEnum.NetworkError:
                    Instance.ExposeMessage(Application.internetReachability == NetworkReachability.NotReachable
                        ? "GUT Guide must have internet access in order to run"
                        : $"GUT Guide was not able to get an HTTP response after {errorArgs.Attempts} attempts");
                    break;
                
                case ErrorArgs.DetailedErrorEnum.InvalidRequest:
                    Instance.ExposeMessage("The request sent from the SDK to the API was invalid");
                    break;
                
                case ErrorArgs.DetailedErrorEnum.PermissionDenied:
                    Instance.ExposeMessage("The API key does not have permission to make requests");
                    break;
                
                case ErrorArgs.DetailedErrorEnum.NotFound:
                    Instance.ExposeMessage("Nothing exists at the URL that you used");
                    break;
                
                case ErrorArgs.DetailedErrorEnum.OutOfQuota:
                    Instance.ExposeMessage("You exceeded the quota for the API");
                    break;
                
                case ErrorArgs.DetailedErrorEnum.ServerError:
                    Instance.ExposeMessage("An error occurred at the API server");
                    break;
                
                case ErrorArgs.DetailedErrorEnum.Unknown:
                    Instance.ExposeMessage("Unknown error occurred. Check app log for details.");
                    Debug.Log(errorArgs.Message);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(errorArgs.DetailedErrorCode),
                        errorArgs.DetailedErrorCode, "Unrecognized error type!");
            }
        }

        /// <summary>
        /// Expose message in the error notification bar
        /// </summary>
        /// <param name="message">Message to expose</param>
        private void ExposeMessage(string message)
        {
            errorText.text = message;
            Show();
        }

        /// <summary>
        /// Hide notification content
        /// </summary>
        private void Hide()
        {
            if (!_isVisible) return;
            
            _animator.SetTrigger(HideAnimationTrigger);
            _isVisible = false;
        }

        /// <summary>
        /// Show notification content
        /// </summary>
        private void Show()
        {
            if (_isVisible) return;
            
            _isVisible = true;
            _animator.SetTrigger(ShowAnimationTrigger);
        }
    }
}