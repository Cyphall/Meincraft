using UnityEngine;

public class Main : MonoBehaviour
{
	public GameObject playerPrefab;
	public GameObject chunkPrefab;

	public World world;

	private void Start()
	{
		world = new World(chunkPrefab, playerPrefab);
	}

	private void FixedUpdate()
	{
		world?.fixedUpdate();
	}
}