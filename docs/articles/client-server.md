---
uid: client-server
title: "Client-Server"
---

Client-server tutorial
======================

The Bonsai.ZeroMQ package allows us to harness the powerful [ZeroMQ](https://zeromq.org/) library to build networked applications in Bonsai. Applications could include:
- Interfacing with remote experimental rigs via network messages
- Performing distributed work across pools of machines (e.g. for computationally expensive deep-learning inference) 
- Streaming video data between clients across a network
- **Real-time interaction between clients in a multiplayer game**

In this article, we will use Bonsai.ZeroMQ to explore this final example and build a basic client-server architecture similar to one that might be used in a multiplayer game.

## Network design
The basic network architecture we want to achieve will be composed of a number of clients sending their state to a server, which then updates the other connected clients with that clients’ state. This is comparable to a multiplayer game in which client players move through the game world and must synchronise that movement via a dedicated server so that all players see each other in their ‘true’ position in the world.

```mermaid
sequenceDiagram
    actor Client1
    actor Client2
    actor Client3
    participant Server
    Client1->>Server: MOVE
    Server->>Client2: SYNC
    Server->>Client3: SYNC
```

An important requirement to point out here is that our server should be choosy about which clients it broadcasts information to. If client 1 updates the server with its current state, that information needs to be sent to all other connected clients, but there is no need to send it back to client 1 as it already knows its own state and this feedback message would be redundant.

ZeroMQ provides a number of socket types that could be used to achieve something approaching this architecture. The Router / Dealer socket pair acting as Server / Client has a couple of advantages for this design: 
- Routers assign a unique address for each connected client allowing clients in turn to be addressed individually
- Messages can be passed between Router / Dealer sockets without the requirement that a reply is received before the next message is sent, as is the case with the Request / Response socket pair.

## Basic client
To begin with, we’ll create a simple client that sends basic messages on a network. In a new Bonsai project:
- Add a [`Dealer`](xref:Bonsai.ZeroMQ.Dealer) operator. 
- In the `ConnectionString` property, set `Address`: localhost:5557, `Action`: Connect, `Protocol`: TCP.

> [!Note]
> In Bonsai.ZeroMQ, the [`Dealer`](xref:Bonsai.ZeroMQ.Dealer) can have two functions based on its inputs. On its own, as above, the [`Dealer`](xref:Bonsai.ZeroMQ.Dealer) operator creates a Dealer socket that listens for messages on the specified network. With the properties specified, we are asking our [`Dealer`](xref:Bonsai.ZeroMQ.Dealer) to listen for messages on the local machine on port 5557 using the TCP protocol. We use the ‘Connect’ argument for the `SocketConnection` property to tell the dealer that it will connect to a static part of the network with a known IP address, in this case the server which we will implement later.

If we add inputs to the [`Dealer`](xref:Bonsai.ZeroMQ.Dealer), it will act as both a sender and receiver of messages on the network. 
- Before the [`Dealer`](xref:Bonsai.ZeroMQ.Dealer) operator add a [`KeyDown`](xref:Bonsai.Windows.Input.KeyDown). 
- Add a [`String`](xref:Bonsai.Expressions.StringProperty) operator in sequence as input to the [`Dealer`](xref:Bonsai.ZeroMQ.Dealer).

:::workflow
![Basic Dealer input](~/workflows/dealer-basic-input.bonsai)
:::

- In the operator properties, set the [`KeyDown`](xref:Bonsai.Windows.Input.KeyDown) `Filter` to the ‘1’ key.
- Set the [`String`](xref:Bonsai.Expressions.StringProperty) `Value` to ‘Client1’. 

If we run the Bonsai project now, the [`Dealer`](xref:Bonsai.ZeroMQ.Dealer) will continue listening for incoming messages on the network, but every time the ‘1’ key is pressed a message containing the string ‘Client1’ will be sent from the socket.

- Copy and paste this client structure a couple of times and change the [`KeyDown`](xref:Bonsai.Windows.Input.KeyDown) and [`String`](xref:Bonsai.Expressions.StringProperty) properties accordingly on each (2, ‘Client2’; 3, ‘Client3’) so that we have 3 total clients that send messages according to different key presses.

:::workflow
![Multiple clients](~/workflows/multiple-clients.bonsai)
:::

> [!Note]
> For the purposes of this article we are creating all of our clients and our server on the same Bonsai project and same machine for ease of demonstration. In a working example, each client and server could be running in separate Bonsai instances on different machines on a network. In this case, localhost would be replaced with the server machine’s IP address.

## Basic server
Now that we have our client pool set up and sending messages, let’s implement a server to listen for those messages. 
- Add a [`Router`](xref:Bonsai.ZeroMQ.Router) operator to the project and set its properties to match the [`Dealer`](xref:Bonsai.ZeroMQ.Dealer) sockets we already added so that it is running on the same network. 
- As the [`Router`](xref:Bonsai.ZeroMQ.Router) is acting as server and will be the ‘static’ part of the network, set its `Action` to ‘Bind’.

As with the [`Dealer`](xref:Bonsai.ZeroMQ.Dealer) operator, a [`Router`](xref:Bonsai.ZeroMQ.Router) operator without any input will simply listen for messages on the network and not send anything in return. If we run the project now and monitor the output of the [`Router`](xref:Bonsai.ZeroMQ.Router) operator, we'll see that each time the client sends a message triggered by its associated key press we get a `ResponseContext` produced at the [`Router`](xref:Bonsai.ZeroMQ.Router). Expanding the output the the [`Router`](xref:Bonsai.ZeroMQ.Router), we can see it contains a `NetMQMessage`. We [expect](https://netmq.readthedocs.io/en/latest/router-dealer/) this message to be composed of 3 frames: an address (in this case the address of the client that sent the message), an empty delimiter frame and the message content. 

- Expose the `Buffer` `byte[]` of the `First` frame. 
- Add an [`Index`](xref:Bonsai.Expressions.IndexBuilder) operator the the first frame buffer and set its `Value` to 1 to access the unique address ID. 
- Add a [`ConvertToString`](xref:Bonsai.ZeroMQ.ConvertToString) to the `Last` frame. 

:::workflow
![Router message parsing](~/workflows/router-message-parsing.bonsai)
:::

Running the workflow and then triggering client messages with key presses, we should see a unique `byte` value for each client in the [`Index`](xref:Bonsai.Expressions.IndexBuilder) operator output and a corresponding `string` in the [`ConvertToString`](xref:Bonsai.ZeroMQ.ConvertToString) operator output.

## Client address tracking
So far our network is rather one-sided. We can send client messages to the server which can in turn receive and parse them, but currently nothing is relayed back to the clients. The first goal for server feedback is that any time a client message is received, the server sends this message back to all connected clients. To do this, we first need a way of keeping track of all active clients. 

- Add a [`Zip`](xref:Bonsai.Reactive.Zip) operator to the [`Index`](xref:Bonsai.Expressions.IndexBuilder) operator and connect the `byte[]` `Buffer` as the second input.

:::workflow
![Address key-value pair](~/workflows/address-kvp.bonsai)
:::

Every time the [`Router`](xref:Bonsai.ZeroMQ.Router) receives a message, the [`Zip`](xref:Bonsai.Reactive.Zip) will create a `Tuple` that can be thought of as a key-value pair, with the unique `byte` address of the client as the key, and the full `byte[]` address used by ZeroMQ for routing as the value. 

- Add a [`DistinctBy`](xref:Bonsai.Reactive.DistinctBy) operator after the [`Zip`](xref:Bonsai.Reactive.Zip) and set the `KeySelector` property to the `byte` value (`Item1`).

:::workflow
![Unique key-value pair](~/workflows/unique-kvp.bonsai)
:::

The [`DistinctBy`](xref:Bonsai.Reactive.DistinctBy) operator filters the output of [`Zip`](xref:Bonsai.Reactive.Zip) according to the unique `byte` value and produces a sequence containing only the distinct – or ‘new’ – values produced by [`Zip`](xref:Bonsai.Reactive.Zip). The output of [`DistinctBy`](xref:Bonsai.Reactive.DistinctBy) will therefore effectively be a sequence of unique client addresses corresponding to each connected client. We also need to store these unique values and make them available to other parts of the Bonsai workflow. 

- Add a [`ReplaySubject`](xref:Bonsai.Reactive.ReplaySubject) operator after [`DistinctBy`](xref:Bonsai.Reactive.DistinctBy) and name it ‘ClientAddresses’. 

:::workflow
![Address ReplaySubject](~/workflows/address-replay-subject.bonsai)
:::

A [`ReplaySubject`](xref:Bonsai.Reactive.ReplaySubject) has the useful feature that it stores its input sequence and replays those values to any current or future subscribers. The effect in this case is that anything that subscribes to `ClientAddresses` will receive all the unique client addresses encountered by the server so far.

## Server --> client communication
Eventually, we will use these unique client addresses to route server messages back to specific client. For now, we'll implement a more basic approach where the server just sends messages back to the client that originally sent them. The Bonsai ZeroMQ library provides a convenient operator for this task in the form of [`SendResponse`](xref:Bonsai.ZeroMQ.SendResponse). 

- Add a [`SendResponse`](xref:Bonsai.ZeroMQ.SendResponse) operator after the [`Router`](xref:Bonsai.ZeroMQ.Router) in a separate branch.
- Inside (double-click on [`SendResponse`](xref:Bonsai.ZeroMQ.SendResponse)) add a [`String`](xref:Bonsai.Expressions.StringProperty) operator with a generic response value like `ServerResponse` after the `Source` operator. 

:::workflow
![Basic server response](~/workflows/server-basic-response.bonsai)
:::

The [`SendResponse`](xref:Bonsai.ZeroMQ.SendResponse) operator has a couple of interesting properties which may not be immediately obvious from this simple example. First, this operator always transmits its response back to the ZeroMQ socket that initiated the request (in this case one of our [`Dealer`](xref:Bonsai.ZeroMQ.Dealer) clients) and we therefore do not need to specify an address in its processing logic. Second, the internal flow of [`SendResponse`](xref:Bonsai.ZeroMQ.SendResponse) logic is computed asynchronously. This is very useful for responses that require more intensive computation and allows a [`Router`](xref:Bonsai.ZeroMQ.Router) to deal with frequent incoming [`Dealer`](xref:Bonsai.ZeroMQ.Dealer) requests efficiently. 

> [!Note]
> Imagine, for example, that our Dealer sockets were sending video snippets to a Router server that is tasked with doing some processing of the video and returning the results back to the Dealers. If the responses were not computed in an asynchronous manner we would start to incur a bottleneck on the router if there were many connected Dealers or frequent Dealer requests.

Running this workflow, you should see a 'bounceback' where any [`Dealer`](xref:Bonsai.ZeroMQ.Dealer) client that sends a message receives a reply from the [`Router`](xref:Bonsai.ZeroMQ.Router) server. However, in order to address these messages to specific other clients we need to take a slightly different approach. 

- Delete the [`SendResponse`](xref:Bonsai.ZeroMQ.SendResponse) and [`ConvertToString`](xref:Bonsai.ZeroMQ.ConvertToString) branches.
- Replace with a [`SelectMany`](xref:Bonsai.Reactive.SelectMany) called `BouceBack` that generates a bounceback message without using the [`SendResponse`](xref:Bonsai.ZeroMQ.SendResponse) operator: 

:::workflow
![Server message multicast](~/workflows/server-message-multicast.bonsai)
:::

We had to change quite a few things to modify this workflow so let's step through the general logic. The first thing to note is that since we are avoiding the [`SendResponse`](xref:Bonsai.ZeroMQ.SendResponse) operator in this implementation we need to pass messages directly into the [`Router`](xref:Bonsai.ZeroMQ.Router). To do this we generate a [`BehaviorSubject`](xref:Bonsai.Reactive.BehaviorSubject) source with a `NetMQMessage` output type and connect it to the [`Router`](xref:Bonsai.ZeroMQ.Response) (can implement this by creating a [`ToMessage`](xref:Bonsai.ZeroMQ.ToMessage) operator, right-clicking it and creating a [`BehaviorSubject`](xref:Bonsai.Reactive.BehaviorSubject) source). This will change the output type of the [`Router`](xref:Bonsai.ZeroMQ.Router) operator from a `ResponseContext` to a `NetMQMessage` so we need to make some modifications to how we process the stream.

We want the [`Router`](xref:Bonsai.ZeroMQ.Router) to generate a reply message every time it receives a request from a [`Dealer`](xref:Bonsai.ZeroMQ.Dealer). Since we are now building this message ourselves instead of using [`SendResponse`](xref:Bonsai.ZeroMQ.SendResponse), we branch off the [`Router`](xref:Bonsai.ZeroMQ.Router) with a [`SelectMany`](xref:Bonsai.Reactive.SelectMany) operator. Inside, we split the `NetMQMessage` into its component `NetMQFrame` parts, taking the `First` frame for the address, using [`Index`](xref:Bonsai.Expressions.IndexBuilder) to grab the middle empty delimiter frame and creating a new `String` which we convert to a `NetMQFrame` for the message content. We [`Merge`](xref:Bonsai.Reactive.Merge) these component frames back together and use a [`Take`](xref:Bonsai.Reactive.Take) operator (with count = 3) followed by [`ToMessage`](xref:Bonsai.ZeroMQ.ToMessage). The [`Take`](xref:Bonsai.Reactive.Take) operator is particularly important here as 1) [`ToMessage`](xref:Bonsai.ZeroMQ.ToMessage) will only complete the message once the observable stream is completed and 2) We need to close the observable anyway to complete the [`SelectMany`](xref:Bonsai.Reactive.SelectMany). Finally, we use a [`MulticastSubject`](xref:Bonsai.Expressions.MulticastSubject) to send our completed message to the [`Router`](xref:Bonsai.ZeroMQ.Router).

