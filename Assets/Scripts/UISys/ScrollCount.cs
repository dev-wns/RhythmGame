using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScrollCount : MonoBehaviour
{
    public TextMeshProUGUI maxText;
    public TextMeshProUGUI curText;

    public void Initialize( int _size )
    {
        if ( maxText )
             maxText.text = _size.ToString();
    }

    public void UpdateText( int _pos )
    {
        if ( curText )
            maxText.text = _pos.ToString();
    }
}
