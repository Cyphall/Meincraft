using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class World
{
	private GameObject _chunkPrefab;
	private Player _player;

	private readonly Vector2 _seed = new Vector2(Random.Range(1f, 1000f), Random.Range(1f, 1000f));
	private const float SCALE = 6;

	private Dictionary<Vector2Int, Chunk> _chunks = new Dictionary<Vector2Int, Chunk>();
	private ConcurrentQueue<Chunk> _applyQueue = new ConcurrentQueue<Chunk>();

	public World(GameObject chunkPrefab, GameObject playerPrefab)
	{
		_chunkPrefab = chunkPrefab;

		const int renderDistance = 16;

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

	private void createChunk(Vector2Int pos)
	{
		if (_chunks.ContainsKey(pos))
		{
			Debug.LogError("A chunk has been overriden without prior deletion");
			destroyChunk(pos);
		}
		
		Chunk chunk = Object.Instantiate(_chunkPrefab, new Vector3Int(pos.x, 0, pos.y) * 16, Quaternion.identity).GetComponent<Chunk>();
		
		_chunks.Add(pos, chunk);
		
		chunk.init();
		
		ThreadPool.QueueUserWorkItem(state => generateChunk(chunk, gen2(getNoise(pos, _seed, SCALE), getNoise(pos, _seed / 1.123f, SCALE / 1.456f)), _applyQueue));
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

	public void placeBlock(Vector3Int pos, BlockType blockType)
	{
		Vector2Int chunkPos = chunkPosFromBlockPos(pos);

		if (!_chunks.ContainsKey(chunkPos))
		{
			Debug.LogError("Chunk where block is placed doesn't exists");
			return;
		}

		_chunks[chunkPos].placeBlock(localBlockPosFromBlockPos(pos), blockType);
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

	private static float[,] getNoise(Vector2Int chunkPos, Vector2 seed, float scale)
	{
		float[,] noise = new float[16, 16];

		for (int x = 0; x < 16; x++)
		{
			for (int y = 0; y < 16; y++)
			{
				noise[x, y] = Mathf.PerlinNoise((seed.x + chunkPos.x + x / 16f) / scale, (seed.y + chunkPos.y + y / 16f) / scale);
			}
		}

		return noise;
	}

	private static float[,] gen1(float[,] a, float[,] b)
	{
		float[,] res = new float[16,16];
		for (int x = 0; x<16; x++)
		{
			for (int y = 0; y<16; y++)
			{
				res[x, y] = (a[x, y] + b[x, y]) / 2;
			}
		}
		return res;
	}
	
	private static float[,] gen2(float[,] a, float[,] b)
	{
		float[,] res = new float[16,16];
		for (int x = 0; x<16; x++)
		{
			for (int y = 0; y<16; y++)
			{
				float temp = a[x, y] + b[x, y];

				if (temp > 1)
				{
					temp = (temp % 1) / 5;
				}
				else
				{
					temp = 1 - (1 - temp) / 3;
				}

				res[x, y] = temp;
			}
		}
		return res;
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

	public static void generateChunk(Chunk chunk, float[,] noise, ConcurrentQueue<Chunk> applyQueue)
	{
		chunk.generate(noise);
		chunk.rebuildMesh();
		
		applyQueue.Enqueue(chunk);
	}
}