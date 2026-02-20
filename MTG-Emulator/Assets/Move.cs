using UnityEngine;

public class Move : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        var inputVert = Input.GetAxis("Vertical") * 15;
        var inputHori = Input.GetAxis("Horizontal") * 15;
        var newPos = new Vector3(inputHori, inputVert) * Time.deltaTime;
        Debug.Log(newPos);
        transform.position += newPos;
    }
}