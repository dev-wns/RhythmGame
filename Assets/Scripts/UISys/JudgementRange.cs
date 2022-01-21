using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JudgementRange : MonoBehaviour
{
    private bool isShow;
    public RectTransform perfectImage, lazyPerfectImage, greatImage, goodImage, badImage;

    private void Awake()
    {
        var rt = transform as RectTransform;
        rt.anchoredPosition = new Vector3( 0f, GameSetting.JudgePos, -1f );
        rt.sizeDelta        = new Vector3( GameSetting.GearWidth, GameSetting.JudgeHeight, 1f );

        isShow = GameSetting.CurrentVisualFlag.HasFlag( GameVisualFlag.ShowJudge );
        if ( !isShow )
        {
            perfectImage.gameObject.SetActive( false );
            lazyPerfectImage.gameObject.SetActive( false );
            greatImage.gameObject.SetActive( false );
            goodImage.gameObject.SetActive( false );
            badImage.gameObject.SetActive( false );
        }

        perfectImage.sizeDelta = lazyPerfectImage.sizeDelta = greatImage.sizeDelta = goodImage.sizeDelta = badImage.sizeDelta =
                              new Vector3( rt.sizeDelta.x, ( float )Screen.width / ( float )Screen.height, 1f );
    }

    private void LateUpdate()
    {
        if ( isShow && NowPlaying.Playback > 0 )
        {
            float weight = GameSetting.Weight;
            perfectImage.localScale     = new Vector2( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.Inst.GetChangedTime( Globals.Abs( NowPlaying.Playback - Judgement.Perfect ) ) )     * weight );
            lazyPerfectImage.localScale = new Vector2( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.Inst.GetChangedTime( Globals.Abs( NowPlaying.Playback - Judgement.LazyPerfect ) ) ) * weight );
            greatImage.localScale       = new Vector2( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.Inst.GetChangedTime( Globals.Abs( NowPlaying.Playback - Judgement.Great ) ) )       * weight );
            goodImage.localScale        = new Vector2( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.Inst.GetChangedTime( Globals.Abs( NowPlaying.Playback - Judgement.Good ) ) )        * weight );
            badImage.localScale         = new Vector2( transform.localScale.x, ( NowPlaying.PlaybackChanged - NowPlaying.Inst.GetChangedTime( Globals.Abs( NowPlaying.Playback - Judgement.Bad  ) ) )        * weight );
        }
    }
}
