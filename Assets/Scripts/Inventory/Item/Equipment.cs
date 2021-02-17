using UnityEngine;
using System.Collections;
/// <summary>
/// 装备类
/// </summary>
public class Equipment : Item{
    public int DefensePoints { get; set; }//防御力
    public EquipmentType EquipType { get; set; }//装备类型

    public Equipment(int id, string name, ItemType type, string description, int capaticy, int buyPrice, int sellPrice, string sprite,int defensePoints, EquipmentType equipType) : base(id, name, type, description, capaticy, buyPrice, sellPrice,sprite) 
    {
        this.DefensePoints = defensePoints;
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
        string newText = string.Format("{0}\n<size=40><color=green>Defense Points：{1}</color>\n<color=red>equipType：{2}</color></size>", text, DefensePoints, strEquipType);
        return newText;
    }
}
