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
    private Coroutine coroutine;

    private void Awake()
    {
        rt = transform as RectTransform;
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
    }

    public void Play( float _startPosX, float _offset, float _waitTime )
    {
        if ( !ReferenceEquals( coroutine, null ) )
        {
            StopCoroutine( coroutine );
            coroutine = null;
        }

        rt.anchoredPosition = new Vector2( _startPosX, rt.anchoredPosition.y );
        coroutine = StartCoroutine( PlayEffect( _offset, _waitTime ) );
    }

    private IEnumerator PlayEffect( float _offset, float _waitTime )
    {
        yield return YieldCache.WaitForSeconds( _waitTime );
        while ( rt.anchoredPosition.x <= 0 )
        {
            yield return null;
            float posX = rt.anchoredPosition.x + ( _offset * Time.deltaTime );
            rt.anchoredPosition = new Vector2( posX, rt.anchoredPosition.y );
        }

        rt.anchoredPosition = new Vector2( 0f, rt.anchoredPosition.y );
    }
}
