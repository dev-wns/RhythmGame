using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollBase : MonoBehaviour
{
    public GameObject[] contents;

    public bool IsDuplicate { get; private set; }
    protected GameObject curOption { get; private set; }
    protected int curIndex;

    protected virtual void Awake()
    {
        SelectPosition( 0 );
    }

    protected GameObject GetContent( int _index )
    {
        if ( contents.Length <= _index ) return null;


        return contents[_index];
    }

    protected GameObject GetContent( string _name )
    {
        for ( int i = 0; i < contents.Length; i++ )
        {
            if ( Equals( contents[i].name, _name ) )
                return contents[i];
        }

        return null;
    }

    protected void SelectPosition( int _pos )
    {
        if ( contents.Length <= 0 ) return;

        curIndex = 0;
        curOption = contents[0];
    }

    protected virtual void PrevMove()
    {
        if ( curIndex == 0 )
        {
            IsDuplicate = true;
            return;
        }

        curOption = contents[--curIndex];
        IsDuplicate = false;
    }

    protected virtual void NextMove()
    {
        if ( curIndex == contents.Length - 1 )
        {
            IsDuplicate = true;
            return;
        }

        curOption = contents[++curIndex];
        IsDuplicate = false;
    }
}
