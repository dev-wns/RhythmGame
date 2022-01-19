using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JudgementRange : MonoBehaviour
{
    private bool isShow;
    public RectTransform koolImage, coolImage, goodImage, badImage;

    private void Awake()
    {
        var rt = transform as RectTransform;
        rt.anchoredPosition = new Vector3( 0f, GameSetting.JudgePos, -1f );
        rt.sizeDelta        = new Vector3( GameSetting.GearWidth, GameSetting.JudgeHeight, 1f );

        isShow = GameSetting.CurrentVisualFlag.HasFlag( GameVisualFlag.ShowJudge );
        if ( !isShow )
        {
            koolImage.gameObject.SetActive( false );
            coolImage.gameObject.SetActive( false );
            goodImage.gameObject.SetActive( false );
            badImage.gameObject.SetActive( false );
        }

        koolImage.sizeDelta = coolImage.sizeDelta = goodImage.sizeDelta = badImage.sizeDelta =
                              new Vector3( rt.sizeDelta.x, ( float )Screen.width / ( float )Screen.height, 1f );
    }

    private void LateUpdate()
    {
        if ( isShow && NowPlaying.Playback > 0 )
        {
            float weight = GameSetting.Weight;
            koolImage.localScale = new Vector3( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.GetChangedTime( Mathf.Abs( NowPlaying.Playback - Judgement.Kool ) ) ) * weight, 1f );
            coolImage.localScale = new Vector3( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.GetChangedTime( Mathf.Abs( NowPlaying.Playback - Judgement.Cool ) ) ) * weight, 1f );
            goodImage.localScale = new Vector3( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.GetChangedTime( Mathf.Abs( NowPlaying.Playback - Judgement.Good ) ) ) * weight, 1f );
            badImage.localScale  = new Vector3( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.GetChangedTime( Mathf.Abs( NowPlaying.Playback - Judgement.Bad  ) ) ) * weight, 1f );
        }
    }
}
