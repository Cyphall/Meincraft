using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
	private MeshCollider _meshCollider;
	private MeshRenderer _meshRenderer;
	private Mesh _mesh;
	private BlockType[,,] blocks {get; } = new BlockType[16, 256, 16];

	private Vector2Int chunkPos {get; set;}

	public void init(Vector2Int pos, float[,] noise)
	{
		gameObject.layer = 8;
		
		_mesh = new Mesh();
		
		GetComponent<MeshFilter>().mesh = _mesh;
		
		_meshRenderer = GetComponent<MeshRenderer>();
		_meshCollider = GetComponent<MeshCollider>();
		
		chunkPos = pos;
		
		for (int x = 0; x < 16; x++)
		{
			for (int z = 0; z < 16; z++)
			{
				for (int y = 0; y < 60 + (int)(noise[x, z] * 6); y++)
				{
					setBlock(new Vector3Int(x, y, z), BlockType.GRASS);
				}
			}
		}
		
		rebuildMesh();
		_meshRenderer.enabled = true;
	}

	private void rebuildMesh()
	{
		_mesh.Clear(false);
		
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector2> uvs = new List<Vector2>();

		int vCount = 0;
        
		for (int z = 0; z < 16; z++)
		{
			for (int y = 0; y < 256; y++)
			{
				for (int x = 0; x < 16; x++)
				{
					if (blocks[x, y, z] == null) continue;

					Vector2 uvOffset = blocks[x, y, z].uvOffset;

					// x + 1
					{
						if ((x + 1 < 16 && blocks[x + 1, y, z] == null) || x + 1 == 16)
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
					}
					// x - 1
					{
						if ((x - 1 > -1 && blocks[x - 1, y, z] == null) || x - 1 == -1)
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
					}
					// y + 1
					{
						if ((y + 1 < 256 && blocks[x, y + 1, z] == null) || y + 1 == 256)
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
					}
					// y - 1
					{
						if ((y - 1 > -1 && blocks[x, y - 1, z] == null) || y - 1 == -1)
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
					}
					// z + 1
					{
						if ((z + 1 < 16 && blocks[x, y, z + 1] == null) || z + 1 == 16)
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
					}
					// z - 1
					{
						if ((z - 1 > -1 && blocks[x, y, z - 1] == null) || z - 1 == -1)
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
		}

		_mesh.vertices = vertices.ToArray();
		_mesh.uv = uvs.ToArray();
		_mesh.triangles = triangles.ToArray();
		
		_mesh.RecalculateNormals();
		
		_meshCollider.sharedMesh = _mesh;
	}

	private static List<T> triFromQuad<T>(T v1, T v2, T v3, T v4)
	{
		return new List<T> {v1, v2, v4, v4, v2, v3};
	}

	private void setBlock(Vector3Int pos, BlockType blockType)
	{
		blocks[pos.x, pos.y, pos.z] = blockType;
	}
	
	public void placeBlock(Vector3Int pos, BlockType blockType)
	{
		setBlock(pos, blockType);
		
		rebuildMesh();
	}
}