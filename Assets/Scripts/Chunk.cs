using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
	private MeshCollider _meshCollider;
	private MeshFilter _meshFilter;
	
	private NativeList<float3> _vertices;
	private NativeList<float2> _uvs;
	private NativeList<int> _triangles;

	public int2 chunkPos { get; private set; }
	
	private byte[] _blocks;

	public void init(int2 pos)
	{
		chunkPos = pos;
	}

	public void applyMesh()
	{
		Destroy(_meshFilter.sharedMesh);
		
		Mesh mesh = new Mesh();

		mesh.SetVertices<float3>(_vertices);
		mesh.SetUVs<float2>(0, _uvs);
		mesh.triangles = _triangles.ToArray();

		mesh.RecalculateNormals();
		
		_meshFilter.sharedMesh = mesh;
		_meshCollider.sharedMesh = mesh;
		
		mesh.UploadMeshData(true);

		freeMeshArrays();
	}

	public void setData(NativeArray<byte> blocks, NativeList<float3> vertices, NativeList<float2> uvs, NativeList<int> triangles)
	{
		_meshCollider = GetComponent<MeshCollider>();
		_meshFilter = GetComponent<MeshFilter>();
		
		_blocks = blocks.ToArray();
		blocks.Dispose();
		
		_vertices = vertices;
		_uvs = uvs;
		_triangles = triangles;
	}

	private void rebuildMesh()
	{
		_vertices = new NativeList<float3>(4096, Allocator.TempJob);
		_uvs = new NativeList<float2>(4096, Allocator.TempJob);
		_triangles = new NativeList<int>(4096, Allocator.TempJob);
		
		int vCount = 0;
        
		for (int z = 0; z < 16; z++)
		{
			for (int y = 0; y < 256; y++)
			{
				for (int x = 0; x < 16; x++)
				{
					if (_blocks[y + z * 256 + x * 4096] == BlockType.AIR) continue;

					float2 uvOffset = BlockType.types[_blocks[y + z * 256 + x * 4096]].uvOffset;

					// x + 1
					if ((x + 1 < 16 && _blocks[y + z * 256 + (x+1) * 4096] == BlockType.AIR) || x + 1 == 16)
					{
						vCount += 4;
						_vertices.Add(new float3(x+1, y, z));
						_vertices.Add(new float3(x+1, y+1, z));
						_vertices.Add(new float3(x+1, y+1, z+1));
						_vertices.Add(new float3(x+1, y, z+1));
						
						_triangles.Add(vCount-4);
						_triangles.Add(vCount-3);
						_triangles.Add(vCount-1);
						_triangles.Add(vCount-1);
						_triangles.Add(vCount-3);
						_triangles.Add(vCount-2);
						
						_uvs.Add(uvOffset + new float2(0.1875f, 0.125f - 0.0625f));
						_uvs.Add(uvOffset + new float2(0.1875f, 0.1875f - 0.0625f));
						_uvs.Add(uvOffset + new float2(0.25f, 0.1875f - 0.0625f));
						_uvs.Add(uvOffset + new float2(0.25f, 0.125f - 0.0625f));
					}
					// x - 1
					if ((x - 1 > -1 && _blocks[y + z * 256 + (x-1) * 4096] == BlockType.AIR) || x - 1 == -1)
					{
						vCount += 4;
						_vertices.Add(new float3(x, y, z + 1));
						_vertices.Add(new float3(x, y + 1, z + 1));
						_vertices.Add(new float3(x, y + 1, z));
						_vertices.Add(new float3(x, y, z));
						
						_triangles.Add(vCount-4);
						_triangles.Add(vCount-3);
						_triangles.Add(vCount-1);
						_triangles.Add(vCount-1);
						_triangles.Add(vCount-3);
						_triangles.Add(vCount-2);

						_uvs.Add(uvOffset + new float2(0.0625f, 0.125f - 0.0625f));
						_uvs.Add(uvOffset + new float2(0.0625f, 0.1875f - 0.0625f));
						_uvs.Add(uvOffset + new float2(0.125f, 0.1875f - 0.0625f));
						_uvs.Add(uvOffset + new float2(0.125f, 0.125f - 0.0625f));
					}
					// y + 1
					if ((y + 1 < 256 && _blocks[(y+1) + z * 256 + x * 4096] == BlockType.AIR) || y + 1 == 256)
					{
						vCount += 4;
						_vertices.Add(new float3(x, y + 1, z + 1));
						_vertices.Add(new float3(x + 1, y + 1, z + 1));
						_vertices.Add(new float3(x + 1, y + 1, z));
						_vertices.Add(new float3(x, y + 1, z));
						
						_triangles.Add(vCount-4);
						_triangles.Add(vCount-3);
						_triangles.Add(vCount-1);
						_triangles.Add(vCount-1);
						_triangles.Add(vCount-3);
						_triangles.Add(vCount-2);

						_uvs.Add(uvOffset + new float2(0.0625f, 0.1875f - 0.0625f));
						_uvs.Add(uvOffset + new float2(0.0625f, 0.25f - 0.0625f));
						_uvs.Add(uvOffset + new float2(0.125f, 0.25f - 0.0625f));
						_uvs.Add(uvOffset + new float2(0.125f, 0.1875f - 0.0625f));
					}
					// y - 1
					if ((y - 1 > -1 && _blocks[(y-1) + z * 256 + x * 4096] == BlockType.AIR) || y - 1 == -1)
					{
						vCount += 4;
						_vertices.Add(new float3(x + 1, y, z + 1));
						_vertices.Add(new float3(x, y, z + 1));
						_vertices.Add(new float3(x, y, z));
						_vertices.Add(new float3(x + 1, y, z));
						
						_triangles.Add(vCount-4);
						_triangles.Add(vCount-3);
						_triangles.Add(vCount-1);
						_triangles.Add(vCount-1);
						_triangles.Add(vCount-3);
						_triangles.Add(vCount-2);

						_uvs.Add(uvOffset + new float2(0.0625f, 0.0625f - 0.0625f));
						_uvs.Add(uvOffset + new float2(0.0625f, 0.125f - 0.0625f));
						_uvs.Add(uvOffset + new float2(0.125f, 0.125f - 0.0625f));
						_uvs.Add(uvOffset + new float2(0.125f, 0.0625f - 0.0625f));
					}
					// z + 1
					if ((z + 1 < 16 && _blocks[y + (z+1) * 256 + x * 4096] == BlockType.AIR) || z + 1 == 16)
					{
						vCount += 4;
						_vertices.Add(new float3(x + 1, y, z + 1));
						_vertices.Add(new float3(x + 1, y + 1, z + 1));
						_vertices.Add(new float3(x, y + 1, z + 1));
						_vertices.Add(new float3(x, y, z + 1));
						
						_triangles.Add(vCount-4);
						_triangles.Add(vCount-3);
						_triangles.Add(vCount-1);
						_triangles.Add(vCount-1);
						_triangles.Add(vCount-3);
						_triangles.Add(vCount-2);

						_uvs.Add(uvOffset + new float2(0f, 0.125f - 0.0625f));
						_uvs.Add(uvOffset + new float2(0f, 0.1875f - 0.0625f));
						_uvs.Add(uvOffset + new float2(0.0625f, 0.1875f - 0.0625f));
						_uvs.Add(uvOffset + new float2(0.0625f, 0.125f - 0.0625f));
					}
					// z - 1
					if ((z - 1 > -1 && _blocks[y + (z-1) * 256 + x * 4096] == BlockType.AIR) || z - 1 == -1)
					{
						vCount += 4;
						_vertices.Add(new float3(x, y, z));
						_vertices.Add(new float3(x, y + 1, z));
						_vertices.Add(new float3(x + 1, y + 1, z));
						_vertices.Add(new float3(x + 1, y, z));
						
						_triangles.Add(vCount-4);
						_triangles.Add(vCount-3);
						_triangles.Add(vCount-1);
						_triangles.Add(vCount-1);
						_triangles.Add(vCount-3);
						_triangles.Add(vCount-2);

						_uvs.Add(uvOffset + new float2(0.125f, 0.125f - 0.0625f));
						_uvs.Add(uvOffset + new float2(0.125f, 0.1875f - 0.0625f));
						_uvs.Add(uvOffset + new float2(0.1875f, 0.1875f - 0.0625f));
						_uvs.Add(uvOffset + new float2(0.1875f, 0.125f - 0.0625f));
					}
				}
			}
		}

		applyMesh();
	}

	private void setBlock(int3 pos, byte blockType)
	{
		if (pos.y < 0 || pos.y > 255)
		{
			Debug.LogError("Cannot place a block bellow height 0 or above height 255");
			return;
		}
		
		_blocks[pos.y + pos.z * 256 + pos.x * 4096] = blockType;
	}
	
	public void placeBlock(int3 pos, byte blockType)
	{
		setBlock(pos, blockType);
		
		rebuildMesh();
	}

	public void freeMeshArrays()
	{
		if (_vertices.IsCreated) _vertices.Dispose();
		if (_uvs.IsCreated) _uvs.Dispose();
		if (_triangles.IsCreated) _triangles.Dispose();
	}

	private void OnDestroy()
	{
		if (_meshFilter) Destroy(_meshFilter.sharedMesh);
		if (_meshCollider) Destroy(_meshCollider.sharedMesh);
		
		freeMeshArrays();
	}
}