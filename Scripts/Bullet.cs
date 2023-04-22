using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    int damage;
    int attackType;
    float speed;
    float offset;
    float range;
    float width;
    bool ghost;
    bool splash;
    bool followThrough;
    Vector2 startPoint;
    Vector2 endPoint;
    float time;
    float attackLinger;
    float lingerTime = 0;
    float peakTime;
    int shooterNum;
    Vector2[] colliderPoints;
    Vector3 scale;
    int z;
    int[] splashed;
    Vector2 meleeTravel;
    [SerializeField] bool shouldBeActive;

    Character shooter;
    
    CapsuleCollider2D capCollider;
    Rigidbody2D bullet;
    SpriteRenderer sprite;
    Sprite projSprite;
    Sprite trigSprite;

    public Sprite[] projectileSprites;
    public Sprite[] triggerSprites;
    int animIndex;

    Animator animation;


    public void SetProjectile(float _range, float _width, float _length, float _speed, int _damage, int _attackType, int _sprite, bool _ghost, bool _splash, float _attackLinger, bool ft, float _offset)
    {
        range = _range;
        width = _width;
        speed = _speed;
        damage = _damage;
        attackType = _attackType;
        animIndex = _sprite;
        projSprite = projectileSprites[_sprite];
        trigSprite = triggerSprites[_sprite];
        capCollider.size = Vector2.one;
        if (_length < _width)
            capCollider.direction = CapsuleDirection2D.Vertical;
        sprite.sprite = projectileSprites[_sprite];
        scale = new Vector3 (_length, _width, 1);
        transform.localScale = scale;
        ghost = _ghost;
        splash = _splash;
        attackLinger = _attackLinger;
        followThrough = ft;
        offset = _offset;
        //bullet.transform.localScale = new Vector3(scale.x * width, scale.y, scale.z);
    }

    // Start is called before the first frame update
    void Awake()
    {
        capCollider = GetComponent<CapsuleCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        bullet = GetComponent<Rigidbody2D>();
        scale = transform.localScale;
        gameObject.SetActive(false);
        shouldBeActive = false;
        animation = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        animation.SetBool("explode", false);
        animation.SetInteger("character", animIndex);
        if (!shouldBeActive)
            EndProjectile();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (time > 0)
        {
            time -= Time.deltaTime;
            if (attackType == -1)
            {
                if (time > peakTime)
                {
                    transform.localScale = Vector3.one * (2 - 1 * (time - peakTime) / (peakTime));
                }
                if (time <= peakTime)
                {
                    transform.localScale = Vector3.one * (2 + 1 * (time - peakTime) / (peakTime));
                }
            }
            if (attackType == 0)
            {
                meleeTravel += bullet.velocity * Time.deltaTime;
                bullet.position = shooter.getPos() + bullet.velocity.normalized * offset + meleeTravel;
            }
        }
        if (time <= 0)
        {
            animation.SetBool("explode", true);
            animation.SetInteger("character", -1);
            time = 0;
            bullet.velocity = Vector2.zero;
            //sprite.sprite = trigSprite;
            transform.localScale = scale;
            if (lingerTime > 0)
            {
                lingerTime -= Time.deltaTime;
            }
            else
                EndProjectile();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == gameObject.tag) return;
        if (collision.transform.position.z != z)
            if (collision.gameObject.layer != 4) return;
        if (collision.tag == "Fence") return;
        if (attackType == -1 && collision.tag == "Wall") return;
        if (attackType == -1 && time > 0) return;
        try
        {
            Character victim = collision.GetComponent<Character>();
            if (splashed[victim.getPlayerNum() - 1] == 0)
            {
                victim.GetHit(damage, shooterNum);
                splashed[victim.getPlayerNum() - 1] = 1;

            }
            if (!splash)
            {
                capCollider.enabled = false;
                if (!followThrough)
                    EndProjectile();
            }
        }
        catch
        {
            if (!ghost)
            {
                capCollider.enabled = false;
                EndProjectile();
            }
        }
    }


    public void Shoot(Vector2 position, Vector2 direction, int player)
    {
        shooterNum = player;
        shouldBeActive = true;
        gameObject.SetActive(true);
        capCollider.enabled = true;
        if (attackType >= 0)
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
        lingerTime = attackLinger;
        peakTime = time / 2;
        //sprite.sprite = projSprite;
        splashed = new int[] { 0, 0, 0, 0 };
        meleeTravel = Vector2.zero;
        animation.SetInteger("character", animIndex);
        animation.SetBool("explode", false);
    }

    public void setShooter(Character _shooter)
    {
        shooter = _shooter;
    }

    public void setZ(int _z)
    {
        z = _z;
    }

    public int getZ()
    {
        return z;
    }

    public float getWidth()
    {
        return width;
    }

    public Vector2 getVel()
    {
        return bullet.velocity;
    }

    public float getDistance()
    {
        return Mathf.Sqrt(bullet.velocity.sqrMagnitude) * time;
    }
    public int getShooter()
    {
        return shooterNum;
    }

    public void EndProjectile()
    {
        bullet.position = new Vector2(-12f, 0f);
        shouldBeActive = false;
        Deactivate();
    }
    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
