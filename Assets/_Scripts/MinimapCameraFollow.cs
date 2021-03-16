using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
	private Vector3 startingPosition;
	private Transform followTarget;
	private Vector3 targetPos;
	private HeroStatus player;
	private bool init = false;
	public GameObject miniIconDie;
	
	void Start()
	{
		player = GetComponentInParent<HeroStatus>();
		startingPosition = transform.position;
        if (!player.isLocalPlayer)
        {
            gameObject.SetActive(false);
            return;
        }
        followTarget = player.gameObject.transform;
	}

	void Update () 
	{
		if(followTarget != null)
		{
			targetPos = new Vector3(followTarget.position.x, followTarget.position.y, transform.position.z);
			if (!player.checkAlive()) {
				InitMiniIconDie();
				Destroy(this);
			}
			
		}
	}

	private void InitMiniIconDie() {
		if (!init) {
			GameObject icon = (GameObject)Instantiate(miniIconDie, followTarget.position, Quaternion.identity);
			init = true;
		}
	}
}
