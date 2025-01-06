using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChannelCounter : MonoBehaviour
{
    [Header("Channel Counter")]
    public TextMeshProUGUI text;

    private void Awake()
    {
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
            text.text = $"{AudioManager.Inst.ChannelsInUse}";
        }
    }
}
