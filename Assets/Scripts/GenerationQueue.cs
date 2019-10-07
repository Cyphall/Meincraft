using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GenerationQueue
{
	private ConcurrentQueue<Chunk> _inputQueue;
	private ConcurrentQueue<Chunk> _outputQueue;
	private Thread _worker;

	public GenerationQueue()
	{
		_inputQueue = new ConcurrentQueue<Chunk>();
		_outputQueue = new ConcurrentQueue<Chunk>();
	}

	public void enqueue(Chunk chunk)
	{
		_inputQueue.Enqueue(chunk);
		if (_worker == null || !_worker.IsAlive)
		{
			_worker = new Thread(() => task(ref _inputQueue, _outputQueue));
			_worker.Start();
		}
	}

	public bool tryDequeue(out Chunk chunk)
	{
		return _outputQueue.TryDequeue(out chunk);
	}

	private static void task(ref ConcurrentQueue<Chunk> input, ConcurrentQueue<Chunk> output)
	{
		while (input.TryDequeue(out Chunk chunk))
		{
			chunk.setBlocks(Biomes.generateChunkBlocks(chunk.chunkPos, Biomes.mountains2));
			chunk.rebuildMesh();
		
			output.Enqueue(chunk);
		}
	}
}