using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GLTFast;
using Palmmedia.ReportGenerator.Core.Common;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectHandler : MonoBehaviour
{
    public Camera ActiveCamera;
    public TextMeshPro ActionText;
    public GameObject[] PleacableObjects;
    public GameObject HoldingContainer;
    public GameObject ComponentsContainer;

    Vector3 MousePosition;
    GameObject HoldingModelObject;
    Vector3 ModelObjectOriginalPosition;
    IDataService DataService = new DataService();

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if(HoldingModelObject){
            // Set Mouse position based on Screen
            MousePosition = ActiveCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 23));
            // Move Holding Object According to Mouse Position
            HoldingModelObject.transform.position = new Vector3(MousePosition.x, 0, MousePosition.z); 
            //TODO: Single Responsability Principle is beeing disrespected
        }

        //TODO :(
        if (Input.GetKeyDown(KeyCode.Q))
        {
            this.DestroyHoldingContainerComponents();
            StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", "*", false,  (string[] paths) => {
                    
                    // Create a temporary game object to hold the placeble object
                    GameObject newGameObject = new GameObject(paths[0].Split("\\").Last());
                    // Set this object as child of Components Container ( in order to organize )
                    newGameObject.transform.SetParent(HoldingContainer.transform);
                    // Load component with the file path
                    var gltf = newGameObject.AddComponent<GLTFast.GltfAsset>();
                    gltf.Url = paths[0];

                    // Set holding object as the created component
                    this.HoldingModelObject = newGameObject;

                }
            );
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            var saveFileObject = new List<Component>();

            int componentsContainerChildCount = this.ComponentsContainer.transform.childCount;
            for (int i = 0; i < componentsContainerChildCount; i++)
            {
                var currentChild = this.ComponentsContainer.transform.GetChild(i);
                saveFileObject.Add( new Component {
                    Name = currentChild.gameObject.name,
                    MeshPath = currentChild.GetComponent<GLTFast.GltfAsset>().Url,
                    RelativePosition = new Position {
                        X = currentChild.transform.position.x,
                        Y = currentChild.transform.position.y,
                        Z = currentChild.transform.position.z
                    }
                });
            }

            if (DataService.SaveData<List<Component>>("./save.json", saveFileObject))
                Debug.LogWarning("Data Saved!");
            else
                Debug.LogError("Data Could not be save.");

        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", "json", false,  (string[] paths) => {
                    var loadedComponents = this.DataService.LoadData<Component[]>(paths[0]);
                    foreach (var component in loadedComponents)
                    {
                        var newGameObject = new GameObject();
                        newGameObject.name = component.Name;
                        newGameObject.transform.position = new Vector3(component.RelativePosition.X,component.RelativePosition.Y, component.RelativePosition.Z );
                        newGameObject.transform.SetParent(HoldingContainer.transform);
                        var gltf = newGameObject.AddComponent<GLTFast.GltfAsset>();
                        gltf.Url = component.MeshPath;
                    }
                }
            );
        }

        // Set new Object Position based on Mouse Position
        if (Input.GetMouseButtonDown(0))
        {
            GameObject newComponent = Instantiate(HoldingModelObject, MousePosition, Quaternion.identity);
            // newComponent.transform.localScale
            newComponent.transform.SetParent(ComponentsContainer.transform);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            this.HoldingModelObject = null;
            this.DestroyHoldingContainerComponents();
        }
            
    }

    void FixedUpdate()
    {

    }

    private IList<string> GetGameObjectComponentsNameList(GameObject gameObject)
    {
        IList<string> result = new List<string>();

        foreach (var component in gameObject.GetComponents<MeshFilter>())
        {
            result.Add(component.name);
        }

        return result;
    }

    async void LoadGltfBinaryFromMemory()
    {
        string filePath = "./Assets/Models/door-with-animation.glb";
        byte[] data = File.ReadAllBytes(filePath);
        var gltf = new GltfImport();
        bool success = await gltf.LoadGltfBinary( data, new Uri(filePath, UriKind.Relative) );
        if (success) success = await gltf.InstantiateMainSceneAsync(transform);
    }

    async void LoadGltfFromPathAndInstantiate(string path)
    { 
        GltfImport model = new GltfImport();
        await model.LoadGltfBinary( File.ReadAllBytes( path ) );
        await model.InstantiateMainSceneAsync(transform);
    }

    void DestroyHoldingContainerComponents(){
        for (int i = 0; i < this.HoldingContainer.transform.childCount; i++){
            Destroy(this.HoldingContainer.transform.GetChild(i).gameObject);
        }
    }
}
