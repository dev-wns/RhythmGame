using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum JUDGE_TYPE { Kool, Cool, Good, Miss }

public class Judgement : MonoBehaviour
{
    private RectTransform rt;

    private const int Kool = 22;
    private const int Cool = 18 + Kool;
    private const int Good = 12 + Cool;
    private const int Bad  = 8 + Good;

    private int koolCount, coolCount, goodCount, badCount, missCount;
    public TextMeshProUGUI koolText, coolText, goodText, badText, missText;
    public bool isShowRange = true;
    public RectTransform koolImage, coolImage, goodImage, badImage;


    private void Awake()
    {
        rt = transform as RectTransform;
        rt.anchoredPosition = new Vector3( 0f, GameSetting.JudgePos, -1f );
        rt.sizeDelta = new Vector3( GameSetting.GearWidth, GameSetting.JudgeHeight, 1f );

        if ( !isShowRange )
        {
            koolImage.gameObject.SetActive( false );
            coolImage.gameObject.SetActive( false );
            goodImage.gameObject.SetActive( false );
            badImage.gameObject.SetActive( false );
        }

        koolImage.anchoredPosition = rt.anchoredPosition;
        coolImage.anchoredPosition = rt.anchoredPosition;
        goodImage.anchoredPosition = rt.anchoredPosition;
        badImage.anchoredPosition = rt.anchoredPosition;

        koolImage.sizeDelta = coolImage.sizeDelta = goodImage.sizeDelta = badImage.sizeDelta = 
            new Vector3( rt.sizeDelta.x, ( float )Screen.width / ( float )Screen.height, 1f );
    }

    private void LateUpdate()
    {
        //if ( isShowRange && NowPlaying.Inst.IsMusicStart )
        //{
        //    koolImage.localScale = new Vector3( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.GetChangedTime( Mathf.Abs( NowPlaying.Playback - Kool ) ) ) * GameSetting.Weight , 1f);
        //    coolImage.localScale = new Vector3( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.GetChangedTime( Mathf.Abs( NowPlaying.Playback - Cool ) ) ) * GameSetting.Weight , 1f);
        //    goodImage.localScale = new Vector3( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.GetChangedTime( Mathf.Abs( NowPlaying.Playback - Good ) ) ) * GameSetting.Weight, 1f );
        //    badImage.localScale  = new Vector3( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.GetChangedTime( Mathf.Abs( NowPlaying.Playback - Bad ) ) ) * GameSetting.Weight, 1f );
        //}

        koolText.text = koolCount.ToString();
        coolText.text = coolCount.ToString();
        goodText.text = goodCount.ToString();
        badText.text = badCount.ToString();
        missText.text = missCount.ToString();
    }

    public bool IsCalculated( float _diff )
    {
        bool isDone = true;
        float diffAbs = _diff >= 0 ? _diff : -_diff;

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
        else if ( diffAbs > Good && diffAbs <= Bad )
        {
            badCount++;
        }
        else
        {
            isDone = false;
        }

        return isDone;
    }

    public bool IsMiss( float _diff )
    {
        if ( _diff < -Bad )
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
