using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScreen : MonoBehaviour
{
    [SerializeField] string[] _loader;
    SpriteRenderer[] options;
    int choice = 0;
    int choices;
    float input;
    bool reset;

    // Start is called before the first frame update
    void Start()
    {
        options = GetComponentsInChildren<SpriteRenderer>();
        choices = options.Length;
    }

    // Update is called once per frame
    void Update()
    {
        input = Input.GetAxis("LS_v1");
        if (input < -0.5)
        {
            if (reset)
                choice = (choice - 1 + choices) % choices;
            reset = false;
        }
        else if (input > 0.5)
        {
            if (reset)
                choice = (choice + 1) % choices;
            reset = false;
        }
        else
        {
            reset = true;
        }

        HighlightChoice();

        if (Input.GetButtonDown("A1"))
        {
            ConfirmChoice();
        }
    }

    void HighlightChoice()
    {
        foreach (SpriteRenderer option in options)
        {
            option.color = Color.white;
        }
        options[choice].color = Color.green;
    }

    void ConfirmChoice()
    {
        SceneManager.LoadScene(_loader[choice]);
    }

}
