//using Godot;
using System;
using System.IO;
using System.Collections.Generic;

public class Load3DO
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	private static string palettePath = @"C:\Users\oscar\HPIs\totala1.hpi_out\palettes\PALETTE.PAL";
	
	public static void Load(string filepath, TAUtil._3do.IModelReaderAdapter adapter) {
		var colors = new List<Color>();
		using (FileStream sourceStream = File.Open(palettePath, FileMode.Open))
		{
			var streamReader = new BinaryReader(sourceStream);

			for (int i=0; i<256; i++)
			{
				var c = new Color();
				c.r = streamReader.ReadByte();
				c.g = streamReader.ReadByte();
				c.b = streamReader.ReadByte();
				c.a = 255;
				// Read another byte - AF ignores this, so I will too
				streamReader.ReadByte();
				colors.Add(c);
				Console.WriteLine(c);
			}
		}
		
		using (FileStream sourceStream = File.Open(filepath, FileMode.Open))
		{
			var streamReader = new BinaryReader(sourceStream);
			var modelReader = new TAUtil._3do.ModelReader(streamReader, adapter);
			modelReader.Read();
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}


struct Color
{
	public int r;
	public int g;
	public int b;
	public int a;
	public System.Numerics.Vector4 ToVector()
	{
		var v = new System.Numerics.Vector4();
		v.W = r;
		v.X = g;
		v.Y = b;
		v.Z = a;
		return v;
	}
}

