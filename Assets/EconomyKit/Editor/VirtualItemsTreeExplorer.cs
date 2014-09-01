﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Rotorz.ReorderableList;
using System;

public class VirtualItemsTreeExplorer
{
    public Action<object> OnSelectionChange = delegate { };
    public object CurrentSelectedItem { get; private set; }

    public VirtualItemsTreeExplorer(VirtualItemsConfig config)
    {
        _config = config;

        _virtualCurrencyListAdaptor = CreateVirtualItemListAdaptor<VirtualCurrency>(config.VirtualCurrencies);
        _virtualCurrencyListControl = new ReorderableListControl();
        _virtualCurrencyListControl.ItemRemoving += OnItemRemoving<VirtualCurrency>;
        _virtualCurrencyListControl.ItemInserted += OnItemInsert<VirtualCurrency>;

        _singleuseItemListAdaptor = CreateVirtualItemListAdaptor<SingleUseItem>(config.SingleUseItems);
        _singleuseItemListControl = new ReorderableListControl();
        _singleuseItemListControl.ItemRemoving += OnItemRemoving<SingleUseItem>;
        _singleuseItemListControl.ItemInserted += OnItemInsert<SingleUseItem>;

        _lifetimeItemListAdaptor = CreateVirtualItemListAdaptor<LifeTimeItem>(config.LifeTimeItems);
        _lifetimeItemListControl = new ReorderableListControl();
        _lifetimeItemListControl.ItemRemoving += OnItemRemoving<LifeTimeItem>;
        _lifetimeItemListControl.ItemInserted += OnItemInsert<LifeTimeItem>;

        _packListAdaptor = CreateVirtualItemListAdaptor<VirtualItemPack>(config.ItemPacks);
        _packListControl = new ReorderableListControl();
        _packListControl.ItemRemoving += OnItemRemoving<VirtualItemPack>;
        _packListControl.ItemInserted += OnItemInsert<VirtualItemPack>;

        _categoryListAdaptor = CreateVirtualCategoryListAdaptor(config.Categories);
        _categoryListControl = new ReorderableListControl();
        _categoryListControl.ItemInserted += OnItemInsert<VirtualCategory>;
        _categoryListControl.ItemRemoving += OnItemRemoving<VirtualCategory>;
    }

    public void Draw(Rect position)
    {
        GUILayout.BeginArea(position, string.Empty, "Box");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+Expand All", GUILayout.Width(90)))
        {
            _isVirtualCurrencyExpanded = true;
            _isSingleUseItemExpanded = true;
            _isLifeTimeItemExpanded = true;
            _isPackExpanded = true;
            _isCategoryExpanded = true;
        }
        if (GUILayout.Button("-Collapse All", GUILayout.Width(90)))
        {
            _isVirtualCurrencyExpanded = false;
            _isSingleUseItemExpanded = false;
            _isLifeTimeItemExpanded = false;
            _isPackExpanded = false;
            _isCategoryExpanded = false;
        }
        GUILayout.EndHorizontal();

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        EditorGUI.BeginChangeCheck();

        _isVirtualCurrencyExpanded = EditorGUILayout.Foldout(_isVirtualCurrencyExpanded, "Virtual Currencies");
        if (_isVirtualCurrencyExpanded)
        {
            _virtualCurrencyListControl.Draw(_virtualCurrencyListAdaptor);
        }
        _isSingleUseItemExpanded = EditorGUILayout.Foldout(_isSingleUseItemExpanded, "Single Use Items");
        if (_isSingleUseItemExpanded)
        {
            _singleuseItemListControl.Draw(_singleuseItemListAdaptor);
        }
        _isLifeTimeItemExpanded = EditorGUILayout.Foldout(_isLifeTimeItemExpanded, "Lifetime Items");
        if (_isLifeTimeItemExpanded)
        {
            _lifetimeItemListControl.Draw(_lifetimeItemListAdaptor);
        }
        _isPackExpanded = EditorGUILayout.Foldout(_isPackExpanded, "Packs");
        if (_isPackExpanded)
        {
            _packListControl.Draw(_packListAdaptor);
        }
        _isCategoryExpanded = EditorGUILayout.Foldout(_isCategoryExpanded, "Categories");
        if (_isCategoryExpanded)
        {
            _categoryListControl.Draw(_categoryListAdaptor);
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(_config);
        }

        GUILayout.Space(30);

        GUILayout.EndScrollView();

        GUILayout.EndArea();
    }

