using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 角色面板类，控制角色面板的逻辑
/// </summary>
public class CharacterPanel : Inventory
{
    //单例模式
    private static CharacterPanel _instance;
    public static CharacterPanel Instance 
    {
        get 
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("CharacterPanel").GetComponent<CharacterPanel>();
            }
            return _instance;
        }
    }

    private Text characterPropertyText;//对角色属性面板中Text组件的引用
    private HeroStatus player;//对角色脚本的引用
    private Text boardText;
    private int attackPoints = 0, defensePoints = 0;

    public override void Start()
    {
        base.Start();
        characterPropertyText = transform.Find("CharacterPropertyPanel/Text").GetComponent<Text>();
        player = GameObject.FindWithTag("Player").GetComponent<HeroStatus>();
        UpdatePropertyText();//初始化显示角色属性值
        //Hide();
    }

    //更新角色属性显示
    private void UpdatePropertyText() 
    {
        attackPoints = 0;
        defensePoints = 0;
        foreach (EquipmentSlot slot in slotArray)//遍历角色面板中的装备物品槽
        {
            if (slot.transform.childCount > 0)//找到有物品的物品槽，获取里面装备的属性值
            {
                Item item = slot.transform.GetChild(0).GetComponent<ItemUI>().Item;
                if (item is Equipment)//如果物品是装备，那就加角色对应的属性
                {
                    Equipment e = (Equipment)item;
                    defensePoints += e.DefensePoints;
                }
                else if (item is Weapon)///如果物品是武器，那就加角色的伤害属性
                {
                    Weapon w = (Weapon)item;
                    attackPoints += w.AttackPoints;
                }
            }
        }
        attackPoints += player.OriginalAP;
        defensePoints += player.OriginalDP;
        //if (!GetComponentInParent<HeroStatus>().thisIsSever() && GetComponentInParent<HeroStatus>().thisIsLocalPlayer()) CmdUpdateSB();
        string text = string.Format("Attack：{0}\nDefense：{1}", attackPoints, defensePoints);
        characterPropertyText.text = text;

        // Update player attackpoints/defensepoints
        player.BasicAttackPoints = attackPoints;
        player.BasicDefensePoints = defensePoints;
    }

    //TODO:更新状态栏
    private void UpdateSB()
    {
    }

/*    private void RpcUpdateSB()
    {
        characterPropertyText.text = GetComponentInParent<NetworkIdentity>().netId.ToString();
        Debug.Log(GetComponent<NetworkIdentity>().netId.ToString());
    }*/


    //直接穿戴功能（不需拖拽）
    public void PutOn(Item item) 
    {
        Item exitItem = null;//临时保存需要交换的物品
        foreach (Slot slot in slotArray)//遍历角色面板中的物品槽，查找合适的的物品槽
        {
            EquipmentSlot equipmentSlot = (EquipmentSlot)slot;
            if (equipmentSlot.IsRightItem(item)) //判断物品是否合适放置在该物品槽里
            {
                if (equipmentSlot.transform.childCount > 0)//判断角色面板中的物品槽合适的位置是否已经有了装备
                {
                    Vendor.Instance.SellItem(item);
                }
                else
                {
                    equipmentSlot.StoreItem(item);
                }
                break;
            }
        }
        if (exitItem != null)
        {
            Knapscak.Instance.StoreItem(exitItem);//把角色面板上是物品替换到背包里面
        }
        UpdatePropertyText();//更新显示角色属性值

        
        //boardText = GameObject.Find("StatusBoardPanel/Text").GetComponent<Text>();
       /* System.Console.WriteLine(boardText.text);
        System.Console.WriteLine("boardText.text");*/
        //boardText.text = "player put on " + item.Name;

        /*UpdateSB();//更新状态板*/

    }

    //脱掉装备功能（不需拖拽）
    public void PutOff(Item item) 
    {
        Vendor.Instance.SellItem(item);
        UpdatePropertyText();//更新显示角色属性值

        //boardText = GameObject.Find("StatusBoardPanel/Text").GetComponent<Text>();
       /* System.Console.WriteLine(boardText.text);
        System.Console.WriteLine("boardText.text");*/
        //boardText.text = "player put off " + item.Name;

       /* UpdateSB();//更新状态板*/
    }
}
