using System.Collections.Generic;
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
        [SerializeField] private InputField buildingSearchInput;
        [Tooltip("The content transform of the GUT buildings cards")]
        [SerializeField] private Transform searchBuildingPanelContentTransform;
        [Tooltip("The prefab of the GUT building list card")]
        [SerializeField] private GameObject gutBuildingCardPrefab;

        /// <summary>
        /// Reference to the <see cref="Animator"/> of this object
        /// </summary>
        private Animator _animator;
        /// <summary>
        /// Local storage list of the <see cref="GutBuildingCard"/>
        /// </summary>
        private List<GutBuildingCard> _gutBuildingCards;
        
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
            _gutBuildingCards = new List<GutBuildingCard>();
        }

        private void Start()
        {
            // Fill panel with the GUT buildings data in the form of info cards
            foreach (var gutBuildingData in Resources.LoadAll<GutBuildingData>("GUTBuildingsData"))
            {
                // Get the references
                var gutBuildingCardObject = Instantiate(gutBuildingCardPrefab, searchBuildingPanelContentTransform);
                var gutBuildingCard = gutBuildingCardObject.GetComponent<GutBuildingCard>();

                // Initialize the card
                gutBuildingCard.Initialize(gutBuildingData);
                
                // Add it to local storage
                _gutBuildingCards.Add(gutBuildingCard);
            }
        }

        private void OnDisable()
        {
            buildingSearchInput.onEndEdit.RemoveAllListeners();
        }

        private void OnEnable()
        {
            buildingSearchInput.onEndEdit.AddListener(OnSearchBuildingInputEndEditCallback);
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
            ClearSearchFilter();
            _animator.SetTrigger(ShowAnimationTrigger);
        }

        /// <summary>
        /// Clear buildings search filter
        /// </summary>
        private void ClearSearchFilter()
        {
            buildingSearchInput.SetTextWithoutNotify("");
            OnSearchBuildingInputEndEditCallback("");
        }

        /// <summary>
        /// Callback called when the user stops typing in the building search input field
        /// </summary>
        /// <param name="value">The current value of the building search input field</param>
        private void OnSearchBuildingInputEndEditCallback(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _gutBuildingCards.ForEach(card => card.gameObject.SetActive(true));
            }
            else
            {
                _gutBuildingCards.ForEach(card => card.gameObject.SetActive(card.Contains(value)));
            }
        }
    }
}