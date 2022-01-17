using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SongInfomation : OptionButton
{
    public TextMeshProUGUI title, artist;
    public Song song;

    public void SetInfo( Song _song )
    {
        song = _song;
        title.text = _song.title;
        artist.text = _song.artist;
    }

    public override void Process()
    {
        base.Process();

        //GameManager.Inst.SelectSong( 1 );
        NowPlaying.Inst.ChartUpdate();
        SceneChanger.Inst.LoadScene( SceneType.Game );
    }
}
