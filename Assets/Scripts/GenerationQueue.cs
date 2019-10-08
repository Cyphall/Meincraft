using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public class GenerationQueue
{
	private List<ChunkData> _processingList;

	public GenerationQueue()
	{
		_processingList = new List<ChunkData>(1024);
	}

	public void enqueue(Chunk chunk)
	{
		ChunkData data = new ChunkData
		{
			chunk = chunk, // Non transmis au job
			
			chunkPos = new int2(chunk.chunkPos.x, chunk.chunkPos.y), // Non modifié par le job
			blocks = new NativeArray<byte>(65536, Allocator.Persistent),
			
			vertices = new NativeList<float3>(4096, Allocator.Persistent),
			uvs = new NativeList<float2>(4096, Allocator.Persistent),
			triangles = new NativeList<int>(4096, Allocator.Persistent)
		};
		
		ChunkJob job = new ChunkJob
		{
			chunkPos = data.chunkPos,
			blocks = data.blocks,
			
			vertices = data.vertices,
			uvs = data.uvs,
			triangles = data.triangles
		};

		data.handle = job.Schedule();
		
		_processingList.Add(data);
	}

	public bool tryDequeue(out Chunk chunk)
	{
		chunk = null;
		if (_processingList.Count != 0)
		{
			ChunkData chunkDataReady = null;
			foreach (ChunkData data in _processingList)
			{
				if (!data.handle.IsCompleted) continue;
			
				data.handle.Complete();
				chunkDataReady = data;
				break;
			}

			if (chunkDataReady == null) return false;
		
			chunkDataReady.chunk.setData(chunkDataReady.blocks, chunkDataReady.vertices, chunkDataReady.uvs, chunkDataReady.triangles);
			_processingList.Remove(chunkDataReady);
			chunk = chunkDataReady.chunk;
			return true;
		}

		return false;
	}
}

internal struct ChunkJob : IJob
{
	public int2 chunkPos;
	public NativeArray<byte> blocks;
	
	public NativeList<float3> vertices;
	public NativeList<float2> uvs;
	public NativeList<int> triangles;
	
