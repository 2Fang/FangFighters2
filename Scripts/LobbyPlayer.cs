using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPlayer : MonoBehaviour
{

    [SerializeField] int playerNo;

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

    float vert;
    float horz;
    bool vertF;
    bool horzF;

    LobbyMaster lobbyMaster;


    // Start is called before the first frame update
    void Start()
    {

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

        lobbyMaster = FindObjectOfType<LobbyMaster>();
    }

    // Update is called once per frame
    void Update()
    {
        /*
         * 1 - start
         * 2 - back
         * 3 - up
         * 4 - down
         * 5 - left
         * 6 - right
         * 7 - A
         * 8 - B
        */
        vert = Input.GetAxis(LS_v);
        if (vert < -0.5)
        {
            if (vertF)
                lobbyMaster.Control(playerNo, 3);
            vertF = false;
        }
        else if (vert > 0.5)
        {
            if (vertF)
                lobbyMaster.Control(playerNo, 4);
            vertF = false;
        }
        else
        {
            vertF = true;
        }
        horz = Input.GetAxis(LS_h);
        if (horz < -0.5)
        {
            if (horzF)
                lobbyMaster.Control(playerNo, 5);
            horzF = false;
        }
        else if (horz > 0.5)
        {
            if (horzF)
                lobbyMaster.Control(playerNo, 6);
            horzF = false;
        }
        else
        {
            horzF = true;
        }

        if (Input.GetButtonDown(start))
            lobbyMaster.Control(playerNo, 1);
        else if (Input.GetButtonDown(back))
            lobbyMaster.Control(playerNo, 2);
        else if (Input.GetButtonDown(A))
            lobbyMaster.Control(playerNo, 7);
        else if (Input.GetButtonDown(B))
            lobbyMaster.Control(playerNo, 8);
        else
        {
            string[] names = Input.GetJoystickNames();
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i].Length < 1)
                {
                    lobbyMaster.Control(i + 1, 8);
                }
            }
        }
    }
}
