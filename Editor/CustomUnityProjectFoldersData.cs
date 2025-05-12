using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomUnityProjectFolders
{
    [CreateAssetMenu(fileName = "CustomUnityProjectFoldersData", menuName = "CustomUnityProjectFoldersData")]
    public class CustomUnityProjectFoldersData : ScriptableObject
    {
        // Tree View
        public bool treeViewEnabled = true;
        public Color treeViewMainBranchColor = Color.green;
        public Color treeViewSubBranchColor = Color.blue;

        // Alternating Backgrounds
        public bool alternatingBackgroundsEnabled = true;
        public Color alternatingBackgroundsFirstColor = Color.black;
        public Color alternatingBackgroundsSecondColor = Color.gray;

        // Custom Folders
        public bool displayDefaultFolderIcons = true;
        public Texture2D newFolderIcon;
        public Texture2D newEmptyFolderIcon;

        // Caches
        public List<FolderCache> folderCache = new();
        // Hidden Caches/Data
        public bool hideDisclaimer;

        [System.Serializable]
        public class FolderCache
        {
            public DefaultAsset folderToCustomise;
            public Texture2D newFolderIcon;
            public Texture2D newEmptyFolderIcon;
            public bool headerEnabled = true;
            public Color headerColor = Color.red;
            public bool doesRecursionEffectThis = false;
            public bool recursionEnabled = true;
            public bool foldoutExpanded = true;
            public List<string> foldersToDraw = new();

            public FolderCache(DefaultAsset folderToCustomise, Texture2D newFolderIcon, Texture2D newEmptyFolderIcon, bool headerEnabled, Color headerColor, bool doesRecursionEffectThis, bool recursionEnabled, bool foldoutExpanded, List<string> foldersToDraw)
            {
                this.folderToCustomise = folderToCustomise;
                this.newFolderIcon = newFolderIcon;
                this.newEmptyFolderIcon = newEmptyFolderIcon;
                this.headerEnabled = headerEnabled;
                this.headerColor = headerColor;
                this.doesRecursionEffectThis = doesRecursionEffectThis;
                this.recursionEnabled = recursionEnabled;
                this.foldoutExpanded = foldoutExpanded;
                this.foldersToDraw = foldersToDraw;
            }
        }
    }
}