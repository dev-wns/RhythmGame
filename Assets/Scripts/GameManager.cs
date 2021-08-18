using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static Dictionary<string /* sound name */, Sound> soundList = new Dictionary<string, Sound>();

    private void Awake()
    {
        DontDestroyOnLoad( this.gameObject );
    }
}
