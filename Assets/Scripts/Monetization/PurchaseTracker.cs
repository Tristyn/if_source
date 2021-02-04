using System;
using System.Collections.Generic;

public class TrackablePurchase
{
    public string orderId;
    [NonSerialized]
    public PlayfabPurchaser purchaser;
    public bool isAppliedToGame;
    public bool isProfileBound;
    public DateTime trackingStartDateUTC;
}

public class PurchaseTracker : Singleton<PurchaseTracker>
{
    public struct Save
    {
        public List<TrackablePurchase> trackablePurchases;
    }

    public struct Profile
    {
        public List<TrackablePurchase> trackablePurchases;
    }

    List<TrackablePurchase> trackablePurchases = new List<TrackablePurchase>();

    public void GetSave(out Save save)
    {
        save.trackablePurchases = new List<TrackablePurchase>();
        for (int i = 0, len = trackablePurchases.Count; i < len; ++i)
        {
            TrackablePurchase trackablePurchase = trackablePurchases[i];
            if (!trackablePurchase.isProfileBound)
            {
                save.trackablePurchases.Add(trackablePurchase);
            }
        }
    }

    public void SetSave(in Save save)
    {
        for (int len = trackablePurchases.Count, i = len - 1; i >= 0; --i)
        {
            TrackablePurchase trackablePurchase = trackablePurchases[i];
            if (!trackablePurchase.isProfileBound)
            {
                trackablePurchases.RemoveAtSwapBack(i);
            }
        }

        trackablePurchases.AddList(save.trackablePurchases);
    }

    public void GetProfile(out Profile profile)
    {
        profile.trackablePurchases = new List<TrackablePurchase>();

        for (int i = 0, len = trackablePurchases.Count; i < len; ++i)
        {
            TrackablePurchase trackablePurchase = trackablePurchases[i];
            if (trackablePurchase.isProfileBound)
            {
                profile.trackablePurchases.Add(trackablePurchase);
            }
        }
    }

    public void SetProfile(in Profile profile)
    {
        for (int len = trackablePurchases.Count, i = len - 1; i >= 0; --i)
        {
            TrackablePurchase trackablePurchase = trackablePurchases[i];
            if (trackablePurchase.isProfileBound)
            {
                trackablePurchases.RemoveAtSwapBack(i);
            }
        }

        trackablePurchases.AddList(profile.trackablePurchases);
    }

    public void Add(PlayfabPurchaser purchaser)
    {
        bool isProfileBound = false;
        for (int i = 0, len = purchaser.catalogItemInfos.Length; i < len; ++i)
        {
            if (purchaser.catalogItemInfos[i].isProfileBound)
            {
                isProfileBound = true;
                break;
            }
        }

        trackablePurchases.Add(new TrackablePurchase
        {
            orderId = purchaser.startPurchaseResult.OrderId,
            isProfileBound = isProfileBound,
            purchaser = purchaser,
            trackingStartDateUTC = DateTime.UtcNow
        });
    }

    public void Remove(string orderId)
    {
        for (int len = trackablePurchases.Count, i = len - 1; i >= 0; --i)
        {
            if (trackablePurchases[i].orderId == orderId)
            {
                trackablePurchases.RemoveAtSwapBack(i);
            }
        }
    }

    public List<TrackablePurchase> TryGetAll()
    {
        if (trackablePurchases.Count > 0)
        {
            List<TrackablePurchase> list = new List<TrackablePurchase>();
            list.AddList(trackablePurchases);
            return list;
        }
        return null;
    }

    public List<TrackablePurchase> TryGetConfirmableThisSession()
    {
        List<TrackablePurchase> list = null;

        for (int len = trackablePurchases.Count, i = len - 1; i >= 0; --i)
        {
            var trackablePurchase = trackablePurchases[i];
            if (trackablePurchase.purchaser != null && trackablePurchase.purchaser.CanConfirmPurchase())
            {
                if (list == null)
                {
                    list = new List<TrackablePurchase>();
                }
                list.Add(trackablePurchase);
            }
        }
        return list;
    }
}