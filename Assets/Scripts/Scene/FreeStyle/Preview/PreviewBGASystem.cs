using UnityEngine;

public class PreviewBGASystem : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    public PreviewBGARenderer  prefab;
    private ObjectPool<PreviewBGARenderer> pool;
    private PreviewBGARenderer background; // current background

    private void Awake()
    {
        pool = new ObjectPool<PreviewBGARenderer>( prefab, transform, 3 );
        scroller.OnSelectSong      += UpdateBGA;
        scroller.OnSoundRestart    += Restart;
        AudioManager.OnUpdatePitch += UpdatePitch;
    }

    private void OnDestroy()
    {
        scroller.OnSelectSong      -= UpdateBGA;
        scroller.OnSoundRestart    -= Restart;
        AudioManager.OnUpdatePitch -= UpdatePitch;
    }

    private void Restart( Song _song )
    {
        background.Restart( _song );
    }

    private void UpdateBGA( Song _song )
    {
        background?.Despawn();
        background = pool.Spawn();
        background.Initialize( _song );
    }

    private void UpdatePitch( float _pitch )
    {
        background.UpdatePitch( _pitch );
    }
}
