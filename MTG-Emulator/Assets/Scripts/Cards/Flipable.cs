using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MTG_Emulator.Cards
{
    public class Flipable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Sprite mtgBackImg;

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
            if (!mouseOver)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                Flip(false);
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                Flip(true);
            }
        }

        private void Flip(bool useMtgBack)
        {
            if (card == null || card.cardData == null)
            {
                Debug.Log("Cant flip the card");
                return;
            }

            if (string.IsNullOrEmpty(frontImgUri))
            {
                frontImgUri = card.cardData.ImageUri;

                if (card.cardData.AltFace != null)
                {
                    altImgUri = card.cardData.AltFace.ImageUri;
                }
            }

            showingBack = !showingBack;

            if (useMtgBack)
            {
                if (mtgBackImg == null)
                {
                    Debug.Log("cant flip to mtg back image");
                    return;
                }

                if (showingBack)
                {
                    image.sprite = mtgBackImg;
                }
                else
                {
                    card.cardData.ImageUri = frontImgUri;
                    StartCoroutine(APIManager.Instance.LoadImage(frontImgUri, image));
                }

                return;
            }

            if (string.IsNullOrEmpty(altImgUri))
            {
                Debug.Log("Cant flip since the card has no alt face");
                return;
            }

            string targetUri = showingBack ? altImgUri : frontImgUri;
            card.cardData.ImageUri = targetUri;

            StartCoroutine(APIManager.Instance.LoadImage(targetUri, image));
        }
    }
}