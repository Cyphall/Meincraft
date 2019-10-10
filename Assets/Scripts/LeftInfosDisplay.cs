using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class LeftInfosDisplay : MonoBehaviour
{
	private Text _posX;
	private Text _posY;
	private Text _posZ;

	private Text _worldSeed;

	private void Start()
	{
		_posX = transform.Find("PosX").GetComponent<Text>();
		_posY = transform.Find("PosY").GetComponent<Text>();
		_posZ = transform.Find("PosZ").GetComponent<Text>();

		_worldSeed = transform.Find("Seed").GetComponent<Text>();
	}

	void Update()
	{
		if (Toolbox.world)
		{
			_worldSeed.text = $"Seed: {Biomes.seed}";

			if (Toolbox.world.player)
			{
				float3 playerPos = Toolbox.world.player.transform.position;
				_posX.text = $"X: {Mathf.FloorToInt(playerPos.x)}";
				_posY.text = $"X: {Mathf.FloorToInt(playerPos.y)}";
				_posZ.text = $"X: {Mathf.FloorToInt(playerPos.z)}";
			}
		}
	}
}