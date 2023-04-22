using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aim : MonoBehaviour
{

    Rigidbody2D aimBar;

    [SerializeField] int playerNum;
    [SerializeField] int component;

    string RS_h;
    string RS_v;
    string RS_B;

    float attackRange;
    float radius;

    float x;
    float y;

    int attackType;


    // Start is called before the first frame update
    void Awake()
    {
        aimBar = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (component == 0)
        {
            y = Input.GetAxis(RS_v);
            x = Input.GetAxis(RS_h);
            if (x == 0)
                x = 0.0000001f;
            aimBar.rotation = Mathf.Rad2Deg * Mathf.Atan(y / x);
            if (attackType == -1)
            {
                ChangeAimBar(new Vector3(new Vector2(x, y).magnitude * 2 * attackRange, 0.1f, 1));
            }
            if (x < 0)
                aimBar.rotation -= 180;
            if (x == 0.0000001f && y < 0f)
                aimBar.rotation = 270;
        }
    }

    public bool CheckButton(string button)
    {
        return button == RS_B;
    }

    public void ChangeAimBar(Vector3 resize)
    {
        gameObject.transform.localScale = resize;
    }

    public void SetRange(float range, int _attackType, float width)
    {
        attackRange = range;
        attackType = _attackType;
        radius = width;
        if (attackType == -1)
        {
            ChangeAimBar(new Vector3(width, width, 1));
        }
    }

    public int GetPiece()
    {
        return component;
    }

    public void setPlayerNum(int num)
    {
        playerNum = num;
        RS_v = "RS_v" + playerNum;
        RS_B = "RS_B" + playerNum;
        RS_h = "RS_h" + playerNum;
    }

}
