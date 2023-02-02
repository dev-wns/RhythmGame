using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class RecordInfomation : MonoBehaviour
{
    public Image rankImage;
    public SpriteAtlas rankAtlas;
    public TextMeshProUGUI chaos;
    public TextMeshProUGUI score;
    public TextMeshProUGUI rate;
    public TextMeshProUGUI accuracy;
    public TextMeshProUGUI date;

    public void SetActive( bool _isActive ) => gameObject.SetActive( _isActive );

    public void SetInfo( RecordData _data )
    {
        chaos.text    = $"{( ( GameRandom )_data.random ).ToString().Replace( '_', ' ' )}";
        score.text    = $"{_data.score:N0}";
        rate.text     = $"x{_data.pitch:N1}";
        accuracy.text = $"{( _data.accuracy * .01f ):N2}%";
        date.text     = _data.date;

        rankImage.sprite = _data.accuracy >= 9500 ? rankAtlas.GetSprite( "Ranking-S" ) :
                           _data.accuracy >= 9000 ? rankAtlas.GetSprite( "Ranking-A" ) :
                           _data.accuracy >= 8500 ? rankAtlas.GetSprite( "Ranking-B" ) :
                           _data.accuracy >= 8000 ? rankAtlas.GetSprite( "Ranking-C" ) :
                                                    rankAtlas.GetSprite( "Ranking-D" );
    }
}
