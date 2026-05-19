using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace MTG_Emulator.Backend.DB.Models
{
    public class TurnHighlighBar : MonoBehaviour
    {
        [SerializeField] private TMP_Text mainPhaseText;
        [SerializeField] private TMP_Text combatPhaseText;
        [SerializeField] private TMP_Text secondMainPhaseText;
        
        [SerializeField] private Image mainPhaseBackground;
        [SerializeField] private Image combatBackground;
        [SerializeField] private Image secondMainPhaseBackground;
        
        private float active = 1f;
        private float inActive = 0.35f;
        
        
        private int currentPhase = 0;
        
        
        private void SetVisual(Image background, TMP_Text text, bool isActive)
        {
            float alpha = isActive ? active : inActive;

            Color backgroundColor = background.color;
            backgroundColor.a = alpha;
            background.color = backgroundColor;

            Color textColor = text.color;
            textColor.a = alpha;
            text.color = textColor;
        }
    
        private void UpdatePhase()
        {
            SetVisual(mainPhaseBackground, mainPhaseText, currentPhase == 0);
            SetVisual(combatBackground, combatPhaseText, currentPhase == 1);
            SetVisual(secondMainPhaseBackground, secondMainPhaseText, currentPhase == 2);
        }

        public void NextPhaseOnClick()
        {
            currentPhase++;

            if (currentPhase > 2)
            {
                currentPhase = 0;
            }

            UpdatePhase();
        }
        
        public void Start()
        {
            UpdatePhase();
        }
    }
    

}  