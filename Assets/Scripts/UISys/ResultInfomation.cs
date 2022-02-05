using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultInfomation : MonoBehaviour
{
    [Header("Song Infomation")]
    public TextMeshProUGUI title;
    public TextMeshProUGUI artist;

    [Header( "Hit Count" )]
    public TextMeshProUGUI totalNotes;
    public TextMeshProUGUI perfect, great, good, bad, miss;

    [Header( "Result" )]
    public TextMeshProUGUI maxCombo;
    public TextMeshProUGUI score;
    public TextMeshProUGUI rate;

    [Header( "Rank" )]
    public Image rank;

    private void Awake()
    {
        Judgement judge = null;
        var obj = GameObject.FindGameObjectWithTag( "Judgement" );
        obj?.TryGetComponent( out judge );
        if ( judge == null ) return;

        var song = NowPlaying.Inst.CurrentSong;
        // Song Infomation
        title.text = song.title;
        artist.text = song.artist;

        // Hit Count 
        totalNotes.text = ( song.noteCount + song.sliderCount ).ToString();
        perfect.text = judge.GetResult( HitResult.Perfect ).ToString();
        great.text   = judge.GetResult( HitResult.Great ).ToString();
        good.text    = judge.GetResult( HitResult.Good ).ToString();
        bad.text     = judge.GetResult( HitResult.Bad ).ToString();
        miss.text    = judge.GetResult( HitResult.Miss ).ToString();

        // Result
        maxCombo.text = judge.GetResult( HitResult.Combo ).ToString();
        score.text    = judge.GetResult( HitResult.Score ).ToString();
        rate.text     = $"{( judge.GetResult( HitResult.Rate ) / 100d ):F2}%";

        Destroy( judge );
    }
}
