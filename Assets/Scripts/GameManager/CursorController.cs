using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{

    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private GameObject clickPositionCursor;
    [SerializeField] private Texture2D pointCursor;
    [SerializeField] private Texture2D attackCursor;

    private void Start()
    {
        customCurser(defaultCursor);
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void customCurser(Texture2D cursor)
    {
        Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
    }

    private void DetectObject()
    {
        if (Camera.main == null) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);

        if (hit2D.collider != null)
        {
            // Debug.Log("tag is:" + hit2D.collider.tag);

            if (Input.GetMouseButtonDown(0) && hit2D.collider.tag == "Confiner")
            {
                ShowClickEffect(hit2D.point);
            }

            else if (hit2D.collider.tag == "Tower")
            {
                customCurser(attackCursor);
            }

            else if (hit2D.collider.tag == "PlayerTower")
            {
                customCurser(pointCursor);
            }

            else
            {
                customCurser(defaultCursor);
            }
        }
    }

    private void ShowClickEffect(Vector2 position)
    {
        // position = new Vector2(position.x, position.y - 0.2f);
        Quaternion rotation = Quaternion.Euler(70, 0, 0);
        Instantiate(clickPositionCursor, position, rotation);
    }

    private void Update()
    {
        DetectObject();
    }
}
