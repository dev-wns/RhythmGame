using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class Scene : MonoBehaviour
{

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

    protected KeyActions keyAction = new KeyActions();
    private static KeyActions DefaultKeyAction = new KeyActions();

    public void InputLock() => keyAction = DefaultKeyAction;

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

        KeyBind();
    }

    protected virtual void Update() => keyAction.ActionCheck();

    protected abstract void KeyBind();
}
