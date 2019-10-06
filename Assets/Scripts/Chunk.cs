using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
	private MeshCollider _meshCollider;
	private MeshFilter _meshFilter;
	
	private Vector3[] _vertices;
	private Vector2[] _uvs;
	private int[] _triangles;

	public Vector2Int chunkPos { get; private set; }
	
	private BlockType[,,] _blocks;

	public void init(Vector2Int pos)
	{
		chunkPos = pos;
		
		_meshCollider = GetComponent<MeshCollider>();
		_meshFilter = GetComponent<MeshFilter>();
	}

	public void applyMesh()
	{
		Destroy(_meshFilter.sharedMesh);
		
		Mesh mesh = new Mesh {vertices = _vertices, uv = _uvs, triangles = _triangles};
		
		// à activer quand on aura plus de lag avec le multithreading
		// mesh.Optimize();

		mesh.RecalculateNormals();
		
		_meshFilter.sharedMesh = mesh;
		_meshCollider.sharedMesh = mesh;
		
		mesh.UploadMeshData(true);

		_vertices = null;
		_uvs = null;
		_triangles = null;
	}

	public void setBlocks(BlockType[,,] blocks)
	{
		_blocks = blocks;
	}

	public void rebuildMesh()
	{
		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> triangles = new List<int>();
		
		int vCount = 0;
        
		for (int z = 0; z < 16; z++)
		{
			for (int y = 0; y < 256; y++)
			{
				for (int x = 0; x < 16; x++)
				{
					if (_blocks[x, y, z] == BlockType.AIR) continue;

					Vector2 uvOffset = _blocks[x, y, z].uvOffset;

					// x + 1
					if ((x + 1 < 16 && _blocks[x + 1, y, z] == BlockType.AIR) || x + 1 == 16)
					{
						vCount += 4;
						vertices.Add(new Vector3(x+1, y, z));
						vertices.Add(new Vector3(x+1, y+1, z));
						vertices.Add(new Vector3(x+1, y+1, z+1));
						vertices.Add(new Vector3(x+1, y, z+1));
						triangles.AddRange(triFromQuad(vCount-4, vCount-3, vCount-2, vCount-1));
						uvs.Add(uvOffset + new Vector2(0.1875f, 0.125f - 0.0625f));
						uvs.Add(uvOffset + new Vector2(0.1875f, 0.1875f - 0.0625f));
						uvs.Add(uvOffset + new Vector2(0.25f, 0.1875f - 0.0625f));
						uvs.Add(uvOffset + new Vector2(0.25f, 0.125f - 0.0625f));
					}
					// x - 1
					if ((x - 1 > -1 && _blocks[x - 1, y, z] == BlockType.AIR) || x - 1 == -1)
					{
						vCount += 4;
						vertices.Add(new Vector3(x, y, z + 1));
						vertices.Add(new Vector3(x, y + 1, z + 1));
						vertices.Add(new Vector3(x, y + 1, z));
						vertices.Add(new Vector3(x, y, z));
						triangles.AddRange(triFromQuad(vCount-4, vCount-3, vCount-2, vCount-1));
						uvs.Add(uvOffset + new Vector2(0.0625f, 0.125f - 0.0625f));
						uvs.Add(uvOffset + new Vector2(0.0625f, 0.1875f - 0.0625f));
						uvs.Add(uvOffset + new Vector2(0.125f, 0.1875f - 0.0625f));
						uvs.Add(uvOffset + new Vector2(0.125f, 0.125f - 0.0625f));
					}
					// y + 1
					if ((y + 1 < 256 && _blocks[x, y + 1, z] == BlockType.AIR) || y + 1 == 256)
					{
						vCount += 4;
						vertices.Add(new Vector3(x, y + 1, z + 1));
						vertices.Add(new Vector3(x + 1, y + 1, z + 1));
						vertices.Add(new Vector3(x + 1, y + 1, z));
						vertices.Add(new Vector3(x, y + 1, z));
						triangles.AddRange(triFromQuad(vCount-4, vCount-3, vCount-2, vCount-1));
						uvs.Add(uvOffset + new Vector2(0.0625f, 0.1875f - 0.0625f));
						uvs.Add(uvOffset + new Vector2(0.0625f, 0.25f - 0.0625f));
						uvs.Add(uvOffset + new Vector2(0.125f, 0.25f - 0.0625f));
						uvs.Add(uvOffset + new Vector2(0.125f, 0.1875f - 0.0625f));
					}
					// y - 1
					if ((y - 1 > -1 && _blocks[x, y - 1, z] == BlockType.AIR) || y - 1 == -1)
					{
						vCount += 4;
						vertices.Add(new Vector3(x + 1, y, z + 1));
						vertices.Add(new Vector3(x, y, z + 1));
						vertices.Add(new Vector3(x, y, z));
						vertices.Add(new Vector3(x + 1, y, z));
						triangles.AddRange(triFromQuad(vCount-4, vCount-3, vCount-2, vCount-1));
						uvs.Add(uvOffset + new Vector2(0.0625f, 0.0625f - 0.0625f));
						uvs.Add(uvOffset + new Vector2(0.0625f, 0.125f - 0.0625f));
						uvs.Add(uvOffset + new Vector2(0.125f, 0.125f - 0.0625f));
						uvs.Add(uvOffset + new Vector2(0.125f, 0.0625f - 0.0625f));
					}
					// z + 1
					if ((z + 1 < 16 && _blocks[x, y, z + 1] == BlockType.AIR) || z + 1 == 16)
					{
						vCount += 4;
						vertices.Add(new Vector3(x + 1, y, z + 1));
						vertices.Add(new Vector3(x + 1, y + 1, z + 1));
						vertices.Add(new Vector3(x, y + 1, z + 1));
						vertices.Add(new Vector3(x, y, z + 1));
						triangles.AddRange(triFromQuad(vCount-4, vCount-3, vCount-2, vCount-1));
						uvs.Add(uvOffset + new Vector2(0f, 0.125f - 0.0625f));
						uvs.Add(uvOffset + new Vector2(0f, 0.1875f - 0.0625f));
						uvs.Add(uvOffset + new Vector2(0.0625f, 0.1875f - 0.0625f));
						uvs.Add(uvOffset + new Vector2(0.0625f, 0.125f - 0.0625f));
					}
					// z - 1
					if ((z - 1 > -1 && _blocks[x, y, z - 1] == BlockType.AIR) || z - 1 == -1)
					{
						vCount += 4;
						vertices.Add(new Vector3(x, y, z));
						vertices.Add(new Vector3(x, y + 1, z));
						vertices.Add(new Vector3(x + 1, y + 1, z));
						vertices.Add(new Vector3(x + 1, y, z));
						triangles.AddRange(triFromQuad(vCount-4, vCount-3, vCount-2, vCount-1));
						uvs.Add(uvOffset + new Vector2(0.125f, 0.125f - 0.0625f));
						uvs.Add(uvOffset + new Vector2(0.125f, 0.1875f - 0.0625f));
						uvs.Add(uvOffset + new Vector2(0.1875f, 0.1875f - 0.0625f));
						uvs.Add(uvOffset + new Vector2(0.1875f, 0.125f - 0.0625f));
					}
				}
			}
		}

		_vertices = vertices.ToArray();
		_uvs = uvs.ToArray();
		_triangles = triangles.ToArray();
	}

	private static List<T> triFromQuad<T>(T v1, T v2, T v3, T v4)
	{
		return new List<T> {v1, v2, v4, v4, v2, v3};
	}

	private void setBlock(Vector3Int pos, BlockType blockType)
	{
		_blocks[pos.x, pos.y, pos.z] = blockType;
	}
	
	public void placeBlock(Vector3Int pos, BlockType blockType)
	{
		if (pos.y < 0 || pos.y > 255)
		{
			Debug.LogError("Cannot place a block bellow height 0 or above height 255");
			return;
		}
		
		setBlock(pos, blockType);
		
		rebuildMesh();
		applyMesh();
	}

	private void OnDestroy()
	{
		Destroy(_meshFilter.sharedMesh);
	}
}