using System;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Biomes
{
	private static Vector2 _seed;

	public static void initSeed()
	{
		_seed = new Vector2(Random.Range(1f, 1000f), Random.Range(1f, 1000f));
	}

	public static BlockType[,,] generateChunkBlocks(Vector2Int chunkPos, BiomeType biomeType)
	{
		BiomeParams biomeParams;
		switch (biomeType)
		{
			case BiomeType.GREENHILLS:
				biomeParams = greenHills(chunkPos);
				break;
			case BiomeType.GREENHILLS2:
				biomeParams = greenHills2(chunkPos);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(biomeType), biomeType, null);
		}
		
		BlockType[,,] blocks = new BlockType[16, 256, 16];
		
		for (int x = 0; x < 16; x++)
		{
			for (int z = 0; z < 16; z++)
			{
				int maxY = biomeParams.minY + (int) (biomeParams.heightMap[x, z] * biomeParams.amplitude);
				maxY = Math.Min(maxY, 256);
				
				for (int y = 0; y < maxY; y++)
				{
					if (y < maxY - 4)
					{
						blocks[x, y, z] = BlockType.STONE;
					}
					else if (y < maxY - 1)
					{
						blocks[x, y, z] = BlockType.DIRT;
					}
					else
					{
						blocks[x, y, z] = BlockType.GRASS;
					}
				}
			}
		}

		return blocks;
	}
	
	private static BiomeParams greenHills(Vector2Int chunkPos)
	{
		BiomeParams biomeParams = new BiomeParams
		{
			heightMap = new float[16, 16],
			minY = 60,
			amplitude = 60
		};
		
		float[,] a = getNoise(chunkPos, _seed, 8);
		float[,] b = getNoise(chunkPos, _seed / 1.123f, 5 / 1.456f);
		
		for (int x = 0; x<16; x++)
		{
			for (int y = 0; y<16; y++)
			{
				biomeParams.heightMap[x, y] = (a[x, y] + b[x, y]) / 2;
			}
		}

		return biomeParams;
	}
	
	private static BiomeParams greenHills2(Vector2Int chunkPos)
	{
		BiomeParams biomeParams = new BiomeParams
		{
			heightMap = new float[16, 16],
			minY = 60,
			amplitude = 30
		};
		
		float[,] a = getNoise(chunkPos, _seed, 4);

		for (int x = 0; x<16; x++)
		{
			for (int y = 0; y<16; y++)
			{
				biomeParams.heightMap[x, y] = a[x, y];
			}
		}
		
		return biomeParams;
	}
	
	private static float[,] getNoise(Vector2Int chunkPos, Vector2 seed, float scale)
	{
		float[,] noise = new float[16, 16];

		for (int x = 0; x < 16; x++)
		{
			for (int y = 0; y < 16; y++)
			{
				noise[x, y] = Mathf.PerlinNoise((seed.x + chunkPos.x + x / 16f) / scale, (seed.y + chunkPos.y + y / 16f) / scale);
			}
		}

		return noise;
	}
}

public enum BiomeType
{
	GREENHILLS,
	GREENHILLS2
}

internal struct BiomeParams
{
	public float[,] heightMap;
	public int minY;
	public int amplitude;
}