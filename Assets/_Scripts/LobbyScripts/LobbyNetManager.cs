using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace _Scripts.LobbyScripts
{
    public class LobbyNetManager : NetworkBehaviour
    {
        private string _relayJoinCode;
        public string PlayerName { get; private set; }

        public async Task SignInTask()
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        public async Task<string> HostNetworkTask()
        {
            RelayServerData relayServerData = await StartHostAllocation(4);
            StartNetworkingTask(true, relayServerData);
            return _relayJoinCode;
        }

        public async Task ClientNetworkTask(string relayJoinCode)
        {
            RelayServerData relayServerData = await StartClientAllocation(relayJoinCode);
            StartNetworkingTask(false, relayServerData);
        }

        private async Task<RelayServerData> StartHostAllocation(int maxPlayers)
        {
            try
            {
                var service = RelayService.Instance;
                Allocation allocation = await service.CreateAllocationAsync(maxPlayers);
                _relayJoinCode = await service.GetJoinCodeAsync(allocation.AllocationId);
                return allocation.ToRelayServerData("wss");
            }
            catch (Exception e)
            {
                // TODO handle error properly
                throw;
            }
        }

        private async Task<RelayServerData> StartClientAllocation(string code)
        {
            try
            {
                var service = RelayService.Instance;
                JoinAllocation allocation = await service.JoinAllocationAsync(code);
                return allocation.ToRelayServerData("wss");
            }
            catch (Exception e)
            {
                // TODO handle error properly
                throw;
            }
        }

        private void StartNetworkingTask(bool isHost, RelayServerData relayServerData)
        {
            try
            {
                NetworkManager
                    .Singleton.GetComponent<UnityTransport>()
                    .SetRelayServerData(relayServerData);
                Debug.Log("Test");
                if (isHost)
                    NetworkManager.Singleton.StartHost();
                else
                    NetworkManager.Singleton.StartClient();
            }
            catch (Exception e)
            {
                // TODO handle error properly
                throw;
            }
        }

        public async Task UpdateName(string newName)
        {
            try
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
                PlayerName = newName;
            }
            catch (Exception e)
            {
                // TODO handle error properly
                Debug.LogError($"Failed to update player name: {e.Message}");
                throw;
            }
        }
    }
}
