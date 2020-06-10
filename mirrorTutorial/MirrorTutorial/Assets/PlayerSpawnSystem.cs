using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSpawnSystem : NetworkBehaviour
{
    [SerializeField]
    private GameObject _playerPrefab;

    private static List<Transform> _spawnPoints = new List<Transform>();

    private int _nextIndex = 0;

    public static void AddSpawnPoint(Transform transform)
    {
        _spawnPoints.Add(transform);

        _spawnPoints = _spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
    }

    public static void RemoveSpawnPoint(Transform transform)
    {
        _spawnPoints.Remove(transform);
    }

    public override void OnStartServer()
    {
        NetworkManagerLobby.OnServerReadied += SpawnPlayer;
    }

    [ServerCallback]
    private void OnDestroy()
    {
        NetworkManagerLobby.OnServerReadied -= SpawnPlayer;
    }

    [Server]
    private void SpawnPlayer(NetworkConnection conn)
    {
        Transform spawnPoint = _spawnPoints.ElementAtOrDefault(_nextIndex);

        if (spawnPoint == null)
        {
            Debug.LogError("Missing spawn point for player: " + _nextIndex);
            return;
        }

        GameObject playerInstance = Instantiate(_playerPrefab, spawnPoint.position, spawnPoint.rotation);
        NetworkServer.Spawn(playerInstance, conn);

        _nextIndex++;
    }


}
