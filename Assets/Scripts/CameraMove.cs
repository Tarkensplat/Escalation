using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        RenderSettings.skybox.SetFloat("_Rotation", 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newTransform = transform.position;

        if (Input.GetKey(KeyCode.D))
        {
            newTransform += Vector3.right * speed * Time.deltaTime;

            // Rotate skybox with building
            float currentSkyRot = RenderSettings.skybox.GetFloat("_Rotation");
            RenderSettings.skybox.SetFloat("_Rotation", currentSkyRot + Time.deltaTime * speed);
        }

        if (Input.GetKey(KeyCode.W))
        {
            newTransform += Vector3.up * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            newTransform += Vector3.left * speed * Time.deltaTime;

            // Rotate skybox with building
            float currentSkyRot = RenderSettings.skybox.GetFloat("_Rotation");
            RenderSettings.skybox.SetFloat("_Rotation", currentSkyRot + Time.deltaTime * -speed);
        }

        if (Input.GetKey(KeyCode.S))
        {
            newTransform += Vector3.down * speed * Time.deltaTime;
        }

        transform.position = newTransform;
    }
}
