using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace LobbyScripts
{
    public class LobbyServiceManager : MonoBehaviour
    {
        public Lobby Lobby { get; private set; }
        private Coroutine _heartBeatCoroutine;

        public async Task HostLobbyTask(string lobbyName, string relayJoinCode, string difficulty)
        {
            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "RelayJoinCode",
                        new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode)
                    },
                    {
                        "HostName",
                        new DataObject(
                            DataObject.VisibilityOptions.Public,
                            AuthenticationService.Instance.PlayerName
                        )
                    },
                    {
                        "Difficulty",
                        new DataObject(DataObject.VisibilityOptions.Public, difficulty)
                    }
                },
            };
            Lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 4, options);
            StartHeartbeat();
        }

        public async Task JoinLobbyTask(string lobbyId)
        {
            Lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
        }

        public async Task<List<Lobby>> GetLobbiesAsync()
        {
            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync();
            return response.Results;
        }

        public void HideLobby()
        {

        }

        private void StartHeartbeat()
        {
            _heartBeatCoroutine = StartCoroutine(SendHeartbeatCoroutine());
        }

        public void StopHeartbeat()
        {
            if (_heartBeatCoroutine != null)
            {
                StopCoroutine(_heartBeatCoroutine);
                _heartBeatCoroutine = null;
            }
        }

        private IEnumerator SendHeartbeatCoroutine()
        {
            while (Lobby != null)
            {
                Task heartbeatTask = LobbyService.Instance.SendHeartbeatPingAsync(Lobby.Id);
                yield return new WaitUntil(() => heartbeatTask.IsCompleted);
                if (heartbeatTask.IsFaulted)
                {
                    Debug.LogError("Failed to send heartbeat: " + heartbeatTask.Exception);
                }

                yield return new WaitForSeconds(15f);
            }
        }

        public async Task DeleteLobbyTask()
        {
            if (Lobby == null)
            {
                Debug.LogWarning("No lobby to delete.");
                return;
            }

            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(Lobby.Id);
                Lobby = null;
                StopHeartbeat();
                Debug.Log("Lobby deleted successfully.");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to delete lobby: {e.Message}");
            }
        }

        public async Task LeaveLobbyTask()
        {
            if (Lobby == null)
            {
                Debug.LogWarning("No lobby to leave.");
                return;
            }

            try
            {
                var playerid = AuthenticationService.Instance.PlayerId;
                await LobbyService.Instance.RemovePlayerAsync(Lobby.Id, playerid);
                Lobby = null;
                StopHeartbeat();
                Debug.Log("Left lobby successfully.");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to leave lobby: {e.Message}");
            }
        }
    }
}