using UnityEngine;

public class ResourceWallet : MonoBehaviour
{
    [field: SerializeField] public int Balance { get; private set; } = 1000;

    public bool Spend(int amount)
    {
        if (Balance < amount) return false;
        Balance -= amount;
        return true;
    }

    public void Add(int amount) => Balance += amount;
}
