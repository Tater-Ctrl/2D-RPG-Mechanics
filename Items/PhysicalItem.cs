using Mirror;

public class PhysicalItem : NetworkBehaviour
{
    public Item scriptableObjectReference;
    [SyncVar]
    public int stackedAmount = 1;

    void Start() {
        if(stackedAmount <= 0) {
            Destroy(this.gameObject);
        }
    }
}
