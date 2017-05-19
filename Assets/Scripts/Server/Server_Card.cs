using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Server_Card : NetworkBehaviour {

    public int cardID;

    [SyncVar]
    public int cardIndex;

    [SyncVar]
    public bool showAll = false;

    public bool cardUsed = true;

    private bool oldShowAll = false;
    public CardData cardData;

    private void Start () {
        cardUsed = true;
    }
}
