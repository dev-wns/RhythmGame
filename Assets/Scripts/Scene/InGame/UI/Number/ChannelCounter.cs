using System.Collections;
using TMPro;
using UnityEngine;

public class ChannelCounter : MonoBehaviour
{
    [Header("Channel Counter")]
    public TextMeshProUGUI text;

    private void Awake()
    {
        text.SetText( "{0:D3}", 0 ); // capacity
        text.SetText( "{0}", 0 );
        StartCoroutine( UpdateChannel() );
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private IEnumerator UpdateChannel()
    {
        while ( true )
        {
            yield return YieldCache.WaitForSeconds( .075f );
            text.SetText( "{0}", AudioManager.Inst.ChannelsInUse );
        }
    }
}
