using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
	private MeshCollider _meshCollider;
	private MeshFilter _meshFilter;
	
	private List<Vector3> _vertices = new List<Vector3>();
	private List<int> _triangles = new List<int>();
	private List<Vector2> _uvs = new List<Vector2>();
	
	public BlockType[,,] blocks = new BlockType[16, 256, 16];

	public void init()
	{
		gameObject.layer = 8;

		_meshCollider = GetComponent<MeshCollider>();
		_meshFilter = GetComponent<MeshFilter>();
	}

	public void applyMesh()
	{
		Mesh mesh = new Mesh {vertices = _vertices.ToArray(), uv = _uvs.ToArray(), triangles = _triangles.ToArray()};

		mesh.RecalculateNormals();
		
		_meshFilter.mesh = mesh;
		_meshCollider.sharedMesh = mesh;
		
		_vertices.Clear();
		_uvs.Clear();
		_triangles.Clear();
	}

	public void generate(float[,] noise)
	{
		for (int x = 0; x < 16; x++)
		{
			for (int z = 0; z < 16; z++)
			{
				int maxY = 30 + (int) (noise[x, z] * 100);
				
				for (int y = 0; y <= maxY; y++)
				{
					if (y <= maxY - 4)
					{
						setBlock(new Vector3Int(x, y, z), BlockType.STONE);
					}
					else if (y <= maxY - 1)
					{
						setBlock(new Vector3Int(x, y, z), BlockType.DIRT);
					}
					else
					{
						setBlock(new Vector3Int(x, y, z), BlockType.GRASS);
					}
				}
			}
		}
	}

	public void rebuildMesh()
	{
		int vCount = 0;
        
		for (int z = 0; z < 16; z++)
		{
			for (int y = 0; y < 256; y++)
			{
				for (int x = 0; x < 16; x++)
				{
					if (blocks[x, y, z] == BlockType.AIR) continue;

					Vector2 uvOffset = blocks[x, y, z].uvOffset;

					// x + 1
					if ((x + 1 < 16 && blocks[x + 1, y, z] == BlockType.AIR) || x + 1 == 16)
					{
						vCount += 4;
						_vertices.Add(new Vector3(x+1, y, z));
						_vertices.Add(new Vector3(x+1, y+1, z));
						_vertices.Add(new Vector3(x+1, y+1, z+1));
						_vertices.Add(new Vector3(x+1, y, z+1));
						_triangles.AddRange(triFromQuad(vCount-4, vCount-3, vCount-2, vCount-1));
						_uvs.Add(uvOffset + new Vector2(0.1875f, 0.125f - 0.0625f));
						_uvs.Add(uvOffset + new Vector2(0.1875f, 0.1875f - 0.0625f));
						_uvs.Add(uvOffset + new Vector2(0.25f, 0.1875f - 0.0625f));
						_uvs.Add(uvOffset + new Vector2(0.25f, 0.125f - 0.0625f));
					}
					// x - 1
					if ((x - 1 > -1 && blocks[x - 1, y, z] == BlockType.AIR) || x - 1 == -1)
					{
						vCount += 4;
						_vertices.Add(new Vector3(x, y, z + 1));
						_vertices.Add(new Vector3(x, y + 1, z + 1));
						_vertices.Add(new Vector3(x, y + 1, z));
						_vertices.Add(new Vector3(x, y, z));
						_triangles.AddRange(triFromQuad(vCount-4, vCount-3, vCount-2, vCount-1));
						_uvs.Add(uvOffset + new Vector2(0.0625f, 0.125f - 0.0625f));
						_uvs.Add(uvOffset + new Vector2(0.0625f, 0.1875f - 0.0625f));
						_uvs.Add(uvOffset + new Vector2(0.125f, 0.1875f - 0.0625f));
						_uvs.Add(uvOffset + new Vector2(0.125f, 0.125f - 0.0625f));
					}
					// y + 1
					if ((y + 1 < 256 && blocks[x, y + 1, z] == BlockType.AIR) || y + 1 == 256)
					{
						vCount += 4;
						_vertices.Add(new Vector3(x, y + 1, z + 1));
						_vertices.Add(new Vector3(x + 1, y + 1, z + 1));
						_vertices.Add(new Vector3(x + 1, y + 1, z));
						_vertices.Add(new Vector3(x, y + 1, z));
						_triangles.AddRange(triFromQuad(vCount-4, vCount-3, vCount-2, vCount-1));
						_uvs.Add(uvOffset + new Vector2(0.0625f, 0.1875f - 0.0625f));
						_uvs.Add(uvOffset + new Vector2(0.0625f, 0.25f - 0.0625f));
						_uvs.Add(uvOffset + new Vector2(0.125f, 0.25f - 0.0625f));
						_uvs.Add(uvOffset + new Vector2(0.125f, 0.1875f - 0.0625f));
					}
					// y - 1
					if ((y - 1 > -1 && blocks[x, y - 1, z] == BlockType.AIR) || y - 1 == -1)
					{
						vCount += 4;
						_vertices.Add(new Vector3(x + 1, y, z + 1));
						_vertices.Add(new Vector3(x, y, z + 1));
						_vertices.Add(new Vector3(x, y, z));
						_vertices.Add(new Vector3(x + 1, y, z));
						_triangles.AddRange(triFromQuad(vCount-4, vCount-3, vCount-2, vCount-1));
						_uvs.Add(uvOffset + new Vector2(0.0625f, 0.0625f - 0.0625f));
						_uvs.Add(uvOffset + new Vector2(0.0625f, 0.125f - 0.0625f));
						_uvs.Add(uvOffset + new Vector2(0.125f, 0.125f - 0.0625f));
						_uvs.Add(uvOffset + new Vector2(0.125f, 0.0625f - 0.0625f));
					}
					// z + 1
					if ((z + 1 < 16 && blocks[x, y, z + 1] == BlockType.AIR) || z + 1 == 16)
					{
						vCount += 4;
						_vertices.Add(new Vector3(x + 1, y, z + 1));
						_vertices.Add(new Vector3(x + 1, y + 1, z + 1));
						_vertices.Add(new Vector3(x, y + 1, z + 1));
						_vertices.Add(new Vector3(x, y, z + 1));
						_triangles.AddRange(triFromQuad(vCount-4, vCount-3, vCount-2, vCount-1));
						_uvs.Add(uvOffset + new Vector2(0f, 0.125f - 0.0625f));
						_uvs.Add(uvOffset + new Vector2(0f, 0.1875f - 0.0625f));
						_uvs.Add(uvOffset + new Vector2(0.0625f, 0.1875f - 0.0625f));
						_uvs.Add(uvOffset + new Vector2(0.0625f, 0.125f - 0.0625f));
					}
					// z - 1
					if ((z - 1 > -1 && blocks[x, y, z - 1] == BlockType.AIR) || z - 1 == -1)
					{
						vCount += 4;
						_vertices.Add(new Vector3(x, y, z));
						_vertices.Add(new Vector3(x, y + 1, z));
						_vertices.Add(new Vector3(x + 1, y + 1, z));
						_vertices.Add(new Vector3(x + 1, y, z));
						_triangles.AddRange(triFromQuad(vCount-4, vCount-3, vCount-2, vCount-1));
						_uvs.Add(uvOffset + new Vector2(0.125f, 0.125f - 0.0625f));
						_uvs.Add(uvOffset + new Vector2(0.125f, 0.1875f - 0.0625f));
						_uvs.Add(uvOffset + new Vector2(0.1875f, 0.1875f - 0.0625f));
						_uvs.Add(uvOffset + new Vector2(0.1875f, 0.125f - 0.0625f));
					}
				}
			}
		}
	}

	public static List<T> triFromQuad<T>(T v1, T v2, T v3, T v4)
	{
		return new List<T> {v1, v2, v4, v4, v2, v3};
	}

	private void setBlock(Vector3Int pos, BlockType blockType)
	{
		blocks[pos.x, pos.y, pos.z] = blockType;
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
}