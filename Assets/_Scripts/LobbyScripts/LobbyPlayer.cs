using Unity.Netcode;

namespace LobbyScripts
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
