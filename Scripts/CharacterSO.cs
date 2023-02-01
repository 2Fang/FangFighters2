using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class CharacterSO : ScriptableObject
{
    [SerializeField] private Sprite[] sprite;

    public Sprite[] CharacterSprite
    {
        get { return sprite; }
    }

}
