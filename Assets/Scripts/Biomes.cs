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
			case BiomeType.MOUNTAINS:
				biomeParams = mountains(chunkPos);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(biomeType), biomeType, null);
		}
		
		BlockType[,,] blocks = new BlockType[16, 256, 16];
		
		for (int x = 0; x < 16; x++)
		{
			for (int z = 0; z < 16; z++)
			{
				int targetY = biomeParams.minY + (int) (biomeParams.heightMap[x, z] * (biomeParams.maxY - biomeParams.minY));
				targetY = Math.Min(targetY, 256);
				
				for (int y = 0; y < targetY; y++)
				{
					if (targetY > biomeParams.rockMin + (biomeParams.rockMax - biomeParams.rockMin) * getNoise(chunkPos, new Vector2Int(x, z), 1, 1, 4))
					{
						blocks[x, y, z] = BlockType.STONE;
					}
					else
					{
						if (y < targetY - 4)
						{
							blocks[x, y, z] = BlockType.STONE;
						}
						else if (y < targetY - 1)
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
		}

		return blocks;
	}
	
	private static BiomeParams mountains(Vector2Int chunkPos)
	{
		BiomeParams biomeParams = new BiomeParams
		{
			heightMap = new float[16, 16],
			
			minY = 60,
			maxY = 180,
			
			rockMin = 120,
			rockMax = 140
		};

		int octaves = 5;

		float lacunarity = 2f;
		float persistance = 0.5f;
		
		for (int x = 0; x<16; x++)
		{
			for (int y = 0; y<16; y++)
			{
				float divider = 0;
				
				for (int i = 0; i < octaves; i++)
				{
					float frequency = Mathf.Pow(lacunarity, i);
					float amplitude = Mathf.Pow(persistance, i);
					biomeParams.heightMap[x, y] += getNoise(chunkPos, new Vector2Int(x, y), frequency, amplitude, 8);
					divider += amplitude;
				}

				biomeParams.heightMap[x, y] /= divider;
				biomeParams.heightMap[x, y] += 0.5f;
			}
		}

		return biomeParams;
	}
	
	private static float getNoise(Vector2Int chunkPos, Vector2Int blockPos, float frequency, float amplitude, float scale)
	{
		return (Math.Min(Mathf.PerlinNoise((_seed.x + chunkPos.x + blockPos.x / 16f) * frequency / scale, (_seed.y + chunkPos.y + blockPos.y / 16f) * frequency / scale), 1) - 0.5f) * amplitude;
	}
}

public enum BiomeType
{
	MOUNTAINS
}

internal struct BiomeParams
{
	public float[,] heightMap;
	public int minY;
	public int maxY;

	public int rockMin;
	public int rockMax;
}