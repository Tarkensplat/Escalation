using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
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
        gameObject.SetActive(false);
    }
}
