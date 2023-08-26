using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.TreeViewExamples;
using UnityEngine;

[Serializable]
internal class GPTTreeElement : TreeElement
{
    public string Filename;
    public string Path;

    public GPTTreeElement(string name, int depth, int id) : base(name, depth, id)
    {
        
    }
}
