using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody rbp;
    public float notoriety;
    public float interactDistance;
    public string nextScene;

    void Start()
    {
        rbp = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && interactDistance >= Vector3.Distance(transform.position, rbp.position))
        {
            NotorietyManager.Notoriety += notoriety;
            SceneManager.LoadScene(nextScene);
        }
    }
}
