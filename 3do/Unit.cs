using Godot;
using System;
using System.Collections.Generic;

public class Unit : Spatial, TAUtil._3do.IModelReaderAdapter
{
	private float SCALE_FACTOR = 1000000; //16384 * 10; //65535; 16384 * 10;// 1000000; // 655350;
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
	private Spatial selectedNode;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		selectedNode = this;
		Load3DO.Load(filePath, this);
	}

	public override void _Process(float delta)
	{
		var spatialNode = this as Spatial;
		var rotation = spatialNode.RotationDegrees;
		rotation.y += delta * 50;
		GetNode<Spatial>(".").RotationDegrees = rotation;
	}

	private List<Vector2> PrimitiveToVector2(List<Vector3> primitive) {
		if (primitive.Count == 0) {
			Console.WriteLine("Warning: Primitive has 0 vertices!");
			return new List<Vector2>();
		}
		if (primitive.Count < 3) {
			Console.WriteLine("Warning: Primitive has > 0 and < 3 vertices!");
			return new List<Vector2>();
		}
		var vec2 = new List<Vector2>();
		// Define basis 0
		var basis0 = primitive[1] - primitive[0];
		basis0 = basis0 / basis0.Length();

		// Find the normal
		var normal = basis0.Cross(primitive[2] - primitive[0]);

		// Find basis 1
		var basis1 = basis0.Cross(normal);
		basis1 = basis1 / basis1.Length();

		// Calculate 2D primitive
		foreach (var v in primitive) {
			vec2.Add(new Vector2(basis0.Dot(v), basis1.Dot(v)));
		}

		return vec2;
	}

	public void AddPrimitive(int color, string texture, int[] vertexIndices, bool isSelectionPrimitive)
	{
		var dbgVecArr = new List<Vector3>();

		var mesh = new ArrayMesh();
		var arr = new Godot.Collections.Array();
		var vertexList = new List<Vector3>();
		for (int i=0; i<vertexIndices.Length; i++) {
			vertexList.Add(vertices[vertexIndices[i]]);
			dbgVecArr.Add(vertices[vertexIndices[i]]);
		}

		// Turn poly into tris
		//var primitive2D = PrimitiveToVector2(vertexList);
		//int[] triIndices = Geometry.TriangulateDelaunay2d(primitive2D.ToArray());
		//var vertexArr = new Godot.Collections.Array();
		//vertexArr.Resize(triIndices.Length);
		//for (int i=0; i<triIndices.Length; i++) {
		//	vertexArr[i] = vertexList[triIndices[i]];
		//}

		var vertexArr = new Godot.Collections.Array(vertexList);

		// Create mesh node
		arr.Add(vertexArr);
		arr.Resize((int) ArrayMesh.ArrayType.Max);
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.LineLoop, arr);
		var m = new MeshInstance();
		m.Mesh = mesh;
		m.SetSurfaceMaterial(0, material);

		currentObject.Add(dbgVecArr);

		if (!isSelectionPrimitive) {
			selectedNode.AddChild(m);
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
		Console.Write($"{selectedNode.Name} -^");
		selectedNode = selectedNode.GetParent<Spatial>();
		Console.WriteLine($" {selectedNode.Name}");

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
		vertices.Clear();
		currentPosition.x = PosToSimScale(position.X);
		currentPosition.y = PosToSimScale(position.Y);
		currentPosition.z = PosToSimScale(position.Z);
		var newChildNode = new Spatial();
		newChildNode.Name = name;
		newChildNode.Transform = new Transform(Quat.Identity, currentPosition);
		selectedNode.AddChild(newChildNode);
		Console.Write($"{selectedNode.Name} ->");
		selectedNode = newChildNode;
		Console.WriteLine($" {selectedNode.Name}");
		//Console.WriteLine($"Raw X: {position.X}, New X: {currentPosition.x}");
		//Console.WriteLine($"CreateChild {name} {position.X} {position.Y} {position.Z}");
	}
}
