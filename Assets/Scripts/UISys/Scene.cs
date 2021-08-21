using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Scene : MonoBehaviour
{
    protected enum SceneType { FreeStyle, Online, Collection, Ranking, Lobby }
    protected enum SfxType { BackGround, Move, Click, Return, }

    [Serializable]
    public struct SceneClips
    {
        public AudioClip background;
        public AudioClip move;
        public AudioClip click;
        public AudioClip escape;
    }


    [Header( "# Clips" )]
    public SceneClips clips;
    private AudioSource bgAudio, sfxAudio;

    protected void SfxPlay( AudioClip _clip )
    {
        if ( _clip == null )
        {
            Debug.LogError( "clip is null." );
            return;
        }

        sfxAudio.PlayOneShot( _clip );
    }

    protected virtual void Start()
    {
        bgAudio  = gameObject.AddComponent<AudioSource>();
        sfxAudio = gameObject.AddComponent<AudioSource>();

        bgAudio.loop = true;
        if ( clips.background != null )
        {
            bgAudio.clip = clips.background;
            bgAudio.Play();
        }
    }
}
