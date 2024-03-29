﻿using System.Collections.Generic;
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
	
	[Range(1, 32)]
	public int renderDistance = 16;

	private void Start()
	{
		Toolbox.world = this;
		
		Biomes.initSeed();
		
		player = Instantiate(playerPrefab, new float3(8, 256, 8), Quaternion.identity).GetComponent<Player>();
	}

	private void createChunk(int2 chunkPos, bool checkOverride = true)
	{
		if (checkOverride && _chunks.ContainsKey(chunkPos))
		{
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
		

		foreach (KeyValuePair<int2, Chunk> pair in _chunks)
		{
			int2 xpos = pair.Key + new int2(1, 0);
			if (!_chunks.ContainsKey(xpos) && math.distance(xpos, chunkWithPlayer) < renderDistance)
			{
				createChunk(xpos, false);
				break;
			}

			int2 xneg = pair.Key + new int2(-1, 0);
			if (!_chunks.ContainsKey(xneg) && math.distance(xneg, chunkWithPlayer) < renderDistance)
			{
				createChunk(xneg, false);
				break;
			}
				
			int2 ypos = pair.Key + new int2(0, 1);
			if (!_chunks.ContainsKey(ypos) && math.distance(ypos, chunkWithPlayer) < renderDistance)
			{
				createChunk(ypos, false);
				break;
			}
			
			int2 yneg = pair.Key + new int2(0, -1);
			if (!_chunks.ContainsKey(yneg) && math.distance(yneg, chunkWithPlayer) < renderDistance)
			{
				createChunk(yneg, false);
				break;
			}
		}

		if (_chunks.Count == 0)
			createChunk(chunkWithPlayer, false);
		
		if (player.transform.position.y < -10)
		{
			Destroy(player.gameObject);
			player = Instantiate(playerPrefab, new float3(8, 256, 8), Quaternion.identity).GetComponent<Player>();
		}
		
		if (_genQueue.tryDequeue(out Chunk chunk))
		{
			if (chunk)
			{
				chunk.applyMesh();
			}
		}
	}
}