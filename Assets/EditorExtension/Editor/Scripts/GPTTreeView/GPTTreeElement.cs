using System;
using UnityEditor.TreeViewExamples;

namespace UniGPT.Editor
{
    [Serializable]
    internal class GPTTreeElement : TreeElement
    {
        public string Filename;
        public string Path;

        public GPTTreeElement(string name, int depth, int id) : base(name, depth, id)
        {

        }
    }
}
