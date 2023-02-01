using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBarBorder : MonoBehaviour
{

    Rigidbody2D _rigidBody;
    Character player;
    Vector2 offset = new Vector2(0f, 0.375f);


    // Start is called before the first frame update
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        player = GetComponentInParent<Character>();
    }

    // Update is called once per frame
    void Update()
    {
        _rigidBody.position = player.getPos() + offset;
    }

}
