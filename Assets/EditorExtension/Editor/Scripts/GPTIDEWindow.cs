using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UniGPT.Editor
{
    public class IDEWindow : EditorWindow
    {
        private static Texture2D logoImage;
        // We are using SerializeField here to make sure view state is written to the window 
        // layout file. This means that the state survives restarting Unity as long as the window
        // is not closed. If omitting the attribute then the state just survives assembly reloading 
        // (i.e. it still gets serialized/deserialized)
        [SerializeField] TreeViewState m_TreeViewState;

        // The TreeView is not serializable it should be reconstructed from the tree data.
        static GPTSimpleTreeView m_TreeView;
        SearchField m_SearchField;
        private Vector2 treeViewScrollPos;
        private Vector2 codeEditorScrollPos;

        private string codeText = "// Welcome to UniGPT Intelligent Editor.\n// To start coding, please select or create a new text file.";

        private List<TreeViewItem> treeViewItems = new List<TreeViewItem>();
        void OnEnable()
        {
            // Check if we already had a serialized view state (state 
            // that survived assembly reloading)
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();

            m_TreeView = new GPTSimpleTreeView(m_TreeViewState);
            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;
        }

        void OnGUI()
        {
            DoToolbar();
            EditorGUILayout.BeginHorizontal();
            DoTreeView();
            DoCodeEditor();
            EditorGUILayout.EndHorizontal();

            // 监听双击事件
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.clickCount == 2)
            {
                IList<int> selectedIDs = m_TreeView.GetSelection();
                if (m_TreeView.IsSelected(selectedIDs[0]))
                {

                }
            }
        }

        void DoToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Project"), EditorStyles.toolbarButton))
            {
                // 点击"Open File"按钮的事件处理逻辑
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_SaveAs"), EditorStyles.toolbarButton))
            {
                // 点击"Save"按钮的事件处理逻辑
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("RotateTool"), EditorStyles.toolbarButton))
            {
                // 点击"Refresh"按钮的事件处理逻辑
                FindScriptsAndRefresh();
            }
            GUILayout.Space(100);
            GUILayout.FlexibleSpace();
            m_TreeView.searchString = m_SearchField.OnToolbarGUI(m_TreeView.searchString);
            GUILayout.EndHorizontal();
        }

        void DoTreeView()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            EditorGUILayout.LabelField("Project", EditorStyles.boldLabel);
            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            m_TreeView.OnGUI(rect);
            EditorGUILayout.EndVertical();
        }

        void DoCodeEditor()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginScrollView(codeEditorScrollPos);

            // 在此处绘制代码文本框以及纵向Slider
            codeText = EditorGUILayout.TextArea(codeText, GUILayout.ExpandHeight(true));

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        [MenuItem("UniGPT/IDEWindow")]
        private static void OpenWindow()
        {
            IDEWindow window = GetWindow<IDEWindow>();
            window.titleContent = new GUIContent("UniGPT Intelligent Editor");
            window.Show();
        }

        private void FindScriptsAndRefresh()
        {
            // 获取工程里所有的C#脚本
            string[] guids = AssetDatabase.FindAssets("t:Monoscript");
            string[] scriptPaths = new string[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                scriptPaths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
            }

            int count = 1;
            // 使用结果按照'/'进行划分获取文件夹信息
            for (int i = 0; i < scriptPaths.Length; i++)
            {
                string[] folders = scriptPaths[i].Split('/');
                TreeViewItem parentItem = GPTSimpleTreeView.root;
                for (int j = 0; j < folders.Length; j++)
                {
                    string folderName = folders[j];
                    TreeViewItem item = FindChildItemByName(parentItem, folderName);
                    if (item == null)
                    {
                        item = new TreeViewItem(count, parentItem.depth + 1, folderName);
                        if (j < folders.Length - 1)
                            item.icon = (Texture2D)EditorGUIUtility.IconContent("Folder Icon").image;
                        else
                            item.icon = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
                        parentItem.AddChild(item);
                    }
                    parentItem = item;
                    count++;
                }
            }

            m_TreeView.Reload();
        }

        private TreeViewItem FindChildItemByName(TreeViewItem parentItem, string name)
        {
            if (parentItem != null)
            {
                if (parentItem.children == null)
                    return null;
                for (int i = 0; i < parentItem.children.Count; i++)
                {
                    TreeViewItem childItem = parentItem.children[i];
                    if (childItem.displayName == name)
                    {
                        return childItem;
                    }
                }
            }
            return null;
        }

        private void OnTreeViewItemDoubleClicked(TreeViewItem item)
        {

        }
    }
}