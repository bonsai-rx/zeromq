# Patterns

ZeroMQ provides a set of [sockets and patterns](https://zguide.zeromq.org/docs/chapter2/) for building distributed systems. We have adapted these patterns to fit neatly into the Bonsai visual language by defining a set of reactive operators named after each socket type.

| Pattern | Description | Operators |
| ------- | ----------- | --------- |
| @pub-sub | One to many data distribution | <xref href="Bonsai.ZeroMQ.Publisher"/>, <xref href="Bonsai.ZeroMQ.Subscriber"/> |
| @req-rep | Remote procedure call | <xref href="Bonsai.ZeroMQ.Request"/>, <xref href="Bonsai.ZeroMQ.Response"/> |
| @router-dealer | Asynchronous requests from multiple clients | <xref href="Bonsai.ZeroMQ.Router"/>, <xref href="Bonsai.ZeroMQ.Dealer"/> |
| @push-pull | Fan-out / fan-in task distribution | <xref href="Bonsai.ZeroMQ.Push"/>, <xref href="Bonsai.ZeroMQ.Pull"/> |
| @proxy | Broker / intermediation patterns | <xref href="Bonsai.ZeroMQ.ProxyFrontend"/>, <xref href="Bonsai.ZeroMQ.ProxyBackend"/> |

Each section in this chapter describes a basic ZeroMQ messaging pattern, usually involving complementary pairs of sockets, and provides examples of use that you can copy and paste directly into the editor.