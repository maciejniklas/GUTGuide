using GUTGuide.DataStructures;
using GUTGuide.UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace GUTGuide.UI.Windows
{
    /// <summary>
    /// Controller of the search GUT buildings list window
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class SearchBuildingPanel : MonoBehaviour
    {
        [Tooltip("Input field of searching building name or shortcut ")]
        [SerializeField] private InputField searchBuildingInput;
        [Tooltip("The content transform of the GUT buildings cards")]
        [SerializeField] private Transform searchBuildingPanelContentTransform;
        [Tooltip("The prefab of the GUT building list card")]
        [SerializeField] private GameObject gutBuildingCardPrefab;

        /// <summary>
        /// Reference to the <see cref="Animator"/> of this object
        /// </summary>
        private Animator _animator;
        
        /// <summary>
        /// Hash code of the hide animation
        /// </summary>
        private static readonly int HideAnimationTrigger = Animator.StringToHash("Hide");
        /// <summary> 
        /// Hash code of the display animation
        /// </summary>
        private static readonly int ShowAnimationTrigger = Animator.StringToHash("Show");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            // Fill panel with the GUT buildings data in the form of info cards
            foreach (var gutBuildingData in Resources.LoadAll<GutBuildingData>("GUTBuildingsData"))
            {
                var gutBuildingCardObject = Instantiate(gutBuildingCardPrefab, searchBuildingPanelContentTransform);
                var gutBuildingCard = gutBuildingCardObject.GetComponent<GutBuildingCard>();

                gutBuildingCard.Initialize(gutBuildingData);
            }
        }

        /// <summary>
        /// Hide panel from the app viewport
        /// </summary>
        public void Hide()
        {
            _animator.SetTrigger(HideAnimationTrigger);
        }

        /// <summary>
        /// Display panel to the app viewport
        /// </summary>
        public void Show()
        {
            _animator.SetTrigger(ShowAnimationTrigger);
        }
    }
}