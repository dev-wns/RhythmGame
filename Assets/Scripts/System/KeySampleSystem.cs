using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


/// <summary>
/// KeySound that plays unconditionally.
/// </summary>
public class KeySampleSystem : MonoBehaviour
{
    public static bool UseAllSamples { get; private set; }

    private InGame scene;
    private List<KeySound> samples = new List<KeySound>();
    private int curIndex;
    private bool isStart;

    private CancellationTokenSource cancelSource = new CancellationTokenSource();

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnGameStart += GameStart;
        scene.OnReLoad    += OnReLoad;
    }

    private void OnDestroy()
    {
        cancelSource?.Cancel();
    }

    private void OnReLoad()
    {
        StopAllCoroutines();
        curIndex = 0;
        UseAllSamples = false;
        cancelSource?.Cancel();
    }

    private async void GameStart()
    {
        UseAllSamples = false;
        await Task.Run( () => Play( cancelSource.Token ) );
    }

    public void SortSamples()
    {
        samples.Sort( delegate ( KeySound _A, KeySound _B )
        {
            if ( _A.time > _B.time ) return 1;
            else if ( _A.time < _B.time ) return -1;
            else return 0;
        } );
    }

    public void AddSample( in KeySound _sample )
    {
        samples.Add( _sample );
    }

    private async void Play( CancellationToken _token )
    {
        while ( !_token.IsCancellationRequested )
        {
            if ( curIndex >= samples.Count )
                 break;

            // 같은 시간에 재생되는 사운드 한번에 처리
            while ( curIndex < samples.Count && samples[curIndex].time < NowPlaying.Playback )
            {
                AudioManager.Inst.Play( samples[curIndex++] );

                if ( curIndex < samples.Count )
                     UseAllSamples = true;
            }

            await Task.Delay( 1 );
        }
    }
}
