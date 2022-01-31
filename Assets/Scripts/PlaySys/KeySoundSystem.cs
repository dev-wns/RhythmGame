using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeySoundSystem : MonoBehaviour
{
    private InGame scene;
    private Lane lane;
    private List<KeySound> hitSounds = new List<KeySound>();
    private KeySound curHitSound;
    private int curIndex;

    private void Awake()
    {
        lane = GetComponent<Lane>();
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnGameStart += () => StartCoroutine( Process() );

    }

    public void AddKeySound( KeySound _sound )
    {
        hitSounds.Add( _sound );
    }

    private IEnumerator Process()
    {
        if ( hitSounds.Count > 0 )
             curHitSound = hitSounds[curIndex];

        WaitUntil waitNextSound = new WaitUntil( () => curHitSound.time <= NowPlaying.Playback );
        while ( curIndex < hitSounds.Count )
        {
            yield return waitNextSound;

            lane.InputSys.hitSound = curHitSound.name;

            if ( ++curIndex < hitSounds.Count )
                 curHitSound = hitSounds[curIndex];
        }
    }
}
