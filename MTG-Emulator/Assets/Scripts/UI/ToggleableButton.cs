using System;
using UnityEngine;
using UnityEngine.UI;

namespace MTG_Emulator.UI
{
    public class ToggleableButton : MonoBehaviour
    {
        [Header("Streamer Mode")]
        [SerializeField]
        private Button toggleCodeButton;

        
        private Image toggleCodeImage;

        [SerializeField]
        private Sprite onSprite; 

        [SerializeField]
        private Sprite offSprite;
        
        public bool State { get; private set; } = true;
        public Button Button { get => toggleCodeButton; }

        private void Start()
        {
            toggleCodeButton.onClick.AddListener(OnClickToggleCode);
            toggleCodeImage = toggleCodeButton.GetComponent<Image>();

        }

        private void OnDestroy()
        {
            toggleCodeButton.onClick.RemoveListener(OnClickToggleCode);
        }

        private void OnClickToggleCode() 
        {
            State = !State;
            toggleCodeImage.sprite = State ? onSprite : offSprite;
        }
    }
}