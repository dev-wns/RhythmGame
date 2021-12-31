using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSelector : SingletonUnity<SoundSelector>
{
    private FileSensor sensor = new FileSensor();


    public void ReLoad()
    {
        sensor.ReLoad();
    }

    private void OnApplicationQuit()
    {
        sensor.Dispose();
    }
}
