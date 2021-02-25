using UnityEngine;
using System.Collections;
/// <summary>
/// 物品基类
/// </summary>
public class Item {
    public int ID { get; set; }
    public string Name { get; set; }
    public ItemType Type { get; set; }
    public string Description { get; set; }
    public int Capacity { get; set; }//容量
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
    public string Sprite { get; set; }//用于后期查找UI精灵路径

    public Item() 
    {
        this.ID = -1;//表示这是一个空的物品类
    }

    public Item(int id,string name,ItemType type,string description,int capaticy,int buyPrice,int sellPrice,string sprite)
    {
        this.ID = id;
        this.Name = name;
        this.Type = type;
        this.Description = description;
        this.Capacity = capaticy;
        this.BuyPrice = buyPrice;
        this.SellPrice = sellPrice;
        this.Sprite = sprite;
    }

    /// <summary>
    /// 物品类型
    /// </summary>
    public enum ItemType
    {
        Consumable,//消耗品
        Equipment,//装备
        Weapon,//武器
    }

    //得到提示框应该显示的内容
    public virtual string GetToolTipText() 
    {
        string strItemType = "";
        switch (Type)
        {
            case ItemType.Consumable:
                strItemType = "Consumable";
                break;
            case ItemType.Equipment:
                strItemType = "Equipment";
                break;
            case ItemType.Weapon:
                strItemType = "Weapon";
                break;
        }

        string text = string.Format("<size=40><color=yellow>{0}</color>\n<color=yellow>Info：{1}</color>\n<color=red>Capacity：{2}</color>\n<color=green>Item type：{3}</color>\n<color=orange>BuyPrice$：{4}</color>\n<color=red>SellPrice$：{5}</color></size>", Name, Description, Capacity, strItemType, BuyPrice, SellPrice);
        return text;
    }
}
