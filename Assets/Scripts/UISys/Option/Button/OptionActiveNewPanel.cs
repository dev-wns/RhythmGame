using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionActiveNewPanel : OptionButton
{
    public GameObject newOptionCanvas;
    public GameObject newOptionPanel;
    public bool IsEnabled = true;

    public override void Process()
    {
        newOptionCanvas.SetActive( IsEnabled );
        newOptionPanel.SetActive( IsEnabled );
    }
}
