using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MTG_Emulator.Cards
{
    public class Flipable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Card card;
        private Image image;
        private string frontImgUri;
        private string altImgUri;
        private bool showingBack;
        private bool mouseOver;

        private void Awake()
        {
            card = GetComponent<Card>();
            image = GetComponentInChildren<Image>();
        }

        public void OnPointerEnter(PointerEventData eventData) => mouseOver = true;
        public void OnPointerExit(PointerEventData eventData) => mouseOver = false;

        private void Update()
        {
            if (mouseOver && Input.GetKeyDown(KeyCode.F))
                Flip();
        }

        private void Flip()
        {
            if (card == null || card.cardData == null)
            {
                Debug.Log("Cant flip the card ");
                return;
            }

            if (card.cardData.AltFace == null || string.IsNullOrEmpty(card.cardData.AltFace.ImageUri))
            {
                Debug.Log("Cant flip since the card has no alt face");
                return;
            }

            if (string.IsNullOrEmpty(frontImgUri))
            {
                frontImgUri = card.cardData.ImageUri;
                altImgUri = card.cardData.AltFace.ImageUri;
            }

            showingBack = !showingBack;
            string targetUri = showingBack ? altImgUri : frontImgUri;
            card.cardData.ImageUri = targetUri;

            StartCoroutine(APIManager.Instance.LoadImage(card.cardData.ImageUri, image));
        }
    }
}