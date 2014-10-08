﻿namespace Beetle23
{
    public class VirtualItemRewardDelegate : IRewardDelegate
    {
        public void Give(Reward reward)
        {
            VirtualItem item = EconomyKit.Config.GetItemByID(reward.RelatedEntityID);
            if (item != null)
            {
                item.Give(reward.RewardNumber);
            }
            else
            {
                UnityEngine.Debug.LogWarning("Virtual item's reward item is not a virtual item.");
            }
        }
    }
}