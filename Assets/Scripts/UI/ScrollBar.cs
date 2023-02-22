using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent( typeof( Scrollbar ) )]
public class ScrollBar : Scrollbar
{
    private Scrollbar scrollbar;
    private float offset;

    protected override void Awake()
    {
        base.Awake();
        scrollbar = GetComponent<Scrollbar>();
    }

    public void Initialize( int _size )
    {
        offset = 1f / ( _size - 1 );
    }

    public void UpdateHandle( int _pos )
    {
        scrollbar.value = offset * _pos;
    }
}
