using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using DG.Tweening;

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
    public float startPosX;
    public float duration;
    public float waitTime;
    private Sequence sequence;
    private Coroutine coroutine;

    private void Awake()
    {
        rt = transform as RectTransform;
    }

    private void Start()
    {
        sequence = DOTween.Sequence().Pause().SetAutoKill( false );
        sequence.Append( rt.DOAnchorPosX( 0f, duration ) );
    }

    public void SetActive( bool _isActive ) => gameObject.SetActive( _isActive );

    public void SetInfo( int _index, RecordData _data )
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

        if ( !ReferenceEquals( coroutine, null ) )
        {
            StopCoroutine( coroutine );
            coroutine = null;
        }
        coroutine = StartCoroutine( PlayEffect( _index ) );
    }

    private IEnumerator PlayEffect( int _index )
    {
        rt.anchoredPosition = new Vector2( startPosX, rt.anchoredPosition.y );
        yield return YieldCache.WaitForSeconds( _index * waitTime );
        sequence.Restart();
    }
}
