using UnityEngine;
using System.Collections;
/// <summary>
/// 武器类
/// </summary>
public class Weapon : Item{
    public int Damage { get; set; }//伤害
    public WeaponType WpType { get; set; }//武器类型

    public Weapon(int id, string name, ItemType type, ItemQuality quality, string description, int capaticy, int buyPrice, int sellPrice, string sprite, int damage, WeaponType wpType)
        : base(id, name, type, quality, description, capaticy, buyPrice, sellPrice,sprite)
    {
        this.Damage = damage;
        this.WpType = wpType;
    }
    /// <summary>
    /// 武器类型
    /// </summary>
    public enum WeaponType
    {
        Null,//没有武器
        OffHand,//副手武器
        MainHand//主手武器
    }

    //对父方法Item.GetToolTipText()进行重写
    public override string GetToolTipText()
    {
        string strWeaponType = "";
        switch (WpType)
        {
            case WeaponType.OffHand:
                strWeaponType = "OffHand";
                break;
            case WeaponType.MainHand:
                strWeaponType = "MainHand";
                break;
        }

        string text = base.GetToolTipText();//调用父类的GetToolTipText()方法
        string newText = string.Format("{0}\n<color=red>damage：{1}</color>\n<color=yellow>Weapon type：{2}</color>", text, Damage, strWeaponType);
        return newText;
    }
}
