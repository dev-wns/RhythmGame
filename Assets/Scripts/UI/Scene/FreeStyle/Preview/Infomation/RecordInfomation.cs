using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class RecordInfomation : MonoBehaviour
{
    private RectTransform rt;

    [Header("Infomation")]
    public Image rankImage;
    public SpriteAtlas rankAtlas;
    public TextMeshProUGUI chaos;
    public TextMeshProUGUI score;
    public TextMeshProUGUI rate;
    public TextMeshProUGUI accuracy;
    public TextMeshProUGUI date;

    [Header("Effect")]
    private Sequence sequence;
    private float startPosX;

    public void Initialize( int _index, float _startPosX )
    {
        rt = transform as RectTransform;
        startPosX = _startPosX;

        sequence = DOTween.Sequence().Pause().SetAutoKill( false ).SetEase( Ease.OutBack );
        sequence.AppendInterval( 0.1f * _index ).Append( rt.DOAnchorPosX( 0f, 1f ) );
        gameObject.SetActive( false );
    }

    public void SetActive( bool _isActive )
    {
        gameObject.SetActive( _isActive );
    }

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

        rt.anchoredPosition = new Vector2( startPosX, rt.anchoredPosition.y );
        sequence.Restart();
    }
}
