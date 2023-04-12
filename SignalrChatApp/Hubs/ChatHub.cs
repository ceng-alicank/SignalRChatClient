using Microsoft.AspNetCore.SignalR;
using SignalrChatApp.Data;
using SignalrChatApp.Models;

namespace SignalrChatApp.Hubs
{
    public class ChatHub:Hub
    {
        public async Task GetNickName(string nickName)
        {
            Client client = new Client()
            {
                ConnectionId = Context.ConnectionId,
                NickName= nickName
            };
            ClientSource.clients.Add(client);
            await Clients.Others.SendAsync("clientJoined" , nickName);
            await Clients.All.SendAsync("clients", ClientSource.clients);
        }
        public async Task SendMessageAsync(string message , string clientName)
        {
            clientName = clientName.Trim();
            Client clientSender = ClientSource.clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            if (clientName == "Tümü")
            {
                await Clients.Others.SendAsync("receiveMessage" , message , clientSender.NickName);
            }
            else
            {
                Client client = ClientSource.clients.FirstOrDefault(c => c.NickName == clientName);
                await Clients.Client(client.ConnectionId).SendAsync("receiveMessage", message , clientSender.NickName);
            }
        }
        public async Task AddGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId,  groupName);

            Group group = new Group
            {
                GroupName = groupName
            };
            group.Clients.Add(ClientSource.clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId));
            GroupSource.Groups.Add(group);
            await Clients.All.SendAsync("groups", GroupSource.Groups);
        }
        public async Task AddClientToGroup(List<string> groupNames) {

            Client client = ClientSource.clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            foreach (var group in groupNames)
            {
                Group _group = GroupSource.Groups.FirstOrDefault(g => g.GroupName == group);
                var result = _group.Clients.Any(c => c.ConnectionId == Context.ConnectionId); 
                if (!result)
                {
                    _group.Clients.Add(client);
                    await Groups.AddToGroupAsync(Context.ConnectionId, group);
                }
            }
        }
        public async Task GetClientToGroup(string groupName)
        {
            if (groupName == "-1")
            {
                await Clients.Caller.SendAsync("clients", ClientSource.clients);
            }

            Group group = GroupSource.Groups.FirstOrDefault(g => g.GroupName == groupName);
            await Clients.Caller.SendAsync("clients", group.Clients);
        }
        public async Task SendMessageToGroupAsync(string groupName , string message)
        {
            await Clients.Groups(groupName).SendAsync("receiveMessage" ,message,  ClientSource.clients.FirstOrDefault(c=>
            c.ConnectionId == Context.ConnectionId
            ).NickName );
        }
    }
}
