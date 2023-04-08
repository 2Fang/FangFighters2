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
    float width;
    Vector2 startPoint;
    Vector2 endPoint;
    float time;
    int shooter;
    Vector3 scale;
    int z;
    
    PolygonCollider2D polyCollider;
    Rigidbody2D bullet;



    public void SetProjectile(int _range, float _width, int _speed, int _damage, int _attackType)
    {
        range = _range;
        width = _width;
        speed = _speed;
        damage = _damage;
        attackType = _attackType;
        //bullet.transform.localScale = new Vector3(scale.x * width, scale.y, scale.z);
    }

    // Start is called before the first frame update
    void Start()
    {
        polyCollider = GetComponent<PolygonCollider2D>();
        bullet = GetComponent<Rigidbody2D>();
        scale = bullet.transform.localScale;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
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
        if (collision.transform.position.z != z) return;
        if (collision.tag == "Fence") return;
        if (collision.tag == "Wall" && attackType == -1) return;
        try
        {
            collision.GetComponent<Character>().GetHit(damage, shooter);
        }
        catch { };
        polyCollider.enabled = false;
        EndProjectile();
    }

    public void Shoot(Vector2 position, Vector2 direction, int player)
    {
        shooter = player;
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

    public void setZ(int _z)
    {
        z = _z;
    }

    public int getZ()
    {
        return z;
    }

    public Vector2 getVel()
    {
        return bullet.velocity;
    }

    public int getShooter()
    {
        return shooter;
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
