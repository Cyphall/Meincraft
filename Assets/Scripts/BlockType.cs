using UnityEngine;

public class BlockType
{
	public static readonly BlockType GRASS = new BlockType(new Vector2(0f, 0f), true);
	public static readonly BlockType IRON = new BlockType(new Vector2(0.25f, 0f), true);
	public static readonly BlockType WOOD = new BlockType(new Vector2(0.5f, 0f), true);
	public static readonly BlockType TEST = new BlockType(new Vector2(0.75f, 0.75f), true);
	public static readonly BlockType AIR = null;
	
	public Vector2 uvOffset;
	public bool opaque { get; }

	private BlockType(Vector2 uvOffset, bool opaque)
	{
		this.uvOffset = uvOffset;
		this.opaque = opaque;
	}
}