	public void Execute()
	{
		Biomes.generateChunkBlocks(blocks, chunkPos, Biomes.mountains);
		
		int vCount = 0;
        
		for (int z = 0; z < 16; z++)
		{
			for (int y = 0; y < 256; y++)
			{
				for (int x = 0; x < 16; x++)
				{
					if (blocks[y + z * 256 + x * 4096] == BlockType.AIR) continue;

					float2 uvOffset = BlockType.types[blocks[y + z * 256 + x * 4096]].uvOffset;

					// x + 1
					if ((x + 1 < 16 && blocks[y + z * 256 + (x+1) * 4096] == BlockType.AIR) || x + 1 == 16)
					{
						vCount += 4;
						vertices.Add(new float3(x+1, y, z));
						vertices.Add(new float3(x+1, y+1, z));
						vertices.Add(new float3(x+1, y+1, z+1));
						vertices.Add(new float3(x+1, y, z+1));
						
						triangles.Add(vCount-4);
						triangles.Add(vCount-3);
						triangles.Add(vCount-1);
						triangles.Add(vCount-1);
						triangles.Add(vCount-3);
						triangles.Add(vCount-2);
						
						uvs.Add(uvOffset + new float2(0.1875f, 0.125f - 0.0625f));
						uvs.Add(uvOffset + new float2(0.1875f, 0.1875f - 0.0625f));
						uvs.Add(uvOffset + new float2(0.25f, 0.1875f - 0.0625f));
						uvs.Add(uvOffset + new float2(0.25f, 0.125f - 0.0625f));
					}
					// x - 1
					if ((x - 1 > -1 && blocks[y + z * 256 + (x-1) * 4096] == BlockType.AIR) || x - 1 == -1)
					{
						vCount += 4;
						vertices.Add(new float3(x, y, z + 1));
						vertices.Add(new float3(x, y + 1, z + 1));
						vertices.Add(new float3(x, y + 1, z));
						vertices.Add(new float3(x, y, z));
						
						triangles.Add(vCount-4);
						triangles.Add(vCount-3);
						triangles.Add(vCount-1);
						triangles.Add(vCount-1);
						triangles.Add(vCount-3);
						triangles.Add(vCount-2);

						uvs.Add(uvOffset + new float2(0.0625f, 0.125f - 0.0625f));
						uvs.Add(uvOffset + new float2(0.0625f, 0.1875f - 0.0625f));
						uvs.Add(uvOffset + new float2(0.125f, 0.1875f - 0.0625f));
						uvs.Add(uvOffset + new float2(0.125f, 0.125f - 0.0625f));
					}
					// y + 1
					if ((y + 1 < 256 && blocks[(y+1) + z * 256 + x * 4096] == BlockType.AIR) || y + 1 == 256)
					{
						vCount += 4;
						vertices.Add(new float3(x, y + 1, z + 1));
						vertices.Add(new float3(x + 1, y + 1, z + 1));
						vertices.Add(new float3(x + 1, y + 1, z));
						vertices.Add(new float3(x, y + 1, z));
						
						triangles.Add(vCount-4);
						triangles.Add(vCount-3);
						triangles.Add(vCount-1);
						triangles.Add(vCount-1);
						triangles.Add(vCount-3);
						triangles.Add(vCount-2);

						uvs.Add(uvOffset + new float2(0.0625f, 0.1875f - 0.0625f));
						uvs.Add(uvOffset + new float2(0.0625f, 0.25f - 0.0625f));
						uvs.Add(uvOffset + new float2(0.125f, 0.25f - 0.0625f));
						uvs.Add(uvOffset + new float2(0.125f, 0.1875f - 0.0625f));
					}
					// y - 1
					if ((y - 1 > -1 && blocks[(y-1) + z * 256 + x * 4096] == BlockType.AIR) || y - 1 == -1)
					{
						vCount += 4;
						vertices.Add(new float3(x + 1, y, z + 1));
						vertices.Add(new float3(x, y, z + 1));
						vertices.Add(new float3(x, y, z));
						vertices.Add(new float3(x + 1, y, z));
						
						triangles.Add(vCount-4);
						triangles.Add(vCount-3);
						triangles.Add(vCount-1);
						triangles.Add(vCount-1);
						triangles.Add(vCount-3);
						triangles.Add(vCount-2);

						uvs.Add(uvOffset + new float2(0.0625f, 0.0625f - 0.0625f));
						uvs.Add(uvOffset + new float2(0.0625f, 0.125f - 0.0625f));
						uvs.Add(uvOffset + new float2(0.125f, 0.125f - 0.0625f));
						uvs.Add(uvOffset + new float2(0.125f, 0.0625f - 0.0625f));
					}
					// z + 1
					if ((z + 1 < 16 && blocks[y + (z+1) * 256 + x * 4096] == BlockType.AIR) || z + 1 == 16)
					{
						vCount += 4;
						vertices.Add(new float3(x + 1, y, z + 1));
						vertices.Add(new float3(x + 1, y + 1, z + 1));
						vertices.Add(new float3(x, y + 1, z + 1));
						vertices.Add(new float3(x, y, z + 1));
						
						triangles.Add(vCount-4);
						triangles.Add(vCount-3);
						triangles.Add(vCount-1);
						triangles.Add(vCount-1);
						triangles.Add(vCount-3);
						triangles.Add(vCount-2);

						uvs.Add(uvOffset + new float2(0f, 0.125f - 0.0625f));
						uvs.Add(uvOffset + new float2(0f, 0.1875f - 0.0625f));
						uvs.Add(uvOffset + new float2(0.0625f, 0.1875f - 0.0625f));
						uvs.Add(uvOffset + new float2(0.0625f, 0.125f - 0.0625f));
					}
					// z - 1
					if ((z - 1 > -1 && blocks[y + (z-1) * 256 + x * 4096] == BlockType.AIR) || z - 1 == -1)
					{
						vCount += 4;
						vertices.Add(new float3(x, y, z));
						vertices.Add(new float3(x, y + 1, z));
						vertices.Add(new float3(x + 1, y + 1, z));
						vertices.Add(new float3(x + 1, y, z));
						
						triangles.Add(vCount-4);
						triangles.Add(vCount-3);
						triangles.Add(vCount-1);
						triangles.Add(vCount-1);
						triangles.Add(vCount-3);
						triangles.Add(vCount-2);

						uvs.Add(uvOffset + new float2(0.125f, 0.125f - 0.0625f));
						uvs.Add(uvOffset + new float2(0.125f, 0.1875f - 0.0625f));
						uvs.Add(uvOffset + new float2(0.1875f, 0.1875f - 0.0625f));
						uvs.Add(uvOffset + new float2(0.1875f, 0.125f - 0.0625f));
					}
				}
			}
		}
	}
}

internal class ChunkData
{
	public Chunk chunk;
	public JobHandle handle;
	
	public int2 chunkPos;
	public NativeArray<byte> blocks;
	
	public NativeList<float3> vertices;
	public NativeList<float2> uvs;
	public NativeList<int> triangles;
}