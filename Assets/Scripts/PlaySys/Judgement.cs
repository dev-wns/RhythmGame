using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum JudgeType { Kool, Cool, Good, Bad, Miss }

public class Judgement : MonoBehaviour
{
    public BpmChanger bpmChanger;
    private RectTransform rt;

    private const float Kool = 28f;
    private const float Cool = 26f + Kool;
    private const float Good = 24f + Cool;
    private const float Bad  = 22f + Good;

    private int slowCount, fastCount;
    private int koolCount, coolCount, goodCount, badCount, missCount;
    public TextMeshProUGUI slowText, fastText;
    public TextMeshProUGUI koolText, coolText, goodText, badText, missText;
    public RectTransform koolImage, coolImage, goodImage, badImage;
    public bool isShowRange = true;

    public delegate void DelJudge( JudgeType _type );
    public event DelJudge OnJudge;

    private void Awake()
    {
        rt = transform as RectTransform;
        rt.anchoredPosition = new Vector3( 0f, GameSetting.JudgePos, -1f );
        rt.sizeDelta = new Vector3( GameSetting.GearWidth, GameSetting.JudgeHeight, 1f );

        //bpmChanger.OnBpmChange += OnBpmChanged;

        if ( !GameSetting.CurrentVisualFlag.HasFlag( GameVisualFlag.ShowJudge ) )
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
        if ( GameSetting.CurrentVisualFlag.HasFlag( GameVisualFlag.ShowJudge ) && NowPlaying.Inst.IsMusicStart )
        {
            koolImage.localScale = new Vector3( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.GetChangedTime( Mathf.Abs( NowPlaying.Playback - Kool ) ) ) * GameSetting.Weight, 1f );
            coolImage.localScale = new Vector3( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.GetChangedTime( Mathf.Abs( NowPlaying.Playback - Cool ) ) ) * GameSetting.Weight, 1f );
            goodImage.localScale = new Vector3( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.GetChangedTime( Mathf.Abs( NowPlaying.Playback - Good ) ) ) * GameSetting.Weight, 1f );
            badImage.localScale  = new Vector3( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.GetChangedTime( Mathf.Abs( NowPlaying.Playback - Bad  ) ) ) * GameSetting.Weight, 1f );
        }
    }

    public bool IsCalculated( float _diff )
    {
        //Globals.Timer.Start();
        bool hasJudge = true;
        float diffAbs = _diff >= 0 ? _diff : -_diff;

        if ( diffAbs >= 0f && diffAbs <= Kool )
        {
            OnJudge?.Invoke( JudgeType.Kool );
            koolCount++;
            koolText.text = koolCount.ToString();
        }
        else if ( diffAbs > Kool && diffAbs <= Cool )
        {
            OnJudge?.Invoke( JudgeType.Cool );
            coolCount++;
            coolText.text = coolCount.ToString();

            if ( _diff >= 0 ) fastText.text = ( ++fastCount ).ToString();
            else              slowText.text = ( ++slowCount ).ToString();
        }
        else if ( diffAbs > Cool && diffAbs <= Good )
        {
            OnJudge?.Invoke( JudgeType.Good );
            goodCount++;
            goodText.text = goodCount.ToString();

            if ( _diff >= 0 ) fastText.text = ( ++fastCount ).ToString();
            else slowText.text = ( ++slowCount ).ToString();

        }
        else if ( diffAbs > Good && diffAbs <= Bad )
        {
            OnJudge?.Invoke( JudgeType.Bad );
            badCount++;
            badText.text = badCount.ToString();

            if ( _diff >= 0 ) fastText.text = ( ++fastCount ).ToString();
            else slowText.text = ( ++slowCount ).ToString();
        }
        else
        {
            hasJudge = false;
        }

        //Debug.Log( Globals.Timer.End );
        return hasJudge;
    }

    public bool IsMiss( float _diff )
    {
        if ( _diff < -Bad )
        {
            missCount++;
            missText.text = missCount.ToString();
            OnJudge?.Invoke( JudgeType.Miss );

            return true;
        }
        else
        {
            return false;
        }
    }
}
