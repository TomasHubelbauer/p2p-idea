using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Threading;

var httpListener = new HttpListener();
httpListener.Prefixes.Add("http://localhost:8000/");
httpListener.Start();

Console.WriteLine(httpListener.Prefixes.Single());

var rooms = new ConcurrentDictionary<string, ConcurrentBag<WebSocket>>();
while (true)
{
  var context = await httpListener.GetContextAsync();
  if (context.Request.IsWebSocketRequest)
  {
    var socketContext = await context.AcceptWebSocketAsync(null);

    // Create the room if it doesn't already exist
    var bag = rooms.GetOrAdd(context.Request.Url.LocalPath, key => new ConcurrentBag<WebSocket>());
    bag.Add(socketContext.WebSocket);

    // Notify all peers in the bag about the new peer's arrival
    foreach (var peerSocket in bag)
    {
      var payload = new ArraySegment<byte>(Encoding.UTF8.GetBytes($"connect:${context.Request.QueryString}"));
      await peerSocket.SendAsync(payload, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    // Fire and forget to be able to accept more clients
    HandleSocket(socketContext.WebSocket);
  }
  else
  {
    context.Response.StatusCode = 400;
    context.Response.Close();
  }
}

async void HandleSocket(WebSocket socket)
{
  do
  {
    byte[] buffer = new byte[1024];
    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    if (result.MessageType == WebSocketMessageType.Close)
    {
      await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
    }
    else
    {
      // Echo the input
      await socket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
    }
  }
  while (socket.State == WebSocketState.Open);
}
