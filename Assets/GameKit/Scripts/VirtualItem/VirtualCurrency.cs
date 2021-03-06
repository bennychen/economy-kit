using UnityEngine;

namespace Codeplay
{
    [System.Serializable]
    public class VirtualCurrency : VirtualItem
    {
        protected override void DoTake(int amount)
        {
            VirtualItemStorage.SetItemBalance(ID, VirtualItemStorage.GetItemBalance(ID) - amount);
        }

        protected override void DoGive(int amount)
        {
            VirtualItemStorage.SetItemBalance(ID, VirtualItemStorage.GetItemBalance(ID) + amount);
        }
    }
}
