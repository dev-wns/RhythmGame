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

    private void Awake()
    {
        textPool = new ObjectPool<TextMeshProUGUI>( textPrefab, transform, maxShowCount + 2 );
    }

    private async void OnEnable()
    {
        NowPlaying.CurrentScene.IsInputLock = true;
        NowPlaying.Inst.OnParse += AddData;
        StartCoroutine( UpdateText() );
        await Task.Run( () => NowPlaying.Inst.Load() );

    }

    private void OnDisable()
    {
        NowPlaying.Inst.OnParse -= AddData;
        while ( textQueue.Count > 0 )
            textPool.Despawn(  textQueue.Dequeue() );
    }

    private void AddData( string _data ) => dataQueue.Enqueue( _data );

    private IEnumerator UpdateText()
    {
        yield return YieldCache.WaitForSeconds( 1f );

        while ( !NowPlaying.Inst.IsParseSong )
        {
            if ( dataQueue.Count > 0 )
            {
                var text  = textPool.Spawn();
                text.transform.SetAsLastSibling();
                text.text = dataQueue.Dequeue();
                DisabledText( text );
            }
            yield return YieldCache.WaitForSeconds( .001f );
        }

        while ( dataQueue.Count > 0 )
        {
            var text  = textPool.Spawn();
            text.transform.SetAsLastSibling();
            text.text = dataQueue.Dequeue();
            DisabledText( text );
            yield return YieldCache.WaitForSeconds( .001f );
        }

        OnReLoadEnd?.Invoke();

        var endText  = textPool.Spawn();
        endText.transform.SetAsLastSibling();
        endText.text = $"작업이 완료되었습니다.";
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
