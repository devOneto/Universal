using System.Collections.Generic;
using UnityEngine;
using UniversalEditor;

[System.Serializable]
public class Component
{
    public string Name { get; set; } = "";
    public string LocalModelFilePath { get; set; } = "";
    public Scale Scale { get; set; } = new Scale();
    public Position RelativePosition { get; set; } = new Position();
    public Rotation Rotation { get; set; } = new Rotation();
    public IList<Component?> Components { get; set; } = new List<Component>();

}