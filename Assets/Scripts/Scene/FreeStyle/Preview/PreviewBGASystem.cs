using System.Collections;
using UnityEngine;

public class PreviewBGASystem : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    public PreviewBGARenderer  bgPrefab;
    private PreviewBGARenderer background;
    private ObjectPool<PreviewBGARenderer> bgPool;

    private void Awake()
    {
        bgPool = new ObjectPool<PreviewBGARenderer>( bgPrefab, 1 );
        scroller.OnSelectSong    += UpdateBGA;
        scroller.OnSoundRestart  += Restart;
        AudioManager.OnUpdatePitch += UpdatePitch;
    }

    private void OnDestroy()
    {
        AudioManager.OnUpdatePitch -= UpdatePitch;
    }

    private void Restart( Song _song )
    {
        background.Restart( _song );
    }

    private void UpdateBGA( Song _song )
    {
        background?.Despawn();
        background = bgPool.Spawn();
        background.SetInfo( this, _song );
    }

    private void UpdatePitch( float _pitch )
    {
        background.UpdatePitch( _pitch );
    }

    public void DeSpawn( PreviewBGARenderer _bg )
    {
        bgPool.Despawn( _bg );
    }
}
