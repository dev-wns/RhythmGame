using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PauseJoke : MonoBehaviour
{
    private TextMeshProUGUI text;
    private string[] jokeList = { "µ¥´Ô", "°è´Ü", "Áî·¹", "·Õ³ò", "µû´Ú", "¿¬Å¸", "µ¿Ä¡" };

    private void Awake()
    {
        if ( !TryGetComponent<TextMeshProUGUI>( out text ) )
             Destroy( this );
    }

    private void OnEnable()
    {
        text.text = jokeList[Random.Range( 0, jokeList.Length - 1 )];
    }
}
