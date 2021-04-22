using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundRotate : MonoBehaviour
{
    public float backgroundRadius;
    public float ringHeight;
    public List<GameObject> buildingList;
    private GameObject player;

    private float rotationAngle;
    private float circumference;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");

        // Add background elements to list
        foreach (Transform child in transform)
        {
            if (child.tag == "Background Building")
            {
                buildingList.Add(child.gameObject);
            }
        }

        rotationAngle = Mathf.PI * 2f / buildingList.Count;

        circumference = player.GetComponent<PlayerMovement>().maxX - player.GetComponent<PlayerMovement>().minX;
    }

    // Update is called once per frame
    void Update()
    {
        int indexer = 0;

        // Set the position of the background buildings based on the player position
        foreach(GameObject b in buildingList)
        {
            float angle = indexer * rotationAngle + 
                ((((player.transform.position.x + circumference / 2) * 360) / circumference) * Mathf.Deg2Rad);

            Vector3 newTransform = player.transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * backgroundRadius;

            newTransform.y = player.transform.position.y + ringHeight;


            b.transform.position = newTransform;

            RotateBuildings(b);

            indexer++;
        }
    }

    // Rotate the buildings toward the player along the x-axis
    void RotateBuildings(GameObject b)
    {
        Vector3 lookPos = player.transform.position - b.transform.position;
        Quaternion lookRot = Quaternion.LookRotation(lookPos, Vector3.up);

        float eulerY = lookRot.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0, eulerY, 0);

        b.transform.rotation = rotation;
    }
}
