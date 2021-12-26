using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Scene : MonoBehaviour, IKeyBind
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

    private KeyActions keyAction = new KeyActions();


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

    public void InputLock( bool _isLock ) => keyAction.IsLock = _isLock;

    public void ChangeKeyAction( SceneAction _type ) => keyAction.ChangeAction( _type );

    public void KeyBind( SceneAction _type, StaticSceneKeyAction _action ) => keyAction.Bind( _type, _action );

    protected virtual void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        Camera.main.orthographicSize = ( Screen.height / ( GlobalSetting.PPU * 2f ) ) * GlobalSetting.PPU;
        KeyBind();
    }

    protected virtual void Update() => keyAction.ActionCheck();

    public abstract void KeyBind();
}
