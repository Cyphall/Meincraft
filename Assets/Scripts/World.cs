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

	private List<Vector2Int> _chunkRemoveQueue = new List<Vector2Int>();
	
	private const float RENDER_DISTANCE = 12 - 0.1f;

	public World(GameObject chunkPrefab, GameObject playerPrefab)
	{
		_chunkPrefab = chunkPrefab;
		Biomes.initSeed();
		
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
		
		ThreadPool.QueueUserWorkItem(state => generateChunk(chunk, Biomes.generateChunkBlocks(chunkPos, BiomeType.MOUNTAINS), _applyQueue));
	}

	private void destroyChunk(Vector2Int chunkPos)
	{
		if (!_chunks.ContainsKey(chunkPos))
		{
			Debug.LogError("Deletion of a non-existing chunk has been requested");
			return;
		}

		Object.Destroy(_chunks[chunkPos].gameObject);
		
		_chunkRemoveQueue.Add(chunkPos);
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
	
	private static Vector2Int chunkPosFromPlayerPos(Vector3 playerPos)
	{
		Vector2Int chunkPos = Vector2Int.zero;

		chunkPos.x = Mathf.FloorToInt(playerPos.x / 16.0f);
		chunkPos.y = Mathf.FloorToInt(playerPos.z / 16.0f);

		return chunkPos;
	}

	public void fixedUpdate()
	{
		Vector2Int chunkWithPlayer = chunkPosFromPlayerPos(_player.transform.position);

		foreach (KeyValuePair<Vector2Int, Chunk> pair in _chunks)
		{
			if (Vector2Int.Distance(pair.Key, chunkWithPlayer) > RENDER_DISTANCE)
			{
				destroyChunk(pair.Key);
			}
		}

		foreach (Vector2Int chunkPos in _chunkRemoveQueue)
		{
			_chunks.Remove(chunkPos);
		}
		_chunkRemoveQueue.Clear();

		for (int x = chunkWithPlayer.x ; x <= chunkWithPlayer.x + RENDER_DISTANCE; x++)
		{
			for (int y = chunkWithPlayer.y ; y <= chunkWithPlayer.y + RENDER_DISTANCE; y++)
			{
				if ((chunkWithPlayer.x - x) * (chunkWithPlayer.x - x) + (chunkWithPlayer.y - y) * (chunkWithPlayer.y - y) > RENDER_DISTANCE * RENDER_DISTANCE) continue;
				
				int xSym = chunkWithPlayer.x - (x - chunkWithPlayer.x);
				int ySym = chunkWithPlayer.y - (y - chunkWithPlayer.y);
				
				Vector2Int pos1 = new Vector2Int(x, y);
				Vector2Int pos2 = new Vector2Int(x, ySym);
				Vector2Int pos3 = new Vector2Int(xSym, y);
				Vector2Int pos4 = new Vector2Int(xSym, ySym);

				if (!_chunks.ContainsKey(pos1))
				{
					createChunk(pos1);
				}
				if (!_chunks.ContainsKey(pos2))
				{
					createChunk(pos2);
				}
				if (!_chunks.ContainsKey(pos3))
				{
					createChunk(pos3);
				}
				if (!_chunks.ContainsKey(pos4))
				{
					createChunk(pos4);
				}
			}
		}
		
		if (_player.transform.position.y < -10)
		{
			Transform transform = _player.transform;
			Vector3 position = transform.position;
			transform.position = new Vector3(position.x, _player.spawnPos.y, position.z);
		}
		
		if (_applyQueue.TryDequeue(out Chunk chunk))
		{
			if (chunk)
				chunk.applyMesh();
		}
	}

	private static void generateChunk(Chunk chunk, BlockType[,,] blocks, ConcurrentQueue<Chunk> applyQueue)
	{
		chunk.setBlocks(blocks);
		chunk.rebuildMesh();
		
		applyQueue.Enqueue(chunk);
	}
}