using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Server_SeatPlayers : NetworkBehaviour {

    public Client_Player[] players;

    public Vector3[] playerSeatPositions;
    public Vector3[] playerSeatRotations;

    private int oldPlayerCount = 0;
    private int currentPlayerCount = 0;

    private bool assignedIDs = false;

    [SyncVar]
    public bool isReady = false;

    private void FixedUpdate() {
        players = FindObjectsOfType<Client_Player>();

        if (isServer) {
            SeatPlayers();

            if (players.Length > 1) {
                RpcSetReady();
            }
        }
    }

    [Server]
    private void SeatPlayers() {
        currentPlayerCount = players.Length;

        if (currentPlayerCount != oldPlayerCount) {
            assignedIDs = false;
            oldPlayerCount = currentPlayerCount;
        }

        for (int i = 0; i < players.Length; i++) {
            players[i].transform.eulerAngles = playerSeatRotations[i];
            players[i].transform.position = playerSeatPositions[i];
            Debug.DrawLine(playerSeatPositions[i], playerSeatPositions[i] + playerSeatRotations[i]);

            if (!assignedIDs) {
                RpcAssignIDs();
            }
        }
    }

    [ClientRpc]
    private void RpcSetReady() {
        isReady = true;
    }

    [ClientRpc]
    private void RpcAssignIDs() {
        for (int i = 0; i < players.Length; i++) {
            players[i].GetComponent<Client_Player>().playerID = i;
        }

        assignedIDs = true;
    }
}
