using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Biomes
{
	public static float2 seed { get; private set; }

	public static void initSeed()
	{
		seed = new float2(Random.Range(1f, 10000f), Random.Range(1f, 10000f));
//		seed = new float2(123.4f, 567.8f);
	}

	public static void generateChunkBlocks(NativeArray<byte> blocks, int2 chunkPos, Func<int2, BiomeParams> biome)
	{
		BiomeParams biomeParams = biome(chunkPos);
		
		for (int x = 0; x < 16; x++)
		{
			for (int z = 0; z < 16; z++)
			{
				int targetY = biomeParams.minY + (int) (biomeParams.heightMap[x, z] * (biomeParams.maxY - biomeParams.minY));
				targetY = Math.Min(targetY, 256);
				
				for (int y = 0; y < targetY; y++)
				{
					if (targetY > biomeParams.rockMin + (biomeParams.rockMax - biomeParams.rockMin) * getRawNoise(chunkPos, new int2(x, z), 4) + 0.5f)
					{
						blocks[y + z * 256 + x * 4096] = BlockType.STONE;
					}
					else
					{
						if (y < targetY - 4)
						{
							blocks[y + z * 256 + x * 4096] = BlockType.STONE;
						}
						else if (y < targetY - 1)
						{
							blocks[y + z * 256 + x * 4096] = BlockType.DIRT;
						}
						else
						{
							blocks[y + z * 256 + x * 4096] = BlockType.GRASS;
						}
					}
				}
			}
		}
	}
	
	public static BiomeParams mountains(int2 chunkPos)
	{
		BiomeParams biomeParams = new BiomeParams
		{
			heightMap = new float[16, 16],
			
			minY = 10,
			maxY = 256,
			
			rockMin = 110,
			rockMax = 130
		};

		for (int x = 0; x<16; x++)
		{
			for (int y = 0; y<16; y++)
			{
				biomeParams.heightMap[x, y] = getNoise(chunkPos, new int2(x, y), 8, 8, persistance:0.4f);
			}
		}

		return biomeParams;
	}

	private static float getNoise(int2 chunkPos, int2 blockPos, int octaves, float scale, float lacunarity = 2f, float persistance = 0.5f)
	{
		float result = 0;
		
		float divider = 0;

		float frequency = 1;
		float amplitude = 1;
				
		for (int i = 0; i < octaves; i++)
		{
			result += getRawNoise(chunkPos, blockPos, scale, frequency, amplitude);
			
			divider += amplitude;
			
			frequency *= lacunarity;
			amplitude *= persistance;
		}

		result /= divider;

		return result + 0.5f;
	}
	
	private static float getRawNoise(int2 chunkPos, int2 blockPos, float scale, float frequency = 1f, float amplitude = 1f)
	{
		float2 noisePos = (seed + new float2(chunkPos) + new float2(blockPos) / 16f) * frequency / scale;
		
		return (Mathf.Clamp(Mathf.PerlinNoise(noisePos.x, noisePos.y), 0, 1) - 0.5f) * amplitude;
	}
}

public struct BiomeParams
{
	public float[,] heightMap;
	public int minY;
	public int maxY;

	public int rockMin;
	public int rockMax;
}