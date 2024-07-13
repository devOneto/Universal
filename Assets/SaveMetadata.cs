using System.Collections.Generic;

[System.Serializable]
public class SaveMetadata {

    public string Name { get; set; } = "Save";

    public IList<Component> Components { get; set; } = new List<Component>();

}