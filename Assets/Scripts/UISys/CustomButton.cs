using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomButton : MonoBehaviour
{
    public void OnContentClick()
    {
        MusicPlayer.LobbyMusicSelect( int.Parse( name ) );
    }
}
