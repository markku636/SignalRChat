using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SignalR.Models;

namespace SignalR
{
    [HubName("chathub")]

    public class ChatHub : Hub
    {
        public static Dictionary<string, ChatUser> connectedUser = new Dictionary<string, ChatUser>();
        
        [HubMethodName("userconnected")]
        public void UserConnected(string username)
        {
            username = HttpUtility.HtmlEncode(username);
            string message = "歡迎使用者" + username + "加入使用客服系統";
            //發送除了自己的其他人
            Clients.Caller.c_loginhello(message);//發送訊息給所有人

            //避免重覆登入
            if (connectedUser.Count(p => p.Value.Name == username) > 0)
            {
                Clients.Caller.c_loginExist("請誤重覆登入");
                return;
            }

            //新增目前使用者至上線清單
            ChatUser newChatUser = new ChatUser();

            newChatUser.Name = username;

            connectedUser.Add(Context.ConnectionId, newChatUser);

            Clients.All.c_getList(connectedUser.Select(p => new { id = p.Key, name = p.Value.Name }).ToList());
        }

        [HubMethodName("sendMessageToId")]
        public void sendMessageToId(string id, string message)
        {
            var fromName = connectedUser.FirstOrDefault(p => p.Key == Context.ConnectionId).Value.Name;

            message = fromName + "對你說：" + message;
            Clients.Client(id).c_sendMessage(message);
        }

        [HubMethodName("sendMessageToAll")]
        public void sendMessageToAll(string message)
        {
            var fromname = connectedUser.FirstOrDefault(p => p.Key == Context.ConnectionId).Value;
            message = fromname.Name + "對所有人說:" + message;
            Clients.Others.c_sendAllMessage(message);
            
        }

        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
           Clients.All.removeList(Context.ConnectionId);
            connectedUser.Remove(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }
    }
}