using System.Collections;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

public class DebugUI : MonoBehaviour
{
    [Header( "FPS" )]
    [SerializeField] private TextMeshProUGUI main;
    //[SerializeField] private TextMeshProUGUI loop;
    //[SerializeField] private TextMeshProUGUI logic;

    //private FrameCounter loopCounter;
    //private FrameCounter logicCounter;

    private float  mainDelta;
    private double loopDelta;
    private double logicDelta;

    private void Awake()
    {
        //loopCounter  = NowPlaying.Inst.LoopFps;
        //logicCounter = NowPlaying.Inst.LogicFps;
        StartCoroutine( UpdateFPS() );
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private IEnumerator UpdateFPS()
    {
        while ( true )
        {
            yield return YieldCache.WaitForSeconds( .1f );

            main.SetText( "{0}", ( int ) ( 1f / Time.deltaTime ) );

            //if ( loopCounter != null )
            //{
            //    loop.SetText( "{0}", loopCounter.FPS );
            //}

            //if ( logicCounter != null )
            //{
            //    logic.SetText( "{0}", logicCounter.FPS );
            //}
        }
    }
}
