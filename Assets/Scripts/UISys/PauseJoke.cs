using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PauseJoke : MonoBehaviour
{
    private TextMeshProUGUI text;
    private string[] jokeList = { "데님", "계단", "즈레", "롱잡", "따닥", "연타", "동치", "알약" };

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
