window.addEventListener('load', () => {
  const roomInput = document.querySelector('#roomInput');
  const connectButton = document.querySelector('#connectButton');

  connectButton.addEventListener('click', () => {
    const webSocket = new WebSocket('ws://localhost:8000/' + roomInput.value);
    window.webSocket = webSocket;

    webSocket.addEventListener('open', () => {
      console.log('opened');
    });

    webSocket.addEventListener('message', event => {
      console.log('received', event.data, event.lastEventId, event.origin, event.ports, event.source);
    });

    webSocket.addEventListener('close', event => {
      console.log('closed', event.code, event.reason, event.wasClean);
    });

    webSocket.addEventListener('error', () => {
      console.log('errored');
    });
  });
});
