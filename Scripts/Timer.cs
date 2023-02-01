using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    string text;
    string secondString;
    string minString;
    Text timerbox;

    void Start()
    {
        timerbox = GetComponent<Text>();
    }

    public void SetTimer(int seconds)
    {
        int mins = 0;
        while (seconds >= 60)
        {
            mins += 1;
            seconds -= 60;
        }
        secondString = seconds + "";
        minString = mins + "";
        if (seconds < 10)
            secondString = "0" + seconds;
        if (mins < 10)
            minString = "0" + mins;
        text = minString + ":" + secondString;
        timerbox.text = text;
    }

    void Update()
    {
        
    }

}
