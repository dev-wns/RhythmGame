using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FreeStyleScroll : ScrollBase, IKeyBind
{
    public SongInfomation prefab;

    public int maxShowCount = 7;
    public int extraCount   = 6;

    private int median, floorMedian;

    private Scene scene;
    private LinkedList<SongInfomation> songs = new LinkedList<SongInfomation>();
    private LinkedListNode<SongInfomation> curSong;
    private CustomVerticalLayoutGroup group;

    private float curPos;
    private float size;
    private RectTransform rt;

    private int curSongIndex;

    private void Awake()
    {
        IsLoop = true;
        rt = transform as RectTransform;
        curPos = rt.anchoredPosition.y;
        
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        group = GetComponent<CustomVerticalLayoutGroup>();
        KeyBind();

        Length = NowPlaying.Inst.Songs.Count;
        for ( int i = 0; i < maxShowCount + extraCount; i++ )
        {
            NowPlaying.Inst.CurrentSongIndex = i;
            var song = Instantiate( prefab, transform );
            song.SetInfo( NowPlaying.Inst.CurrentSong );

            songs.AddLast( song );
        }

        floorMedian = Mathf.FloorToInt( ( maxShowCount + extraCount ) / 2f );
        median      = Mathf.RoundToInt( ( maxShowCount + extraCount ) / 2f );
        Select( median );
        curSongIndex = median;

        curSong = songs.First;
        for ( int i = 0; i < floorMedian; i++ )
            curSong = curSong.Next;

        //var childs = GetComponentsInChildren<SongInfomation>();
        //foreach ( var child in childs )
        //{
        //    songs.AddLast( child );
        //}

        size = ( songs.First.Value.transform as RectTransform ).sizeDelta.y + group.spacing;

        group.SetLayoutVertical();
    }

    public void KeyBind()
    {
        scene.Bind( SceneAction.Main, KeyCode.UpArrow,   () => PrevMove() );
        scene.Bind( SceneAction.Main, KeyCode.DownArrow, () => NextMove() );
    }

    public override void PrevMove()
    {
        base.PrevMove();

        var first = songs.First.Value;
        var last = songs.Last.Value;
        last.rt.anchoredPosition = new Vector2( first.rt.anchoredPosition.x, first.rt.anchoredPosition.y + size );

        NowPlaying.Inst.CurrentSongIndex = CurrentIndex;
        int infoIndex = CurrentIndex - median < 0 ?
                Length + CurrentIndex - median :
                CurrentIndex - median;
        last.SetInfo( NowPlaying.Inst.GetSongIndexAt( infoIndex ) );

        songs.RemoveLast();
        songs.AddFirst( last );

        //curSong.Value.ActiveOutline( false );
        //curSong = curSong.Previous;
        //curSong.Value.ActiveOutline( true );

        curPos -= size;
        rt.DOAnchorPosY( curPos, .25f );

        Debug.Log( $"{CurrentIndex}  {infoIndex}" );
    }

    public override void NextMove()
    {
        base.NextMove();

        var first = songs.First.Value;
        var last  = songs.Last.Value;
        first.rt.anchoredPosition = new Vector2( last.rt.anchoredPosition.x, last.rt.anchoredPosition.y - size );

        NowPlaying.Inst.CurrentSongIndex = CurrentIndex;
        int infoIndex = CurrentIndex + median > Length - 1 ?
                        ( CurrentIndex + median ) - Length :
                        CurrentIndex + median;
        first.SetInfo( NowPlaying.Inst.GetSongIndexAt( infoIndex ) );

        songs.RemoveFirst();
        songs.AddLast( first );

        //curSong.Value.ActiveOutline( false );
        //curSong = curSong.Next;
        //curSong.Value.ActiveOutline( true );

        curPos += size;
        rt.DOAnchorPosY( curPos, .25f );

        Debug.Log( $"{CurrentIndex}  {infoIndex}" );
    }
}
