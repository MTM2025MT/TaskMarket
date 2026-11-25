using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace TaskMarket.Handlers
{
    // The JWT is passed as ?access_token=... (see JwtBearerEvents.OnMessageReceived in Program.cs)
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly MessageHandler _messageHandler;
        public ChatHub(MessageHandler messageHandler)
        {
            _messageHandler = messageHandler;
        }

        // Tracks group membership on the server because SignalR doesn't expose group state
        // Outer key = group name (taskId), inner key = ConnectionId
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _groupMembers
            = new();

        private int CurrentUserId => int.TryParse(Context.UserIdentifier, out var id) ? id : 0;

        public async Task JoinTaskChat(int taskId)
        {
            var access = await _messageHandler.GetTaskForParticipantAsync(taskId, CurrentUserId);
            if (access.IsError)
            {
                throw new HubException(access.FirstError.Description);
            }

            var taskIdAsString = taskId.ToString();
            await Groups.AddToGroupAsync(Context.ConnectionId, taskIdAsString);

            var members = _groupMembers.GetOrAdd(taskIdAsString, _ => new ConcurrentDictionary<string, byte>());
            members[Context.ConnectionId] = 0;
        }

        // The React app calls this when closing the task
        public async Task LeaveTaskChat(int taskId)
        {
            var taskIdAsString = taskId.ToString();

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, taskIdAsString);

            if (_groupMembers.TryGetValue(taskIdAsString, out var members))
            {
                members.TryRemove(Context.ConnectionId, out _);
                if (members.IsEmpty)
                {
                    _groupMembers.TryRemove(taskIdAsString, out _);
                }
            }
        }

        // Returns true if any connection is currently in the group for the given taskId
        public bool CheckIfGroupExists(int taskId)
        {
            return _groupMembers.ContainsKey(taskId.ToString());
        }

        // `user` is kept for client compatibility but ignored: the sender always comes from the JWT
        public async Task SendMessage(int user, string message, int taskId)
        {
            var result = await _messageHandler.SendTaskMessageAsync(taskId, CurrentUserId, message);
            if (result.IsError)
            {
                throw new HubException(result.FirstError.Description);
            }

            var taskIdAsString = taskId.ToString();

            // Ensure caller is joined (idempotent)
            if (!_groupMembers.TryGetValue(taskIdAsString, out var members) || !members.ContainsKey(Context.ConnectionId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, taskIdAsString);
                _groupMembers.GetOrAdd(taskIdAsString, _ => new ConcurrentDictionary<string, byte>())[Context.ConnectionId] = 0;
            }

            var saved = result.Value;
            await Clients.Group(taskIdAsString).SendAsync("ReceiveMessage", new
            {
                id = saved.Id,
                senderId = saved.SenderId,
                receiverId = saved.ReceiverId,
                content = saved.Content,
                taskId = saved.TaskId,
                sentAt = saved.SentAt
            });
        }

        // Ensure we clean up if a connection drops without calling LeaveTaskChat
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            foreach (var kvp in _groupMembers)
            {
                var members = kvp.Value;
                members.TryRemove(connectionId, out _);
                if (members.IsEmpty)
                {
                    _groupMembers.TryRemove(kvp.Key, out _);
                }
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
