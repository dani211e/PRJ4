using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite backImg;

    private Sprite frontUp;
    private bool faceDown = true;

    void Awake()
    {
        if(image == null) image = GetComponent<Image>();
    }

    public void Init(Sprite front, bool startFaceDown)
    {
        frontUp = front;
        faceDown = startFaceDown;
        Refresh();
    }

    public void Refresh()
    {
        image.sprite = faceDown ? backImg : frontUp;
    }
}
