using UnityEngine;
using System.Collections;
/// <summary>
/// 装备类
/// </summary>
public class Equipment : Item{
    public int Strength { get; set; }//力量
    public int Intellect { get; set; }//智力
    public int Agility { get; set; }//敏捷
    public int Stamina { get; set; }//体力
    public EquipmentType EquipType { get; set; }//装备类型

    public Equipment(int id, string name, ItemType type, ItemQuality quality, string description, int capaticy, int buyPrice, int sellPrice, string sprite,int strength, int intellect,int agility,int stamina,EquipmentType equipType) : base(id, name, type, quality, description, capaticy, buyPrice, sellPrice,sprite) 
    {
        this.Strength = strength;
        this.Intellect = intellect;
        this.Agility = agility;
        this.Stamina = stamina;
        this.EquipType = equipType;
    }

    public enum EquipmentType 
    {
        None,      //不能装备
        Head,      //头部
        Neck,      //脖子
        Ring,       //戒指
        Leg,        //腿部
        Chest,    //胸部
        Bracer,    //护腕
        Boots,     //鞋子
        Shoulder,//肩部
        Belt,       //腰带
        OffHand //副手
    }

    //对父方法Item.GetToolTipText()进行重写
    public override string GetToolTipText()
    {
        string strEquipType = "";
        switch (EquipType)
        {
            case EquipmentType.Head:
                strEquipType = "Head";
                break;
            case EquipmentType.Neck:
                strEquipType = "Neck";
                break;
            case EquipmentType.Ring:
                strEquipType = "Ring";
                break;
            case EquipmentType.Leg:
                strEquipType = "Leg";
                break;
            case EquipmentType.Chest:
                strEquipType = "Chest";
                break;
            case EquipmentType.Bracer:
                strEquipType = "Bracer";
                break;
            case EquipmentType.Boots:
                strEquipType = "Boots";
                break;
            case EquipmentType.Shoulder:
                strEquipType = "Shoulder";
                break;
            case EquipmentType.Belt:
                strEquipType = "Belt";
                break;
            case EquipmentType.OffHand:
                strEquipType = "OffHand";
                break;
        }

        string text = base.GetToolTipText();//调用父类的GetToolTipText()方法
        string newText = string.Format("{0}\n<color=green>Strength：{1}</color>\n<color=yellow>Intelligent：{2}</color>\n<color=white>agility：{3}</color>\n<color=blue>stamina：{4}</color>\n<color=red>equipType：{5}</color>", text, Strength, Intellect, Agility, Stamina, strEquipType);
        return newText;
    }
}
