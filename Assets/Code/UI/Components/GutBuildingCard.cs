using GUTGuide.DataStructures;
using GUTGuide.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GUTGuide.UI.Components
{
    /// <summary>
    /// Controller of the UI representation of the <see cref="GutBuildingData"/>
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class GutBuildingCard : MonoBehaviour
    {
        [Tooltip("Reference to the text component of the number label")]
        [SerializeField] private Text numberLabel;
        [Tooltip("Reference to the text component of the full name label")]
        [SerializeField] private Text fullNameLabel;
        [Tooltip("Reference to the text component of the shortcut label")]
        [SerializeField] private Text shortcutLabel;
        [Tooltip("Reference to the text component of the address label")]
        [SerializeField] private Text addressLabel;
        [Tooltip("The toggle that indicates that the building represented by this card is currently the pointed one")]
        [SerializeField] private Toggle currentlyPointingAtToggle;

        /// <summary>
        /// The <see cref="UnityEvent"/> called when the pointing arrow spawns
        /// </summary>
        public UnityEvent onStartPointingToBuilding;

        /// <summary>
        /// Identifier of the building on the map
        /// </summary>
        private string _id;
        /// <summary>
        /// Reference to the <see cref="Button"/> component of the card
        /// </summary>
        private Button _button;

        /// <summary>
        /// Static reference to the card of the currently pointing building
        /// </summary>
        private static GutBuildingCard _currentlyPointingAtCard;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonPressedCallback);
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonPressedCallback);
        }

        /// <summary>
        /// Check if <see cref="GutBuildingCard"/> contains given text in the name, shortcut or address
        /// </summary>
        /// <param name="text">Searched word</param>
        /// <returns>True if the word is found, False if not</returns>
        public bool Contains(string text)
        {
            return fullNameLabel.text.ToLower().Contains(text.ToLower()) ||
                   shortcutLabel.text.ToLower().Contains(text.ToLower()) ||
                   addressLabel.text.ToLower().Contains(text.ToLower());
        }

        /// <summary>
        /// Initially fill the card with the given data
        /// </summary>
        /// <param name="gutBuildingData">Dataset of the GUT building</param>
        public void Initialize(GutBuildingData gutBuildingData)
        {
            _id = gutBuildingData.id;
            numberLabel.text = gutBuildingData.number.ToString("000");
            fullNameLabel.text = gutBuildingData.fullName;
            shortcutLabel.text = gutBuildingData.shortcut;
            addressLabel.text = gutBuildingData.address;
        }

        /// <summary>
        /// Because the card is moved to the top of the scroll list when its building starts being pointed after its
        /// deselect it has to move back to its original position
        /// </summary>
        private void MoveToChronologicalPosition()
        {
            var currentlyPointingAtCardIdNumber = int.Parse(_currentlyPointingAtCard.numberLabel.text);
            
            for (var index = 0; index < transform.parent.childCount; index++)
            {
                var idNumber = int.Parse(transform.parent.GetChild(index).GetComponent<GutBuildingCard>().numberLabel
                    .text);

                if (idNumber <= currentlyPointingAtCardIdNumber) continue;
                
                _currentlyPointingAtCard.transform.SetSiblingIndex(index - 1);
                return;
            }
            
            _currentlyPointingAtCard.transform.SetSiblingIndex(transform.parent.childCount - 1);
        }

        /// <summary>
        /// Mark this card as the pointed by arrow
        /// </summary>
        /// <returns>True if it is allowed to spawn the arrow because this card was not the one that is currently
        /// pointed. False if this is the currently pointed card and it could only be deselected and the arrow
        /// hidden.</returns>
        private bool MarkAsCurrentlyPointingTo()
        {
            if (_currentlyPointingAtCard != null)
            {
                MoveToChronologicalPosition();
                _currentlyPointingAtCard.currentlyPointingAtToggle.isOn = false;

                if (_currentlyPointingAtCard == this)
                {
                    _currentlyPointingAtCard = null;
                    return false;
                }
                
                _currentlyPointingAtCard = null;
            }

            currentlyPointingAtToggle.isOn = true;
            _currentlyPointingAtCard = this;
            transform.SetSiblingIndex(0);

            return true;
        }

        private void OnButtonPressedCallback()
        {
            var buildingTransform = GutBuildingsParent.Instance.GetBuildingTransform(_id);

            if (buildingTransform == null)
            {
                ErrorHandler.CustomError("Building out of range");
                return;
            }

            if (MarkAsCurrentlyPointingTo())
            {
                PointingArrow.Instance.SpawnAt(buildingTransform);
                onStartPointingToBuilding?.Invoke();
            }
            else
            {
                PointingArrow.Instance.Hide();
            }
        }
    }
}