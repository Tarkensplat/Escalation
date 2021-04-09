using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public GameObject[] copies;
    Vector3 startPosition;
    public float value;

    public float offset;
    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(startPosition.x, startPosition.y + Mathf.Sin(Time.time)/2, startPosition.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        NotorietyManager.Notoriety += value;
        for (int i = 0; i < copies.Length; i++)
        {
            copies[i].SetActive(false);
        }
        gameObject.SetActive(false);
    }
}
