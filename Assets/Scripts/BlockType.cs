using System.Collections.Generic;
using UnityEngine;

public class BlockType
{
	public static readonly List<BlockType> ALL = new List<BlockType>();
	
	public static readonly BlockType AIR = null;
	public static readonly BlockType STONE = new BlockType(new Vector2(0.75f, 0f));
	public static readonly BlockType GRASS = new BlockType(new Vector2(0f, 0f));
	public static readonly BlockType DIRT = new BlockType(new Vector2(0f, 0.25f));
	public static readonly BlockType WOOD = new BlockType(new Vector2(0.5f, 0f));
	public static readonly BlockType IRON = new BlockType(new Vector2(0.25f, 0f));

	public Vector2 uvOffset;

	private BlockType(Vector2 uvOffset)
	{
		this.uvOffset = uvOffset;
		ALL.Add(this);
	}
}