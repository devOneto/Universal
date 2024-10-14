using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorCameraControl : MonoBehaviour
{

    Vector3 cameraInitialPosition = new Vector3(0,20,0);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
      if( Input.GetKeyDown(KeyCode.UpArrow) ) transform.position += Vector3.forward;
      if( Input.GetKeyDown(KeyCode.DownArrow) ) transform.position += Vector3.back;
      if( Input.GetKeyDown(KeyCode.LeftArrow) ) transform.position += Vector3.left;
      if( Input.GetKeyDown(KeyCode.RightArrow) ) transform.position += Vector3.right;
      if( Input.GetKeyDown(KeyCode.End) ) transform.position = cameraInitialPosition;
    }
}
