using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    // Start is called before the first frame update
    public Color myColor = new Color(0.0f, 0.392f, 0.0f);

    void Start()
    {
        Renderer rebnerer = GetComponent<Renderer>();
        rebnerer.material.color = myColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
