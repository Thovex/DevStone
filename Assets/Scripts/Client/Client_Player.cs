using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

// Please don't mind the mess :> <3

public class Client_Player : NetworkBehaviour {

    [Header("Destroy On Dupe Start Vars")]
    public GameObject[] localPlayerOnlyVars;

    [Header("Important Player Information")]
    [SyncVar(hook = "OnIDChanged")]
    public int playerID;

    [SyncVar(hook = "OnPlayerHPChanged")]
    public int playerHP = 100;

    [SyncVar(hook = "OnPlayerAPChanged")]
    public int playerAP = 1;

    public int playerCurrentAP = 1;

    private int oldCardCount = 0;
    private int newCardCount;

    private bool attackTarget = false;

    private Client_Player enemy;

    public List<GameObject> cardSlots = new List<GameObject>();
    public List<GameObject> boardSlots = new List<GameObject>();

    private KeyCode[] cardSelectKeyCodes = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5 };
    private KeyCode[] boardSelectKeyCodes = { KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0 };

    private GameObject cardSelected;
    private GameObject boardCardSelected;
    private GameObject attackCardSelected;

    private Server_TurnManager turnManager;
    private Server_SeatPlayers seatPlayers;
    private Server_Deck serverDeck;

    private MeshRenderer meshRenderer;

    private Text turnText;
    private Text apText;
    private Text hpText;

    private void Start () {
        turnManager = FindObjectOfType<Server_TurnManager>();
        seatPlayers = FindObjectOfType<Server_SeatPlayers>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (cardSlots.Count < transform.GetChild(0).childCount) {
            for (int i = 0; i < transform.GetChild(0).childCount; i++) {
                cardSlots.Add(transform.GetChild(0).GetChild(i).gameObject);
            }
        }

        if (boardSlots.Count < transform.GetChild(1).childCount) {
            for (int i = 0; i < transform.GetChild(1).childCount; i++) {
                boardSlots.Add(transform.GetChild(1).GetChild(i).gameObject);
            }
        }

        if (!isLocalPlayer) {
            for (int i = 0; i < localPlayerOnlyVars.Length; i++) {
                Destroy(localPlayerOnlyVars[i]);
            }
        }

        if (isLocalPlayer) {
            turnText = localPlayerOnlyVars[2].transform.GetChild(0).GetComponent<Text>();
            apText = localPlayerOnlyVars[2].transform.GetChild(1).GetComponent<Text>();
            hpText = localPlayerOnlyVars[2].transform.GetChild(2).GetComponent<Text>();

        }
    }

    private void FixedUpdate() {

        serverDeck = FindObjectOfType<Server_Deck>();

        Client_Player[] players = seatPlayers.players;

        foreach (Client_Player player in players) {
            if (player != this) {
                enemy = player;
            }
        }

        switch (playerID) {
            case 0: meshRenderer.material.color = Color.red; break;
            case 1: meshRenderer.material.color = Color.blue; break;
            default: meshRenderer.material.color = Color.black; break;
        }

        RequestCardSync();

        if (!isLocalPlayer) { return; }

        if (oldCardCount != newCardCount) {
            Debug.Log("Sync Cards");
        }

        turnText.text = "Turn #" + turnManager.turnNr + " Your Turn: " + turnManager.playerTurn[playerID].ToString();
        hpText.text = "HP: " + playerHP;
        apText.text = "AP: " + playerCurrentAP + "/"+ playerAP;

        if (Input.GetKeyDown(KeyCode.Space)) {
            if (turnManager.playerTurn[playerID]) {
                Debug.Log("Your turn.");
                CmdNextTurn();
                return;
            }

            Debug.Log("Not your turn.");
        }

        for (int i =0;i<cardSelectKeyCodes.Length;i++) {
            if (Input.GetKeyDown(cardSelectKeyCodes[i])) {
                if (turnManager.playerTurn[playerID]) {
                    if (cardSlots[i].transform.childCount > 0) {
                        cardSelected = cardSlots[i].transform.GetChild(0).gameObject;
                        cardSelected.GetComponent<MeshRenderer>().material.color = Color.red;

                        foreach (GameObject g in cardSlots) {
                            if (g != cardSlots[i]) {
                                if (g.transform.childCount > 0) {
                                    GameObject cardSlotChild = g.transform.GetChild(0).gameObject;
                                    cardSlotChild.GetComponent<MeshRenderer>().material.color = Color.white;
                                }
                            }
                        }
                        Debug.Log("Selected Card: " + cardSelected);
                    } else {
                        cardSelected = null;
                        Debug.Log("No card.");
                    }
                    return;
                }
                Debug.Log("Not yer turn.");
            }
        }

        if (!attackTarget) {
            for (int i = 0; i < boardSelectKeyCodes.Length; i++) {
                if (Input.GetKeyDown(boardSelectKeyCodes[i])) {
                    if (turnManager.playerTurn[playerID]) {
                        if (boardSlots[i].transform.childCount > 0) {
                            boardCardSelected = boardSlots[i].transform.GetChild(0).gameObject;
                            boardCardSelected.GetComponent<MeshRenderer>().material.color = Color.blue;

                            foreach (GameObject g in boardSlots) {
                                if (g != boardSlots[i]) {
                                    if (g.transform.childCount > 0) {
                                        GameObject boardSlotChild = g.transform.GetChild(0).gameObject;
                                        boardSlotChild.GetComponent<MeshRenderer>().material.color = Color.white;
                                    }
                                }
                            }
                            Debug.Log("Selected Card: " + boardCardSelected);
                        } else {
                            boardCardSelected = null;
                            Debug.Log("No card.");
                        }
                        return;
                    }
                    Debug.Log("Not yer turn.");
                }
            }
        }

        if (attackTarget) {
            for (int i = 0; i < boardSelectKeyCodes.Length; i++) {
                if (Input.GetKeyDown(boardSelectKeyCodes[i])) {
                    if (turnManager.playerTurn[playerID]) {
                        if (enemy.boardSlots[i].transform.childCount > 0) {
                            attackCardSelected = enemy.boardSlots[i].transform.GetChild(0).gameObject;
                            attackCardSelected.GetComponent<MeshRenderer>().material.color = Color.blue;

                            foreach (GameObject g in enemy.boardSlots) {
                                if (g != enemy.boardSlots[i]) {
                                    if (g.transform.childCount > 0) {
                                        GameObject boardSlotChild = g.transform.GetChild(0).gameObject;
                                        boardSlotChild.GetComponent<MeshRenderer>().material.color = Color.white;
                                    }
                                }
                            }
                            Debug.Log("Selected Card: " + attackCardSelected);
                        } else {
                            attackCardSelected = enemy.gameObject;
                            Debug.Log("Selected Enemy.");
                        }
                        return;
                    }
                    Debug.Log("Not yer turn.");
                }
            }
        }


        if (Input.GetKeyDown(KeyCode.A)) {
            Server_Card selectedBoardCard = boardCardSelected.GetComponent<Server_Card>();
            if (boardCardSelected != null && !selectedBoardCard.cardUsed) {
                attackTarget = true;
            }

            if (attackTarget && boardCardSelected != null && attackCardSelected != null) {
                if (attackCardSelected != enemy.gameObject) {
                    CmdAttack(boardCardSelected, attackCardSelected);
                    selectedBoardCard.cardUsed = true;
                    attackTarget = false;

                    boardCardSelected = null;
                    attackCardSelected = null;
                } else {
                    CmdAttackPlayer(boardCardSelected, attackCardSelected);
                    selectedBoardCard.cardUsed = true;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            Server_Card selectedHandCard = cardSelected.GetComponent<Server_Card>();
            if (playerCurrentAP >= selectedHandCard.cardData.AP) {
                if (!selectedHandCard.showAll) {
                    if (!selectedHandCard.cardUsed) {
                        cardSelected.GetComponent<MeshRenderer>().material.color = Color.white;
                        CmdPlayCard(cardSelected);
                        playerCurrentAP -= selectedHandCard.cardData.AP;
                        cardSelected = null;
                    }
                }
            }
        }
    }

    public void NewTurnCalled(int index) {
        if (isLocalPlayer) {
            if (playerID == index) {
                CmdDrawCard();
            }
        }
    }

    private void OnIDChanged(int value) {
        playerID = value;
    }

    private void OnPlayerHPChanged(int value) {
        playerHP = value;
    }

    private void OnPlayerAPChanged(int value) {
        playerAP = value;
    }

    public List<GameObject> GetCards() {
        List<GameObject> slotsWithCard = new List<GameObject>();
        for (int i = 0; i < cardSlots.Count; i++) {
            if (cardSlots[i].transform.childCount > 0) {
                slotsWithCard.Add(cardSlots[i].gameObject);
            }
        }

        for (int i = 0; i < boardSlots.Count; i++) {
            if (boardSlots[i].transform.childCount > 0) {
                slotsWithCard.Add(boardSlots[i].gameObject);
            }
        }

        return slotsWithCard;
    }

    private void RequestCardSync() {
        List<Server_Card> serverCards = new List<Server_Card>();

        foreach (GameObject g in GetCards()) {
            serverCards.Add(g.transform.GetChild(0).GetComponent<Server_Card>());
        }

        if (isLocalPlayer) {
            foreach (Server_Card s in serverCards) {

                if (playerID == 0) {
                    s.GetComponent<MeshRenderer>().material.mainTexture = serverDeck.cardStats[serverDeck.cardIDs0[s.cardIndex]].texture;
                    s.GetComponent<MeshFilter>().mesh = serverDeck.cardStats[serverDeck.cardIDs0[s.cardIndex]].mesh;
                    s.cardData = serverDeck.cardStats[serverDeck.cardIDs0[s.cardIndex]];
                }

                if (playerID == 1) {
                    s.GetComponent<MeshRenderer>().material.mainTexture = serverDeck.cardStats[serverDeck.cardIDs1[s.cardIndex]].texture;
                    s.GetComponent<MeshFilter>().mesh = serverDeck.cardStats[serverDeck.cardIDs1[s.cardIndex]].mesh;
                    s.cardData = serverDeck.cardStats[serverDeck.cardIDs1[s.cardIndex]];
                }
            }
        } else {
            foreach (Server_Card s in serverCards) {
                if (playerID == 0) {
                    if (s.showAll) {
                        s.GetComponent<MeshRenderer>().material.mainTexture = serverDeck.cardStats[serverDeck.cardIDs0[s.cardIndex]].texture;
                        s.GetComponent<MeshFilter>().mesh = serverDeck.cardStats[serverDeck.cardIDs0[s.cardIndex]].mesh;
                        s.cardData = serverDeck.cardStats[serverDeck.cardIDs0[s.cardIndex]];
                    }
                }

                if (playerID == 1) {
                    if (s.showAll) {
                        s.GetComponent<MeshRenderer>().material.mainTexture = serverDeck.cardStats[serverDeck.cardIDs1[s.cardIndex]].texture;
                        s.GetComponent<MeshFilter>().mesh = serverDeck.cardStats[serverDeck.cardIDs1[s.cardIndex]].mesh;
                        s.cardData = serverDeck.cardStats[serverDeck.cardIDs1[s.cardIndex]];
                    }
                }
            }
        }
    }

    [ClientRpc]
    private void RpcSyncCardOnce(int _playerID, GameObject _theObject, GameObject _gameObject) {
        Server_Card serverCard = _theObject.GetComponent<Server_Card>();
        serverCard.cardID = _playerID;
        serverCard.cardUsed = true;
        GameObject cardSlots = _gameObject.transform.GetChild(0).gameObject;

        Transform slot = null;

        for (int i = 0; i < cardSlots.transform.childCount; i++) {
            if (cardSlots.transform.GetChild(i).childCount == 0) {
                slot = cardSlots.transform.GetChild(i);
                break;
            }
        }

        if (slot == null) {
            NetworkServer.Destroy(_theObject);
            return;
        }

        _theObject.transform.parent = slot;

        Vector3 distance = Vector3.zero;
        Vector3 rotation = Vector3.zero;

        switch (_playerID) {
            case 0:
                distance.y = 1f;
                distance.z = 2f; 
                rotation = new Vector3(0, 180, 0);
                break;
            case 1:
                distance.y = 1f;
                distance.z = -2;
                rotation = new Vector3(0, 0, 0);
                break;
        }

        _theObject.transform.eulerAngles = rotation;
        _theObject.transform.position = _theObject.transform.parent.position;
        _theObject.transform.localScale = new Vector3(1f, 2f, 0.2f);


        if (_playerID == 0) {
            serverCard.cardIndex = serverDeck.cardIDs0index[0];
            serverDeck.cardIDs0index.RemoveAt(0);

        }

        if (_playerID == 1) {
            serverCard.cardIndex = serverDeck.cardIDs1index[0];
            serverDeck.cardIDs1index.RemoveAt(0);
        }
    }

    [ClientRpc]
    private void RpcPlayCardSync(GameObject _theObject, GameObject _gameObject) {

        GameObject boardSlots = _gameObject.transform.GetChild(1).gameObject;

        Transform slot = null;

        for (int i = 0; i < boardSlots.transform.childCount; i++) {
            if (boardSlots.transform.GetChild(i).childCount == 0) {
                slot = boardSlots.transform.GetChild(i);
                break;
            }
        }

        if (slot == null) {
            NetworkServer.Destroy(_theObject);
            return;
        }

        _theObject.transform.parent = slot;
        _theObject.transform.rotation = _theObject.transform.parent.rotation;
        _theObject.transform.position = _theObject.transform.parent.position;

    }

    [Command]
    private void CmdAttack(GameObject a, GameObject d) {
        Server_Card aCard = a.GetComponent<Server_Card>();
        Server_Card dCard = d.GetComponent<Server_Card>();

        dCard.cardData.HP -= aCard.cardData.Atk;
        aCard.cardData.HP -= dCard.cardData.Atk;

        if (dCard.cardData.HP <= 0) {
            NetworkServer.Destroy(d);
        }

        if (aCard.cardData.HP <= 0) {
            NetworkServer.Destroy(a);
        }
    }


    [Command]
    private void CmdAttackPlayer(GameObject a, GameObject d) {
        Server_Card aCard = a.GetComponent<Server_Card>();
        Client_Player dPlayer = d.GetComponent<Client_Player>();

        dPlayer.playerHP -= aCard.cardData.Atk;

        if (dPlayer.playerHP <= 0) {
            NetworkServer.Destroy(d);
        }
    }


    [Command]
    private void CmdPlayCard(GameObject c) {
        if (c != null) {
            Server_Card cServerCard = c.GetComponent<Server_Card>();
            cServerCard.showAll = true;
            RpcPlayCardSync(c, gameObject);
        }
    }

    [Command]
    private void CmdDrawCard() {
        GameObject card = Instantiate(serverDeck.cardResources[0]);
        NetworkServer.Spawn(card);
        RpcSyncCardOnce(playerID, card, gameObject);
    }

    [Command]
    private void CmdNextTurn() {
        turnManager.NextTurn();
    }
}