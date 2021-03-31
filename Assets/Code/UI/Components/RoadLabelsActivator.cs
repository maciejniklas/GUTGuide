using System;
using Code.Map;
using UnityEngine;
using UnityEngine.UI;

namespace GUTGuide.UI.Components
{
    [RequireComponent(typeof(Button))]
    public class RoadLabelsActivator : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonPressed);
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonPressed);
        }

        private void OnButtonPressed()
        {
            var colorComponent = (((int) iconImage.color.b) + 1) % 2;
            
            iconImage.color = new Color(colorComponent, colorComponent, colorComponent, 1);
            
            RoadLabeller.Instance.gameObject.SetActive(colorComponent != 0);
        }
    }
}