using UnityEngine;
using UnityEngine.UI;
using Mirror;

public enum HeroClass : byte
{
    Worrior,
    Wizzard,
    Priest,
    Paladin
}

public class HeroStatus : NetworkBehaviour
{
    //Synchronized Fields
    [SyncVar]
    public int maxHealth = 100;
    [SyncVar]
    public int currentHealth;
    [SerializeField]
    [SyncVar]
    private bool alive = true;
    [SyncVar]
    private HeroClass heroClass;

    //Basic Attributes：
    public HealthBar healthBar;

    private int basicAttackPoints = 10;
    public int BasicAttackPoints  { get { return basicAttackPoints; } }  //Basic Attack Points

    private int basicDefensePoints = 10;
    public int BasicDefensePoints{ get { return basicDefensePoints; } }  //Basic Defense Points

    private Text coinText;  //Get gold amount from Coin GameObject
    private int coinAmount = 100;  //Gold owned by the player, can be used to purchase items
    public int CoinAmount
    {
        get { return coinAmount; }
        set { coinAmount = value; coinText.text = coinAmount.ToString(); }
    }

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        coinText = GameObject.Find("Coin").GetComponentInChildren<Text>();
        coinText.text = coinAmount.ToString();
    }

    private void FixedUpdate()
    {
        healthBar.SetHealth(currentHealth);
    }

    void Update()
    {
        //For testing
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(10);
        }*/

        //按下B键控制背包的显示和隐藏
        //Bag
        if (Input.GetKeyDown(KeyCode.B))
        {
            Knapscak.Instance.DisplaySwitch();
        }
        //按下V键控制角色面板的显示和隐藏
        //Character Panel
        if (Input.GetKeyDown(KeyCode.V))
        {
            CharacterPanel.Instance.DisplaySwitch();
        }
        //按下N键商店小贩面板的显示和隐藏
        //Hide Shop Panel
        if (Input.GetKeyDown(KeyCode.N))
        {
            Vendor.Instance.DisplaySwitch();
        }
        //按下S键状态面板的显示和隐藏
        //Status Board
        if (Input.GetKeyDown(KeyCode.S))
        {
            StatusBoard.Instance.DisplaySwitch();
        }
    }

    public bool checkAlive()
    {
        return alive;
    }

    [Command]
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            alive = false;
        }
    }

    //消费金币
    //Use Coins
    public bool ConsumeCoin(int amount)
    {
        if (coinAmount >= amount)
        {
            coinAmount -= amount;
            coinText.text = coinAmount.ToString();  //更新金币数量 Update Coin Amount
            return true;  //消费成功 Successful purchase
        }
        return false;  //否则消费失败 Failed purchase
    }

    //赚取金币
    //Earn Coins
    public void EarnCoin(int amount)
    {
        this.coinAmount += amount;
        coinText.text = coinAmount.ToString();  //更新金币数量 Update Coin Amount
    }
}
