using System.Collections.Generic;
using UnityEngine;

public class World
{
	private GameObject _chunkPrefab;

	private Dictionary<Vector2Int, Chunk> _chunks = new Dictionary<Vector2Int, Chunk>();

	private int _seedX = Random.Range(1, 10000);
	private int _seedY = Random.Range(1, 10000);

	public World(GameObject chunkPrefab)
	{
		_chunkPrefab = chunkPrefab;

		const int renderDistance = 8;

		for (int x = -renderDistance; x < renderDistance; x++)
		{
			for (int y = -renderDistance; y < renderDistance; y++)
			{
				createChunk(new Vector2Int(x, y), true);
			}
		}

		foreach (Chunk chunk in _chunks.Values)
		{
			chunk.enable();
		}
	}

	private void createChunk(Vector2Int pos, bool worldInit)
	{
		if (_chunks.ContainsKey(pos))
		{
			Debug.LogError("A chunk has been overriden without prior deletion");
			destroyChunk(pos);
		}
		
		GameObject chunk = Object.Instantiate(_chunkPrefab, new Vector3Int(pos.x, 0, pos.y) * 16, Quaternion.identity);
		
		_chunks.Add(pos, chunk.GetComponent<Chunk>());
		
		getChunk(pos).init(pos, this, getNoise(pos), worldInit);
	}

	private void destroyChunk(Vector2Int pos)
	{
		if (!_chunks.ContainsKey(pos))
		{
			Debug.LogError("Deletion of a non-existing chunk has been requested");
			return;
		}

		Object.Destroy(_chunks[pos].gameObject);
		
		_chunks.Remove(pos);
	}
	
	public Chunk getChunk(Vector2Int pos)
	{
		return _chunks.ContainsKey(pos) ? _chunks[pos] : null;
	}

	public void placeBlock(Vector3Int pos, BlockType blockType)
	{
		if (pos.y < 0 || pos.y > 255) return;
		
		Vector2Int chunkPos = chunkPosFromBlockPos(pos);

		if (!_chunks.ContainsKey(chunkPos))
		{
			Debug.LogError("Chunk where block is placed doesn't exists");
			return;
		}

		_chunks[chunkPos].placeBlock(localBlockPosFromBlockPos(pos), blockType);
	}

	private Vector2Int chunkPosFromBlockPos(Vector3Int blockPos)
	{
		Vector2Int chunkPos = Vector2Int.zero;

		chunkPos.x = Mathf.FloorToInt(blockPos.x / 16.0f);
		chunkPos.y = Mathf.FloorToInt(blockPos.z / 16.0f);

		return chunkPos;
	}

	private Vector3Int localBlockPosFromBlockPos(Vector3Int blockPos)
	{
		Vector3Int localBlockPos = Vector3Int.zero;
		
		localBlockPos.x = Mathf.RoundToInt(Mathf.Repeat(blockPos.x, 16));
		localBlockPos.y = blockPos.y;
		localBlockPos.z = Mathf.RoundToInt(Mathf.Repeat(blockPos.z, 16));

		return localBlockPos;
	}

	private float[,] getNoise(Vector2Int chunkPos)
	{
		float[,] noise = new float[16,16];

		for (int x = 0; x < 16; x++)
		{
			for (int y = 0; y < 16; y++)
			{
				noise[x, y] = Mathf.PerlinNoise(chunkPos.x + x / 16f + _seedX, chunkPos.y + y / 16f + _seedY);
			}
		}

		return noise;
	}
}