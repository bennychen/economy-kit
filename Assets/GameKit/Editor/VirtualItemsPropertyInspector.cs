using UnityEngine;
using UnityEditor;
using Rotorz.ReorderableList;
using System.Collections.Generic;

namespace Codeplay
{
    public class UpgradeItemListAdaptor : GenericClassListAdaptor<UpgradeItem>
    {
        public UpgradeItemListAdaptor(List<UpgradeItem> list, float itemHeight,
            GenericListAdaptorDelegate.ItemCreator<UpgradeItem> itemCreator,
            GenericListAdaptorDelegate.ClassItemDrawer<UpgradeItem> itemDrawer)
            : base(list, itemHeight, itemCreator, itemDrawer, null)
        {
        }

        public override bool CanDrag(int index)
        {
            return false;
        }

        public override bool CanRemove(int index)
        {
            return index == Count - 1;
        }
    }

    public class VirtualItemsPropertyInspector : ItemPropertyInspector
    {
        public VirtualItemsPropertyInspector(VirtualItemsTreeExplorer treeExplorer)
            : base(treeExplorer)
        {
            _purchaseListView = new PurchaseInfoListView(_currentDisplayItem as PurchasableItem);
            _packListView = new PackInfoListView(_currentDisplayItem as VirtualItemPack);
            _categoryPropertyView = new CategoryPropertyView(this, _currentDisplayItem as VirtualCategory);

            _upgradeListControl = new ReorderableListControl(ReorderableListFlags.DisableDuplicateCommand);
            _upgradeListControl.ItemInserted += OnInsertUpgradeItem;
            _upgradeListControl.ItemRemoving += OnRemoveUpgradeItem;
        }

        public override IItem[] GetAffectedItems(string itemID)
        {
            List<IItem> items = new List<IItem>();

			//check item id in upgradeItem
			foreach (var item in GameKit.Config.Upgrades)
			{
				if (item.RelatedItemID.Equals(itemID))
				{
					items.Add(item);
				}
			}

            // check virtual currency id in purchase
            foreach (var item in GameKit.Config.VirtualItems)
            {
                if (item is PurchasableItem)
                {
                    PurchasableItem purchasableItem = item as PurchasableItem;
                    foreach (var purchase in purchasableItem.PurchaseInfo)
                    {
                        if (!purchase.IsMarketPurchase && purchase.VirtualCurrencyID.Equals(itemID))
                        {
                            items.Add(purchasableItem);
                            break;
                        }
                    }
                }
            }

            // check item id in pack
            foreach (var pack in GameKit.Config.ItemPacks)
            {
                foreach (var packElement in pack.PackElements)
                {
                    if (packElement.ItemID.Equals(itemID))
                    {
                        items.Add(pack);
                        break;
                    }
                }
            }

            // check item id in category
            foreach (var category in GameKit.Config.Categories)
            {
                foreach (var id in category.ItemIDs)
                {
                    if (id.Equals(itemID))
                    {
                        items.Add(category);
                        break;
                    }
                }
            }

            // check gates && scores & mission/rewards
            foreach (var world in GameKit.Config.Worlds)
            {
                if (world.Gate.IsGroup)
                {
					foreach (var subGateID in world.Gate.SubGatesID)
                    {
						Gate subGate = GameKit.Config.GetSubGateByID(subGateID);
						if ((subGate.Type == GateType.VirtualItemGate || subGate.Type == GateType.PurchasableGate) && 
							subGate.RelatedItemID.Equals(itemID))
                        {
							items.Add(subGate);
                        }
                    }
                }
                else if ((world.Gate.Type == GateType.VirtualItemGate || world.Gate.Type == GateType.PurchasableGate) &&
                         world.Gate.RelatedItemID.Equals(itemID))
                {
                    items.Add(world.Gate);
                }

                foreach (var score in world.Scores)
                {
                    if (score.IsVirtualItemScore &&
                        score.RelatedVirtualItemID.Equals(itemID))
                    {
                        items.Add(score);
                    }
                }

                foreach (var mission in world.Missions)
                {
                    foreach (var reward in mission.Rewards)
                    {
                        if (reward.Type == RewardType.VirtualItemReward &&
                            reward.RelatedItemID.Equals(itemID))
                        {
                            items.Add(reward);
                        }
                    }
                }
            }
            return items.ToArray();
        }

