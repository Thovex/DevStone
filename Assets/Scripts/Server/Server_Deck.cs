using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class CardData {
    public enum cardrarity { common, uncommon, rare, epic, legendary };

    [Header("Card Details")]
    public string name;
    public cardrarity rarity;

    [Header("Card Stats")]
    public int HP;
    public int AP;
    public int Atk;

    [Header("Card Settings")]
    public Texture2D texture;
    public Mesh mesh;
}

public class Server_Deck : NetworkBehaviour {
    public int maxCards = 25;

    public SyncListInt cardIDs0 = new SyncListInt();
    public SyncListInt cardIDs0index = new SyncListInt();

    public SyncListInt cardIDs1 = new SyncListInt();
    public SyncListInt cardIDs1index = new SyncListInt();

    public List<CardData> cardStats = new List<CardData>();

    [HideInInspector]
    public GameObject[] cardResources;

    private void Start() {
        System.Random[] _systemRandom = { new System.Random(0), new System.Random(1) };
        cardResources = Resources.LoadAll<GameObject>("Cards");

        for (int i = 0; i < maxCards; i++) {
            cardIDs0.Add(_systemRandom[0].Next(0, cardResources.Length));
            cardIDs0index.Add(i);

            cardIDs1.Add(_systemRandom[1].Next(0, cardResources.Length));
            cardIDs1index.Add(i);
        }
    }


    private void FixedUpdate() {
        if (cardIDs0.Count == 0) {
            if (cardIDs1.Count == 0) {
                Destroy(gameObject);
            }
        }
    }
}