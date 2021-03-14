using UnityEngine;
using UnityEngine.UI;

public class StatusBoard : Inventory
{
    //单例模式
    private static StatusBoard _instance;
    public static StatusBoard Instance 
    {
        get 
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("StatusBoardPanel").GetComponent<StatusBoard>();
            }
            return _instance;
        }
    }

    [SerializeField]
    private Text otherPlayerText;

    public void UpdatePlayerAttribute(string text)
    {
        otherPlayerText = transform.Find("AvatarPlayer/SectionFrame/Attributes").GetComponent<Text>();
        otherPlayerText.text = text;
    }


    private Text boardText;
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        boardText = GetComponentInChildren<Text>();
        boardText.text = "Status Board";
        Hide();
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
}