        protected override void DoOnExplorerSelectionChange(IItem item)
        {
            if (item is VirtualItem)
            {
                GUI.FocusControl(string.Empty);

                if (item is SingleUseItem || item is LifeTimeItem)
                {
					(item as VirtualItem).RefreshUpgrades();
                    _upgradeListAdaptor = new UpgradeItemListAdaptor((item as VirtualItem).Upgrades, 20,
                        () => { return new UpgradeItem(); },
                        (position, theItem, index) =>
                        {
                            var size = GUI.skin.GetStyle("label").CalcSize(new GUIContent(theItem.ID));
                            GUI.Label(new Rect(position.x, position.y, size.x, position.height), theItem.ID);
                            if (GUI.Button(new Rect(position.x + size.x + 10, position.y, 50, position.height), "Edit"))
                            {
                                _treeExplorer.SelectItem(theItem);
                            }
                            return theItem;
                        });
                }
                if (item is PurchasableItem)
                {
                    if (item is VirtualItemPack)
                    {
                        _packListView.UpdateDisplayItem(item as VirtualItemPack);
                    }
                    _purchaseListView.UpdateDisplayItem(item as PurchasableItem);
                }

                //Debug.Log(GameKitEditorWindow.GetInstance().FindVirtualItemPropertyPath(item as VirtualItem));
            }
            else if (item is VirtualCategory)
            {
                _categoryPropertyView.UpdateDisplayItem(item as VirtualCategory);
            }
        }

        protected override float DoDrawItem(Rect position, IItem item)
        {
            VirtualItem virtualItem = item as VirtualItem;
            if (virtualItem != null)
            {
                return DrawVirtualItem(position, virtualItem);
            }
            else
            {
                VirtualCategory category = item as VirtualCategory;
                if (category != null)
                {
                    GUILayout.BeginArea(new Rect(position.x, position.y + 5, position.width, position.height - 10));
                    _categoryPropertyView.Draw(position, category);
                    GUILayout.EndArea();
                    return position.height;
                }
            }
            return 0;
        }

        protected override IItem GetItemWithConflictingID(IItem item, string id)
        {
            if (item is VirtualItem)
            {
                return GameKit.Config.GetVirtualItemByID(id);
            }
            else
            {
                return GameKit.Config.GetCategoryByID(id);
            }
        }

        private float DrawVirtualItem(Rect position, VirtualItem item)
        {
            float yOffset = 0;
            float width = position.width;

            _isVirtualItemPropertiesExpanded = EditorGUI.Foldout(new Rect(0, 0, width, 20),
                _isVirtualItemPropertiesExpanded, "Item");
            yOffset += 20;
            if (_isVirtualItemPropertiesExpanded)
            {
                //EditorGUI.LabelField(new Rect(0, yOffset, width, 20), "Internal ID", item.InternalID);
                //yOffset += 20;
                DrawIDField(new Rect(0, yOffset, width, 20), item, !(item is UpgradeItem), true);
                yOffset += 20;
                item.Name = EditorGUI.TextField(new Rect(0, yOffset, width, 20), "Name", item.Name);
                yOffset += 20;
                item.Description = EditorGUI.TextField(new Rect(0, yOffset, width, 20), "Desription", item.Description);
                yOffset += 20;
                if (!(item is UpgradeItem))
                {
                    EditorGUI.LabelField(new Rect(0, yOffset, width, 20), "Category", item.Category == null ? "None" : item.Category.ID);
                    yOffset += 20;
                }
                item.Icon = EditorGUI.ObjectField(new Rect(0, yOffset, width, 50), "Icon", item.Icon, typeof(Texture2D), false) as Texture2D;
                yOffset += 55;
                item.Extend = EditorGUI.ObjectField(new Rect(0, yOffset, width, 20), "Extend",
                    item.Extend, typeof(ScriptableObject), false) as ScriptableObject;
                yOffset += 20;
            }

            if (item is LifeTimeItem)
            {
                yOffset += 20;
                LifeTimeItem lifetimeItem = item as LifeTimeItem;
                lifetimeItem.IsEquippable = EditorGUI.Toggle(new Rect(0, yOffset, width, 20), "Is Equippable", lifetimeItem.IsEquippable);
                yOffset += 20;
            }
            else if (item is VirtualItemPack)
            {
                yOffset += 20;
                _isPackInfoExpanded = EditorGUI.Foldout(new Rect(0, yOffset, width, 20), _isPackInfoExpanded, "Pack Info");
                yOffset += 20;
                if (_isPackInfoExpanded)
                {
                    _packListView.Draw(new Rect(0, yOffset, width, 100));
                    yOffset += 100;
                }
            }
            if (item is PurchasableItem)
            {
                yOffset += 20;
                _isPurchaseInfoExpanded = EditorGUI.Foldout(new Rect(0, yOffset, width, 20), _isPurchaseInfoExpanded, "Purchase Info");
                yOffset += 20;
                if (_isPurchaseInfoExpanded)
                {
                    _purchaseListView.Draw(new Rect(0, yOffset, width, 100));
                    yOffset += 100;
                }
            }
            if (item is SingleUseItem || item is LifeTimeItem)
            {
                yOffset += 20;
                _isUpgradeInfoExpanded = EditorGUI.Foldout(new Rect(0, yOffset, width, 20),
                    _isUpgradeInfoExpanded, "Upgrade Info (" + item.Upgrades.Count + " levels)");
                yOffset += 20;
                if (_isUpgradeInfoExpanded)
                {
                    float height = _upgradeListControl.CalculateListHeight(_upgradeListAdaptor);
                    _upgradeListControl.Draw(new Rect(0, yOffset, width, height), _upgradeListAdaptor);
                    yOffset += height;
                }
            }
            if (item is UpgradeItem)
            {
                yOffset += 20;
				VirtualItem relatedItem = (item as UpgradeItem).RelatedItem;
                EditorGUI.LabelField(new Rect(0, yOffset, 250, 20), "Related Item",
                    relatedItem == null ? "NULL" : relatedItem.ID);
                if (GUI.Button(new Rect(255, yOffset, 50, 20), "Edit"))
                {
                    _treeExplorer.SelectItem(relatedItem);
                }
                yOffset += 20;
            }
            return yOffset;
        }

