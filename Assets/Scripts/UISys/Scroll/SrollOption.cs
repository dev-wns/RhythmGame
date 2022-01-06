using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollOption : ScrollBase
{
    public List<GameObject> contents;

    protected GameObject curOption { get; private set; }
    protected GameObject prevOption { get; private set; }

    protected virtual void Awake()
    {
        SelectPosition( 0 );
        CreateContents();
        maxCount = contents.Count;
    }

    protected virtual void CreateContents() { }

    protected GameObject GetContent( int _index )
    {
        if ( contents.Count <= _index ) return null;


        return contents[_index];
    }

    protected GameObject GetContent( string _name )
    {
        for ( int i = 0; i < contents.Count; i++ )
        {
            if ( Equals( contents[i].name, _name ) )
                return contents[i];
        }

        return null;
    }

    protected void SelectPosition( int _pos )
    {
        if ( contents.Count <= 0 ) return;

        curIndex = 0;
        curOption = contents[0];
    }

    public override void PrevMove()
    {
        base.PrevMove();

        curOption = contents[curIndex];
        prevOption = contents[prevIndex];
    }

    public override void NextMove()
    {
        base.NextMove();

        curOption = contents[curIndex];
        prevOption = contents[prevIndex];
    }
}
