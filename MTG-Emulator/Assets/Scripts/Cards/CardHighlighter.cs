using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MTG_Emulator.Cards
{
    public class CardHighlighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Card card;

        private Image image;

        private void Awake()
        {
            image = GameObject.Find("HighlightedImage").GetComponent<Image>();
        }

        private void Start()
        {
            image.enabled = false;
        }

        private void OnValidate()
        {
            Assert.IsNotNull(card);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            image.sprite = card.cardSprite;
            image.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData) => image.enabled = false;
    }
}