If we run the workflow now, we should see the same behavior as before (server bounces message back to initiating client).

## SelectMany detour
Now our network has a complete loop of client --> server --> client communication, but only the client that sends a message receives anything back from the server. Instead we’d like all clients to know when any of the clients sends a message. We already have access to the connected clients from `ClientAddresses`, and we know how to package data and send it back to clients via the [`Router`](xref:Bonsai.ZeroMQ.Router). In an imperative language we would do something like:

```
foreach (var client in Clients) {
    Router.SendMessage(client.Address, Message);
}
```

using a loop to send the message back to each client in turn. In a reactive / observable sequence based framework we have to think about this a bit differently. The solution is to use a [`SelectMany`](xref:Bonsai.Reactive.SelectMany) operator and it is worth taking a detour here to understand its use in some detail since we have already used it to generate our bounceback message and will apply it again for addressing multiple clients.

The [`SelectMany`](xref:Bonsai.Reactive.SelectMany) operator can be a tricky one to understand. Lee Campbell’s excellent [Introduction to Rx](http://introtorx.com/Content/v1.0.10621.0/08_Transformation.html#SelectMany) book does a good job of summarising its utility, suggesting we think of it as “from one, select many” or “from one, select zero or more”. In our case, we can think of [`SelectMany`](xref:Bonsai.Reactive.SelectMany) as a way to repeat some processing logic several times and feed the output of each repeat into a single sequence. More concretely, taking a single message and repeating the act of sending it several times for each client address. It is easier to show by example, so let’s set up a toy example in our project. 

Create a [`KeyDown`](xref:Bonsai.Windows.Input.KeyDown) operator followed by a [`SelectMany`](xref:Bonsai.Reactive.SelectMany). Set the `Filter` for the [`KeyDown`](xref:Bonsai.Windows.Input.KeyDown) to a key that hasn’t been assigned to a client yet – here I will use ‘A’. Inside the [`SelectMany`](xref:Bonsai.Reactive.SelectMany) oeprator add a [`SubscribeSubject`](xref:Bonsai.Expressions.SubscribeSubject) and set its subscription to the `ClientAddresses` subject we created earlier to replay unique client addresses. Add a [`TakeUntil`](xref:Bonsai.Reactive.TakeUntil) operator after the [`SubscribeSubject`](xref:Bonsai.Expressions.SubscribeSubject) and connect the output of [`TakeUntil`](xref:Bonsai.Reactive.TakeUntil) to the [`WorkflowOutput`](xref:Bonsai.Expressions.WorkflowOutputBuilder) (disconnecting the `Source` operator). Finally, create a [`KeyUp`](xref:Bonsai.Windows.Input.KeyUp) operator and connect it to [`TakeUntil`](xref:Bonsai.Reactive.TakeUntil). Set the key `Filter` for [`KeyUp`](xref:Bonsai.Windows.Input.KeyUp) to the same as previously created [`KeyDown`](xref:Bonsai.Windows.Input.KeyDown) operator outside the [`SelectMany`](xref:Bonsai.Reactive.SelectMany).

:::workflow
![SelectMany detour](~/workflows/select-many-detour.bonsai)
:::

Run the project and inspect the output of the [`SelectMany`](xref:Bonsai.Reactive.SelectMany) operator. If no client messages are triggered and we press ‘A’ to trigger the [`SelectMany`](xref:Bonsai.Reactive.SelectMany) nothing will be returned. If we trigger a single client and press ‘A’ again [`SelectMany`](xref:Bonsai.Reactive.SelectMany) gives us the address of that client. If we trigger a second client and press ‘A’ we get the addresses of these first two clients in sequence, and so on if we add the third client. Whenever we press ‘A’ we get a sequence of all the connected client addresses. Every time we trigger [`SelectMany`](xref:Bonsai.Reactive.SelectMany) with a [`KeyDown`](xref:Bonsai.Windows.Input.KeyDown) we generate a new sequence that immediately subscribes to `ClientAddresses`, a [`ReplaySubject`](xref:Bonsai.Reactive.ReplaySubject) which replays all our unique client addresses into the sequence. We could keep initiating these new sequences by continually pressing ‘A’ and if a new client address were to be added then all these sequences would report the new address (you can test this by connecting the [`SusbcribeSubject`](xref:Bonsai.Expressions.SubscribeSubject) directly to the workflow output and deleting [`KeyUp`](xref:Bonsai.Windows.Input.KeyUp) and [`TakeUntil`](xref:Bonsai.Reactive.TakeUntil)). Instead, we want to complete each new sequence once it has given us all the client addresses so we use an arbitrary event (releasing the key that initiated the sequence) to trigger [`TakeUntil`](xref:Bonsai.Reactive.TakeUntil) and close the sequence. The overall effect is something similar to a loop that iterates over all client addresses every time we request it with a key press. This is the general structure of what we want to achieve next in our server logic to broadcast messages back to all connected clients.

## All client broadcast
To apply the logic of the [`SelectMany`](xref:Bonsai.Reactive.SelectMany) example to server broadcast, we need something to trigger the [`SelectMany`](xref:Bonsai.Reactive.SelectMany) sequence creation, and something to trigger termination. We already have a trigger for sequence creation in the output of the [`Router`](xref:Bonsai.ZeroMQ.Router) since we want to run our [`SelectMany`](xref:Bonsai.Reactive.SelectMany) sequence every time a client message is received. For our sequence temination trigger, we want something that is guaranteed to fire after the server receives a client message, but before the next message is received so that our [`SelectMany`](xref:Bonsai.Reactive.SelectMany) sequence for each message responds only to that particular message. A simple solution is therefore to use the arrival of the next message as our sequence termination trigger.

- Add a [`Skip`](xref:Bonsai.Reactive.Skip) operator after the [`Router`](xref:Bonsai.ZeroMQ.Router) in a separate branch and connect this to a [`PublishSubject`](xref:Bonsai.Reactive.PublishSubject). 
- Set the [`Skip`](xref:Bonsai.Reactive.Skip) operator's `Count` property is set to 1, and name the [`PublishSubject`](xref:Bonsai.Reactive.PublishSubject) 'NextMessage'.

:::workflow
![Server next message](~/workflows/server-next-message.bonsai)
:::

The logic here is that we use [`Skip`](xref:Bonsai.Reactive.Skip) to create a sequence that lags exactly 1 message behind the [`Router`](xref:Bonsai.ZeroMQ.Router) sequence of received messages, i.e. when the first message is received, `NextMessage` will not produce a result until the second message is received. We can then use this inside our [`SelectMany`](xref:Bonsai.Reactive.SelectMany) logic for generating server messages. 

- Add a [`SelectMany`](xref:Bonsai.Reactive.SelectMany) operator after the [`Router`](xref:Bonsai.ZeroMQ.Router) in a separate branch and name it ‘SelectAllClients’.
- Inside the [`SelectMany`](xref:Bonsai.Reactive.SelectMany) operator, create 2 [`SubscribeSubject`](xref:Bonsai.Expressions.SubscribeSubject) operators and link them to the `ClientAddresses` and `NextMessage` subjects. 
- Connect the `ClientAddresses` subscription to the workflow output via a [`TakeUntil`](xref:Bonsai.Reactive.TakeUntil) operator and use `NextMessage` as the second input. 

Now, our `SelectAllClients` will produce a sequence of all unique client addresses every time the server receives a message. Connect the output of `SelectAllClients` to a [`WithLatestFrom`](xref:Bonsai.Reactive.WithLatestFrom) with the [`Router`](xref:Bonsai.ZeroMQ.Router) as its second input. In this context [`WithLatestFrom`](xref:Bonsai.Reactive.WithLatestFrom) combines each client address from `SelectAllClients` with the most recent received message. The result is that when a message is received from the client, we produce several copies of the message 'addressed' to each connected client.

:::workflow
![Select all clients and package message](~/workflows/format-select-all-clients.bonsai)
:::

To send these messages back to our clients, we will modify the logic in our previous `BounceBack` operator. 

- Create a [`SelectMany`](xref:Bonsai.Reactive.SelectMany) called `BroadcastAll` that takes the `byte[]` addresses from `SelectAllClients` and reformats the original message with this address as the first frame. 
- [`Multicast`](xref:Bonsai.Expressions.MulticastSubject) back into the router to send the original address back to all clients.

Inside `BroadcastAll` the source consists of a `Tuple` of our key-value-pair of client ID / address and the received message. We take the `byte[]` address and use [`ConvertToFrame`](xref:Bonsai.ZeroMQ.ConvertToFrame) to convert it to a `NetMQFrame` and then merge it with the empty delimiter and the message payload. As before, we [`Take`](xref:Bonsai.Reactive.Take) 3 elements to close the message construction stream, convert to a `NetMQMessage` with [`ToMessage`](xref:Bonsai.ZeroMQ.ToMessage) and then [`Multicast`](xref:Bonsai.Expressions.MulticastSubject) into `RouterMessages`. If you run the workflow now you should see that each time a [`Dealer`](xref:Bonsai.ZeroMQ.Dealer) produces a message, all clients receive a copy of that message.

:::workflow
![Broadcast to all clients](~/workflows/broadcast-all-clients.bonsai)
:::

## Leave-one-out broadcast
This is getting pretty close to our original network architecture goal but there is still some redundancy present. When client 1 sends a message to the server, clients 1, 2 and 3 all receive a copy of that message back from the server. this is fine for clients 2 and 3 as they are not aware of client 1's messages without server communication; but client 1 does not need this message copy since it already originated the message. Our goal then is that the server should send message copies back to all clients except the client that originated message.

- Create a [`Condition`](xref:Bonsai.Reactive.Condition) before `BroadcastAll` to filter only non sender clients called `NonSenderClients`. 
- Inside `NonSenderClients` expose the `byte` corresponding to the client ID and index 1 of the first frame of the `NetMQMessage` from `Source1`. 
- [`Zip`](xref:Bonsai.Reactive.Zip) these together and use [`NotEqual`](xref:Bonsai.Expressions.NotEqualBuilder) to compare the client ID of the message with the existing clients and discard where the IDs are the same.

:::workflow
![Broadcast to non-senders](~/workflows/broadcast-non-sender-clients.bonsai)
:::

Running the workflow you should see that we have now achieved the desired architecture. When a client [`Dealer`](xref:Bonsai.ZeroMQ.Dealer) sends a message, it is broadcast to all other joined clients except for itself.