    private T DrawItem<T>(Rect position, T item, int index) where T : class
    {
        string id = string.Empty;
        if (item is VirtualItem)
        {
            id = (item as VirtualItem).ID;
        }
        else if (item is VirtualCategory)
        {
            id = (item as VirtualCategory).ID;
        }
        else
        {
            return item;
        }

        GUIStyle labelStyle = GUI.skin.GetStyle("Label");
        labelStyle.richText = true;
        string caption = Tab + id;
        if (item == CurrentSelectedItem)
        {
            caption = "<color=red>" + caption + "</color>";
        }
        if (GUI.Button(position, caption, "Label"))
        {
            SelectItem(item);
        }
        labelStyle.richText = false;
        return item;
    }

    private void OnItemInsert<T>(object sender, ItemInsertedEventArgs args) where T : class
    {
        GenericClassListAdaptor<T> listAdaptor = args.adaptor as GenericClassListAdaptor<T>;
        if (listAdaptor != null)
        {
            VirtualItem item = listAdaptor[args.itemIndex] as VirtualItem;
            if (item != null)
            {
                item.ID = item.name;
            }
            else
            {
                VirtualCategory category = listAdaptor[args.itemIndex] as VirtualCategory;
                if (category != null)
                {
                    category.ID = "New Category";
                }
            }
        }
    }

    private void OnItemRemoving<T>(object sender, ItemRemovingEventArgs args) where T : class
    {
        GenericClassListAdaptor<T> listAdaptor = args.adaptor as GenericClassListAdaptor<T>;
        T item = listAdaptor[args.itemIndex];
        if (listAdaptor != null)
        {
            if (item is ScriptableObject)
            {
                ScriptableObject assetItem = item as ScriptableObject;
                if (EditorUtility.DisplayDialog("Confirm to delete",
                        "Confirm to delete asset [" + assetItem.name + ".asset]?", "OK", "Cancel"))
                {
                    args.Cancel = false;
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(assetItem));
                }
                else
                {
                    args.Cancel = true;
                }
            }
            else if (item is VirtualCategory)
            {
                VirtualCategory category = item as VirtualCategory;
                if (EditorUtility.DisplayDialog("Confirm to delete",
                        "Confirm to delete category [" + category.ID + "]?", "OK", "Cancel"))
                {
                    args.Cancel = false;
                }
                else
                {
                    args.Cancel = true;
                }
            }
        }
    }

    private GenericClassListAdaptor<T> CreateVirtualItemListAdaptor<T>(List<T> items) where T : VirtualItem
    {
        return new GenericClassListAdaptor<T>(items, 15,
            () =>
            {
                return VirtualItemsEditUtil.CreateNewVirtualItem<T>();
            }, DrawItem<T>);
    }

    private GenericClassListAdaptor<VirtualCategory> CreateVirtualCategoryListAdaptor(List<VirtualCategory> categories)
    {
        return new GenericClassListAdaptor<VirtualCategory>(categories, 15,
                                () =>
                                {
                                    return new VirtualCategory();
                                }, DrawItem<VirtualCategory>);
    }

    private void SelectItem(object item)
    {
        if (item != CurrentSelectedItem)
        {
            CurrentSelectedItem = item;
            OnSelectionChange(item);
        }
    }

    private VirtualItemsConfig _config;
    private bool _isVirtualCurrencyExpanded;
    private bool _isSingleUseItemExpanded;
    private bool _isLifeTimeItemExpanded;
    private bool _isPackExpanded;
    private bool _isCategoryExpanded;

    private ReorderableListControl _virtualCurrencyListControl;
    private GenericClassListAdaptor<VirtualCurrency> _virtualCurrencyListAdaptor;
    private ReorderableListControl _singleuseItemListControl;
    private GenericClassListAdaptor<SingleUseItem> _singleuseItemListAdaptor;
    private ReorderableListControl _lifetimeItemListControl;
    private GenericClassListAdaptor<LifeTimeItem> _lifetimeItemListAdaptor;
    private ReorderableListControl _packListControl;
    private GenericClassListAdaptor<VirtualItemPack> _packListAdaptor;
    private ReorderableListControl _categoryListControl;
    private GenericClassListAdaptor<VirtualCategory> _categoryListAdaptor;

    private const string Tab = "        ";
    private Vector2 _scrollPosition;
}