using UnityEngine;
using System.Collections;
/// <summary>
/// 消耗品类
/// </summary>
public class Consumable : Item{
    public int Hp { get; set; }
    public int Mp { get;set; }

    public Consumable(int id,string name,ItemType type,string description,int capaticy,int buyPrice,int sellPrice,string sprite,int hp,int mp):base(id,name,type,description,capaticy,buyPrice,sellPrice,sprite){
        this.Hp = hp;
        this.Mp = mp;
    }

    //对父方法Item.GetToolTipText()进行重写
    public override string GetToolTipText()
    {
        string text = base.GetToolTipText();//调用父类的GetToolTipText()方法
        string newText = string.Format("{0}\n<size=40><color=red>add hp：{1}HP</color>\n<color=yellow>add mp：{2}MP</color></size>", text, Hp, Mp);
        return newText;
    }
    public override string ToString()
    {
        string str = "";
        str += ID;
        str += Name;
        str += Type;
        str += Description;
        str += Capacity;
        str += BuyPrice;
        str += SellPrice;
        str += Sprite;
        str += Hp;
        str += Mp;
        return str;
    }
}
