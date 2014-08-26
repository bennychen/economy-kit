﻿using System.Collections.Generic;
using UnityEngine;

public class VirtualCategory : ScriptableObject
{
    [SerializeField]
    public string ID;

    [SerializeField]
    public List<VirtualItem> Items;

    private void OnEnable()
    {
        if (Items == null)
        {
            Items = new List<VirtualItem>();
        }
    }
}