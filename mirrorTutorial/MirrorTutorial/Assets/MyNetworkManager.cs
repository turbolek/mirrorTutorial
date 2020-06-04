using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyNetworkManager : NetworkManager
{
    [SerializeField]
    private GameObject _petPrefab;
    [SerializeField]
    private Transform _petParent;

    public override void OnStartServer()
    {
        base.OnStartServer();
        GameObject petObject = Instantiate(_petPrefab, _petParent.position, _petParent.rotation);
        NetworkServer.Spawn(petObject);
    }
}
