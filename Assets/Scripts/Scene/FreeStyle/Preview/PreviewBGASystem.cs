using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PreviewBGASystem : MonoBehaviour
{
    [SerializeField] private FreeStyle scene;
    [SerializeField] private SoundPitchOption pitchOption;
    [SerializeField] private PreviewBGARenderer prefab;
    private ObjectPool<PreviewBGARenderer> pool;
    private PreviewBGARenderer previous;
    private PreviewBGARenderer current;


    private void Awake()
    {
        pool = new ObjectPool<PreviewBGARenderer>( prefab, transform, 3 );
        scene.OnLoadAsync += LoadAsync;
        scene.OnStart     += Play;
        scene.OnRestart   += Play;
        pitchOption.OnUpdatePitch += UpdatePitch;
    }

    private void OnDestroy()
    {
        scene.OnLoadAsync -= LoadAsync;
        scene.OnStart     -= Play;
        scene.OnRestart   -= Play;
        pitchOption.OnUpdatePitch -= UpdatePitch;
    }

    private void Play( in Song _song )
    {
        previous?.Despawn();
        previous = null;

        current?.Play( _song );
    }

    private async UniTask LoadAsync( Song _song, CancellationToken _token )
    {
        //PreviewBGARenderer current = null;
        try
        {
            previous = current;
            current  = pool.Spawn();
            await current.LoadAsync( _song, _token );

            // background?.Despawn(); // previous
            // background = current;
        }
        catch( OperationCanceledException )
        {
            current?.Despawn();
            throw;
        }
    }

    private void UpdatePitch( float _pitch )
    {
        current?.UpdatePitch( _pitch );
    }
}
