using System.Collections;
using UnityEngine;

public class PreviewBGASystem : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    public PreviewBGARenderer bgPrefab;
    private ObjectPool<PreviewBGARenderer> bgPool;
    private PreviewBGARenderer background;

    private void Awake()
    {
        bgPool = new ObjectPool<PreviewBGARenderer>( bgPrefab, 1 );
        scroller.OnSelectSong += ChangeImage;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void ChangeImage( Song _song )
    {
        background?.Despawn();
        background = bgPool.Spawn();
        background.SetInfo( this, _song );
    }


    public void DeSpawn( PreviewBGARenderer _bg )
    {
        bgPool.Despawn( _bg );
    }
}
