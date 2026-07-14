using Fusion;

public class PlayerData : NetworkBehaviour
{
    [Networked]
    public int PlayerNumber { get; set; }
}