using Unity.Netcode;
using Unity.Services.Authentication;

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
            SetNameRpc(AuthenticationService.Instance.PlayerName);
            LobbyController.Instance.HandleNewPlayer(NetworkObject.NetworkObjectId); // Register the player
        }

        [Rpc(SendTo.Everyone)]
        private void SetNameRpc(string name)
        {
            gameObject.name = name + " (Lobby Player)";
        }
    }
}
