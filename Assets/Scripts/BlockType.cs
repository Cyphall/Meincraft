using System.Collections.Generic;
using Unity.Mathematics;

public class BlockType
{
	public static Dictionary<byte, BlockType> types;

	public const byte AIR = 0;
	public const byte STONE = 1;
	public const byte GRASS = 2;
	public const byte DIRT = 3;
	public const byte WOOD = 4;
	public const byte IRON = 5;

	public float2 uvOffset;

	static BlockType()
	{
		types = new Dictionary<byte, BlockType>
		{
			{AIR, null},
			{STONE, new BlockType(new float2(0.75f, 0f))},
			{GRASS, new BlockType(new float2(0f, 0f))},
			{DIRT, new BlockType(new float2(0f, 0.25f))},
			{WOOD, new BlockType(new float2(0.5f, 0f))},
			{IRON, new BlockType(new float2(0.25f, 0f))}
		};
	}

	private BlockType(float2 uvOffset)
	{
		this.uvOffset = uvOffset;
	}
}