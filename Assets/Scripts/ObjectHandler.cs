using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GLTFast;
using SFB;
using TMPro;
// using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

public class ObjectHandler : MonoBehaviour
{
    public Camera ActiveCamera;
    public TextMeshPro ActionText;
    public GameObject HoldingContainer;
    public GameObject ComponentsContainer;
    public float VerticalMovementSnapValue;
    public float TransformPositionSnapValue;

    Vector3 MousePosition;
    GameObject HoldingModelObject;
    Vector3 ModelObjectOriginalPosition;
    IDataService DataService = new DataService();

    float ObjectModelInPlacementVerticalValue;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (HoldingModelObject)
        {
            // Set Mouse position based on Screen
            MousePosition = ActiveCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, ActiveCamera.transform.position.y));
            
            // Move Holding Object According to Mouse Position with grid snapping
            HoldingModelObject.transform.position = new Vector3( GetSnappedPosition(MousePosition.x) , ObjectModelInPlacementVerticalValue, GetSnappedPosition(MousePosition.z));
            
            // Set scale correction
            // HoldingModelObject.transform.localScale = new Vector3(50f,50f,50f);
            
            
            if (Input.GetKeyDown(KeyCode.R)) {
                HoldingModelObject.transform.eulerAngles = new Vector3{
                    x = HoldingModelObject.transform.eulerAngles.x,
                    y = HoldingModelObject.transform.eulerAngles.y + 45,
                    z = HoldingModelObject.transform.eulerAngles.z
                };
            }

            if( Input.GetKeyDown(KeyCode.Q) ){
                ObjectModelInPlacementVerticalValue += VerticalMovementSnapValue;
            }

            if( Input.GetKeyDown(KeyCode.E) ){
                ObjectModelInPlacementVerticalValue -= VerticalMovementSnapValue;
            }
            
            if( Input.GetKeyDown(KeyCode.T) ){
                HoldingModelObject.transform.localScale *= 1.1f;
            }

            if( Input.GetKeyDown(KeyCode.G) ){
                HoldingModelObject.transform.localScale /= 1.1f;
            }


            if (Input.GetMouseButtonDown(0)) SetHoldingObjectPositionOnScene( HoldingModelObject.transform.position );

            // Remove Holding Object
            if (Input.GetKeyDown(KeyCode.Escape)) CleanHoldingObjectContainer();
        }

        //TODO : Better user input management
        if(!HoldingModelObject){
            if (Input.GetKeyDown(KeyCode.Q)) ImportGLTFModelToScene();
            if (Input.GetKeyDown(KeyCode.S)) Save();
            if (Input.GetKeyDown(KeyCode.L)) Load();
        }
        

    }

    private int GetSnappedPosition(float x)
    {

        var quocient = (int) (x / TransformPositionSnapValue);
        var inferior = (int) TransformPositionSnapValue * quocient;
        var superior = (int) TransformPositionSnapValue * ( quocient + 1);

        // Debug.Log(@$" x: {x}, snap: {TransformPositionSnapValue}, quocient: {quocient}, inferior: {inferior}, superior: {superior} ");

        if( Math.Abs( x - inferior ) < Math.Abs( x - superior ) )
            return inferior;
        else
            return superior;
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

    void ImportGLTFModelToScene()
    {
        ObjectModelInPlacementVerticalValue = 0.0f;
        this.DestroyHoldingContainerComponents();
        StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", "glb", false, (string[] paths) =>
        {

            if( paths.Count() == 0 || paths[0] == "" ) return;

            // Treat path
            var splitedPath =  paths[0].Contains("/") ? paths[0].Split("/") : paths[0].Split("\\"); // if in windows it uses \, uhg...
            string modelFileName = splitedPath.Last();
            string modelName = modelFileName.Split(".")[0];

            // Create a temporary game object to hold the placeble object
            GameObject newGameObject = new GameObject();
            newGameObject.name = modelName;
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

    void SetHoldingObjectPositionOnScene( Vector3 position )
    {
        GameObject newComponent = Instantiate(HoldingModelObject, position, Quaternion.Euler(HoldingModelObject.transform.eulerAngles.x, HoldingModelObject.transform.eulerAngles.y, HoldingModelObject.transform.eulerAngles.z) );
        newComponent.tag = "Component";
        newComponent.transform.SetParent(ComponentsContainer.transform);
    }

    void DestroyHoldingContainerComponents()
    {
        for (int i = 0; i < this.HoldingContainer.transform.childCount; i++)
        {
            Destroy(this.HoldingContainer.transform.GetChild(i).gameObject);
        }
    }

    void CleanHoldingObjectContainer()
    {
        this.HoldingModelObject = null;
        this.DestroyHoldingContainerComponents();
    }

    void Save()
    {
        var saveFileObject = new SaveMetadata{
            Name = "Save",
            Components = {}
        };

        for (int i = 0; i < ComponentsContainer.transform.childCount; i++)
        {
            GameObject currentGameObject = ComponentsContainer.transform.GetChild(i).gameObject;
            if(currentGameObject.tag == "Component"){
                saveFileObject.Components.Add(CreateComponentMetadataFromGameObjectToSave(ComponentsContainer.transform.GetChild(i).gameObject));
            }
        }

        if(this.ComponentsContainer.transform.childCount <= 0){
            Debug.LogError("Nothing to save."); 
            return;
        }

        if (DataService.SaveData<SaveMetadata>("./save.json", saveFileObject))
            Debug.LogWarning("Data Saved!");
        else
            Debug.LogError("Data Could not be save.");
    }

    void Load()
    {
        StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", "json", false, (string[] paths) => {
            
            SaveMetadata loadedSaveData = this.DataService.LoadData<SaveMetadata>(paths[0]);

            GameObject savedGameObjectContainer = new GameObject( loadedSaveData.Name );
            savedGameObjectContainer.transform.position = Vector3.zero;
            savedGameObjectContainer.tag = "Component";

            foreach (Component component in loadedSaveData.Components )
            {
                GameObject newGameObject = CreateGameObjectFromUnivComponent(component);
                
                if ( ComponentsContainer.transform.childCount == 0 ){
                    newGameObject.transform.SetParent(savedGameObjectContainer.transform);
                }
            }

            savedGameObjectContainer.transform.SetParent(ComponentsContainer.transform);

        });
    }

    GameObject CreateGameObjectFromUnivComponent ( Component component ) {

        // Create Game Object
        var newGameObject = new GameObject();
        newGameObject.name = component.Name;
        
        newGameObject.transform.position = new Vector3(component.RelativePosition.X, component.RelativePosition.Y, component.RelativePosition.Z);
        newGameObject.transform.Rotate(new Vector3(component.Rotation.X,component.Rotation.Y,component.Rotation.Z)); 
        newGameObject.transform.localScale = new Vector3( component.Scale.X, component.Scale.Y, component.Scale.Z );

        var gltf = newGameObject.AddComponent<GLTFast.GltfAsset>();
        gltf.Url = component.LocalModelFilePath;
        newGameObject.tag = "Component";

        int currentChildIndex = 0;
        while( currentChildIndex != component.Components.Count() ) {
            
            Component currentChildComponent = component.Components[currentChildIndex];
            GameObject childComponentGameObject = CreateGameObjectFromUnivComponent(currentChildComponent);
            childComponentGameObject.transform.SetParent(newGameObject.transform);
            currentChildIndex++;
        }
        
        return newGameObject;

    }

    Component CreateComponentMetadataFromGameObjectToSave( GameObject gameObject ){

        Component resultComponent  = new Component {
            Name = gameObject.name,
            LocalModelFilePath = gameObject.GetComponent<GLTFast.GltfAsset>() ? gameObject.GetComponent<GLTFast.GltfAsset>().Url : "",
            RelativePosition = new UniversalEditor.Position
            {
                X = gameObject.transform.position.x,
                Y = gameObject.transform.position.y,
                Z = gameObject.transform.position.z
            },
            Rotation = new UniversalEditor.Rotation {
                X = gameObject.transform.eulerAngles.x,
                Y = gameObject.transform.eulerAngles.y,
                Z = gameObject.transform.eulerAngles.z,
            },
            Scale = new UniversalEditor.Scale {
                X = gameObject.transform.localScale.x,
                Y = gameObject.transform.localScale.y,
                Z = gameObject.transform.localScale.z,
            }
        };
        int childCount = 0;
        while(childCount != gameObject.transform.childCount  ) {
            GameObject currentChild = gameObject.transform.GetChild(childCount).gameObject;
            if(currentChild.tag == "Component"){
                resultComponent.Components.Add( CreateComponentMetadataFromGameObjectToSave( currentChild ) );
            }
            childCount++;
        }
        return resultComponent;
    }

    private string getFileNameFromPath(string url) => url.Contains("/") ? url.Split("/").Last() : url.Split("\\").Last();

    private string getPathFromFileName(string name) => Application.dataPath + (Application.dataPath.Contains("/") ? $"/Models/{name}" : $"\\Models\\{name}");
}
