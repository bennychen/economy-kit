using UnityEngine;
using System.Collections;

namespace Codeplay
{
    public class PurchasableGateDelegate : GateDelegate
    {
        public PurchasableGateDelegate(Gate gate)
            : base(gate)
        {
            _lifetimeItem = gate.RelatedItem as LifeTimeItem;

            if (_lifetimeItem != null)
            {
                if (!_context.IsOpened)
                {
                    _lifetimeItem.OnPurchased += OnPurchasedItem;
                }
            }
            else
            {
                Debug.LogError("Purchasable gate [" + gate.ID + "] isn't connected with a purchasable lifetime item!!!");
            }
        }

        public override IItem GetRelatedItem(string itemID)
        {
            return GameKit.Config.GetVirtualItemByID(itemID);
        }

        public override bool IsOpened
        {
            get
            {
                return _lifetimeItem != null && _lifetimeItem.Owned;
            }
        }

        public override void UnregisterEvents()
        {
            _lifetimeItem.OnPurchased -= OnPurchasedItem;
        }

        public override void RegisterEvents()
        {
            _lifetimeItem.OnPurchased += OnPurchasedItem;
        }

        private void OnPurchasedItem()
        {
            _context.OnOpened();
        }

        private LifeTimeItem _lifetimeItem;
    }
}

