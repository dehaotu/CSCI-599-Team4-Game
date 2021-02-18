using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusBoard : MonoBehaviour
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

    //控制面板的显示和隐藏相关变量
    private CanvasGroup canvasGroupMy;
    public CanvasGroup CanvasGroupMy      //对CanvasGroup的引用，用于制作隐藏显示效果
    {
        get
        {
            if (canvasGroupMy == null)
            {
                canvasGroupMy = GameObject.Find("StatusBoardPanel").GetComponent<CanvasGroup>();
            }
            return canvasGroupMy;
        }
    }

    //面板的显示方法
    public void Show() 
    {
        this.CanvasGroupMy.alpha = 1;//面板显示
    }
    //面板的隐藏方法
    public void Hide() 
    {
        this.CanvasGroupMy.alpha = 0;//面板隐藏
    }

    public void DisplaySwitch() 

    {

        if (this.CanvasGroupMy.alpha == 1)
        {
            Hide();
        }
        else Show();
    }

    private Text boardText;
    // Start is called before the first frame update
    void Start()
    {
        boardText = GameObject.Find("StatusBoardPanel/Text").GetComponent<Text>();
        boardText.text = "SB";
        Show();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}