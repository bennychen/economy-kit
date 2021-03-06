﻿using UnityEngine;
using UnityEditor;
using Rotorz.ReorderableList;
using System.Collections.Generic;

namespace Codeplay
{
    public class PurchaseInfoListView
    {
        public PurchaseInfoListView(PurchasableItem item)
        {
            _listControl = new ReorderableListControl(ReorderableListFlags.DisableDuplicateCommand |
                ReorderableListFlags.ShowIndices);
            _listControl.ItemInserted += OnItemInsert;
            _listControl.ItemRemoving += OnItemRemoving;

            _purchasePopupDrawers = new List<ItemPopupDrawer>();
            UpdatePurchasePopupDrawers();
        }

        public PurchasableItem CurrentPurchasableItem
        {
            get
            {
                return _currentPurchasableItem;
            }
        }

        public void UpdateDisplayItem(PurchasableItem item)
        {
            _currentPurchasableItem = item;
            if (item != null)
            {
                _listAdaptor = new GenericClassListAdaptor<Purchase>(item.PurchaseInfo, 18,
                    CreatePurchase, DrawOnePurchase);

                UpdatePurchasePopupDrawers();
            }
        }

        public void Draw(Rect position)
        {
            GUI.BeginGroup(position, string.Empty, "Box");
            float listHeight = _listControl.CalculateListHeight(_listAdaptor);
            bool hasScrollBar = listHeight + 20 > position.height;
            _scrollPosition = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), _scrollPosition,
                new Rect(0, 0, position.width - 20, listHeight + 20));

            float xOffset = 0;
            GUI.Label(new Rect(0, 0, position.width * PurchaseTypeWidth, 20),
                "Purchase Type", GameKitEditorDrawUtil.TitleStyle);
            xOffset += position.width * PurchaseTypeWidth;
            GUI.Label(new Rect(xOffset, 0, position.width * PurchaseAssociatedWidth, 20),
                "Associated ID/Item", GameKitEditorDrawUtil.TitleStyle);
            xOffset += position.width * PurchaseAssociatedWidth;
            GUI.Label(new Rect(xOffset, 0, position.width * PurchasePriceWidth, 20),
                "Price", GameKitEditorDrawUtil.TitleStyle);

            if (_listAdaptor != null)
            {
                _listControl.Draw(new Rect(0, 20,
                    position.width - (hasScrollBar ? 10 : 0), listHeight), _listAdaptor);
            }

            GUI.EndScrollView();
            GUI.EndGroup();
        }

        private Purchase CreatePurchase()
        {
            return new Purchase();
        }

        private void OnItemRemoving(object sender, ItemRemovingEventArgs args) 
        {
            UpdatePurchasePopupDrawers();
        }

        private void OnItemInsert(object sender, ItemInsertedEventArgs args) 
        {
            UpdatePurchasePopupDrawers();
        }

        private Purchase DrawOnePurchase(Rect position, Purchase purchase, int index)
        {
            if (purchase == null) return null;

            float xOffset = position.x;
            DrawType(new Rect(xOffset, position.y, position.width * PurchaseTypeWidth - 1, position.height),
                purchase, index);
            xOffset += position.width * PurchaseTypeWidth;
            if (purchase.Type == PurchaseType.PurchaseWithMarket)
            {
                DrawMarketID(new Rect(xOffset, position.y, position.width * PurchaseAssociatedWidth - 1, position.height),
                    purchase);
            }
            else
            {
                ItemPopupDrawer drawer = index < _purchasePopupDrawers.Count ? _purchasePopupDrawers[index] : null;
                if (drawer != null)
                {
                    purchase.VirtualCurrencyID = drawer.Draw(new Rect(xOffset, position.y, 
                        position.width * PurchaseAssociatedWidth - 1, position.height), purchase.VirtualCurrencyID, GUIContent.none);
                }
            }
            xOffset += position.width * PurchaseAssociatedWidth;
            DrawPrice(new Rect(xOffset, position.y, position.width * PurchasePriceWidth - 1, position.height), purchase);

            return purchase;
        }

        private void DrawType(Rect position, Purchase purchase, int index)
        {
            purchase.Type = (PurchaseType)EditorGUI.EnumPopup(position, purchase.Type);
        }

        private void DrawMarketID(Rect position, Purchase purchase)
        {
            purchase.MarketID = EditorGUI.TextField(position, purchase.MarketID);
        }

        private void DrawPrice(Rect position, Purchase purchase)
        {
            if (purchase.IsMarketPurchase)
            {
                purchase.Price = EditorGUI.FloatField(position, purchase.Price);
            }
            else
            {
                purchase.Price = EditorGUI.IntField(position, (int)purchase.Price);
            }
        }

        private void UpdatePurchasePopupDrawers()
        {
            _purchasePopupDrawers.Clear();
            if (_currentPurchasableItem != null)
            {
                for (int i = 0; i < _currentPurchasableItem.PurchaseInfo.Count; i++)
                {
                    _purchasePopupDrawers.Add(new ItemPopupDrawer(ItemType.VirtualItem,
                        false, VirtualItemType.VirtualCurrency));
                }
            }
        }

        private ReorderableListControl _listControl;
        private GenericClassListAdaptor<Purchase> _listAdaptor;
        private PurchasableItem _currentPurchasableItem;
        private List<ItemPopupDrawer> _purchasePopupDrawers;
        private Vector2 _scrollPosition;

        private const float PurchaseTypeWidth = 0.4f;
        private const float PurchaseAssociatedWidth = 0.4f;
        private const float PurchasePriceWidth = 0.2f;
    }
}
