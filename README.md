# P2P Idea

In this repository, I will attempt to prototype an idea for P2P information exchange.

A web socket server exists for peers to connect to.
Upon connecting to the web server using web sockets, the peer connects to all other peers using WebRTC.

- A shared state exists - a string of text, a list of files, a drawing canvas, whatever
- Any peer can change the state and the changes propagate to other peers using WebRTC
- Upon joining, new peer syncs the state so that the state is again distributed across all the peers
- If the server goes down, the peers are still connected using WebRTC
- If the WebRTC connection goes down, the peers can reconnect by finding each other using the server
- How do we protect a session from being joined by undesired peers?
- How do we protect the data on the server from attackers? (WebRTC has its own protection layer.)

## The Use-Case

The idea here is that a person is able to run a randevous node (a web socket signaling server)
which by itself doesn't transmit sensitive data, so security lapses in the signaling server
deployment do not result in information disclosure.

There is still a risk of eavesdropping, an attacker could also join the server and ask to sync
all the state.

Maybe a solution to that problem would be trust bootstrapping:

- The server spins up with no clients to it
- The first client to connect is made an admin
- Another client is connected but not yet verified
- The admin generates a challenge (e.g.: a PIN) and asks the new client to resolve it
- If the client is able to set up an out of band channel to exchange the challenge
  - They are able to fill the challenge response in and send it over the signaling channel to the admin
  - The admin receives the challenge response and knows they can connect to the responding peer
- If the client is an attacker who is unable to set up this connection, the admin never connects to them
- Now we have two joined clients and another new client trying to join in
- Both peers provide a unique challenge and individually connect to the new peer when they resolve it

This ensures that any undesired peer is unable to connect.
If they conspire with a legitimate peer, they will only be able to connect to them.
The legitimate peer can choose to relay information from other peers to the undesired peer.
When discovered, the conspiring peer can be disconnected from cutting off the illegitimate peer as well.

In a perfect situation, the star topology is achieved and grows without gaps and peers cross-validate.
A malicious peer will visually show up as a gap in the network topology, which can be provided to the peers to inspect.

## The Flow

- Server starts

- Client A connects to room R
- Room R opens
- Server notifies client A about their arrival
- Client A refused to connect to themselves since no key was provided
- Client A invites client B to room R with key K - offline

- Client B connects to room R with key K
- Room R exists
- Server notifies clients A and B about client B's arrival
- Client A connects to client B as their key matches their invite
- Client B refuses to connect to themselves since unknown key was provided (they do not invite)
- Client B invites client C to room R with key L - offline
- Client B notifies client A about the client C invite - WebRTC (to avoid MITM)

- Client C connects to room R with key L
- Room R exists
- Server notifies clients A, B and C about client C's arrival
- Client A connects to client C as their key matches client B's invite they shared
- Client B connects to client C as their key matches their invite
- Client C refuses to connect to themselves since unknown key was provided (they do not invite)

- Etc…
