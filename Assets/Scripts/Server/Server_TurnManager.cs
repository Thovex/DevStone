using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Server_TurnManager : NetworkBehaviour {

    [SyncVar]
    public int turnNr;

    public SyncListBool playerTurn = new SyncListBool();

    private Client_Player[] players;

    private int oldPlayerCount = 0;
    private int currentPlayerCount = 0;

    private void FixedUpdate () {
        players = FindObjectsOfType<Client_Player>();

        currentPlayerCount = players.Length;

        if (currentPlayerCount != oldPlayerCount) {
            playerTurn.Clear();
            oldPlayerCount = currentPlayerCount;

            for (int i = 0; i < currentPlayerCount; i++) {
                playerTurn.Add(false);
            }

            if (currentPlayerCount > 0) {
                playerTurn[0] = true;
            }
        }
    }

    private void SetTurnInvoker() {
        RpcSetTurns();
    }

    [ClientRpc]
    private void RpcSetTurns() {
        turnNr++;

        int index = 0;

        for (int i = 0; i < playerTurn.Count; i++) {
            if (playerTurn[i]) {
                index = i;
            }
        }

        foreach (Client_Player p in players) {
            p.playerAP += 1;
            p.playerCurrentAP = p.playerAP;
            p.NewTurnCalled(index);
        }

        Server_Card[] allCards = FindObjectsOfType<Server_Card>();
        foreach (Server_Card c in allCards) {
            c.cardUsed = false;
        }
    }

    [Server]
    public void NextTurn() {
        Invoke("SetTurnInvoker", 1f);
        for (int i = 0; i < playerTurn.Count; i++) {
            if (playerTurn[i] == true) {
                playerTurn[i] = false;
                if (i + 1 < playerTurn.Count) {
                    playerTurn[i + 1] = true;
                } else {
                    playerTurn[0] = true;
                }
                return;
            }
        }
    }
}
