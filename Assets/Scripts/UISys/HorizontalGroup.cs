using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[DisallowMultipleComponent]
[ExecuteAlways]
public class HorizontalGroup : MonoBehaviour
{
    [Serializable]
    public struct Padding
    {
        public int left;
        public int right;
        public int top;
        public int bottom;
    }

    public List<Transform> objs = new List<Transform>();
    public Padding padding;
    public int spacing;
    public TextAnchor childAlignment;

    private void Update()
    {
        if ( !Application.isPlaying )
        {
            objs.Clear();
            var children = GetComponentsInChildren<Transform>();
            if ( children.Length > 1 )
            {
                // 추가
                for ( int i = 1; i < children.Length; i++ )
                {
                    objs.Add( children[i] );
                }

                // child 정렬

            }
        }
    }
}
