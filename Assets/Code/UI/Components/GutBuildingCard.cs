using GUTGuide.DataStructures;
using UnityEngine;
using UnityEngine.UI;

namespace GUTGuide.UI.Components
{
    /// <summary>
    /// Controller of the UI representation of the <see cref="GutBuildingData"/>
    /// </summary>
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

        /// <summary>
        /// Identifier of the building on the map
        /// </summary>
        private string _id;

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
    }
}