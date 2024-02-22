using System.Collections;
using System.Collections.Generic;
using System.IO;
using Palmmedia.ReportGenerator.Core.Common;
using UnityEngine;

public class ObjectHandler : MonoBehaviour
{
    public Camera ActiveCamera;
    public GameObject[] PleacableObjects;

    Vector3 MousePosition;
    GameObject ModelObject;
    Vector3 ModelObjectOriginalPosition;
    IDataService DataService = new DataService();

    // Start is called before the first frame update
    void Start()
    {
        ModelObject = PleacableObjects[0];
        ModelObjectOriginalPosition = ModelObject.transform.position;
        
        var loadedObjects = this.DataService.LoadData<List<ObjectModel>>("./save.json");
        
        if( loadedObjects != null ){
            foreach (var objectModel in loadedObjects)
            {
                Instantiate( GameObject.Find(objectModel.Name), new Vector3 { x = objectModel.X , y = objectModel.Y, z = objectModel.Z }, Quaternion.identity);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {

        //TODO :(
        if(Input.GetKeyDown(KeyCode.Q))
        {
            ModelObject.transform.position = ModelObjectOriginalPosition;
            ModelObject = PleacableObjects[0];
            ModelObjectOriginalPosition = ModelObject.transform.position;
        }
        if(Input.GetKeyDown(KeyCode.W))
        {
            ModelObject.transform.position = ModelObjectOriginalPosition;
            ModelObject = PleacableObjects[1];
            ModelObjectOriginalPosition = ModelObject.transform.position;
        }
        if(Input.GetKeyDown(KeyCode.E))
        {
            ModelObject.transform.position = ModelObjectOriginalPosition;
            ModelObject = PleacableObjects[2];
            ModelObjectOriginalPosition = ModelObject.transform.position;
        }
        if(Input.GetKeyDown(KeyCode.S))
        {
            var saveFileObject = new List<ObjectModel>();

            foreach (var ObjectModel in GameObject.FindGameObjectsWithTag("ObjectModel"))
            {
                saveFileObject.Add( new ObjectModel {
                    Name = ObjectModel.GetComponents<MeshFilter>()[0].name,
                    MeshName = ObjectModel.GetComponents<MeshFilter>()[0].name,
                    X = ObjectModel.transform.position.x,
                    Y = ObjectModel.transform.position.y,
                    Z = ObjectModel.transform.position.z
                });
            }
            
            if (DataService.SaveData<List<ObjectModel>>("./save.json",saveFileObject))
                Debug.LogWarning("Data Saved!");
            else
                Debug.LogError("Data Could not be save.");

        }

        // Set Mouse position based on Screen
        MousePosition = ActiveCamera.ScreenToWorldPoint( new Vector3(Input.mousePosition.x, Input.mousePosition.y, 23)) ;
        ModelObject.transform.position = new Vector3(MousePosition.x, 0 , MousePosition.z); //TODO: Single Responsability Principle is beeing disrespected

        // Set new Object Position based on Mouse Position
        if(Input.GetMouseButtonDown(0))
            Instantiate(ModelObject, MousePosition, Quaternion.identity); //TODO: usar prefabs
        
    }

    void FixedUpdate()
    {

    }

    private IList<string> GetGameObjectComponentsNameList( GameObject gameObject)
    {
        IList<string> result = new List<string>();

        foreach (var component in gameObject.GetComponents<MeshFilter>())
        {
            result.Add(component.name);
        }

        return result;
    } 
}
