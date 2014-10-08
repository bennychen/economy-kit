﻿using UnityEngine;

namespace Beetle23
{
    [System.Serializable]
    public class VirtualCurrency : VirtualItem
    {
        protected override void TakeBalance(int amount)
        {
            EconomyStorage.SetItemBalance(ID, EconomyStorage.GetItemBalance(ID) - amount);
        }

        protected override void GiveBalance(int amount)
        {
            EconomyStorage.SetItemBalance(ID, EconomyStorage.GetItemBalance(ID) + amount);
        }
    }
}
