using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] CharacterSO characterStats;
    [SerializeField] LobbySO lobbySelection;
    [SerializeField] Sprite[] characterSprites;
    [SerializeField] Color[] teamColours;

    Rigidbody2D _rigidBody;
    SpriteRenderer sprite;
    Circle circle;
    Scores[] scores;

    int[] kills = new int[4];
    int[] damageDealt = new int[4];
    int deaths;
    float centreControl;
    float controlTimer = 1;

    bool bot;
    float invulnerability = 6f;
    int team = 1;
    bool started = false;
    int bounty = 1;
    float respawnTimer;


    //variable between different characters
    int movementSpeed = 3;
    float maxHealth = 4200;
    float attackRange = 5;
    float attackWidth = 1f;
    float attackLength = 1f;
    float attackSpeed = 6;
    float health;
    float maxAmmo = 3;
    float maxAttackCooldown = 50;
    float reloadSpeed = 0.004f;
    int attackDamage = 2400;
    int numProjectiles = 1;
    bool ghost;
    bool splash;
    float superChargeRate;
    float attackLinger;
    float burst;
    bool followThrough;
    float offset;

    int bulletsToShoot;
    Vector2 angle;
    float burstTimer;
    float ammo;
    float attackCooldown = 0;
    float maxHealCooldown = 250;
    float healCooldown = 0;
    int attackType = 0;

    [SerializeField] int playerNum;

    [SerializeField] string LS_h = "LS_h";
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

    Aim[] aimbars;
    [SerializeField] GameObject[] projectiles;
    Bullet[] bullets;

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
        sprite = GetComponentInParent<SpriteRenderer>();
        circle = GetComponentInChildren<Circle>();
        ogColor = sprite.color;

        ChangeCharacter();
        aimbars = new Aim[2];
        foreach (Aim aimbar in GetComponentsInChildren<Aim>())
        {
            aimbars[aimbar.GetPiece()] = aimbar;
        }
        aimbars[0].SetRange(attackRange, attackType, attackWidth);
        aimbars[1].SetRange(attackRange, attackType, attackWidth);
        if (attackType == -1)
            aimbars[0].ChangeAimBar(new Vector3(attackRange * 2f, 0.1f, 1));
        else
            aimbars[0].ChangeAimBar(new Vector3(attackLength + attackRange * 2f, attackWidth, 1));
        aimbars[0].gameObject.SetActive(false);
        aimbars[1].gameObject.SetActive(false);
        healthBar = GetComponentInChildren<HealthBar>();
        ammoBar = GetComponentInChildren<AmmoBar>();
        scores = FindObjectsOfType<Scores>();
        bullets = new Bullet[projectiles.Length];
        for (int i = 0; i < projectiles.Length; i++)
        {
            bullets[i] = projectiles[i].GetComponent<Bullet>();
            bullets[i].SetProjectile(attackRange, attackWidth, attackLength, attackSpeed, attackDamage, attackType, lobbySelection.Selections[playerNum - 1], ghost, splash, attackLinger, followThrough, offset);
            projectiles[i].tag = "team" + team;
        }


        if (lobbySelection.Bots[playerNum - 1] == 0)
            bot = false;
        else
            bot = true;

        invulnerable = 0;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!started)
        {
            started = true;
        }
        if (died)
        {
            Respawning();
            return;
        }
        if (invulnerable > 0)
            Invulnerable();

        if (controlTimer > 0)
        {
            controlTimer -= Time.deltaTime;
            if (controlTimer <= 0)
            {
                controlTimer = 1;
                centreControl += Mathf.Sqrt(Mathf.Pow(_rigidBody.position.x, 2) + Mathf.Pow(_rigidBody.position.y, 2));
            }
        }

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
        Shoot();
    }

    void Update()
    {
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
                aimbars[0].gameObject.SetActive(true);
                if (attackType == -1)
                    aimbars[1].gameObject.SetActive(true);
            }
            aimbars[0].transform.position = _rigidBody.position;
            aimbars[1].transform.position = _rigidBody.position + attackRange * new Vector2(Input.GetAxis(RS_h), Input.GetAxis(RS_v));
        }
        else
        {
            if (RTrig)
            {
                RTrig = false;
                aimbars[0].gameObject.SetActive(false);
                aimbars[1].gameObject.SetActive(false);
            }
        }

        if (Input.GetButtonDown(RB))
        {
            Attack(new Vector2(Input.GetAxis(RS_h), Input.GetAxis(RS_v)));
        }



    }

    public void Attack(Vector2 _angle)
    {
        if (attackCooldown > 0) return;
        if (ammo < 1) return;
        ammo -= 1f;
        healCooldown = maxHealCooldown;
        attackCooldown = maxAttackCooldown;
        bulletsToShoot = numProjectiles;
        burstTimer = 0;
        angle = _angle;
    }

    void Shoot()
    {
        if (bulletsToShoot > 0)
        {
            if (burstTimer > 0)
            {
                burstTimer -= Time.deltaTime;
            }
            else
            {
                bullets[RotateAmmo()].Shoot(_rigidBody.position, angle, playerNum);
                burstTimer = burst;
                bulletsToShoot -= 1;
            }
        }
    }

    public void GetHit(int damage, int shooter)
    {
        if (invulnerable > 0)
            return;
        health -= damage;
        healCooldown = maxHealCooldown;
        if (health <= 0)
        {
            damageDealt[shooter - 1] += (int)health + damage;
            health = 0;
            kills[shooter - 1] += bounty;
            deaths += bounty;
            foreach (Scores score in scores)
            {
                score.updateScore(shooter, bounty);
            }
            bounty = 2;
            Die();
        }
        else
            damageDealt[shooter - 1] += damage;
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

    public void raiseBounty()
    {
        if (bounty < 6)
            bounty++;
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

    public int[][] getScores()
    {
        return new int[][] { kills, damageDealt, new int[] { deaths }, new int[] { (int)centreControl } };
    }

    public Vector2[] getSpawns()
    {
        return spawnPoints;
    }

    public void setPlayerNum(int num)
    {
        playerNum = num;
    }

    public void Die()
    {
        died = true;
        transform.position = 2 * spawnPoints[Random.Range(0, 1)];
        respawnTimer = 3f;
        if (attackType == 0)
        {
            foreach (Bullet bullet in bullets)
            {
                bullet.EndProjectile();
            }
        }
    }

    public float[] GatherInfo(Character character)
    {
        float[] playerInfo = new float[15];
        if (character.playerNum == playerNum)
        {
            playerInfo[0] = _rigidBody.position.x / 10;
            playerInfo[1] = _rigidBody.position.y / 5;
        }
        else
        {
            playerInfo[0] = (_rigidBody.position.x - character.getPos().x) / 10;
            playerInfo[1] = (_rigidBody.position.y - character.getPos().y) / 5;
        }
        playerInfo[2] = health / 10000;
        playerInfo[3] = maxHealth / 10000;
        playerInfo[4] = ammo / 10;
        playerInfo[5] = attackDamage / 10000;
        playerInfo[6] = attackRange / 20;
        playerInfo[7] = attackType;
        playerInfo[8] = attackWidth / 5;
        playerInfo[9] = movementSpeed / 5;
        playerInfo[10] = splash ? 1 : -1;
        playerInfo[11] = ghost ? 1 : -1;
        playerInfo[12] = died ? -1 : ((invulnerability > 0) ? 0 : 1);
        playerInfo[13] = bounty;
        playerInfo[14] = (team == 1) ? 1 : -1;

        return playerInfo;
    }


    void Respawning()
    {
        respawnTimer -= Time.deltaTime;
        if (respawnTimer <= 0)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        died = false;
        invulnerable = invulnerability;
        health = maxHealth;
        ammo = maxAmmo;
        healthBar.reSize(health / maxHealth);
        _rigidBody.position = spawnPoints[(int)(Random.value * spawnPoints.Length)];
    }

    public void RespawnPos(Vector2[] positions1, Vector2[] positions2)
    {
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
        if (team > 0)
            circle.changeColour(teamColours[team - 1]);
        if (i == 4)
        {
            i = (int)(Random.value * 4);
            lobbySelection.Selections[playerNum - 1] = i;
        }
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
        attackWidth = 2f;
        attackLength = 2f;
        attackSpeed = 6;
        maxAmmo = 3;
        maxAttackCooldown = 0.2f;
        reloadSpeed = 0.6f;
        attackDamage = 2000;
        attackCooldown = 0;
        maxHealCooldown = 3;
        attackType = -1;
        numProjectiles = 1;
        ghost = true;
        splash = true;
        superChargeRate = 0.25f;
        attackLinger = 1;
        burst = 0;
        followThrough = true;
        offset = 0.2f;
    }

    private void PurpleDragon()
    {
        sprite.sprite = characterSprites[1];
        movementSpeed = 3;
        maxHealth = 5400;
        attackRange = 7;
        attackWidth = 0.9f;
        attackLength = 0.5f;
        attackSpeed = 4;
        maxAmmo = 3;
        maxAttackCooldown = 0.4f;
        reloadSpeed = 0.8f;
        attackDamage = 1200;
        attackCooldown = 0;
        maxHealCooldown = 3;
        attackType = 1;
        numProjectiles = 3;
        ghost = false;
        splash = false;
        superChargeRate = 0.2f;
        attackLinger = 0;
        burst = 0.1f;
        followThrough = false;
        offset = 0.2f;
    }

    private void Dinosaur()
    {
        sprite.sprite = characterSprites[2];
        movementSpeed = 3;
        maxHealth = 7000;
        attackRange = 0.5f;
        attackWidth = 2f;
        attackLength = 1.2f;
        attackSpeed = 1f;
        maxAmmo = 3;
        maxAttackCooldown = 0.3f;
        reloadSpeed = 0.6f;
        attackDamage = 3000;
        attackCooldown = 0;
        maxHealCooldown = 3;
        attackType = 0;
        numProjectiles = 1;
        ghost = true;
        splash = true;
        superChargeRate = 0.2f;
        attackLinger = 0;
        burst = 0;
        followThrough = true;
        offset = 0;
    }

    private void Kangaroo()
    {
        sprite.sprite = characterSprites[3];
        movementSpeed = 3;
        maxHealth = 5800;
        attackRange = 1.5f;
        attackWidth = 1.2f;
        attackLength = 1.2f;
        attackSpeed = 3f;
        maxAmmo = 3;
        maxAttackCooldown = 0.3f;
        reloadSpeed = 1;
        attackDamage = 1500;
        attackCooldown = 0;
        maxHealCooldown = 3;
        attackType = 0;
        numProjectiles = 2;
        ghost = false;
        splash = false;
        superChargeRate = 0.4f;
        attackLinger = 0;
        burst = 0.2f;
        followThrough = true;
        offset = 0.2f;
    }


}
