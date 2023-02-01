using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    [SerializeField] CharacterSO characterStats;
    [SerializeField] LobbySO lobbySelection;
    [SerializeField] Sprite[] characterSprites;
    [SerializeField] Color[] teamColours;

    Rigidbody2D _rigidBody;
    BoxCollider2D collider;
    SpriteRenderer sprite;
    Circle circle;

    bool bot;
    float invulnerability = 6f;
    int team = 1;
    bool started = false;


    //variable between different characters
    int movementSpeed = 3;
    float maxHealth = 4200;
    int attackRange = 5;
    float attackWidth = 1f;
    int attackSpeed = 6;
    float health;
    float maxAmmo = 3;
    float maxAttackCooldown = 50;
    float reloadSpeed = 0.004f;
    int attackDamage = 2400;

    float ammo;
    float attackCooldown = 0;
    float maxHealCooldown = 250;
    float healCooldown = 0;
    int attackType = 0;

    [SerializeField] int playerNum;

    string LS_h = "LS_h";
    string LS_v = "LS_v";
    string LS_B = "LS_B";
    string RS_h = "RS_h";
    string RS_v = "RS_v";
    string RS_B = "RS_B";
    string LT = "LT";
    string RT = "RT";
    string LB = "LB";
    string RB = "RB";
    string DPAD_h = "DPAD_h";
    string DPAD_v = "DPAD_v";
    string start = "Start";
    string back = "Back";
    string A = "A";
    string B = "B";
    string X = "X";
    string Y = "Y";

    bool LTrig = false;
    bool RTrig = false;

    float desiredRotation = 0f;

    Aim aimbar;
    [SerializeField] GameObject[] projectiles;

    HealthBar healthBar;
    AmmoBar ammoBar;

    bool died = false;
    Vector2[] spawnPoints;

    float invulnerable = 0;
    Color ogColor;

    bool active;


    // Start is called before the first frame update
    void Start()
    {
        string playerNo = playerNum + "";
        LS_h += playerNo;
        LS_v += playerNo;
        LS_B += playerNo;
        RS_h += playerNo;
        RS_v += playerNo;
        RS_B += playerNo;
        LT += playerNo;
        RT += playerNo;
        LB += playerNo;
        RB += playerNo;
        DPAD_h += playerNo;
        DPAD_v += playerNo;
        start += playerNo;
        back += playerNo;
        A += playerNo;
        B += playerNo;
        X += playerNo;
        Y += playerNo;

        health = maxHealth;
        ammo = maxAmmo;

        _rigidBody = GetComponentInParent<Rigidbody2D>();
        collider = GetComponentInParent<BoxCollider2D>();
        sprite = GetComponentInParent<SpriteRenderer>();
        circle = GetComponentInChildren<Circle>();
        ogColor = sprite.color;

        ChangeCharacter();

        foreach (Aim bar in FindObjectsOfType<Aim>())
        {
            if (bar.CheckButton(RS_B))
            {
                aimbar = bar;
            }
        }
        aimbar.ChangeAimBar(new Vector3(attackRange * 2f, attackWidth, 1));
        aimbar.gameObject.SetActive(false);
        healthBar = GetComponentInChildren<HealthBar>();
        ammoBar = GetComponentInChildren<AmmoBar>();

        for (int i = 0; i < projectiles.Length; i++)
        {
            projectiles[i].GetComponent<Bullet>().SetProjectile(attackRange, attackSpeed, attackDamage, attackType);
            projectiles[i].tag = "team" + team;
        }


        if (lobbySelection.Bots[playerNum - 1] == 0)
            bot = false;
        else
            bot = true;

        print("STARTING");
        //Respawn();
        invulnerable = 0;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!started)
        {
            //Die();
            started = true;
        }
        if (died)
            Respawn();
        if (invulnerable > 0)
            Invulnerable();


        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
            if (attackCooldown <= 0)
                attackCooldown = 0;
        }
        if (health < maxHealth)
        {
            if (healCooldown == 0)
            {
                health += (int)(maxHealth * 0.2);
                if (health > maxHealth)
                    health = maxHealth;
                healCooldown = maxHealCooldown / 2;
                healthBar.reSize(health / maxHealth);
            }
            else
            {
                healCooldown -= Time.deltaTime;
                if (healCooldown <= 0)
                    healCooldown = 0;
            }

        }
        if (ammo < maxAmmo)
        {
            ammo += reloadSpeed * Time.deltaTime;
            ammoBar.reSize(ammo / maxAmmo);
        }

        if (!bot && active)
            Move();

    }


    void Move()
    {

        _rigidBody.velocity = new Vector2(Input.GetAxis(LS_h) * movementSpeed, -Input.GetAxis(LS_v) * movementSpeed);

        if (Mathf.Abs(_rigidBody.velocity.x) >= 0.5 || Mathf.Abs(_rigidBody.velocity.y) >= 0.5)
        {
            desiredRotation = Mathf.Rad2Deg * Mathf.Atan(_rigidBody.velocity.y / _rigidBody.velocity.x);
            if (_rigidBody.velocity.x < 0)
                desiredRotation += 180;
            _rigidBody.rotation = desiredRotation;
        }

        if (Mathf.Abs(_rigidBody.rotation - desiredRotation) > 1)
        {
            _rigidBody.velocity = Vector2.zero;
            _rigidBody.rotation = desiredRotation;
        }

        if (Input.GetAxis(RT) >= 0.9)
        {
            if (!RTrig)
            {
                RTrig = true;
                aimbar.gameObject.SetActive(true);
            }
            aimbar.transform.position = _rigidBody.position;
        }
        else
        {
            if (RTrig)
            {
                RTrig = false;
                aimbar.gameObject.SetActive(false);
            }
        }

        if (Input.GetButtonDown(RB))
        {
            Attack(new Vector2(Input.GetAxis(RS_h), Input.GetAxis(RS_v)));
        }



    }

    public void Attack(Vector2 angle)
    {
        if (attackCooldown > 0) return;
        if (ammo < 1) return;
        ammo -= 1f;
        healCooldown = maxHealCooldown;
        attackCooldown = maxAttackCooldown;
        projectiles[RotateAmmo()].GetComponent<Bullet>().Shoot(_rigidBody.position, angle);

    }

    public void GetHit(int damage)
    {
        if (invulnerable > 0)
            return;
        health -= damage;
        healCooldown = maxHealCooldown;
        if (health <= 0)
        {
            health = 0;
            Die();
        }
        healthBar.reSize(health / maxHealth);
    }


    int RotateAmmo()
    {
        for (int i = 0; i < projectiles.Length; i++)
        {
            if (!projectiles[i].activeInHierarchy)
                return i;
        }
        return 0;

    }

    public Vector2 getPos()
    {
        return _rigidBody.position;
    }

    public float getRange()
    {
        return attackRange;
    }

    public int getPlayerNum()
    {
        return playerNum;
    }


    public void Die()
    {
        died = true;
        gameObject.SetActive(false);

    }

    void Respawn()
    {
        died = false;
        invulnerable = invulnerability;
        health = maxHealth;
        ammo = maxAmmo;
        healthBar.reSize(health / maxHealth);
        print("RESPAWNING");
        print(playerNum);
        print(spawnPoints[0]);
        _rigidBody.position = spawnPoints[(int)(Random.value * spawnPoints.Length)];
    }

    public void RespawnPos(Vector2[] positions1, Vector2[] positions2)
    {
        print("SETTING SPAWN OF " + playerNum);
        if (team == 0)
        {
            spawnPoints = new Vector2[] { new Vector2(15, 15) };
        }
        if (team == 1)
        {
            spawnPoints = positions1;
        }
        if (team == 2)
        {
            spawnPoints = positions2;
        }
        print(spawnPoints[0]);
        Respawn();
    }

    void Invulnerable()
    {
        invulnerable -= 2 * Time.deltaTime;
        sprite.color = ogColor - new Color(0, 0, 0, Mathf.Sin(invulnerable));
        if (invulnerable <= 0)
        {
            invulnerable = 0;
            sprite.color = ogColor;
        }
    }

    public void SetMovement(Vector2 speed)
    {
        _rigidBody.velocity = speed * movementSpeed;
    }

    public void SetGameActive(bool state)
    {
        active = state;
        if (!active)
        {
            _rigidBody.velocity = Vector2.zero;
            invulnerable = 1000;
        }
    }

    public bool PlayerInGame()
    {
        return active;
    }

    public void SetRotation()
    {
        if (Mathf.Abs(_rigidBody.velocity.x) >= 0.5 || Mathf.Abs(_rigidBody.velocity.y) >= 0.5)
        {
            desiredRotation = Mathf.Rad2Deg * Mathf.Atan(_rigidBody.velocity.y / _rigidBody.velocity.x);
            if (_rigidBody.velocity.x < 0)
                desiredRotation += 180;
            _rigidBody.rotation = desiredRotation;
        }
    }


    public void ChangeCharacter()
    {
        int i = lobbySelection.Selections[playerNum - 1];
        team = lobbySelection.Team[playerNum - 1];
        gameObject.tag = "team" + team;
        if (team > 0)
            circle.changeColour(teamColours[team - 1]);
        if (i == 4)
            i = (int)(Random.value * 4);
        if (i == 0)
            BlueBug();
        else if (i == 1)
            PurpleDragon();
        else if (i == 2)
            Dinosaur();
        else if (i == 3)
            Kangaroo();
        
    }


    //Character definitions
    /* Worth noting 
     * 
     * 
     * reload speed:
     * very slow - 0.4
     * slow - 0.6
     * normal - 0.8
     * fast - 1
     * very fast - 1.2
     * 
     * attackCooldown:
     * interval of x seconds between attacks
     */

    public void BlueBug()
    {
        sprite.sprite = characterSprites[0];
        movementSpeed = 3;
        maxHealth = 3800;
        attackRange = 5;
        attackWidth = 1;
        attackSpeed = 6;
        maxAmmo = 3;
        maxAttackCooldown = 0.2f;
        reloadSpeed = 0.6f;
        attackDamage = 2000;
        attackCooldown = 0;
        maxHealCooldown = 3;
        attackType = 0;
    }

    private void PurpleDragon()
    {
        sprite.sprite = characterSprites[1];
        movementSpeed = 3;
        maxHealth = 5400;
        attackRange = 7;
        attackWidth = 1;
        attackSpeed = 4;
        maxAmmo = 3;
        maxAttackCooldown = 0.4f;
        reloadSpeed = 0.8f;
        attackDamage = 1600;
        attackCooldown = 0;
        maxHealCooldown = 3;
        attackType = 0;
    }

    private void Dinosaur()
    {
        sprite.sprite = characterSprites[2];
        movementSpeed = 3;
        maxHealth = 7000;
        attackRange = 2;
        attackWidth = 1;
        attackSpeed = 5;
        maxAmmo = 3;
        maxAttackCooldown = 0.3f;
        reloadSpeed = 0.6f;
        attackDamage = 3000;
        attackCooldown = 0;
        maxHealCooldown = 3;
        attackType = 0;
    }

    private void Kangaroo()
    {
        sprite.sprite = characterSprites[3];
        movementSpeed = 3;
        maxHealth = 5800;
        attackRange = 3;
        attackWidth = 1;
        attackSpeed = 6;
        maxAmmo = 3;
        maxAttackCooldown = 0.3f;
        reloadSpeed = 1;
        attackDamage = 2500;
        attackCooldown = 0;
        maxHealCooldown = 3;
        attackType = 0;
    }


}

