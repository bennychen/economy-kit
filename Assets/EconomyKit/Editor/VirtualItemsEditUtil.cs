﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class VirtualItemsEditUtil
{
    public static string[] DisplayedCategories { get; private set; }
    public static string[] DisplayedVirtualCurrencyIDs { get; private set; }
    public static string[] DisplayedItemIDs { get; private set; }

    public static T CreateNewVirtualItem<T>() where T : VirtualItem
    {
        T asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, "Assets/EconomyKit/Resources/NewVirtualItem.asset");
        AssetDatabase.SaveAssets();
        return asset;
    }

    public static void UpdateDisplayedOptions()
    {
        UpdateDisplayedCategories();
        UpdateDisplayedVirtualCurrencyIDs();
        UpdateDisplayedItemIDs();
    }

    public static void UpdateItemCategoryByIndex(VirtualItem item, int newCategoryIndex)
    {
        if (newCategoryIndex == 0)
        {
            item.Category = null;
        }
        else
        {
            item.Category = EconomyKit.Config.GetCategoryByID(DisplayedCategories[newCategoryIndex]);
        }
    }

    public static void UpdatePurchaseByIndex(VirtualItem item, int newPrimaryCurrencyIndex, int newSecondaryCurrencyIndex)
    {
        if (item is PurchasableItem)
        {
            PurchasableItem purchasable = item as PurchasableItem;
            if (purchasable.PrimaryPurchase.Type == PurchaseType.PurchaseWithVirtualCurrency)
            {
                purchasable.PrimaryPurchase.AssociatedID =
                    EconomyKit.Config.GetItemByID(DisplayedVirtualCurrencyIDs[newPrimaryCurrencyIndex]).ID;
            }
            if (purchasable.SecondaryPurchase.Type == PurchaseType.PurchaseWithVirtualCurrency)
            {
                purchasable.SecondaryPurchase.AssociatedID =
                    EconomyKit.Config.GetItemByID(DisplayedVirtualCurrencyIDs[newSecondaryCurrencyIndex]).ID;
            }
        }
    }

    public static void UpdateRelatedItemByIndex(VirtualItem item, int newItemIndex)
    {
        if (item is UpgradeItem)
        {
            UpgradeItem upgradeItem = item as UpgradeItem;
            upgradeItem.RelatedItemID = EconomyKit.Config.GetItemByID(DisplayedItemIDs[newItemIndex]).ID;
        }
    }

    public static void UpdatePackElementItemByIndex(PackElement element, int newItemIndex)
    {
        if (element != null)
        {
            element.ItemID = EconomyKit.Config.GetItemByID(DisplayedItemIDs[newItemIndex]).ID;
        }
    }

    public static int GetCategoryIndexById(string categoryId)
    {
        for (int i = 0; i < DisplayedCategories.Length; i++)
        {
            if (DisplayedCategories[i].Equals(categoryId))
            {
                return i;
            }
        }
        Debug.LogError("Failed to find categogy id: [" + categoryId + "]");
        return 0;
    }

    public static int GetVirtualCurrencyIndexById(string virtualCurrencyId)
    {
        for (int i = 0; i < DisplayedVirtualCurrencyIDs.Length; i++)
        {
            if (DisplayedVirtualCurrencyIDs[i].Equals(virtualCurrencyId))
            {
                return i;
            }
        }
        return 0;
    }

    public static int GetItemIndexById(string itemId)
    {
        for (int i = 0; i < DisplayedItemIDs.Length; i++)
        {
            if (DisplayedItemIDs[i].Equals(itemId))
            {
                return i;
            }
        }
        return 0;
    }

    private static void UpdateDisplayedCategories()
    {
        List<string> categoryTitles = new List<string>();
        categoryTitles.Add("None");
        foreach (var category in EconomyKit.Config.Categories)
        {
            categoryTitles.Add(category.ID);
        }
        DisplayedCategories = categoryTitles.ToArray();
    }

    private static void UpdateDisplayedVirtualCurrencyIDs()
    {
        List<string> ids = new List<string>();
        foreach (var item in EconomyKit.Config.VirtualCurrencies)
        {
            ids.Add(item.ID);
        }
        DisplayedVirtualCurrencyIDs = ids.ToArray();
    }

    private static void UpdateDisplayedItemIDs()
    {
        List<string> ids = new List<string>();
        foreach (var item in EconomyKit.Config.VirtualCurrencies)
        {
            ids.Add(item.ID);
        }
        foreach (var item in EconomyKit.Config.SingleUseItems)
        {
            ids.Add(item.ID);
        }
        foreach (var item in EconomyKit.Config.LifeTimeItems)
        {
            ids.Add(item.ID);
        }
        DisplayedItemIDs = ids.ToArray();
    }
}