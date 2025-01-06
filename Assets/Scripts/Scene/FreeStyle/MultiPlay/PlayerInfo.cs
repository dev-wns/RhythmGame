using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class PlayerInfo : MonoBehaviour
{
    [Header( "Group" )]
    public GameObject playerInfo;
    public GameObject guestInfo;

    [Header( "Infomation Texts" )]
    public TextMeshProUGUI userName;
    public TextMeshProUGUI level;
    public TextMeshProUGUI accuracy;
    public TextMeshProUGUI playCount;

    private void Awake()
    {
        UpdateGroup();
    }

    public void UpdateGroup()
    {
        if ( NowPlaying.UserInfo is null )
        {
            playerInfo.SetActive( false );
            guestInfo.SetActive( true );
        }
        else
        {
            playerInfo.SetActive( true );
            guestInfo.SetActive( false );
        }
    }

    public void UpdateUserInfo( USER_INFO _data )
    {
        userName.text  = _data.name;
        level.text     = $"{_data.level}";
        accuracy.text  = $"{_data.accuracy:F1}%";
        playCount.text = $"{_data.playCount}";
    }
}
