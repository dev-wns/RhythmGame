using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FreeStyleScroll : ScrollBase, IKeyBind
{
    public SongInfomation prefab;

    public int maxShowCount = 7;
    public int extraCount   = 6;
    private int median;

    private Scene scene;
    private LinkedList<SongInfomation> songs = new LinkedList<SongInfomation>();
    private CustomVerticalLayoutGroup group;
    private LinkedListNode<SongInfomation> curSong;
    // active note
    private LinkedListNode<SongInfomation> previous, next;

    private RectTransform rt;
    private float curPos;
    private float size;

    private void Awake()
    {
        NowPlaying.Inst.CurrentSongIndex = 4;
        IsLoop = true;
        rt = transform as RectTransform;
        curPos = rt.anchoredPosition.y;
        
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        group = GetComponent<CustomVerticalLayoutGroup>();
        KeyBind();

        median = Mathf.RoundToInt( ( maxShowCount + extraCount ) / 2f );

        // 객체 할당
        Length = NowPlaying.Inst.Songs.Count - 1;
        int count = NowPlaying.Inst.CurrentSongIndex - ( median - 1 ) < 0 ?
                    NowPlaying.Inst.CurrentSongIndex - ( median - 1 ) + Length :
                    NowPlaying.Inst.CurrentSongIndex - ( median - 1 );

        Debug.Log( count );
        for ( int i = 0; i < maxShowCount + extraCount; i++ )
        {
            if ( count > Length - 1 ) count = 0;
            var song = Instantiate( prefab, transform );
            song.Initialize();
            song.SetInfo( NowPlaying.Inst.GetSongIndexAt( count++ ) );

            songs.AddLast( song );
        }
       
        Select( NowPlaying.Inst.CurrentSongIndex );

        // 레이아웃 갱신
        group.Initialize();
        group.SetLayoutVertical();

        // 중앙 위치에 있는 객체
        curSong = songs.First;
        for ( int i = 0; i < median - 1; i++ )
        {
            curSong = curSong.Next;
        }
        size = curSong.Value.rt.sizeDelta.y + group.spacing;

        LinkedListNode<SongInfomation> first = songs.First;
        LinkedListNode<SongInfomation> last  = songs.Last;
        // Active 조절기준이 될 객체
        previous = next = curSong;
        int showRoundCount = Mathf.RoundToInt( maxShowCount / 2f );
        for ( int i = 0; i < showRoundCount; i++ )
        {
            previous = previous.Previous;
            next     = next.Next;

            first.Value.gameObject.SetActive( false );
            last.Value.gameObject.SetActive( false );
            first = first.Next;
            last  = last.Previous;
        }
        
        curSong.Value.ActiveOutline( true );
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
        var last  = songs.Last.Value;
        last.rt.anchoredPosition = new Vector2( first.rt.anchoredPosition.x, first.rt.anchoredPosition.y + size );

        NowPlaying.Inst.CurrentSongIndex = CurrentIndex;
        int infoIndex = CurrentIndex - ( median - 1 ) < 0 ?
                        CurrentIndex - ( median - 1 ) + Length :
                        CurrentIndex - ( median - 1 );
        last.SetInfo( NowPlaying.Inst.GetSongIndexAt( infoIndex ) );
        
        next = next.Previous;
        next.Value.gameObject.SetActive( false );

        previous.Value.gameObject.SetActive( true );
        previous = previous.Previous;
        
        songs.RemoveLast();
        songs.AddFirst( last );

        curSong.Value.ActiveOutline( false );
        curSong = curSong.Previous;
        curSong.Value.ActiveOutline( true );

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
        int infoIndex = CurrentIndex + median > Length ?
                        CurrentIndex + median - Length :
                        CurrentIndex + median;
        first.SetInfo( NowPlaying.Inst.GetSongIndexAt( infoIndex ) );
        
        previous = previous.Next;
        previous.Value.gameObject.SetActive( false );

        next.Value.gameObject.SetActive( true );
        next = next.Next;

        songs.RemoveFirst();
        songs.AddLast( first );

        curSong.Value.ActiveOutline( false );
        curSong = curSong.Next;
        curSong.Value.ActiveOutline( true );

        curPos += size;
        rt.DOAnchorPosY( curPos, .25f );

        Debug.Log( $"{CurrentIndex}  {infoIndex}" );
    }
}
