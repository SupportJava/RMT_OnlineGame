﻿using Domain.Communication;
using Domain.Model;
using ServerProj.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ServerProj
{
    [Serializable]
    public class ClientHandler
    {
        public Socket socket;
        NetworkStream stream;
        BinaryFormatter formatter = new BinaryFormatter();
        bool isEnd = false;
        LobbyService service;

        List<Task> tasks = new List<Task>();

        public ClientHandler(Socket socket)
        {
            this.socket = socket;
            stream = new NetworkStream(socket);

            service = new LobbyService(socket);
        }

        #region Handling requests
        public void ProcessRequests()
        {
            while (!isEnd)
            {
                try
                {
                    var request = (Request)formatter.Deserialize(stream);
                    ProcessSingleRequest(request);
                    if (request.Operation == OperationRequest.GameAccepted) break;
                    if (request.Operation == OperationRequest.BreakThread) break;
                }
                catch (Exception ex)
                {
                   //This thread will stop from Server class when server is closed
                   isEnd = true;
                }
            }
        }

        public void ProcessSingleRequest(Request request)
        {
            try
            {
                switch (request.Operation)
                {
                    case OperationRequest.CreateANewPlayer: service.AddANewPlayer(request.Body); break;
                    case OperationRequest.MakeANewLobbyGame: if(service.MakeANewLobbyGame()) RefreshLobby(); break;
                    case OperationRequest.LobbyRefresh: RefreshLobby();break;
                    case OperationRequest.WelcomeLobby: WelcomeToLobby(); break;
                    case OperationRequest.CreateGameRequest: SendGameRequest(request.Body);break;
                    case OperationRequest.GameRejected: SendRejectGameNotification(request.Body); break;
                    case OperationRequest.GameAccepted: GameAccepted(request.Body); break;
                }


            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.Message);
                //throw;
                MessageBox.Show("ProcessingSingleRequest Exception ClientHandler");
                socket.Close();
               stream.Close();
               
            }
        }
        #endregion

        #region Handling responses
        private void SendSingleResponse(NetworkStream str, Response response) => formatter.Serialize(str, response);

        public void SendResponseToAll(Response response)
        {
            foreach (var player in Server.onlineUsers)
            {
                var str = new NetworkStream(player.Socket);
                SendSingleResponse(str, response);
            }
        }

        // mozda ne treba, imamo konstruktor, nek stoji
        private Response CreateResponse(string message, bool flag, OperationResponse operation)
        {
            return new Response
            {
                Message = message,
                Flag = flag,
                Operation = operation
            };
        }
        #endregion

        #region Methods for communication

        private void RefreshLobby()
        {
            string games = service.DisplayAllLobbyGames();
            SendResponseToAll(new Response(games, true, OperationResponse.RefreshLobby));
        }

        private void WelcomeToLobby()
        {
            SendSingleResponse(stream, new Response(service.DisplayAllLobbyGames(), true, OperationResponse.RefreshLobby));
        }

        #region Game requst send and reject

        public void SendGameRequest(string id)
        {
            var temp = service.FindStreamById(id);

            if (temp.Item1 == null)
                return;

            SendSingleResponse(temp.Item1, new Response(temp.Item2,true,OperationResponse.ReceivedGameRequest));
        }

        private void SendRejectGameNotification(string id)
        {
            var player = service.FindAPlayerById(id);

            if (player == null) return;

            var str = new NetworkStream(player.Socket);

            SendSingleResponse(str, new Response("Game rejected", true, OperationResponse.GameRejectedNotification));
        }

        #endregion

        private void GameAccepted(string opponentId)
        {
            var player = service.LocalPlayer;
            var opponentPlayer = service.FindAPlayerById(opponentId);

            SendSingleResponse(new NetworkStream(opponentPlayer.Socket), new Response("", true, OperationResponse.GameAcceptedOpponent));

            var gameHandler = new GameHandler(player, opponentPlayer);


            tasks.Add(Task.Run(() => gameHandler.GameCommunication()));
        }
        #endregion


        #region Stopping Thread

        public void StopClientHandler()
        {
            socket.Close();
            stream.Close();
        }

        #endregion
    }
}
