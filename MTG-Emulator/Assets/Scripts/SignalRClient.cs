using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using MTG_Emulator.Unity.Synchronization;
using MTG_Emulator.Unity.Synchronization.Events;
using Newtonsoft.Json;
using UnityEngine;

public class SignalRClient : MonoBehaviour, ISynchronizationEventHandler
{
    private readonly Uri apiURL = new Uri("http://localhost:5042");
    private const string hubName = "GameState";

    private HubConnection connection;

    public async void Awake()
    {
        DontDestroyOnLoad(gameObject);

        try
        {
            connection = new HubConnectionBuilder().WithUrl(apiURL + hubName)
                .AddNewtonsoftJsonProtocol()
                .Build();

            connection.On<MoveCardEvent>(nameof(ISynchronizationEventHandler.OnMoveCard), OnMoveCard);
            connection.On<NewCardEvent>(nameof(ISynchronizationEventHandler.OnNewCard), OnNewCard);

            await connection.StartAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void OnDestroy()
    {
        Task.Run(() => connection.StopAsync());
    }

    public void OnMoveCard(MoveCardEvent e)
    {
        // Debug.Log($"Move card received with {e.Position}");

        // We don't have a defined place/method of storing current cards atm, so use search by name
        // (this is badbadbadbadbad for performance, should be cleaned later)
        var obj = GameObject.Find(e.Card);
        obj.transform.position = new Vector3(e.Position.X, e.Position.Y, 0);
    }

    public void OnNewCard(NewCardEvent e)
    {
        Debug.Log($"New card received with {e.Position}");
    }

    public async Task BroadcastMoveCard(MoveCardEvent e)
    {
        await connection.SendAsync("MoveCard", e);
    }
}