using UnityEngine;
using UnityEngine.AI;
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
    public string playerName = "";
    [SyncVar]
    public int maxHealth = 100;
    [SyncVar]
    public int currentHealth;
    [SerializeField]
    [SyncVar]
    public bool alive = true;
    [SyncVar]
    [SerializeField]
    private float respawnCountDown;
    private bool respawning = false; // true when respawning
    public float respawnTime = 2f;
    [SyncVar]
    private HeroClass heroClass;

    //Basic Attributes：
    public HealthBar healthBar;
    [SerializeField]
    [SyncVar]
    private int basicAttackPoints = 10;
    public int BasicAttackPoints { get { return basicAttackPoints; } set { SetAttack(value); } }  //Basic Attack Points
    [SerializeField]
    [SyncVar]
    private int basicDefensePoints = 10;
    public int BasicDefensePoints { get { return basicDefensePoints; } set { SetDefense(value); } }  //Basic Defense Points

    private int originalAP = 10;
    public int OriginalAP { get { return originalAP; } }
    private int originalDP = 10;
    public int OriginalDP { get { return originalDP; } }

    private Text coinText;  //Get gold amount from Coin GameObject
    [SyncVar]
    public int coinAmount = 100;  //Gold owned by the player, can be used to purchase items
    public int CoinAmount
    {
        get { return coinAmount; }
        set { coinAmount = value; coinText.text = coinAmount.ToString(); }
    }

    /* --- Start Chat Control --- */
    public static event Action<HeroStatus, string> OnMessage;

    private Rigidbody2D rb;
    private NavMeshAgent agent;


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
        if (isLocalPlayer)
        {
            coinText = transform.Find("Inventory Menu/Coin").GetComponentInChildren<Text>();
            coinText.text = coinAmount.ToString();
        }
        transform.Find("Status Canvas/Name").GetComponent<Text>().text = playerName;
        StatusBoard.Instance.setParentLocalPlayer(gameObject);
        Debug.Log(GetComponent<NetworkIdentity>().netId.ToString());
        agent = gameObject.GetComponent<NavMeshAgent>();

    }

    private void FixedUpdate()
    {
        healthBar.SetHealth(currentHealth);
    }

    void Update()
    {
        //按下B键控制背包的显示和隐藏
        //Bag
        // if (Input.GetKeyDown(KeyCode.B) && !ChatWindow.isEditingInputField)
        // {
        //     Knapscak.Instance.DisplaySwitch();
        // }
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
        if (isLocalPlayer)
        {
            if (!alive && !respawning)
            {
                respawning = true;
                respawnCountDown = respawnTime;
                CanvasGroup deathCanvas = transform.Find("DeathCanvas").GetComponent<CanvasGroup>();
                deathCanvas.alpha = 1;
                deathCanvas.blocksRaycasts = true;
            }
            else if (!alive && respawning && respawnCountDown > 0)
            {
                respawnCountDown -= Time.deltaTime;
            }
            else if (!alive && respawning && respawnCountDown <= 0)
            {

                agent.enabled = false;
                CanvasGroup deathCanvas = transform.Find("DeathCanvas").GetComponent<CanvasGroup>();
                deathCanvas.alpha = 0;
                deathCanvas.blocksRaycasts = false;
                CmdRespawn();
            }
        }
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

    [Command]
    void CmdRespawn()
    {
        RpcRespawn();

    }

    [ClientRpc]
    private void RpcRespawn()
    {
        GameObject[] sapwnPoints = GameObject.FindGameObjectsWithTag("PlayerSpawns");
        GameObject chosenSpawn = sapwnPoints[UnityEngine.Random.Range(0, sapwnPoints.Length)];

        transform.position = chosenSpawn.transform.position;
        agent.enabled = true;
        alive = true;
        respawning = false;
        currentHealth = maxHealth;
        /* Debug.Log("teleported");
        Debug.Log(chosenSpawn.name);
        Debug.Log(chosenSpawn.transform.position);*/
    }


    public void SetAttack(int value)
    {
        if (isServer) basicAttackPoints = value;
        else CmdSetAttack(value);
    }

    [Command]
    public void CmdSetAttack(int value)
    {
        SetAttack(value);
    }

    public void SetDefense(int value)
    {
        if (isServer) basicDefensePoints = value;
        else CmdSetDefense(value);
    }

    [Command]
    public void CmdSetDefense(int value)
    {
        SetDefense(value);
    }
    /*
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
        }*/

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

    //消费金币 TODO: command
    //Use Coins
    public bool ConsumeCoin(int amount)
    {

        if (coinAmount >= amount)
        {
            ConsumeCoinHelper(amount);
            return true;  //消费成功 Successful purchase
        }
        else return false;  //否则消费失败 Failed purchase

    }

    void ConsumeCoinHelper(int amount)
    {

        coinAmount -= amount;
        coinText = transform.Find("Inventory Menu/Coin").GetComponentInChildren<Text>();
        coinText.text = coinAmount.ToString();  //更新金币数量 Update Coin Amount
    }

    /*  [Command]
      void CmdConsumeCoin(int amount)
      {
          ConsumeCoinHelper(amount);
      }
  */
    //赚取金币 TODO: command
    //Earn Coins
    public void EarnCoin(int amount)
    {
        this.coinAmount += amount;
        coinText = transform.Find("Inventory Menu/Coin").GetComponentInChildren<Text>();
        //CmdEarnMoney(amount);
        //RpcUpdateCoinAmout(coinAmount);
        coinText.text = coinAmount.ToString();  //更新金币数量 Update Coin Amount
        Debug.Log("这个是：" + coinAmount);
        //coinText.text = "1000";  
    }

    [ClientRpc]
    public void RpcUpdateCoinAmout(int amount)
    {
        coinText.text = amount.ToString();
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
