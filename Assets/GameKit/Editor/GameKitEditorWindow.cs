using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Beetle23
{
    public class GameKitEditorWindow : EditorWindow
    {
        [MenuItem("Window/GameKit Editor Window")]
        private static void OpenVirutalItemEditorWindow()
        {
            GameKitEditorWindow.GetInstance();
        }

        public static GameKitEditorWindow GetInstance()
        {
            if (_instance == null)
            {
                _instance = EditorWindow.GetWindow<GameKitEditorWindow>("Virtual Item Edit Window");
            }
            return _instance;
        }

        private static GameKitEditorWindow _instance;

        private void OnEnable()
        {
            GetConfigAndCreateIfNonExist();

            if (_config == null)
            {
                _config = GameKit.Config;
            }
            if (_itemsExplorer == null)
            {
                _itemsExplorer = new VirtualItemsTreeExplorer(_config);
            }
            if (_itemInspector == null)
            {
                _itemInspector = new VirtualItemsPropertyInspector(_itemsExplorer.CurrentSelectedItem);
                _itemsExplorer.OnSelectionChange += _itemInspector.OnExplorerSelectionChange;
            }

            VirtualItemsEditUtil.UpdateDisplayedOptions();
        }

        private void OnDisable()
        {
            if (_itemInspector != null)
            {
                _itemsExplorer.OnSelectionChange -= _itemInspector.OnExplorerSelectionChange;
            }
        }

        private void OnFocus()
        {
            if (_config != null)
            {
                _config.RemoveNullRefs();
                _config.UpdateIdToCategoryMap();
                _config.UpdateIdToItemMap();
            }
            VirtualItemsEditUtil.UpdateDisplayedOptions();
        }

        private static GameKitConfig GetConfigAndCreateIfNonExist()
        {
            string configFilePath = VirtualItemsEditUtil.DefaultVirtualItemDataPath + "/GameKitConfig.asset";
            GameKitConfig virtualItemsConfig = AssetDatabase.LoadAssetAtPath(configFilePath, typeof(GameKitConfig)) as GameKitConfig;
            if (virtualItemsConfig == null)
            {
                virtualItemsConfig = VirtualItemsEditUtil.CreateAsset<GameKitConfig>(configFilePath);
            }
            return virtualItemsConfig;
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();

            _itemsExplorer.Draw(new Rect(10, 5, 250, position.height - 10));
            if (_itemInspector != null)
            {
                _itemInspector.Draw(new Rect(270, 5, position.width - 280, position.height - 10));
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_config);
            }
        }

        private GameKitConfig _config;
        private VirtualItemsTreeExplorer _itemsExplorer;
        private VirtualItemsPropertyInspector _itemInspector;

        private const float RowHeight = 20;
    }
}