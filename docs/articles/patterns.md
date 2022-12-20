# Patterns

ZeroMQ provides a set of [sockets and patterns](https://zguide.zeromq.org/docs/chapter2/) for building distributed systems. We have adapted these patterns to fit neatly into the Bonsai visual language by defining a set of reactive operators named after each socket type.

| Pattern | Description | Sockets |
| ------- | ----------- | ------- |
| [Publish-Subscribe](pub-sub.md) | One to many data distribution | <xref href="Bonsai.ZeroMQ.Publisher"/>, <xref href="Bonsai.ZeroMQ.Subscriber"/> |

Each section in this chapter describes a basic ZeroMQ messaging pattern, usually involving complementary pairs of sockets, and provides examples of use that you can copy and paste directly into the editor. Most examples make use of the [OSC](xref:Bonsai.Osc) package for formatting and parsing binary-coded messages for numeric data, or the <xref href="Bonsai.Vision.EncodeImage"/> and <xref href="Bonsai.Vision.DecodeImage"/> operators for serializing video data.