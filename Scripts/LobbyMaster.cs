using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyMaster : MonoBehaviour
{
    [SerializeField] LobbySO lobbySelection;

    [SerializeField] int noCharacters;
    [SerializeField] int columns;
    [SerializeField] float scale;
    [SerializeField] int characterScale;

    public GameObject circlePrefab;
    public GameObject podiumPrefab;
    public GameObject pIconPrefab;
    public GameObject characterPrefab;

    GameObject[] iconBorders;
    GameObject[] icons;
    public Sprite[] iconSprites;

    GameObject[] podiums = new GameObject[4];
    SpriteRenderer[] podiumStates = new SpriteRenderer[4];
    public Sprite[] podiumSpriteSheet;
    int[] podiumSkins = { 0, 0, 0, 0 };

    int[] playersInLobby = new int[4];
    int[] botsInLobby = new int[4];
    int[] playerSelections = new int[4];

    GameObject[] characterPreviews = new GameObject[4];
    SpriteRenderer[] characterSprites = new SpriteRenderer[4];
    public Sprite[] characterSpriteSheet;

    GameObject[] pIcons = new GameObject[4];
    public Sprite[] pIconSpriteSheet;
    SpriteRenderer[] pIconSprites = new SpriteRenderer[4];
    Vector3[] pIconPlace = { new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0) };

    bool ready;

    // Start is called before the first frame update
    void Start()
    {
        iconBorders = new GameObject[noCharacters];
        icons = new GameObject[noCharacters];
        SpriteRenderer iconSprite;
        for (int i = 0; i < noCharacters; i++)
        {
            iconBorders[i] = Instantiate(circlePrefab, new Vector3(-9f + 2.5f * (i % columns), 3.75f - 2.25f * (int)(i / columns), 0), Quaternion.identity);
            icons[i] = Instantiate(circlePrefab, new Vector3(-9f + 2.5f * (i % columns), 3.75f - 2.25f * (int)(i / columns), 0), Quaternion.identity);
            iconBorders[i].transform.localScale = new Vector2(scale, scale);
            iconSprite = icons[i].GetComponent<SpriteRenderer>();
            iconSprite.sprite = iconSprites[i];
            iconSprite.color = Color.white;
            iconSprite.sortingOrder = 2;
            icons[i].transform.localScale = new Vector2(scale * 0.9f, scale * 0.9f);

        }
        for (int i = 0; i < 4; i++)
        {
            podiums[i] = Instantiate(podiumPrefab, new Vector3(new float[]{-8.5f, -5.5f, 5.5f, 8.5f}[i], -3.5f, 0f), Quaternion.identity);
            podiumStates[i] = podiums[i].GetComponent<SpriteRenderer>();
            characterPreviews[i] = Instantiate(characterPrefab, new Vector3(new float[] { -8.5f, -5.5f, 5.5f, 8.5f }[i], -2f, 0f), Quaternion.identity);
            characterSprites[i] = characterPreviews[i].GetComponent<SpriteRenderer>();
            characterSprites[i].color = new Color(1, 1, 1, 0.5f);
            characterSprites[i].transform.localScale = new Vector2(characterScale * 0.6f, characterScale * 0.6f);
            characterPreviews[i].SetActive(false);
            pIcons[i] = Instantiate(pIconPrefab, new Vector3(-9f, 3.75f, 0) + pIconPlace[i], Quaternion.identity);
            pIconSprites[i] = pIcons[i].GetComponent<SpriteRenderer>();
            pIconSprites[i].sortingOrder = 4;
            pIcons[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Control(int playerNo, int playerInput)
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
        playerNo--;
        if (playerInput == 1)
        {
            if (playersInLobby[playerNo] == 0) // if player not in lobby add them to lobby
            {
                print("player " + playerNo + "joined");
                playersInLobby[playerNo] = 1;
                botsInLobby[playerNo] = 0;
                pIcons[playerNo].SetActive(true);
                characterPreviews[playerNo].SetActive(true);
                pIconSprites[playerNo].sprite = pIconSpriteSheet[playerNo];
                podiumSkins[playerNo] = 1;
                podiumStates[playerNo].sprite = podiumSpriteSheet[podiumSkins[playerNo]];
                playerSelections[playerNo] = 0;
            }

            if (ready)
            {
                LoadBattle();
            }
        }
        else if (playerInput == 2 || playerInput == 8)
        {
            if (playersInLobby[playerNo] > 0) //only does something if player is already in lobby
            {
                playersInLobby[playerNo] -= 1;
                if (playersInLobby[playerNo] == 0) //remove player from lobby
                {
                    pIcons[playerNo].SetActive(false);
                    characterPreviews[playerNo].SetActive(false);
                    podiumSkins[playerNo] = 0;
                    podiumStates[playerNo].sprite = podiumSpriteSheet[0];
                }
                else //deselect character
                {
                    characterSprites[playerNo].color = new Color(1, 1, 1, 0.5f);
                    characterSprites[playerNo].transform.localScale = new Vector2(characterScale * 0.6f, characterScale * 0.6f);
                }
            }
        }
        if (playersInLobby[playerNo] == 1) //player in lobby but not locked in
        {
            if (playerInput == 3) // up
            {
                if (playerSelections[playerNo] < columns && playerSelections[playerNo] < noCharacters)
                    playerSelections[playerNo] = noCharacters;
                else if (playerSelections[playerNo] >= noCharacters)
                    playerSelections[playerNo] = noCharacters - 1;
                else
                    playerSelections[playerNo] -= columns;
            }
            else if (playerInput == 4) // down
            {
                if (playerSelections[playerNo] >= noCharacters)
                    playerSelections[playerNo] = 0;
                else if (playerSelections[playerNo] >= noCharacters - columns)
                    playerSelections[playerNo] = noCharacters;
                else
                    playerSelections[playerNo] += columns;
            }
            else if (playerInput == 5) // left
            {
                if (playerSelections[playerNo] < noCharacters)
                {
                    if (playerSelections[playerNo] % columns == 0)
                    {
                        playerSelections[playerNo] += columns - 1;
                        if (playerSelections[playerNo] >= noCharacters)
                            playerSelections[playerNo] = noCharacters - 1;
                    }
                    else
                        playerSelections[playerNo] -= 1;
                }
                else
                {
                    if (playerSelections[playerNo] == noCharacters)
                        playerSelections[playerNo] += 2;
                    else
                        playerSelections[playerNo] -= 1;
                }
            }
            else if (playerInput == 6) // right
            {
                if (playerSelections[playerNo] < noCharacters)
                {
                    if (playerSelections[playerNo] % columns == columns - 1)
                        playerSelections[playerNo] -= columns - 1;
                    else if (playerSelections[playerNo] == noCharacters - 1)
                        while (playerSelections[playerNo] % columns != 0)
                            playerSelections[playerNo] -= 1;
                    else
                        playerSelections[playerNo] += 1;
                }
                else
                {
                    if (playerSelections[playerNo] == noCharacters + 2)
                        playerSelections[playerNo] -= 2;
                    else
                        playerSelections[playerNo] += 1;
                }
            }
            else if (playerInput == 7) // A
            {
                if (playerSelections[playerNo] == noCharacters) // remove bot
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (botsInLobby[i] == 2)
                        {
                            botsInLobby[i] = 0;
                            pIcons[i].SetActive(false);
                            characterPreviews[i].SetActive(false);
                            podiumSkins[i] = 0;
                            podiumStates[i].sprite = podiumSpriteSheet[0];
                            break;
                        }
                    }
                }
                if (playerSelections[playerNo] == noCharacters + 1) // change team
                {
                    podiumSkins[playerNo] = 3 - podiumSkins[playerNo];
                    podiumStates[playerNo].sprite = podiumSpriteSheet[podiumSkins[playerNo]];
                }
                if (playerSelections[playerNo] == noCharacters + 2) // add bot
                {
                    for (int i = 3; i >= 0; i--)
                    {
                        if (playersInLobby[i] == 0 && botsInLobby[i] == 0)
                        {
                            botsInLobby[i] = 2;
                            pIcons[i].SetActive(true);
                            characterPreviews[i].SetActive(true);
                            characterSprites[i].sprite = characterSpriteSheet[noCharacters - 1];
                            pIconSprites[i].sprite = pIconSpriteSheet[4];
                            podiumSkins[i] = 1 + (int)(2 * Random.value);
                            podiumStates[i].sprite = podiumSpriteSheet[podiumSkins[i]];
                            playerSelections[i] = noCharacters - 1;
                            pIcons[i].transform.position = new Vector3(-9f + 2.5f * (playerSelections[i] % columns), 3.75f - 2.25f * (playerSelections[i] / columns), 0) + pIconPlace[i];
                            break;
                        }
                    }
                }
                if (playerSelections[playerNo] < noCharacters) // lock in character
                {
                    playersInLobby[playerNo] = 2;
                    characterSprites[playerNo].color = new Color(1, 1, 1, 1);
                    characterSprites[playerNo].transform.localScale = new Vector2(characterScale, characterScale);
                    CheckForReady();
                }
            }

            characterSprites[playerNo].sprite = characterSpriteSheet[playerSelections[playerNo]];

            if (playerSelections[playerNo] >= noCharacters)
            {
                int x = playerSelections[playerNo] - noCharacters;
                pIcons[playerNo].transform.position = new Vector3(-2.5f + 2.5f * x, -3.5f, 0) + pIconPlace[playerNo];
            }
            else
                pIcons[playerNo].transform.position = new Vector3(-9f + 2.5f * (playerSelections[playerNo] % columns), 3.75f - 2.25f * (playerSelections[playerNo] / columns), 0) + pIconPlace[playerNo];
        }
    }

    void CheckForReady()
    {
        ready = false;
        int[] teams = new int[3];
        print("TRY TO START");
        for (int i = 0; i < 4; i++)
        {
            teams[podiumSkins[i]] ++;
        }
        print(teams[0]);
        print(teams[1]);
        print(teams[2]);
        if (teams[1] == 1)
        {
            if (teams[2] == 1 || teams[2] == 2)
                ready = true;
        }
        else if (teams[1] == 2)
        {
            if (teams[2] == 1 || teams[2] == 2)
                ready = true;
        }
    }

    void LoadBattle()
    {
        lobbySelection.Selections = playerSelections;
        lobbySelection.Team = podiumSkins;
        lobbySelection.Bots = botsInLobby;
        SceneManager.LoadScene("Battle");
    }

}
