using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusU : MonoBehaviour
{
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SinglePlayers gameManager = FindObjectOfType<SinglePlayers>();
            if (gameManager != null)
            {
                gameManager.TransferStatusU(collision.gameObject);
            }
        }
    }
}

