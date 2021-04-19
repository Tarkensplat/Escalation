using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertBot : MonoBehaviour
{
    public GameObject[] copies;
    float alertDelay = 1f;
    int jumpForce = 8;
    Rigidbody rb;
    public AudioSource alert;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            alert.Play();
            //decrease notoriety by 5, don't go below 0
            NotorietyManager.Notoriety = NotorietyManager.Notoriety > 5 ? NotorietyManager.Notoriety - 5 : 0;

            for (int i = 0; i < copies.Length; i++)
            {
                rb = copies[i].GetComponent<Rigidbody>();
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                Destroy(copies[i], alertDelay);
            }
            rb = GetComponent<Rigidbody>();
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Destroy(gameObject, alertDelay);
        }
    }
}
