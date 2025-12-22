using TMPro;
using UnityEngine;


public class PlayerInfo : MonoBehaviour
{
    [Header( "Group" )]
    public GameObject playerInfo;

    [Header( "Infomation Texts" )]
    public TextMeshProUGUI userName;
    public TextMeshProUGUI level;
    public TextMeshProUGUI accuracy;
    public TextMeshProUGUI playCount;

    public void UpdateUserInfo( USER_INFO _data )
    {
        userName.SetText( _data.name );
        level.SetText( "{0}", _data.level );
        accuracy.SetText( "{0:F1}", _data.accuracy );
        playCount.SetText( "{0}", _data.playCount );
    }
}
