using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CustomUnityProjectFolders
{
    [InitializeOnLoad]
    public class CustomUnityProjectFolders
    {
        private static CustomUnityProjectFoldersData customUnityProjectFoldersData;
        private static int secretCount;
        private static int drawnBackgroundCount;
        private static bool hasDrawnSelectionRectY0;

        /// <summary>
        /// Called when Unity editor pre-loads on save or boot of Unity.
        /// </summary>
        static CustomUnityProjectFolders()
        {
            customUnityProjectFoldersData = LoadCustomUnityProjectFolderData();
            EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGUI;
            hasDrawnSelectionRectY0 = false;
        }

        // The following method is adapted from Federico Bellucci (https://github.com/febucci/unitypackage-custom-hierarchy)
        // Used under a modified MIT license:
        // Copyright (c) 2020 Federico Bellucci - febucci.com
        // 
        // Permission is hereby granted, free of charge, to any person obtaining a copy of this software/algorithm and associated
        // documentation files (the "Software"), to use, copy, modify, merge or distribute copies of the Software, and to permit
        // persons to whom the Software is furnished to do so, subject to the following conditions:
        // 
        // - The Software, substantial portions, or any modified version be kept free of charge and cannot be sold commercially.
        // 
        // - The above copyright and this permission notice shall be included in all copies, substantial portions or modified
        // versions of the Software.
        // 
        // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
        // WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
        // COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
        // OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
        // 
        // For any other use, please ask for permission by contacting the author.
        /// <summary>
        /// A method that is called manually to load/create the Scriptable Object data.
        /// </summary>
        /// <returns></returns> Returns the loaded/created asset.
        private static CustomUnityProjectFoldersData LoadCustomUnityProjectFolderData()
        {
            var asset = (CustomUnityProjectFoldersData)AssetDatabase.LoadAssetAtPath("Assets/CustomUnityProjectFoldersData.asset", typeof(CustomUnityProjectFoldersData));
            if (asset == null)
            {
                try
                {
                    asset = ScriptableObject.CreateInstance<CustomUnityProjectFoldersData>();
                    AssetDatabase.CreateAsset(asset, "Assets/CustomUnityProjectFoldersData.asset");
                    AssetDatabase.SaveAssets();
                    Debug.Log("Didn't find a CustomUnityProjectFoldersData, so creating one.");
                }
                catch
                {
                    Debug.Log("Failed to create a CustomUnityProjectFoldersData, please manually make one and rename to 'CustomUnityProjectFoldersData'.");
                }
            }
            return asset;
        }

        /// <summary>
        /// Editor method that is called on different events like mouse hover over the project window or clicks etc.
        /// </summary>
        /// <param name="guid"></param> This param is a guid string of letters and numbers that Unity assigns each filepath in a meta file.
        /// <param name="selectionRect"></param> This param is a X Y Width Height Area of where the folder/header needs drawing.
        private static void ProjectWindowItemOnGUI(string guid, Rect selectionRect)
        {
            // Get the folder path from a string guid (mainly found in .meta files).
            string folderPath = AssetDatabase.GUIDToAssetPath(guid);

            if (customUnityProjectFoldersData.alternatingBackgroundsEnabled)
                DrawAlternatingBackground(folderPath, selectionRect);
            if (customUnityProjectFoldersData.treeViewEnabled)
                DrawTreeView(folderPath, selectionRect);
            DrawHeader(folderPath, selectionRect);
            DrawFolder(folderPath, selectionRect);
        }

        #region DrawAlternatingBackground
        /// <summary>
        /// Draws a Alternating Background only if the header is disabled for that Rect area.
        /// </summary>
        /// <param name="folderPath"></param> As name states, folderPath passed through from loading it from the guid.
        /// <param name="selectionRect"></param> This param is a X Y Width Height Area of where the folder/header needs drawing.
        private static void DrawAlternatingBackground(string folderPath, Rect selectionRect)
        {
            // Find in the FolderCache the first folderData that has this path to draw.
            CustomUnityProjectFoldersData.FolderCache folderData = customUnityProjectFoldersData.folderCache.FirstOrDefault(x => AssetDatabase.GetAssetPath(x.folderToCustomise) == folderPath);
            
            // If the folderData is not equal to null and the header is enabled don't draw because we will only redraw the header again over the top (save performance).
            if (folderData != null && folderData.headerEnabled)
            {
                return;
            }

            // Get the colors to use from the FolderData.
            Color firstColor = customUnityProjectFoldersData.alternatingBackgroundsFirstColor;
            Color secondColor = customUnityProjectFoldersData.alternatingBackgroundsSecondColor;

            // Hard code the alpha as I draw ontop and cannot draw behind the pre-created default Unity text.
            firstColor.a = secondColor.a = 0.3f;

            // If the selection rect height is 16 (tree view size on the left when in two column mode) then draw.
            if (selectionRect.height == 16)
            {
                // If I have drawn the assets folder reset the count so we know not to draw over again and re-draw instead prevented some bugs.
                if (hasDrawnSelectionRectY0 && selectionRect.y == 0)
                {
                    drawnBackgroundCount = 0;
                }
                if (selectionRect.y == 0)
                {
                    hasDrawnSelectionRectY0 = true;
                }
                DrawRect(selectionRect, drawnBackgroundCount % 2 == 0 ? firstColor : secondColor);
                drawnBackgroundCount++;
            }
        }
        #endregion

        #region DrawTreeView
        /// <summary>
        /// Draws the tree view in the project window left column area.
        /// </summary>
        /// <param name="folderPath"></param> As name states, folderPath passed through from loading it from the guid.
        /// <param name="selectionRect"></param> This param is a X Y Width Height Area of where the folder/header needs drawing.
        private static void DrawTreeView(string folderPath, Rect selectionRect)
        {
            // Confirms it is the left column we currently have the selectionRect of.
            if (selectionRect.height != 16)
            {
                return;
            }

            // This handles an empty guid/folderPath and draws for them.
            if (folderPath == null || folderPath == string.Empty)
            {
                if (selectionRect.y is not 16 and not 32 and not 48)
                {
                    return;
                }
                DrawRect(new(selectionRect.x - 8 - 14, selectionRect.y, 2, 16), customUnityProjectFoldersData.treeViewSubBranchColor); // Blue line down.
                DrawRect(new(selectionRect.x - 8, selectionRect.y, 2, 16), customUnityProjectFoldersData.treeViewMainBranchColor); // Green line down.
                DrawRect(new(selectionRect.x - 6, selectionRect.y + 7, 6, 2), customUnityProjectFoldersData.treeViewMainBranchColor); // Green line across.
                return;
            }

            // Check if we have sub directories dont draw the line across 
            if (Directory.GetDirectories(folderPath).Length == 0)
            {
                DrawRect(new(selectionRect.x - 8, selectionRect.y, 2, 16), customUnityProjectFoldersData.treeViewMainBranchColor); // Green line down.
                DrawRect(new(selectionRect.x - 6, selectionRect.y + 7, 6, 2), customUnityProjectFoldersData.treeViewMainBranchColor); // Green line across.
            }

            // Get the count of how many parents this has by using the path and counting how many / is in it for example Assets/_Scripts/Player would be 2 parents.
            int count = folderPath.Split("/").Length;
            for (int i = 1; i < count; i++)
            {
                DrawRect(new(selectionRect.x - 8 - (i * 14), selectionRect.y, 2, 16), customUnityProjectFoldersData.treeViewSubBranchColor); // Blue line across.
            }
        }
        #endregion

        #region DrawHeader
        /// <summary>
        /// Draws the header for the text over the top of the alternating background.
        /// </summary>
        /// <param name="folderPath"></param> As name states, folderPath passed through from loading it from the guid.
        /// <param name="selectionRect"></param> This param is a X Y Width Height Area of where the folder/header needs drawing.
        private static void DrawHeader(string folderPath, Rect selectionRect)
        {
            // Get the folder item data the same way as earlier.
            CustomUnityProjectFoldersData.FolderCache folderItemData = customUnityProjectFoldersData.folderCache.FirstOrDefault(x => x.foldersToDraw.Contains(folderPath));

            if (folderItemData == null || !folderItemData.headerEnabled)
            {
                return;
            }

            // We know we have the original folder (and we haven't grabbed it as recursion is enabled somewhere) so check if recursion is enabled for the header if not return
            if (AssetDatabase.GetAssetPath(folderItemData.folderToCustomise) != folderPath && !folderItemData.doesRecursionEffectThis)
            {
                return;
            }

            // Set the colour based on the folder item data and hard code the alpha.
            Color color = folderItemData.headerColor;
            color.a = 0.3f;
            if (selectionRect.height == 16)
            {
                DrawRect(selectionRect, color);
            }
        }
        #endregion

        #region DrawFolder
        /// <summary>
        /// Draws the folder icon over the top of the old folder icon (can't replace it as Unity doesn't support this)
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="selectionRect"></param>
        private static void DrawFolder(string folderPath, Rect selectionRect)
        {
            if (folderPath == null || folderPath == string.Empty || !AssetDatabase.IsValidFolder(folderPath))
            {
                if (selectionRect.y > 48 && selectionRect.height == 16 && customUnityProjectFoldersData.newFolderIcon != null)
                {
                    DrawTexture(new(selectionRect.x, selectionRect.y, 16, 16), customUnityProjectFoldersData.newFolderIcon);
                }
                return;
            }

            // If we can't get it so its null check if we are recursive setting it somewhere else.
            CustomUnityProjectFoldersData.FolderCache folderItemData = customUnityProjectFoldersData.folderCache.FirstOrDefault(x => x.foldersToDraw.Contains(folderPath));

            Rect newRect;
            // Quick null check to not paint.
            if (folderItemData == null)
            {
                if (customUnityProjectFoldersData.displayDefaultFolderIcons && customUnityProjectFoldersData.newFolderIcon != null)
                {
                    Texture textureToDraw = Directory.GetFiles(folderPath).Length > 0 ?
                        customUnityProjectFoldersData.newFolderIcon : customUnityProjectFoldersData.newEmptyFolderIcon;
                    newRect = new(selectionRect.x - 3, selectionRect.y, selectionRect.width + 6, selectionRect.height - 15);
                    if (selectionRect.height == 16)
                    {
                        newRect = new(selectionRect.x, selectionRect.y, 16, 16);
                    }
                    DrawTexture(newRect, textureToDraw);
                }
                return;
            }

            // Change the rect size based on the window layout and draw the icon.
            newRect = new(selectionRect.x - 3, selectionRect.y, selectionRect.width + 6, selectionRect.height - 15);
            if (selectionRect.height == 16)
            {
                newRect = new(selectionRect.x, selectionRect.y, 16, 16);
            }

            // Set the folder icon to if the folder contains data or not
            Texture folderToDraw = Directory.GetFiles(folderPath).Length > 0 ?
                    folderItemData.newFolderIcon : folderItemData.newEmptyFolderIcon;

            // Texture to draw null check debug a warning if it is null and return.
            if (folderToDraw == null)
            {
                Debug.LogWarning($"CustomUnityProjectFolderError : Null texture at: {AssetDatabase.GetAssetPath(folderItemData.folderToCustomise)}");
                return;
            }
            DrawTexture(newRect, folderToDraw);
        }
        #endregion

        /// <summary>
        /// Custom method that points to the Unity API EditorGUI.DrawRect.
        /// </summary>
        /// <param name="rect"></param> Rect (rectangle) to use.
        /// <param name="color"></param> Color to draw at the Rect.
        private static void DrawRect(Rect rect, Color color)
        {
            EditorGUI.DrawRect(rect, color);
        }

        /// <summary>
        /// Custom method to draw a texture (folder) that points to the Unity API GUI.DrawTexture.
        /// </summary>
        /// <param name="rect"></param> Rect (rectangle) to use.
        /// <param name="texture"></param> Texture to draw.
        private static void DrawTexture(Rect rect, Texture texture)
        {
            DrawRect(rect, new Color32(51, 51, 51, 255));
            GUI.DrawTexture(rect, texture);
        }
    }
}