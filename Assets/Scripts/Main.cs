using UnityEngine;

public class Main : MonoBehaviour
{
	public GameObject playerPrefab;
	public GameObject chunkPrefab;

	private Player _player;

	public World world;

	private void Start()
	{
		world = new World(chunkPrefab);
		
		_player = Instantiate(playerPrefab, new Vector3(0, 80, 0), Quaternion.identity).GetComponent<Player>();
		_player.setMainScript(this);
	}

	private void FixedUpdate()
	{
		if (_player && _player.transform.position.y < -5)
		{
			_player.transform.position = _player.spawnPos;
		}
	}
}