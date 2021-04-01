using System;
using UnityEngine;
using UnityEngine.UI;

namespace GUTGuide.UI.Components
{
    public class SwapIconsAtPressed : MonoBehaviour
    {
        [SerializeField] private Sprite initialIcon;
        [SerializeField] private Sprite otherIcon;
        [SerializeField] private Image icon;

        private bool flag;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnPressed);
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnPressed);
        }

        private void OnPressed()
        {
            flag = !flag;

            if (flag)
            {
                icon.sprite = otherIcon;
            }
            else
            {
                icon.sprite = initialIcon;
            }
        }
    }
}