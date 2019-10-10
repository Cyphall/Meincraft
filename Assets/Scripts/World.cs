using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class World : MonoBehaviour
{
	public GameObject chunkPrefab;
	public GameObject playerPrefab;
	
	public Player player { get; private set; }

	private Dictionary<int2, Chunk> _chunks = new Dictionary<int2, Chunk>();
	private GenerationQueue _genQueue = new GenerationQueue();

	private List<int2> _chunkRemoveList = new List<int2>(64);
	
	[Range(1, 60)]
	public int renderDistance = 16;

	private void Start()
	{
		Toolbox.world = this;
		
		Biomes.initSeed();
		
		player = Instantiate(playerPrefab, new float3(8, 256, 8), Quaternion.identity).GetComponent<Player>();
	}

	private void createChunk(int2 chunkPos, bool overrideAlert = true)
	{
		if (_chunks.ContainsKey(chunkPos))
		{
			if (overrideAlert)
				Debug.LogError("A chunk has been overriden without prior deletion");
			return;
		}
		
		Chunk chunk = Instantiate(chunkPrefab, new float3(chunkPos.x, 0, chunkPos.y) * 16, Quaternion.identity).GetComponent<Chunk>();
		
		_chunks.Add(chunkPos, chunk);
		
		chunk.init(chunkPos);
		
		_genQueue.enqueue(chunk);
	}

	private void destroyChunk(int2 chunkPos)
	{
		if (!_chunks.ContainsKey(chunkPos))
		{
			Debug.LogError("Deletion of a non-existing chunk has been requested");
			return;
		}

		Destroy(_chunks[chunkPos].gameObject);
		
		_chunkRemoveList.Add(chunkPos);
	}

	public void placeBlock(int3 blockPos, byte blockType)
	{
		int2 chunkPos = chunkPosFromBlockPos(blockPos);

		if (!_chunks.ContainsKey(chunkPos))
		{
			Debug.LogError("Chunk where block is placed doesn't exists");
			return;
		}

		_chunks[chunkPos].placeBlock(localBlockPosFromBlockPos(blockPos), blockType);
	}

	private static int2 chunkPosFromBlockPos(int3 blockPos)
	{
		int2 chunkPos = int2.zero;

		chunkPos.x = Mathf.FloorToInt(blockPos.x / 16.0f);
		chunkPos.y = Mathf.FloorToInt(blockPos.z / 16.0f);

		return chunkPos;
	}

	private static int3 localBlockPosFromBlockPos(int3 blockPos)
	{
		int3 localBlockPos = int3.zero;
		
		localBlockPos.x = Mathf.RoundToInt(Mathf.Repeat(blockPos.x, 16));
		localBlockPos.y = blockPos.y;
		localBlockPos.z = Mathf.RoundToInt(Mathf.Repeat(blockPos.z, 16));

		return localBlockPos;
	}
	
	private static int2 chunkPosFromPlayerPos(float3 playerPos)
	{
		int2 chunkPos = int2.zero;

		chunkPos.x = Mathf.FloorToInt(playerPos.x / 16.0f);
		chunkPos.y = Mathf.FloorToInt(playerPos.z / 16.0f);

		return chunkPos;
	}

	private void Update()
	{
		float effectiveRenderDistance = renderDistance - 0.1f;
		
		int2 chunkWithPlayer = chunkPosFromPlayerPos(player.transform.position);

		foreach (KeyValuePair<int2, Chunk> pair in _chunks)
		{
			if (math.distance(pair.Key, chunkWithPlayer) > effectiveRenderDistance)
			{
				destroyChunk(pair.Key);
			}
		}
		
		_chunkRemoveList.ForEach(element => _chunks.Remove(element));
		_chunkRemoveList.Clear();

		for (int x = chunkWithPlayer.x ; x <= chunkWithPlayer.x + effectiveRenderDistance; x++)
		{
			for (int y = chunkWithPlayer.y ; y <= chunkWithPlayer.y + effectiveRenderDistance; y++)
			{
				if ((chunkWithPlayer.x - x) * (chunkWithPlayer.x - x) + (chunkWithPlayer.y - y) * (chunkWithPlayer.y - y) > effectiveRenderDistance * effectiveRenderDistance) continue;
				
				int xSym = chunkWithPlayer.x - (x - chunkWithPlayer.x);
				int ySym = chunkWithPlayer.y - (y - chunkWithPlayer.y);

				createChunk(new int2(x, y), false);
				createChunk(new int2(x, ySym), false);
				createChunk(new int2(xSym, y), false);
				createChunk(new int2(xSym, ySym), false);
			}
		}
		
		if (player.transform.position.y < -10)
		{
			player.transform.position += new Vector3(0, player.spawnPos.y, 0);
		}
		
		if (_genQueue.tryDequeue(out Chunk chunk))
		{
			if (chunk)
				chunk.applyMesh();
			else
				chunk.freeMem();
		}
	}
}