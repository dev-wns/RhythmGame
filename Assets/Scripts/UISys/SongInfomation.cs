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

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        builder.Capacity = _song.artist.Length + 8 + _song.creator.Length;
        builder.Append( _song.artist ).Append( " // " ).Append( _song.creator );

        title.text = _song.title;
        artist.text = _song.artist;
    }

    public override void Process()
    {
        base.Process();

        //GameManager.Inst.SelectSong( 1 );
        NowPlaying.Inst.Select( GameManager.Inst.CurrentSong );
        SceneChanger.Inst.LoadScene( SCENE_TYPE.GAME );
    }
}