        private void OnInsertUpgradeItem(object sender, ItemInsertedEventArgs args)
        {
            int upgradeIndex = args.itemIndex + 1;
            string suffix = upgradeIndex < 10 ? "00" + upgradeIndex :
                upgradeIndex < 100 ? "0" + upgradeIndex : upgradeIndex.ToString();
			GenericClassListAdaptor<UpgradeItem> listAdaptor = args.adaptor as GenericClassListAdaptor<UpgradeItem>;
			UpgradeItem upgradeItem = listAdaptor[args.itemIndex];
			upgradeItem.ID = string.Format("{0}-upgrade{1}", _currentDisplayItem.ID, suffix);
			upgradeItem.RelatedItemID = _currentDisplayItem.ID;
			GameKit.Config.Upgrades.Add(upgradeItem);
			GameKit.Config.UpdateMapsAndTree();
            (_treeExplorer as VirtualItemsTreeExplorer).OnVirtualItemUpgradesChange(_currentDisplayItem as VirtualItem);
        }

        private void OnRemoveUpgradeItem(object sender, ItemRemovingEventArgs args)
        {
            GenericClassListAdaptor<UpgradeItem> listAdaptor = args.adaptor as GenericClassListAdaptor<UpgradeItem>;
            UpgradeItem upgradeItem = listAdaptor[args.itemIndex];
            if (listAdaptor != null)
            {
                if (EditorUtility.DisplayDialog("Confirm to delete",
                        "Confirm to delete upgrade [" + upgradeItem.ID + "]?", "OK", "Cancel"))
                {
                    args.Cancel = false;
					GameKit.Config.Upgrades.Remove(upgradeItem);
					GameKit.Config.UpdateMapsAndTree();
                    (_treeExplorer as VirtualItemsTreeExplorer).OnVirtualItemUpgradesChange(_currentDisplayItem as VirtualItem);
                }
                else
                {
                    args.Cancel = true;
                }
            }
        }

        private bool _isVirtualItemPropertiesExpanded = true;
        private bool _isPackInfoExpanded = true;
        private bool _isPurchaseInfoExpanded = true;
        private bool _isUpgradeInfoExpanded = true;

        private PurchaseInfoListView _purchaseListView;
        private PackInfoListView _packListView;
        private CategoryPropertyView _categoryPropertyView;
        private ReorderableListControl _upgradeListControl;
        private UpgradeItemListAdaptor _upgradeListAdaptor;
    }
}
