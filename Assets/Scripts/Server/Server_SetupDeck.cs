using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Server_SetupDeck : NetworkBehaviour {

    public GameObject deck;
    public Server_SeatPlayers seatPlayers;

    [SyncVar]
    private bool hasSpawned = false;

    private void FixedUpdate() {
        if (seatPlayers.isReady) {
            if (!hasSpawned) {
                ClientScene.RegisterPrefab(deck);

                if (isServer) {
                    RpcSpawnDeck();
                }

                seatPlayers.isReady = false;
            }
        }
    }

    [ClientRpc]
    public void RpcSpawnDeck() {
        hasSpawned = true;
        NetworkServer.Spawn(Instantiate(deck));
    }
}
