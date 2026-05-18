using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using MTG_Emulator.Unity.Synchronization;
using MTG_Emulator.Unity.Synchronization.Events;
using Newtonsoft.Json;
using UnityEngine;

public class SignalRClient : MonoBehaviour, ISyncEventHandler
{
    private readonly Uri apiURL = new Uri("http://localhost:5042/");
    private const string hubName = "GameState";

    private HubConnection connection;

    public static SignalRClient Instance;
    public event EventHandler<MoveCardEvent> OnMoveCardEvent;
    public event EventHandler<NewCardEvent> OnNewCardEvent;
    public event EventHandler<PlayerStatsEvent> OnPlayerStatsEvent;
    public event EventHandler<TurnChangedEvent> OnTurnChangedEvent;
    public event EventHandler<TurnOrderEvent> OnTurnOrderCreatedEvent; 

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        try
        {
            connection = new HubConnectionBuilder().WithUrl(apiURL + hubName)
                .AddNewtonsoftJsonProtocol()
                .Build();

            connection.On<MoveCardEvent>(nameof(ISyncEventHandler.OnMoveCard), OnMoveCard);
            connection.On<NewCardEvent>(nameof(ISyncEventHandler.OnNewCard), OnNewCard);
            connection.On<PlayerStatsEvent>(nameof(ISyncEventHandler.OnUpdatePlayerStats), OnUpdatePlayerStats);
            connection.On<TurnChangedEvent>(nameof(ISyncEventHandler.OnTurnChanged), OnTurnChanged);
            connection.On<TurnOrderEvent>(nameof(ISyncEventHandler.OnTurnOrderCreated), OnTurnOrderCreated);

           var t = connection.StartAsync();
           t.Wait();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void OnDestroy()
    {
        Task.Run(() => connection?.StopAsync());
    }

    public void OnMoveCard(MoveCardEvent e)
    {
        OnMoveCardEvent?.Invoke(this, e);
    }

    public void OnNewCard(NewCardEvent e)
    {
        OnNewCardEvent?.Invoke(this, e);
    }

    public void OnUpdatePlayerStats(PlayerStatsEvent e)
    {
        OnPlayerStatsEvent?.Invoke(this, e);
    }

    public void Broadcast(SyncEvent e)
    {
        var t = connection.SendAsync(e.Method, e);
        t.Wait();
    }

    public void OnTurnChanged(TurnChangedEvent e)
    {
        OnTurnChangedEvent?.Invoke(this, e);
    }

    public void OnTurnOrderCreated(TurnOrderEvent e)
    {
        OnTurnOrderCreatedEvent?.Invoke(this, e);
    }
    
    
}