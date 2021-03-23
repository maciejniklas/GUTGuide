using System;
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

        private Animator _animator;
        
        private static readonly int HideAnimationTrigger = Animator.StringToHash("Hide");
        private static readonly int ShowAnimationTrigger = Animator.StringToHash("Show");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Hide()
        {
            _animator.SetTrigger(HideAnimationTrigger);
        }

        public void Show()
        {
            _animator.SetTrigger(ShowAnimationTrigger);
        }
    }
}