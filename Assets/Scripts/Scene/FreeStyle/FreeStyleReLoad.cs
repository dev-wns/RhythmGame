using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class FreeStyleReLoad : MonoBehaviour
{
    public TextMeshProUGUI prefab;
    public int maxShowCount;
    private Queue<string> dataQueue = new Queue<string>();
    private ObjectPool<TextMeshProUGUI> textPool;
    private Queue<TextMeshProUGUI> textQueue = new Queue<TextMeshProUGUI>();
    public UnityEvent OnReLoadEnd;

    private Coroutine corUpdateTexts;
    private Song prevSelectedSong;
    private int fileCount;

    private void Awake()
    {
        textPool = new ObjectPool<TextMeshProUGUI>( prefab, transform, maxShowCount + 2 );
        NowPlaying.OnParsing += AddText;
    }

    private void OnDestroy()
    {
        NowPlaying.OnParsing -= AddText;
    }

    private async void OnEnable()
    {
        NowPlaying.CurrentScene.IsInputLock = true;

        fileCount        = Global.Path.GetFilesInSubDirectories( Global.Path.SoundDirectory, "*.wns" ).Length;
        prevSelectedSong = NowPlaying.CurrentSong;
        corUpdateTexts   = StartCoroutine( UpdateTexts() );

        await Task.Run( DataStorage.Inst.LoadSongs );
        if ( DataStorage.OriginSongs.Count == 0 )
        {
            if ( !ReferenceEquals( corUpdateTexts, null ) )
            {
                StopCoroutine( corUpdateTexts );
                corUpdateTexts = null;
            }

            var text  = textPool.Spawn();
            text.transform.SetAsLastSibling();
            text.text = $"성공 : {DataStorage.OriginSongs.Count}  실패 : {fileCount - DataStorage.OriginSongs.Count}";
            DisableText( text );

            NowPlaying.CurrentScene.IsInputLock = false;
        }

        NowPlaying.Inst.Search( prevSelectedSong );
    }

    private void OnDisable()
    {
        while ( textQueue.Count > 0 )
            textPool.Despawn( textQueue.Dequeue() );
    }

    private void AddText( Song _song ) => dataQueue.Enqueue( _song.title );

    private IEnumerator UpdateTexts()
    {
        int index     = 0;
        while ( index < fileCount )
        {
            if ( dataQueue.Count > 0 )
            {
                var text = textPool.Spawn();
                text.transform.SetAsLastSibling();
                text.text = dataQueue.Dequeue();
                index    += 1;
                DisableText( text );
            }

            yield return YieldCache.WaitForSeconds( .005f );
        }

        var endText = textPool.Spawn();
        endText.transform.SetAsLastSibling();
        endText.text = $"성공 : {DataStorage.OriginSongs.Count}  실패 : {fileCount - DataStorage.OriginSongs.Count}";
        DisableText( endText );

        NowPlaying.CurrentScene.IsInputLock = false;
    }

    private void DisableText( TextMeshProUGUI _text )
    {
        textQueue.Enqueue( _text );
        if ( textPool.ActiveCount > maxShowCount )
             textPool.Despawn( textQueue.Dequeue() );
    }
}
