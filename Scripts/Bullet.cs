using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    int damage;
    int attackType;
    float speed;
    [SerializeField] float offset;
    float range;
    Vector2 startPoint;
    Vector2 endPoint;
    float time;
    
    PolygonCollider2D polyCollider;
    Rigidbody2D bullet;



    public void SetProjectile(int _range, int _speed, int _damage, int _attackType)
    {
        range = _range;
        speed = _speed;
        damage = _damage;
        attackType = _attackType;
    }

    // Start is called before the first frame update
    void Start()
    {
        polyCollider = GetComponent<PolygonCollider2D>();
        bullet = GetComponent<Rigidbody2D>();
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {/*
        if (bullet.position == endPoint)
        {
            EndProjectile();
        }
        */
        if (time > 0)
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                time = 0;
                EndProjectile();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == gameObject.tag) return;
        if (collision.tag == "Fence") return;
        try
        {
            collision.GetComponent<Character>().GetHit(damage);
        }
        catch { };
        polyCollider.enabled = false;
        EndProjectile();
    }

    public void Shoot(Vector2 position, Vector2 direction)
    {
        gameObject.SetActive(true);
        polyCollider.enabled = true;
        direction = direction.normalized;
        bullet.position = position + direction * offset;
        //startPoint = position;
        bullet.rotation = Mathf.Rad2Deg * Mathf.Atan(direction.y / direction.x);
        if (direction.x < 0)
        {
            bullet.rotation += 180;
        }
        bullet.velocity = direction * speed;
        //endPoint = startPoint + direction * range;
        time = range / speed;

    }

    void EndProjectile()
    {
        bullet.position = new Vector2(-12f, 0f);
        Deactivate();
    }
    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
