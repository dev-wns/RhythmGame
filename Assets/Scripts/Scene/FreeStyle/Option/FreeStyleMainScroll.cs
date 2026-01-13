using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FreeStyleMainScroll : ScrollBase
{
    [SerializeField] private FreeStyle        scene;
    [SerializeField] private FreeStyleSearch  search;

    private RectTransform rt => transform as RectTransform;
    private Vector2 contentOriginPos;
    private bool HasAnySongs => Length != 0;

    [Header("Scroll")]
    [SerializeField] private SongScrollerItem prefab;
    [SerializeField] private int maxShowCount = 7;

    private LinkedList<SongScrollerItem> songs = new ();
    private LinkedListNode<SongScrollerItem> medianNode;
    private CustomVerticalLayoutGroup group;

    private int   median;
    private float curPos;
    private float size;

    [Header( "Count Text" )]
    [SerializeField] private TextMeshProUGUI maxText;
    [SerializeField] private TextMeshProUGUI curText;

    [Header("Contents")]
    [SerializeField] private GameObject noContents;

    [Header( "Notify" )]
    private Song  curSong;
    private Tween notifyTween;
    private const float NotifyDelay = .125f;
    public  event Action<Song> OnSelectNotify;

    private void Awake()
    {
        IsLoop = true;
        group = GetComponent<CustomVerticalLayoutGroup>();

        search.OnSearch += UpdateLayoutAndSong;
        scene.OnKeyBind += KeyBind;

        median = Mathf.FloorToInt( maxShowCount / 2f );
        contentOriginPos = rt.anchoredPosition;

        // 객체 할당
        for ( int i = 0; i < maxShowCount; i++ )
        {
            var song = Instantiate( prefab, transform );
            song.Initialize( maxShowCount );
            song.gameObject.SetActive( false );
            songs.AddLast( song );
        }
    }

    private void OnDestroy()
    {
        search.OnSearch       -= UpdateLayoutAndSong;
        scene.OnKeyBind       -= KeyBind;

        notifyTween?.Kill();
        notifyTween = null;
    }

    private async void Start()
    {
        await UniTask.WaitUntil( () => DataStorage.Inst.IsSongParsing );
        Length = Browser.Inst.Songs.Count;

        noContents.SetActive( !HasAnySongs );
        if ( HasAnySongs )
        {
            Initialize();
            SelectNotify();
        }
    }

    private void LazyNotify()
    {
        if ( CurrentIndex != Browser.Inst.CurrentIndex )
        {
            notifyTween?.Kill();
            notifyTween = DOVirtual.DelayedCall( NotifyDelay, SelectNotify );
        }
    }

    private void SelectNotify()
    {
        curSong = Browser.Inst.Select( CurrentIndex );
        OnSelectNotify?.Invoke( curSong );
    }

    private void UpdateLayoutAndSong()
    {
        Initialize();
        if ( !curSong.title.Contains(   Browser.Inst.SelectedSong.title,   StringComparison.OrdinalIgnoreCase ) &&
             !curSong.version.Contains( Browser.Inst.SelectedSong.version, StringComparison.OrdinalIgnoreCase ) )
        {
            SelectNotify();
        }
    }

    private void Initialize()
    {
        Length = Browser.Inst.Songs.Count;

        // 이전 UI 이펙트 초기화
        Select( Browser.Inst.CurrentIndex );
        int index = ( CurrentIndex - median + Length ) % Length; // First Object Index

        int medianIndex = 0;
        medianNode = songs.First;
        foreach ( var song in songs )
        {
            if ( medianIndex < median )
            {
                medianNode = medianNode.Next;
                medianIndex++;
            }

            song.gameObject.SetActive( HasAnySongs );
            song.SetInfo( Browser.Inst.Songs[index] );
            song.ResetPosition();

            index = index + 1 < Length ? index + 1 : 0;
        }

        // 레이아웃 갱신
        group.Initialize();
        group.SetLayoutVertical();

        size = medianNode.Value.rt.sizeDelta.y + group.Spacing;

        // Count Text
        maxText?.SetText( "{0}", Length );
        curText?.SetText( "{0}", ( CurrentIndex + 1 ) );

        rt.anchoredPosition = contentOriginPos;
        curPos = contentOriginPos.y;

        UpdateColor();
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        rt.DOKill();
        rt.DOAnchorPosY( curPos, .3f );

        medianNode.Value.DOKill();
        medianNode.Value.MoveX( 0f );
        int count = 1;
        var prevNode = medianNode.Previous;
        var nextNode = medianNode.Next;

        while ( prevNode != null && nextNode != null )
        {
            if ( prevNode != null )
            {
                prevNode.Value.MoveX( 50f * count );
                prevNode = prevNode.Previous;
            }

            if ( nextNode != null )
            {
                nextNode.Value.MoveX( 50f * count );
                nextNode = nextNode.Next;
            }

            count++;
        }
    }

    private void UpdateColor()
    {
        var songList = songs.First;
        for ( int i = 0; i < songs.Count; i++ )
        {
            songList.Value.UpdateColor( i, maxShowCount );
            songList = songList.Next;
        }
    }

    #region Input
    public override void PrevMove()
    {
        base.PrevMove();

        // 객체 위치 스왑
        var first = songs.First.Value;
        var last  = songs.Last.Value;
        last.rt.anchoredPosition = new Vector2( first.rt.anchoredPosition.x, first.rt.anchoredPosition.y + size );

        // Song 정보 수정
        int index = ( CurrentIndex - median + Length ) % Length; // First Object Index
        last.SetInfo( Browser.Inst.Songs[index] );

        // 노드 이동
        songs.RemoveLast();
        songs.AddFirst( last );
        last.rt.SetAsFirstSibling();

        // 위치 갱신
        medianNode = medianNode.Previous;

        curPos -= size;
        curText.SetText( "{0}", ( CurrentIndex + 1 ) );

        UpdatePosition();
        UpdateColor();
    }

    public override void NextMove()
    {
        base.NextMove();

        // 객체 위치 스왑
        var first = songs.First.Value;
        var last  = songs.Last.Value;
        first.rt.anchoredPosition = new Vector2( last.rt.anchoredPosition.x, last.rt.anchoredPosition.y - size );

        // Song 정보 수정
        int infoIndex = ( CurrentIndex + median ) % Length;
        first.SetInfo( Browser.Inst.Songs[infoIndex] );

        // 노드 이동
        songs.RemoveFirst();
        songs.AddLast( first );
        first.rt.SetAsLastSibling();

        // 위치 갱신
        medianNode = medianNode.Next;

        curPos += size;
        curText.SetText( "{0}", ( CurrentIndex + 1 ) );

        UpdatePosition();
        UpdateColor();
    }

    private void Scroll( Action _action )
    {
        if ( !HasAnySongs ) return;

        AudioManager.Inst.Play( SFX.MainSelect );
        _action?.Invoke();
    }
    #endregion

    public void KeyBind( UnityInputAction _input )
    {
        // UI만 갱신
        _input.Bind( SceneAction.Main, KeyCode.UpArrow,   new InputRepeater( () => Scroll( PrevMove ) ) );
        _input.Bind( SceneAction.Main, KeyCode.DownArrow, new InputRepeater( () => Scroll( NextMove ) ) );

        // 현재 선택된 채보 알림
        _input.Bind( SceneAction.Main, KeyState.Down, KeyCode.UpArrow,   () => notifyTween?.Kill() );
        _input.Bind( SceneAction.Main, KeyState.Down, KeyCode.DownArrow, () => notifyTween?.Kill() );

        _input.Bind( SceneAction.Main, KeyState.Up, KeyCode.UpArrow,   LazyNotify );
        _input.Bind( SceneAction.Main, KeyState.Up, KeyCode.DownArrow, LazyNotify );
    }
}
