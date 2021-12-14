using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene : MonoBehaviour
{
    protected enum SceneType { Lobby, FreeStyle, InGame };

    [Serializable]
    public struct ClipSfx
    {
        public AudioClip move;
        public AudioClip click;
        public AudioClip escape;
    }

    private AudioSource audioSource;
    public AudioClip bgClip;
    public ClipSfx clips;

    protected void SfxPlay( AudioClip _clip )
    {
        if ( _clip == null )
        {
            Debug.LogError( "clip is null." );
            return;
        }

        audioSource.PlayOneShot( _clip );
    }

    protected void BgPlay( AudioClip _clip )
    {
        if ( _clip == null )
        {
            Debug.LogError( "clip is null." );
            return;
        }

        audioSource.clip = _clip;
        audioSource.Play();
    }


    protected virtual void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        Camera.main.orthographicSize = ( Screen.height / ( GlobalSetting.PPU * 2f ) ) * GlobalSetting.PPU;
    }

    protected void Change( SceneType _type ) 
    {
        DG.Tweening.DOTween.KillAll();
        SceneManager.LoadScene( _type.ToString() );
        SoundManager.Inst.AllStop();
        // StartCoroutine( ChangeAsync( _type ) ); 
    }

    private IEnumerator ChangeAsync( SceneType _type )
    {
        var acyncOper = SceneManager.LoadSceneAsync( _type.ToString() );

        while ( !acyncOper.isDone ) yield return null;

    }
}
