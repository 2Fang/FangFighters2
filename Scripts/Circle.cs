using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    // Start is called before the first frame update

    SpriteRenderer circle;

    void Start()
    {
        circle = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void changeColour(Color colour)
    {
        circle.color = colour;
    }

}
