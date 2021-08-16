using UnityEngine;

public class ScrInit : MonoBehaviour
{
    private void Awake ()
    {
        DontDestroyOnLoad( this.gameObject );
        QualitySettings.vSyncCount = 0;
    }
}
