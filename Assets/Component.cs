using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Component
{
    public string Name { get; set; } = "";
    public string MeshPath { get; set; } = "";
    public Position RelativePosition { get; set; }
    public ICollection<Component> Children { get; set; }

}