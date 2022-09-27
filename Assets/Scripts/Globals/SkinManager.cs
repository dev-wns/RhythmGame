using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum NoteSkinType { Default, Aqua, Count, }

public class SkinManager : Singleton<SkinManager>
{
    [Serializable]
    public struct NoteSkin
    {
        public NoteSkinType type;
        public NoteSkinParts left, center, right;
    }

    [Serializable]
    public struct NoteSkinParts
    {
        public Sprite normal, head, body, tail;
    }

    [SerializeField]
    public List<NoteSkin> NoteSkins = new List<NoteSkin>();
    public static NoteSkin CurrentNoteSkin;

    private static bool isOnce = false;

    protected override void Awake()
    {
        base.Awake();
        if ( !isOnce ) CurrentNoteSkin = NoteSkins[0];
        isOnce = true;
    }
}
