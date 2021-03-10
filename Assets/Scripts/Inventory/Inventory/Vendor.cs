using UnityEngine;
using System.Collections;

public class Vendor : Inventory {

    //单例模式
    private static Vendor _instance;
    public static Vendor Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("VendorPanel").GetComponent<Vendor>();
            }
            return _instance;
        }
    }

    private HeroStatus player;//对主角Player脚本的引用，用于购买物品功能

    public override void Start()
    {
        base.Start();
        InitShop();
        player = GameObject.FindWithTag("Player").GetComponent<HeroStatus>();
        //Hide();
    }

    //初始化商贩
    private void InitShop() 
    {
        for(int i = 1; i < 8; i++)
        {
            StoreItem(i);
        }
    }

    //主角购买物品
    public void BuyItem(Item item) 
    {
        bool isSusscess = player.ConsumeCoin(item.BuyPrice);//主角消耗金币购买物品
        if (isSusscess)
        {
            if (item.Type == Item.ItemType.Consumable) Knapscak.Instance.StoreItem(item);
            else CharacterPanel.Instance.PutOn(item);
        }
    }

    //主角售卖物品
    public void SellItem(Item item) 
    {
        player.EarnCoin(item.SellPrice);//主角赚取到售卖物品的金币
        InventoryManager.Instance.ReduceAmountItem(1);//鼠标上的物品减少或者销毁
    }
}
