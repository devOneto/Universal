using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Component
{
    public string Name { get; set; } = "";
    public string MeshPath { get; set; } = "";
    public Position RelativePosition { get; set; } = new Position();
    public Rotation Rotation { get; set; } = new Rotation();
    public IList<Component?> Components { get; set; } = new List<Component>();

}