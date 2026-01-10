using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class FreeStyleReload : MonoBehaviour
{
    [SerializeField] private FreeStyle scene;
    [SerializeField] private TextMeshProUGUI prefab;
    [SerializeField] private GameObject uiBack;
    [SerializeField] private uint maxShowCount;

    private ObjectPool<TextMeshProUGUI> uiPool;
    private Queue<TextMeshProUGUI>  uiQueue   = new ();
    private ConcurrentQueue<string> dataQueue = new ();
    [SerializeField] private UnityEvent OnReloadEnd;

    private Coroutine corUpdateTexts;
    private Song prevSelectedSong;
    private int fileCount;
    private int uiCount;     // 현재 보여진 텍스트UI 개수

    private void Awake()
    {
        uiPool = new ObjectPool<TextMeshProUGUI>( prefab, transform, maxShowCount + 2 );
        DataStorage.Inst.OnSongParsed += AddText;
    }

    private void OnDestroy()
    {
        DataStorage.Inst.OnSongParsed -= AddText;
    }

    private async void OnEnable()
    {
        scene.IsInputLock = true;
        uiBack.SetActive( false );

        uiCount          = 0;
        prevSelectedSong = Browser.Inst.CurrentSong;
        corUpdateTexts   = StartCoroutine( UpdateTexts() );

        await UniTask.RunOnThreadPool( DataStorage.Inst.LoadSongs );
        fileCount = DataStorage.Inst.Songs.Count;

        await UniTask.WaitUntil( () => uiCount == fileCount );
        if ( !ReferenceEquals( corUpdateTexts, null ) )
        {
            StopCoroutine( corUpdateTexts );
            corUpdateTexts = null;
        }

        var endText = uiPool.Spawn();
        endText.transform.SetAsLastSibling();
        endText.SetText( "성공 : {0}  실패 : {1}", DataStorage.Inst.Songs.Count, ( fileCount - DataStorage.Inst.Songs.Count ) );
        DisableText( endText );

        uiBack.SetActive( true );
        scene.IsInputLock = false;
        OnReloadEnd?.Invoke();
    }

    private void OnDisable()
    {
        while ( uiQueue.Count > 0 )
            uiPool.Despawn( uiQueue.Dequeue() );
    }

    private void AddText( Song _song ) => dataQueue.Enqueue( _song.title );

    private IEnumerator UpdateTexts()
    {
        while ( true ) // index < fileCount )
        {
            if ( dataQueue.Count > 0 )
            {
                TextMeshProUGUI text = uiPool.Spawn();
                text.transform.SetAsLastSibling();
                if ( dataQueue.TryDequeue( out string _text ) )
                {
                    text.SetText( _text );
                    DisableText( text );
                    uiCount += 1;
                }
            }

            yield return YieldCache.WaitForSeconds( .005f );
        }
    }

    private void DisableText( TextMeshProUGUI _text )
    {
        uiQueue.Enqueue( _text );
        if ( uiPool.ActiveCount > maxShowCount )
             uiPool.Despawn( uiQueue.Dequeue() );
    }
}
