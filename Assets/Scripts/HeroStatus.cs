using UnityEngine;
using UnityEngine.UI;
public class HeroStatus : MonoBehaviour
{
    //角色基本属性：
    private int basicAttackPoints = 10;
    public int BasicAttackPoints  { get { return basicAttackPoints; } }//基本攻击点

    private int basicDefensePoints = 10;
    public int BasicDefensePoints{ get { return basicDefensePoints; } }//基本防御点

    private Text coinText;//对金币Text组件的引用
    private int coinAmount = 100;//角色所持有的金币，用于从商贩那里购买物品
    public int CoinAmount
    {
        get { return coinAmount; }
        set { coinAmount = value; coinText.text = coinAmount.ToString(); }
    }

    public int maxHealth = 100;
    public int currentHealth;

    [SerializeField] private bool alive = true;

    public HealthBar healthBar;
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        coinText = GameObject.Find("Coin").GetComponentInChildren<Text>();
        coinText.text = coinAmount.ToString();
    }

    void Update()
    {
        // for testing
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(10);
        }

        //按下B键控制背包的显示和隐藏
        if (Input.GetKeyDown(KeyCode.B))
        {
            Knapscak.Instance.DisplaySwitch();
        }
        //按下V键控制角色面板的显示和隐藏
        if (Input.GetKeyDown(KeyCode.V))
        {
            CharacterPanel.Instance.DisplaySwitch();
        }
        //按下N键商店小贩面板的显示和隐藏
        if (Input.GetKeyDown(KeyCode.N))
        {
            Vendor.Instance.DisplaySwitch();
        }

        //按下S键状态面板的显示和隐藏
        if (Input.GetKeyDown(KeyCode.S))
        {
            StatusBoard.Instance.DisplaySwitch();
        }
    }

    public bool checkAlive()
    {
        return alive;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
        if (currentHealth <= 0)
        {
            alive = false;
        }
    }

    //消费金币
    public bool ConsumeCoin(int amount)
    {
        if (coinAmount >= amount)
        {
            coinAmount -= amount;
            coinText.text = coinAmount.ToString();//更新金币数量
            return true;//消费成功
        }
        return false;//否则消费失败
    }

    //赚取金币
    public void EarnCoin(int amount)
    {
        this.coinAmount += amount;
        coinText.text = coinAmount.ToString();//更新金币数量
    }
}
