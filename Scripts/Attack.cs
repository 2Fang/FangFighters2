using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] private float attackCooldown;
    [SerializeField] private GameObject[] fireballs;

    private Animator anim;
    private Character playerMovement;
    private float cooldownTimer = Mathf.Infinity;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<Character>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("RB1"))
        {
            print("IMMA ATTACK");
            Attacking();
        }

        cooldownTimer += Time.deltaTime;
    }

    private void Attacking()
    {
        //anim.SetTrigger("attack");
        print("IM ATTACKING");
        cooldownTimer = 0;

        fireballs[FindFireball()].transform.position = playerMovement.transform.position;
        print(fireballs);
        fireballs[FindFireball()].GetComponent<Fireball>().SetDirection(Mathf.Sign(transform.localScale.x));
    }
    private int FindFireball()
    {
        for (int i = 0; i < fireballs.Length; i++)
        {
            if (!fireballs[i].activeInHierarchy)
                return i;
        }
        return 0;
    }
}