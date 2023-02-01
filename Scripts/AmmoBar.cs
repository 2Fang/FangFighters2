using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBar : MonoBehaviour
{

    Rigidbody2D _rigidBody;
    SpriteRenderer sprite;
    Character player;
    float length;
    Vector2 offset = new Vector2(-0.25f, 0.375f);

    // Start is called before the first frame update
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        player = GetComponentInParent<Character>();
        length = sprite.transform.localScale.x;

        sprite.transform.localScale = new Vector3(length, 0.2f, 1);
    }

    // Update is called once per frame
    void Update()
    {
        _rigidBody.position = player.getPos() + offset;

    }

    public void reSize(float ratio)
    {
        sprite.transform.localScale = new Vector3(length * ratio, 0.2f, 1);
    }
}
