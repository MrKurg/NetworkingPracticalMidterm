using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class OverlayUpdate : MonoBehaviour
{
    private Text myText;
    private static float position;

    public static void UpdatePos(float pos)
    {
        position = pos;
    }

    // Start is called before the first frame update
    void Start()
    {
        myText = GetComponent<Text>();

    }

    // Update is called once per frame
    void Update()
    {
        myText.text = "Position X: " + position;

    }
}
