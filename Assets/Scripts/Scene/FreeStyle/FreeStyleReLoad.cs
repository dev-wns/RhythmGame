using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class FreeStyleReLoad : MonoBehaviour
{
    public TextMeshProUGUI textPrefab;
    public int maxShowCount;
    private Queue<string> dataQueue = new Queue<string>();
    private ObjectPool<TextMeshProUGUI> textPool; 
    private Queue<TextMeshProUGUI> textQueue = new Queue<TextMeshProUGUI>();
    public UnityEvent OnReLoadEnd;

    private Coroutine ShowParsingText;
    private bool isParsingEnd;

    private void Awake()
    {
        textPool = new ObjectPool<TextMeshProUGUI>( textPrefab, transform, maxShowCount + 2 );
    }

    private async void OnEnable()
    {
        NowPlaying.CurrentScene.IsInputLock = true;
        NowPlaying.OnParsing += AddText;
        NowPlaying.OnParsingEnd += ParsingEnd;
        isParsingEnd = false;
        ShowParsingText = StartCoroutine( ShowText() );

        await Task.Run( NowPlaying.Inst.Load );
    }

    private void OnDisable()
    {
        NowPlaying.OnParsing -= AddText;
        NowPlaying.OnParsingEnd -= ParsingEnd;
        while ( textQueue.Count > 0 )
            textPool.Despawn( textQueue.Dequeue() );
    }

    private void ParsingEnd() => isParsingEnd = true;

    private void AddText( Song _song ) => dataQueue.Enqueue( System.IO.Path.GetFileNameWithoutExtension( _song.filePath ) );

    private void Update()
    {
        if ( isParsingEnd )
        {
            if ( NowPlaying.Inst.TotalFileCount == 0 )
            {
                if ( !ReferenceEquals( ShowParsingText, null ) )
                {
                    StopCoroutine( ShowParsingText );
                    ShowParsingText = null;
                }

                OnReLoadEnd?.Invoke();
                var text  = textPool.Spawn();
                text.transform.SetAsLastSibling();
                text.text = $"성공 : {NowPlaying.Inst.Songs.Count}  실패 : {NowPlaying.Inst.TotalFileCount - NowPlaying.Inst.Songs.Count}";
                DisabledText( text );

                NowPlaying.CurrentScene.IsInputLock = false;
            }

            isParsingEnd = false;
        }
    }

    private IEnumerator ShowText()
    {
        yield return new WaitUntil( () => NowPlaying.IsParsing );

        // 파싱 중일 때
        while ( NowPlaying.IsParsing )
        {
            if ( dataQueue.Count > 0 )
            {
                var text = textPool.Spawn();
                text.transform.SetAsLastSibling();
                text.text = dataQueue.Dequeue();
                DisabledText( text );
            }

            yield return YieldCache.WaitForSeconds( .001f );
        }

        // 나머지 데이터 보여주기
        while ( dataQueue.Count > 0 )
        {
            var text = textPool.Spawn();
            text.transform.SetAsLastSibling();
            text.text = dataQueue.Dequeue();
            DisabledText( text );
            yield return YieldCache.WaitForSeconds( .001f );
        }

        OnReLoadEnd?.Invoke();
        var endText  = textPool.Spawn();
        endText.transform.SetAsLastSibling();
        endText.text = $"성공 : {NowPlaying.Inst.Songs.Count}  실패 : {NowPlaying.Inst.TotalFileCount - NowPlaying.Inst.Songs.Count}";
        DisabledText( endText );

        NowPlaying.CurrentScene.IsInputLock = false;
    }

    private void DisabledText( TextMeshProUGUI _text )
    {
        textQueue.Enqueue( _text );
        if ( textPool.ActiveCount > maxShowCount )
             textPool.Despawn( textQueue.Dequeue() );
    }
}
