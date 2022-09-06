using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class FreeStyleMainScroll : ScrollBase, IKeyBind
{
    public SongInfomation prefab;
    private RectTransform rt;

    [Header("Scroll")]
    public ScrollBar scrollbar;
    public int maxShowCount = 7;
    public int extraCount   = 8;
    
    private int median;
    private float curPos;
    private float size;
    
    [Header( "Count Text" )]
    public TextMeshProUGUI maxText;
    public TextMeshProUGUI curText;

    [Header("Scene")]
    private Scene scene;
    private LinkedList<SongInfomation> songs = new LinkedList<SongInfomation>();
    private LinkedListNode<SongInfomation> curNode, prevNode, nextNode;
    private CustomVerticalLayoutGroup group;

    private Song curSong;
    private float playback;
    private float soundLength;
    private uint previewTime;
    private readonly uint waitPreviewTime = 500;
    
    public delegate void DelSelectSong( Song _song );
    public event DelSelectSong OnSelectSong;
    

    private void Awake()
    {
        IsLoop = true;
        rt = transform as RectTransform;
        curPos = rt.anchoredPosition.y;

        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        group = GetComponent<CustomVerticalLayoutGroup>();
        KeyBind();

        median = Mathf.FloorToInt( ( maxShowCount + extraCount ) / 2f );

        // ��ü �Ҵ�
        scrollbar.Initialize( NowPlaying.Inst.Songs.Count );
        Length    = NowPlaying.Inst.Songs.Count;
        int count = NowPlaying.Inst.CurrentSongIndex - median < 0 ?
                    NowPlaying.Inst.CurrentSongIndex - median + Length :
                    NowPlaying.Inst.CurrentSongIndex - median;

        for ( int i = 0; i < maxShowCount + extraCount; i++ )
        {
            if ( count > Length - 1 ) count = 0;
            var song = Instantiate( prefab, transform );
            song.Initialize();
            song.SetInfo( NowPlaying.Inst.GetSongIndexAt( count++ ) );
            songs.AddLast( song );
        }
        Select( NowPlaying.Inst.CurrentSongIndex );

        // ���̾ƿ� ����
        group.Initialize();
        group.SetLayoutVertical();

        // �߾� ��ġ�� �ִ� ��ü
        curNode = songs.First;
        for ( int i = 0; i < median; i++ )
        {
            curNode = curNode.Next;
        }
        size = curNode.Value.rt.sizeDelta.y + group.spacing;

        // Active ���������� �� ��ü
        prevNode = songs.First;
        nextNode = songs.Last;
        int extraHalf = Mathf.FloorToInt( extraCount / 2f );
        for ( int i = 0; i < extraHalf; i++ )
        {
            prevNode.Value.gameObject.SetActive( false );
            nextNode.Value.gameObject.SetActive( false );
            prevNode = prevNode.Next;
            nextNode = nextNode.Previous;
        }

        // Count Text
        if ( maxText ) maxText.text = Length.ToString();
    }

    private void Start()
    {
        curNode.Value.rt.DOAnchorPosX( -100f, .5f );
        UpdateSong();
    }

    private void Update()
    {
        playback += Time.deltaTime * 1000f;

        if ( soundLength + waitPreviewTime < playback &&
             !SoundManager.Inst.IsPlaying( ChannelType.BGM ) )
        {
            SoundManager.Inst.Play( GameSetting.CurrentPitch, false );
            SoundManager.Inst.Position = GetPreviewTime( curSong.previewTime );
            playback = previewTime;
        }
    }

    public override void PrevMove()
    {
        base.PrevMove();

        // ��ü ��ġ ����
        var first = songs.First.Value;
        var last  = songs.Last.Value;
        last.rt.anchoredPosition = new Vector2( first.rt.anchoredPosition.x, first.rt.anchoredPosition.y + size );

        // Song ���� ����
        int infoIndex = CurrentIndex - median < 0 ?
                        CurrentIndex - median + Length :
                        CurrentIndex - median;
        last.SetInfo( NowPlaying.Inst.GetSongIndexAt( infoIndex ) );
        
        // Ȱ��ȭ
        nextNode.Value.gameObject.SetActive( false );
        nextNode = nextNode.Previous;

        prevNode = prevNode.Previous;
        prevNode.Value.gameObject.SetActive( true );

        // ��� �̵�
        songs.RemoveLast();
        songs.AddFirst( last );
      
        // ��ġ ����
        curNode.Value.rt.DOAnchorPosX( 0f, .5f );
        curNode = curNode.Previous;
        curNode.Value.rt.DOAnchorPosX( -100f, .5f );

        curPos -= size;
        rt.DOAnchorPosY( curPos, .25f );

        UpdateSong();
    }

    public override void NextMove()
    {
        base.NextMove();

        // ��ü ��ġ ����
        var first = songs.First.Value;
        var last  = songs.Last.Value;
        first.rt.anchoredPosition = new Vector2( last.rt.anchoredPosition.x, last.rt.anchoredPosition.y - size );
        
        // Song ���� ����
        int infoIndex = CurrentIndex + median >= Length ?
                        CurrentIndex + median - Length :
                        CurrentIndex + median;
        first.SetInfo( NowPlaying.Inst.GetSongIndexAt( infoIndex ) );

        // Ȱ��ȭ
        prevNode.Value.gameObject.SetActive( false );
        prevNode = prevNode.Next;

        nextNode = nextNode.Next;
        nextNode.Value.gameObject.SetActive( true );

        // ��� �̵�
        songs.RemoveFirst();
        songs.AddLast( first );

        // ��ġ ����
        curNode.Value.rt.DOAnchorPosX( 0f, .5f );
        curNode = curNode.Next;
        curNode.Value.rt.DOAnchorPosX( -100f, .5f );

        curPos += size;
        rt.DOAnchorPosY( curPos, .25f );

        UpdateSong();
    }

    private void UpdateScrollBar()
    {
        scrollbar.UpdateHandle( CurrentIndex );

        if ( curText ) 
             curText.text = ( CurrentIndex + 1 ).ToString();
    }

    private void UpdateSong()
    {
        UpdateScrollBar();

        NowPlaying.Inst.CurrentSongIndex = CurrentIndex;
        curSong = NowPlaying.Inst.CurrentSong;
        soundLength = curSong.totalTime;

        OnSelectSong( curSong );

        SoundManager.Inst.LoadBgm( curSong.audioPath, false, true, false );
        SoundManager.Inst.Play( GameSetting.CurrentPitch, false );
        SoundManager.Inst.FadeIn( 1f );

        previewTime = GetPreviewTime( curSong.previewTime );
        SoundManager.Inst.Position = previewTime;
        playback = previewTime;
    }

    private uint GetPreviewTime( int _time ) => _time <= 0 ? ( uint )( soundLength * Mathf.PI * .1f ) : ( uint )_time;

    public void KeyBind()
    {
        scene.Bind( SceneAction.Main, KeyCode.Return, () => SoundManager.Inst.Play( SoundSfxType.MainClick ) );
        scene.Bind( SceneAction.Main, KeyCode.Return, () => scene.LoadScene( SceneType.Game ) );

        scene.Bind( SceneAction.Main, KeyCode.UpArrow, () => SoundManager.Inst.Play( SoundSfxType.MainSelect ) );
        scene.Bind( SceneAction.Main, KeyCode.UpArrow, () => PrevMove() );

        scene.Bind( SceneAction.Main, KeyCode.DownArrow, () => SoundManager.Inst.Play( SoundSfxType.MainSelect ) );
        scene.Bind( SceneAction.Main, KeyCode.DownArrow, () => NextMove() );
    }
}
