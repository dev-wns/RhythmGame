using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum JudgeType { None, Kool, Cool, Good, Bad, Miss }

public class Judgement : MonoBehaviour
{
    public BpmChanger bpmChanger;
    private RectTransform rt;

    private const float Kool = 22f;
    private const float Cool = 24f + Kool;
    private const float Good = 26f + Cool;
    private const float Bad  = 28f + Good;

    private int koolCount, coolCount, goodCount, badCount, missCount;
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
        badImage.anchoredPosition  = rt.anchoredPosition;

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

    public JudgeType GetJudgeType( float _diff )
    {
        float diffAbs = _diff >= 0 ? _diff : -_diff;

        if ( diffAbs <= Kool )                        return JudgeType.Kool;
        else if ( diffAbs > Kool && diffAbs <= Cool ) return JudgeType.Cool;
        else if ( diffAbs > Cool && diffAbs <= Good ) return JudgeType.Good;
        else if ( diffAbs > Good && diffAbs <= Bad  ) return JudgeType.Bad;
        else if ( _diff < -Bad )                      return JudgeType.Miss;
        else                                          return JudgeType.None;
    }

    public void OnJudgement( JudgeType _type )
    {
        switch ( _type )
        {
            case JudgeType.None: break;
            case JudgeType.Kool:
            {
                koolCount++;
                koolText.text = koolCount.ToString();
            }
            break;

            case JudgeType.Cool:
            {
                coolCount++;
                coolText.text = coolCount.ToString();
            } break;

            case JudgeType.Good:
            {
                goodCount++;
                goodText.text = goodCount.ToString();
            } break;

            case JudgeType.Bad:
            {
                badCount++;
                badText.text = badCount.ToString();
            } break;

            case JudgeType.Miss:
            {
                missCount++;
                missText.text = missCount.ToString();
            } break;
        }

        OnJudge?.Invoke( _type );
    }
}
