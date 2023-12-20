using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    
    // Player related variables
    public const int MaxNumPlayers = 2;
    public List<Player> Players => players.ToList();
    [SerializeField] private List<Player> players = new();
    
    public Player LocalPlayer => GetLocalPlayer();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private Player GetLocalPlayer()
    {
        Player localPlayer = players.FirstOrDefault(x => x != null && x.IsLocalPlayer);

        if (localPlayer == null)
        {
            Debug.LogError("Local player not found.");
        }

        return localPlayer;
    }
    
    public void AddPlayer(Player newPlayer)
    {
        players.Add(newPlayer);
        Game.Instance.OnPlayersSpawned.Invoke();
    }
}
