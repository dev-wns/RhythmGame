using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum JUDGE_TYPE { Kool, Cool, Good, Miss }
public class Judgement : MonoBehaviour
{
    private RectTransform rt;

    private const int Kool = 22;
    private const int Cool = 35 + Kool;
    private const int Good = 28 + Cool;

    private int koolCount, coolCount, goodCount, missCount;
    public TextMeshProUGUI koolText, coolText, goodText;
    public bool isShowRange = true;
    public RectTransform koolImage, coolImage, goodImage;


    private void Awake()
    {
        rt = transform as RectTransform;
        rt.anchoredPosition = new Vector3( 0f, GlobalSetting.JudgeLine, -1f );
        rt.sizeDelta = new Vector3( GlobalSetting.GearWidth, GlobalSetting.JudgeHeight, 1f );

        if ( !isShowRange )
        {
            koolImage.gameObject.SetActive( false );
            coolImage.gameObject.SetActive( false );
            goodImage.gameObject.SetActive( false );
        }

        koolImage.anchoredPosition = rt.anchoredPosition;
        coolImage.anchoredPosition = rt.anchoredPosition;
        goodImage.anchoredPosition = rt.anchoredPosition;

        Debug.Log( Screen.width / Screen.height );
        koolImage.sizeDelta = coolImage.sizeDelta = goodImage.sizeDelta = new Vector3( rt.sizeDelta.x, ( float )Screen.width / ( float )Screen.height, 1f );
    }

    private void LateUpdate()
    {
        //koolChanged = ( NowPlaying.GetChangedTime( Mathf.Abs( NowPlaying.Playback - Kool ) ) - NowPlaying.PlaybackChanged ) * GameSetting.Weight;
        //coolChanged = ( NowPlaying.GetChangedTime( Mathf.Abs( NowPlaying.Playback - Cool ) ) - NowPlaying.PlaybackChanged ) * GameSetting.Weight;
        //goodChanged = ( NowPlaying.GetChangedTime( Mathf.Abs( NowPlaying.Playback - Good ) ) - NowPlaying.PlaybackChanged ) * GameSetting.Weight;

        if ( isShowRange && NowPlaying.Inst.IsMusicStart )
        {
            koolImage.localScale = new Vector3( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.GetChangedTime( Mathf.Abs( NowPlaying.Playback - Kool ) ) ) * GameSetting.Weight , 1f);
            coolImage.localScale = new Vector3( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.GetChangedTime( Mathf.Abs( NowPlaying.Playback - Cool ) ) ) * GameSetting.Weight , 1f);
            goodImage.localScale = new Vector3( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.GetChangedTime( Mathf.Abs( NowPlaying.Playback - Good ) ) ) * GameSetting.Weight , 1f);
        }

        koolText.text = koolCount.ToString();
        coolText.text = coolCount.ToString();
        goodText.text = goodCount.ToString();
    }

    public bool IsCalculated( float _diff )
    {
        bool isDone = true;
        float diffAbs = Mathf.Abs( _diff );

        // Kool 22 Cool 35 Good 28
        if ( diffAbs <= Kool )
        {
            koolCount++;
        }
        else if ( diffAbs > Kool && diffAbs <= Cool )
        {
            coolCount++;
        }
        else if ( diffAbs > Cool && diffAbs <= Good )
        {
            goodCount++;
        }
        else
        {
            isDone = false;
        }

        return isDone;
    }

    public bool IsMiss( float _diff )
    {
        float diffAbs = Mathf.Abs( _diff );
        if ( _diff < -Good )
        {
            missCount++;
            return true;
        }
        else
        {
            return false;
        }
    }
}
