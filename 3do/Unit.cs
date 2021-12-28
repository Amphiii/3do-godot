using Godot;
using System;
using System.Collections.Generic;

public class Unit : Spatial, TAUtil._3do.IModelReaderAdapter
{
	private float SCALE_FACTOR = 65535; //16384 * 10;// 1000000; // 655350;
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	List<Godot.Vector3> vertices = new List<Godot.Vector3>();
	[Export]
	private string filePath;

	[Export]
	private Material material;

	private string currentObjectName = null;
	private List<List<Vector3>> currentObject = null;

	private Vector3 currentPosition = new Vector3(0, 0, 0);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Load3DO.Load(filePath, this);
	}

	public override void _Process(float delta)
	{
		var spatialNode = this as Spatial;
		var rotation = spatialNode.RotationDegrees;
		rotation.y += delta * 50;
		GetNode<Spatial>(".").RotationDegrees = rotation;
	}

	public void AddPrimitive(int color, string texture, int[] vertexIndices, bool isSelectionPrimitive)
	{
		var dbgVecArr = new List<Vector3>();

		var mesh = new ArrayMesh();
		var arr = new Godot.Collections.Array();
		var vertexArr = new Godot.Collections.Array();
		vertexArr.Resize(vertexIndices.Length);
		//Console.WriteLine(vertexIndices.Length);
		for (int i=0; i<vertexIndices.Length; i++) {
			vertexArr[i] = vertices[vertexIndices[i]];
			//Console.WriteLine(vertices[vertexIndices[i]].x);
			//Console.WriteLine(vertices[vertexIndices[i]].y);
			//Console.WriteLine(vertices[vertexIndices[i]].z);
			dbgVecArr.Add(vertices[vertexIndices[i]]);
		}
		arr.Add(vertexArr);
		arr.Resize((int) ArrayMesh.ArrayType.Max);
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.LineLoop, arr);
		var m = new MeshInstance();
		m.Mesh = mesh;
		var pos = new Vector3(currentPosition);
		var t = new Transform(Quat.Identity, pos);
		m.Transform = t;
		m.SetSurfaceMaterial(0, material);

		currentObject.Add(dbgVecArr);

		if (!isSelectionPrimitive) {
			this.AddChild(m);
		}

	}

	private float PosToSimScale(int position) {
		if (position == 0) {
			return 0;
		}
		return (float) ((double) position / SCALE_FACTOR);
	}

	public void AddVertex(TAUtil._3do.Vector v)
	{
		var newV = new Vector3();
		newV.x = PosToSimScale(v.X);
		newV.y = PosToSimScale(v.Y);
		newV.z = PosToSimScale(v.Z);
		vertices.Add(newV);
	}

	public void BackToParent()
	{
		//Console.WriteLine("BackToParent");
	}

	public void CreateChild(string name, TAUtil._3do.Vector position)
	{
		if (currentObject != null) {
			Console.WriteLine("Name: " + currentObjectName);
			Console.WriteLine($"Offset: x: {currentPosition.x} y: {currentPosition.y} z: {currentPosition.z}");
			Console.WriteLine("Vertices:");
			for (int i=0; i < currentObject.Count; i++) {
				Console.WriteLine($"Primitive {i}:");
				var p = currentObject[i];
				for (int j=0; j<p.Count; j++) {
					var v = p[j];
					Console.WriteLine($"{j}: x: {v.x} y: {v.y} z: {v.z}");
				}
			}
		}
		currentObject = new List<List<Vector3>>();
		currentObjectName = name;
		currentPosition.x = PosToSimScale(position.X);
		currentPosition.y = PosToSimScale(position.Y);
		currentPosition.z = PosToSimScale(position.Z);
		//Console.WriteLine($"Raw X: {position.X}, New X: {currentPosition.x}");
		//Console.WriteLine($"CreateChild {name} {position.X} {position.Y} {position.Z}");
	}
}
