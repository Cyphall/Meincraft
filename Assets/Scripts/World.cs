using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class World
{
	private GameObject _chunkPrefab;
	private Player _player;

	private Dictionary<Vector2Int, Chunk> _chunks = new Dictionary<Vector2Int, Chunk>();
	private ConcurrentQueue<Chunk> _applyQueue = new ConcurrentQueue<Chunk>();

	public World(GameObject chunkPrefab, GameObject playerPrefab)
	{
		_chunkPrefab = chunkPrefab;
		Biomes.initSeed();

		const int renderDistance = 8;

		for (int x = 0; x < renderDistance; x++)
		{
			for (int y = -renderDistance; y < renderDistance; y++)
			{
				createChunk(new Vector2Int(x, y));
			}
		}
		for (int x = -renderDistance; x < 0; x++)
		{
			for (int y = -renderDistance; y < renderDistance; y++)
			{
				createChunk(new Vector2Int(x, y));
			}
		}
		
		_player = Object.Instantiate(playerPrefab, new Vector3(8, 256, 8), Quaternion.identity).GetComponent<Player>();
		_player.setWorld(this);
	}

	private void createChunk(Vector2Int chunkPos)
	{
		if (_chunks.ContainsKey(chunkPos))
		{
			Debug.LogError("A chunk has been overriden without prior deletion");
			destroyChunk(chunkPos);
		}
		
		Chunk chunk = Object.Instantiate(_chunkPrefab, new Vector3Int(chunkPos.x, 0, chunkPos.y) * 16, Quaternion.identity).GetComponent<Chunk>();
		
		_chunks.Add(chunkPos, chunk);
		
		chunk.init();
		
		ThreadPool.QueueUserWorkItem(state => generateChunk(chunk, Biomes.generateChunkBlocks(chunkPos, BiomeType.GREENHILLS2), _applyQueue));
	}

	private void destroyChunk(Vector2Int chunkPos)
	{
		if (!_chunks.ContainsKey(chunkPos))
		{
			Debug.LogError("Deletion of a non-existing chunk has been requested");
			return;
		}

		Object.Destroy(_chunks[chunkPos].gameObject);
		
		_chunks.Remove(chunkPos);
	}

	public void placeBlock(Vector3Int blockPos, BlockType blockType)
	{
		Vector2Int chunkPos = chunkPosFromBlockPos(blockPos);

		if (!_chunks.ContainsKey(chunkPos))
		{
			Debug.LogError("Chunk where block is placed doesn't exists");
			return;
		}

		_chunks[chunkPos].placeBlock(localBlockPosFromBlockPos(blockPos), blockType);
	}

	private static Vector2Int chunkPosFromBlockPos(Vector3Int blockPos)
	{
		Vector2Int chunkPos = Vector2Int.zero;

		chunkPos.x = Mathf.FloorToInt(blockPos.x / 16.0f);
		chunkPos.y = Mathf.FloorToInt(blockPos.z / 16.0f);

		return chunkPos;
	}

	private static Vector3Int localBlockPosFromBlockPos(Vector3Int blockPos)
	{
		Vector3Int localBlockPos = Vector3Int.zero;
		
		localBlockPos.x = Mathf.RoundToInt(Mathf.Repeat(blockPos.x, 16));
		localBlockPos.y = blockPos.y;
		localBlockPos.z = Mathf.RoundToInt(Mathf.Repeat(blockPos.z, 16));

		return localBlockPos;
	}

	public void fixedUpdate()
	{
		if (_player.transform.position.y < -10)
		{
			_player.transform.position = _player.spawnPos;
		}
		
		if (_applyQueue.TryDequeue(out Chunk chunk))
		{
			chunk.applyMesh();
		}
	}

	public static void generateChunk(Chunk chunk, BlockType[,,] blocks, ConcurrentQueue<Chunk> applyQueue)
	{
		chunk.setBlocks(blocks);
		chunk.rebuildMesh();
		
		applyQueue.Enqueue(chunk);
	}
}