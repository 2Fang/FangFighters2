using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aim : MonoBehaviour
{

    Rigidbody2D aimBar;

    [SerializeField] int playerNum;

    string RS_h;
    string RS_v;
    string RS_B;

    float x;
    float y;


    // Start is called before the first frame update
    void Awake()
    {
        aimBar = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        y = Input.GetAxis(RS_v);
        x = Input.GetAxis(RS_h);
        aimBar.rotation = Mathf.Rad2Deg * Mathf.Atan(y / x);
        if (x < 0)
            aimBar.rotation -= 180;
        if (x == 0f && y < 0f)
            aimBar.rotation = 270;
    }

    public bool CheckButton(string button)
    {
        return button == RS_B;
    }

    public void ChangeAimBar(Vector3 resize)
    {
        gameObject.transform.localScale = resize;
    }

    public void setPlayerNum(int num)
    {
        playerNum = num;
        RS_v = "RS_v" + playerNum;
        RS_B = "RS_B" + playerNum;
        RS_h = "RS_h" + playerNum;
    }

}
