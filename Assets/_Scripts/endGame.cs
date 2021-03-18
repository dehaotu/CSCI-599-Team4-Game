using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class endGame : NetworkBehaviour
{
    // Start is called before the first frame update
    private CanvasGroup winCanvas;
    void Start()
    {
        winCanvas = GameObject.Find("WinCanvas").GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!transform.GetComponent<MonsterBase1Controller>().isMonsterAlive())
        {
            Win();
        }
    }

    void Win()
    {
        winCanvas.alpha = 1;
        winCanvas.blocksRaycasts = true;
        winCanvas.interactable = true;
    }

}
