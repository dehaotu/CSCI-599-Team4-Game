using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;

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
    [SerializeField]
    private float respawnCountDown;
    [SyncVar]
    private HeroClass heroClass;

    //Basic Attributes：
    public HealthBar healthBar;
    [SerializeField]
    [SyncVar(hook = nameof(SetAttack))]
    private int basicAttackPoints = 10;
    public int BasicAttackPoints { get { return basicAttackPoints; } set { SetAttack(value); } }  //Basic Attack Points
    [SerializeField]
    [SyncVar(hook = nameof(SetDefense))]
    private int basicDefensePoints = 10;
    public int BasicDefensePoints{ get { return basicDefensePoints; } set { SetDefense(value); } }  //Basic Defense Points

    private int originalAP = 10;
    public int OriginalAP {get {return originalAP;}}
    private int originalDP = 10;
    public int OriginalDP {get {return originalDP;}}

    private Text coinText;  //Get gold amount from Coin GameObject
    [SyncVar]
    private int coins;
    private int coinAmount = 100;  //Gold owned by the player, can be used to purchase items
    public int CoinAmount
    {
        get { return coinAmount; }
        set { coinAmount = value; coinText.text = coinAmount.ToString(); }
    }

    /* --- Start Chat Control --- */
    public static event Action<HeroStatus, string> OnMessage;

    [Command]
    public void CmdSend(string message)
    {
        if (message.Trim() != "")
            RpcReceive(message.Trim());
    }

    [ClientRpc]
    public void RpcReceive(string message)
    {
        OnMessage?.Invoke(this, message);
    }
    /* --- End Chat control --- */

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        coinText = GameObject.Find("Coin").GetComponentInChildren<Text>();
        coinText.text = coinAmount.ToString();
        Debug.Log(GetComponent<NetworkIdentity>().netId.ToString());
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
        if (Input.GetKeyDown(KeyCode.B) && !ChatWindow.isEditingInputField)
        {
            Knapscak.Instance.DisplaySwitch();
        }
        //按下V键控制角色面板的显示和隐藏
        //Character Panel
        if (Input.GetKeyDown(KeyCode.V) && !ChatWindow.isEditingInputField)
        {
            CharacterPanel.Instance.DisplaySwitch();
        }
        //按下N键商店小贩面板的显示和隐藏
        //Hide Shop Panel
        if (Input.GetKeyDown(KeyCode.N) && !ChatWindow.isEditingInputField)
        {
            Vendor.Instance.DisplaySwitch();
        }
        //按下S键状态面板的显示和隐藏
        //Status Board
        if (Input.GetKeyDown(KeyCode.S) && !ChatWindow.isEditingInputField)
        {
            StatusBoard.Instance.DisplaySwitch();
        }

        // show death screen
        if (isLocalPlayer && !alive) { }

    }

    public bool checkAlive()
    {
        return alive;
    }


    public void TakeDamage(int damage)
    {
        if (!this.isServer) return;
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            alive = false;
        }
    }

    private void Respawn()
    {
        
    }


    public void SetAttack(int value)
    {
        if(isServer) basicAttackPoints = value;
    }

    public void SetDefense(int value)
    {
        if (isServer) basicDefensePoints = value;
    }

    public void SetAttack(int oldAttack, int newAttack)
    {
        //string text = string.Format("Attack：{0}\nDefense：{1}", newAttack, basicDefensePoints);
        //StatusBoard.Instance.UpdatePlayerAttribute(text);
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(var player in players)
        {
            if (!player.GetComponent<HeroStatus>().isLocalPlayer)
            {
                string text = string.Format("Attack：{0}\nDefense：{1}", newAttack, player.GetComponent<HeroStatus>().BasicDefensePoints);
                StatusBoard.Instance.UpdatePlayerAttribute(text);
            }
        }
        
    }

    public void SetDefense(int oldDefense, int newDefense)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (!player.GetComponent<HeroStatus>().isLocalPlayer)
            {
                string text = string.Format("Attack：{0}\nDefense：{1}", player.GetComponent<HeroStatus>().BasicAttackPoints, newDefense);
                StatusBoard.Instance.UpdatePlayerAttribute(text);
            }
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
            coins = coinAmount;
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
        coins = coinAmount;
    }

    /// <summary>
    /// Only Top gameobject can have network identity, thus need to have this function for children scripts
    /// </summary>
    /// <returns></returns>
    public bool thisIsLocalPlayer()
    {
        return isLocalPlayer;
    }

    public bool thisIsSever()
    {
        return isServer;
    }
}
