using Unity.Netcode;

namespace _Scripts.LobbyScripts
{
    public class LobbyPlayer : NetworkBehaviour
    {
        private void Start()
        {
            if (!IsOwner)
            {
                enabled = false;
                return;
            }
            LobbyController.Instance.HandleNewPlayer(NetworkObject.NetworkObjectId); // Register the player
        }
    }
}
