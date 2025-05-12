using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUnityProjectFolders
{
    [CustomEditor(typeof(CustomUnityProjectFoldersData))]
    public class CustomUnityProjectFoldersDataEditor : Editor
    {
        public VisualTreeAsset customUnityProjectFoldersDataVisualTree;
        public VisualTreeAsset customUnityProjectFoldersDataItemVisualTree;

        private CustomUnityProjectFoldersData customUnityProjectFoldersData;

        private readonly Dictionary<string, Texture> inspectorIcons = new();

        private static readonly List<string> subFolders = new();

        /// <summary>
        /// When the inspector opens the CustomUnityProjectFoldersData ScriptableObject this OnEnable fires.
        /// </summary>
        private void OnEnable()
        {
            customUnityProjectFoldersData = (CustomUnityProjectFoldersData)target;

            // For loop to pre-load all of Unity's icons 
            string[] iconsToLoad = { "d_console.infoicon", "d_console.warnicon", "d_console.erroricon" };
            foreach (string iconToLoad in iconsToLoad)
            {
                inspectorIcons.Add(iconToLoad, EditorGUIUtility.IconContent(iconToLoad).image);
            }
        }

        /// <summary>
        /// Called after OnEnable but allows us to pass through a VisualElement (fancy word for box and data).
        /// </summary>
        /// <returns></returns>
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();

            customUnityProjectFoldersDataVisualTree.CloneTree(root);

            // Tree View (main area toggles sub area)
            // This line below grabs from the root VisualTree a Toggle named "TreeMainToggle".
            Toggle treeMainToggle = root.Q<Toggle>("TreeMainToggle");
            // This grabs a sub area (another VisualElement) named "TreeSubArea".
            VisualElement treeSubArea = root.Q<VisualElement>("TreeSubArea");
            // Finally with these I have created a method below that takes in the new value of the toggle (true/false) and an area then it will show/hide it based on that value.
            treeMainToggle.RegisterValueChangedCallback(e => HideSubAreas(e.newValue, treeSubArea));
            
            // Tree View (colors update inspector icons)
            ColorField mainBranchColor = root.Q<ColorField>("MainBranchColor");
            ColorField subBranchColor = root.Q<ColorField>("SubBranchColor");
            VisualElement mainTreeIcon = root.Q<VisualElement>("MainTreeIcon");
            VisualElement subTreeIcon = root.Q<VisualElement>("SubTreeIcon");
            mainBranchColor.RegisterValueChangedCallback(e => TreeViewColorChanged(e.newValue, subTreeIcon));
            subBranchColor.RegisterValueChangedCallback(e => TreeViewColorChanged(e.newValue, mainTreeIcon));

            // Alternating Background (main area toggles sub area)
            Toggle alternatingBackgroundToggle = root.Q<Toggle>("AlternatingBackgroundMainToggle");
            VisualElement alternatingBackgroundSubArea = root.Q<VisualElement>("AlternatingBackgroundSubArea");
            alternatingBackgroundToggle.RegisterValueChangedCallback(e => HideSubAreas(e.newValue, alternatingBackgroundSubArea));

            // Alternating Background (colors update inspector icons)
            ColorField alternatingBackgroundFirstColor = root.Q<ColorField>("AlternatingBackgroundFirstColor");
            ColorField alternatingBackgroundSecondColor = root.Q<ColorField>("AlternatingBackgroundSecondColor");
            VisualElement alternatingColorFirstIcon = root.Q<VisualElement>("AlternatingColorFirstIcon");
            VisualElement alternatingColorSecondIcon = root.Q<VisualElement>("AlternatingColorSecondIcon");
            alternatingBackgroundFirstColor.RegisterValueChangedCallback(e => AlternatingBackgroundColorChanged(e.newValue, alternatingColorFirstIcon));
            alternatingBackgroundSecondColor.RegisterValueChangedCallback(e => AlternatingBackgroundColorChanged(e.newValue, alternatingColorSecondIcon));

            // Custom Folders
            Toggle displayDefaultFolderIconsToggle = root.Q<Toggle>("DisplayDefaultFolderIconsToggle");
            VisualElement displayDefaultFolderIconsSubArea = root.Q<VisualElement>("DisplayDefaultFolderIconsSubArea");
            VisualElement displayDefaultFolderIconsToggleArea = root.Q<VisualElement>("DisplayDefaultFolderIconsToggleArea");
            displayDefaultFolderIconsToggle.RegisterValueChangedCallback(e =>
            {
                displayDefaultFolderIconsToggleArea.style.marginBottom = e.newValue == true ? 0 : 5;
                HideSubAreas(e.newValue, displayDefaultFolderIconsSubArea);
            });

            // This is what paints the Custom Folders section.
            UpdateScrollView(root);
            // A button to add another element to the Custom Folder list.
            Button addAnotherElementToList = root.Q<Button>("AddAnotherElementToList");
            // This is a work-around to prevent a bug where click events would fire for unknown reasons.
            addAnotherElementToList.clickable.activators.Clear();
            addAnotherElementToList.RegisterCallback<MouseDownEvent>(e => AddAnotherElementToList(root));

            // Disclaimer Note
            VisualElement disclaimerArea = root.Q<VisualElement>("DisclaimerArea");
            // This is a manual if the button to hide the disclaimer is pressed store the new value in the data ScriptableObject so it's remembered and update the display based on that.
            disclaimerArea.style.display = customUnityProjectFoldersData.hideDisclaimer ? DisplayStyle.None : DisplayStyle.Flex;
            if (!customUnityProjectFoldersData.hideDisclaimer)
            {
                // Hides the disclaimer button and never shows it again by telling the ScriptableObject to hideDisclaimer.
                Button disclaimerButton = root.Q<Button>("DisclaimerButton");
                disclaimerButton.clickable.activators.Clear();
                disclaimerButton.RegisterCallback<MouseDownEvent>(e =>
                {
                    customUnityProjectFoldersData.hideDisclaimer = true;
                    HideSubAreas(false, disclaimerArea);
                });
            }

            // Returns the new root VisualElement to draw as the inspector.
            return root;
        }

        /// <summary>
        /// If a color value was updated take the new value, set the tint color of the folders to be that color and repaint 
        /// the project window for faster feedback so I don't want for the Unity event to fire.
        /// </summary>
        /// <param name="newValue"></param> New Color value.
        /// <param name="visualElement"></param> VisualElement of what to change the color of.
        private void TreeViewColorChanged(Color newValue, VisualElement visualElement)
        {
            visualElement.style.unityBackgroundImageTintColor = newValue;
            EditorApplication.RepaintProjectWindow();
        }

        /// <summary>
        /// If a color value was updated set the background color of the branches and repaint. 
        /// </summary>
        /// <param name="newValue"></param> New Color value.
        /// <param name="visualElement"></param> VisualElement of what to change the color of.
        private void AlternatingBackgroundColorChanged(Color newValue, VisualElement visualElement)
        {
            visualElement.style.backgroundColor = newValue;
            EditorApplication.RepaintProjectWindow();
        }

        /// <summary>
        /// If a toggle was enabled/disabled hide the sub area.
        /// </summary>
        /// <param name="newValue"></param> True/False value
        /// <param name="visualElement"></param> VisualElement parent of what data to hide.
        private void HideSubAreas(bool newValue, VisualElement visualElement)
        {
            visualElement.style.display = newValue ? DisplayStyle.Flex : DisplayStyle.None;
            EditorApplication.RepaintProjectWindow();
        }

        /// <summary>
        /// Button to add another element to list in the Custom Folders.
        /// </summary>
        /// <param name="root"></param> Takes in root VisualElement to know where to add itself to the scrollview.
        private void AddAnotherElementToList(VisualElement root)
        {
            // Creates a new folder data with mostly null values.
            customUnityProjectFoldersData.folderCache.Add(new CustomUnityProjectFoldersData.FolderCache(null, null, null, false, Color.red, false, false, true, new List<string>()));
            // Forces an update of the ScriptableObject.
            serializedObject.Update();
            // Tells the ScriptableObject it needs to be manually saved.
            EditorUtility.SetDirty(customUnityProjectFoldersData);
            // Saves the ScriptableObject.
            AssetDatabase.SaveAssets();

            UpdateScrollView(root);
        }

        /// <summary>
        /// Updates the scroll view to display it in a custom way.
        /// </summary>
        /// <param name="root"></param> Takes in root VisualElement to know where to add itself to the scrollview.
        private void UpdateScrollView(VisualElement root)
        {
            // Gets the ScrollView reference from root.
            ScrollView customFoldersScrollView = root.Q<ScrollView>("CustomFoldersScrollView");
            // Finds the list property and caches it.
            SerializedProperty folderCacheProperty = serializedObject.FindProperty("folderCache"); 
            // Force clears the ScrollView data.
            customFoldersScrollView.Clear();

            for (int i = 0; i < customUnityProjectFoldersData.folderCache.Count; i++)
            {
                // Get the reference/data to/of the specific item in the list we are currently working with for example i = 0 so index 0.
                CustomUnityProjectFoldersData.FolderCache folderItemData = customUnityProjectFoldersData.folderCache[i];
                SerializedProperty folderItemProperty = folderCacheProperty.GetArrayElementAtIndex(i);

                // Creates a temporary new VisualElement from a VisualTreeAsset.
                VisualElement tempVisualElementItem = new();
                customUnityProjectFoldersDataItemVisualTree.CloneTree(tempVisualElementItem);

                // Gets the foldout from the temp VisualElement.
                Foldout tempFoldout = tempVisualElementItem.Q<Foldout>("Foldout");

                // Sets itself to be expanded/hidden based on it's previous value.
                SerializedProperty foldoutExpandedProperty = folderItemProperty.FindPropertyRelative("foldoutExpanded");
                // BindProperty method allows it so if the foldout is closed we can update a bool to the new value.
                tempFoldout.BindProperty(foldoutExpandedProperty);

                // Set the foldout text and value (expanded) based on the cached data.
                tempFoldout.text = folderItemData.folderToCustomise == null ? "Select a folder" : AssetDatabase.GetAssetPath(folderItemData.folderToCustomise);
                tempFoldout.value = folderItemData.foldoutExpanded;

                // Change callback to unexpand the foldout and SetDirty and repaint.
                tempFoldout.RegisterValueChangedCallback(e =>
                {
                    folderItemData.foldoutExpanded = e.newValue;
                    EditorUtility.SetDirty(customUnityProjectFoldersData);
                    EditorApplication.RepaintProjectWindow();
                });

                // Displays the folder images/error icons if there is a problem.
                // Grabs the foldoutHeader based on finding the unity-checkmark (dropdown button) and then getting its parent.
                VisualElement foldoutHeader = tempFoldout.Q<VisualElement>("unity-checkmark").parent;
                // Aligns this so I can fit more stuff in the row of the header.
                foldoutHeader.style.flexDirection = FlexDirection.Row;
                foldoutHeader.style.alignItems = Align.Center;
                foldoutHeader.style.overflow = Overflow.Hidden;

                // Adds a margin to the left as it was bugged and hidden.
                VisualElement checkmark = foldoutHeader.Q<VisualElement>("unity-checkmark");
                checkmark.style.marginLeft = 10;

                // Creates a temp spacer VisualElement to create a space between the checkmark on the left and the new data I want on the right.
                VisualElement spacer = new();
                // Makes it grow to the full available space.
                spacer.style.flexGrow = 1;
                // Adds it to the foldoutHeader.
                foldoutHeader.Add(spacer);

                // Creates a new VisualElement for the data on the right side of the header.
                VisualElement foldoutRightHeaderData = new();
                // Aligns it in case I need to draw two images.
                foldoutRightHeaderData.style.flexDirection = FlexDirection.Row;
                foldoutRightHeaderData.style.alignItems = Align.Center;
                foldoutRightHeaderData.style.overflow = Overflow.Hidden;
                foldoutRightHeaderData.style.marginRight = 5;

                DrawFolderIconsInHeader(folderItemData, foldoutRightHeaderData);

                // Adds the new right header data to the header.
                foldoutHeader.Add(foldoutRightHeaderData);

                // Folder to customise
                ObjectField folderToCustomise = tempVisualElementItem.Q<ObjectField>("FolderToCustomise");
                folderToCustomise.bindingPath = folderItemProperty.FindPropertyRelative("folderToCustomise").propertyPath;
                folderToCustomise.RegisterValueChangedCallback(e =>
                {
                    // Check to wether I should redraw the foldout header based on if the value was updated.
                    if (e.newValue != e.previousValue)
                    {
                        foldoutRightHeaderData.Clear();
                        DrawFolderIconsInHeader(folderItemData, foldoutRightHeaderData);
                    }

                    // Updates and repaints the ProjectWindow when the value changes.
                    folderItemData.foldersToDraw.Clear();
                    string folderPath = AssetDatabase.GetAssetPath(e.newValue);
                    if (!folderItemData.foldersToDraw.Contains(folderPath))
                    {
                        folderItemData.foldersToDraw.Add(folderPath);
                    }
                    EditorApplication.RepaintProjectWindow();
                });

                // Folder icons
                // Gets the ObjectField (Texture2D) and binds the path.
                ObjectField newFolderIcon = tempVisualElementItem.Q<ObjectField>("NewFolderIcon");
                newFolderIcon.bindingPath = folderItemProperty.FindPropertyRelative("newFolderIcon").propertyPath;
                ObjectField newEmptyFolderIcon = tempVisualElementItem.Q<ObjectField>("NewEmptyFolderIcon");
                newEmptyFolderIcon.bindingPath = folderItemProperty.FindPropertyRelative("newEmptyFolderIcon").propertyPath;

                // Foreach ObjectField register a callback to check if the foldout header should be updated.
                ObjectField[] folderIconObjectFields = {newFolderIcon, newEmptyFolderIcon};
                foreach(ObjectField folderIconObjectField in folderIconObjectFields)
                {
                    folderIconObjectField.RegisterValueChangedCallback(e =>
                    {
                        if (e.newValue != e.previousValue)
                        {
                            foldoutRightHeaderData.Clear();
                            DrawFolderIconsInHeader(folderItemData, foldoutRightHeaderData);
                        }
                    });
                }

                // Header
                // Binds all new paths so that if the value is updated the ScriptableObject knows it.
                VisualElement headerSubArea = tempVisualElementItem.Q<VisualElement>("HeaderSubArea");
                Toggle headerEnabledToggle = tempVisualElementItem.Q<Toggle>("HeaderEnabledToggle");
                headerEnabledToggle.bindingPath = folderItemProperty.FindPropertyRelative("headerEnabled").propertyPath;
                headerEnabledToggle.RegisterValueChangedCallback(e => HideSubAreas(e.newValue, headerSubArea));

                ColorField headerColor = tempVisualElementItem.Q<ColorField>("HeaderColor");
                headerColor.bindingPath = folderItemProperty.FindPropertyRelative("headerColor").propertyPath;
                headerColor.RegisterValueChangedCallback(e => EditorApplication.RepaintProjectWindow());
                tempVisualElementItem.Q<Toggle>("HeaderRecursionToggle").bindingPath = folderItemProperty.FindPropertyRelative("doesRecursionEffectThis").propertyPath;

                // Recursion
                Toggle recursionToggle = tempVisualElementItem.Q<Toggle>("RecursionToggle");
                recursionToggle.bindingPath = folderItemProperty.FindPropertyRelative("recursionEnabled").propertyPath;
                recursionToggle.RegisterValueChangedCallback(e =>
                {
                    // Checks if the new value is false if so clear the list of folders to draw for this object and add only itself.
                    if (!e.newValue)
                    {
                        string folderPath = AssetDatabase.GetAssetPath(folderItemData.folderToCustomise);
                        folderItemData.foldersToDraw.Clear();
                        folderItemData.foldersToDraw.Add(folderPath);
                        EditorApplication.RepaintProjectWindow();
                        return;
                    } 
                    
                    // Else e.newValue == true:
                    // If enabled set all the sub folders we need to draw for this element.
                    if (folderItemData.folderToCustomise == null)
                    {
                        return;
                    }
                    subFolders.Clear();
                    List<string> foundSubFolders = GetSubFolders(AssetDatabase.GetAssetPath(folderItemData.folderToCustomise));
                    foreach (string foundSubFolder in foundSubFolders)
                    {
                        if (folderItemData.foldersToDraw.Contains(foundSubFolder))
                        {
                            continue;
                        }
                        folderItemData.foldersToDraw.Add(foundSubFolder);
                    }
                });

                // Button to delete this specific element form the list.
                Button deleteElementInList = tempVisualElementItem.Q<Button>("DeleteElementInList");
                // Bug fix for accepting any type of click when I only want left clicks.
                deleteElementInList.clickable.activators.Clear();
                // Callback for if a MouseDownEvent fires.
                deleteElementInList.RegisterCallback<MouseDownEvent>(e =>
                {
                    // Removes this element from the list.
                    customUnityProjectFoldersData.folderCache.Remove(folderItemData);
                    // Removes this element from the ScrollView.
                    customFoldersScrollView.Clear();
                    UpdateScrollView(root);
                    // Updates and saves the ScriptableObject.
                    serializedObject.Update();
                    EditorUtility.SetDirty(customUnityProjectFoldersData);
                    AssetDatabase.SaveAssets();
                });

                // Add this new temp VisualElement to the ScrollView.
                customFoldersScrollView.Add(tempVisualElementItem);
            }
        }

        /// <summary>
        /// Draws the folder icons or a error in the header.
        /// </summary>
        /// <param name="folderItemData"></param> FolderCache for 
        /// <param name="foldoutHeader"></param>
        private void DrawFolderIconsInHeader(CustomUnityProjectFoldersData.FolderCache folderItemData, VisualElement foldoutHeader)
        {
            List<Texture> iconsToDraw = new()
                {
                    folderItemData.newFolderIcon,
                    folderItemData.newEmptyFolderIcon
                };
            bool hasDrawnError = false;

            // If the folder icon isn't set display an error.
            if (folderItemData.folderToCustomise == null)
            {
                AddFolderIcon(null, "Folder not set", foldoutHeader, 5);
                hasDrawnError = true;
            }

            // If we haven't already drawn the error draw the texture icons/error.
            if (hasDrawnError)
            {
                return;
            }
            foreach (var iconToDraw in iconsToDraw)
            {
                if (hasDrawnError && iconToDraw == null)
                {
                    return;
                }
                // Draw the texture if it isn't null or draw an error and set the tooltip to "Folder icon not set".
                AddFolderIcon(iconToDraw, iconToDraw == null ? "Folder icon not set" : string.Empty, foldoutHeader, 0);
                hasDrawnError = iconToDraw == null;
            }
        }

        /// <summary>
        /// Creates a new folder icon with different data to display.
        /// </summary>
        /// <param name="texture"></param> The texture to draw.
        /// <param name="tooltip"></param> The tooltip to write.
        /// <param name="foldoutHeader"></param> The foldout header of what to add it to.
        /// <param name="marginRightSize"></param> The margin between each texture.
        private void AddFolderIcon(Texture texture, string tooltip, VisualElement foldoutHeader, int marginRightSize)
        {
            // Create a new VisualElement with the data above.
            VisualElement folderIcon = new()
            {
                style =
                {
                    width = 16,
                    height = 16,
                    marginTop = 1,
                    marginRight = marginRightSize,
                    backgroundImage = texture != null ? (Texture2D)texture : (Texture2D)inspectorIcons["d_console.erroricon"]
                },
                tooltip = tooltip
            };
            foldoutHeader.Add(folderIcon);
        }

        /// <summary>
        /// Grabs and returns all SubFolders from a folderPath.
        /// </summary>
        private static List<string> GetSubFolders(string folderPath)
        {
            foreach (string folder in Directory.GetDirectories(folderPath))
            {
                // For some reason getting the path like this makes it have Assets\Scripts instead of the needed Assets/Scripts.
                subFolders.Add(folder.Replace("\\", "/"));
                // Recursion to get that new folders sub folders.
                GetSubFolders(folder);
            }
            return subFolders;
        }
    }
}