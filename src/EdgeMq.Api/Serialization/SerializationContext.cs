using System.Collections.Immutable;
using System.Text.Json.Serialization;
using EdgeMq.Model;

namespace EdgeMq.Api.Serialization;

[JsonSerializable(typeof(Queue))]
[JsonSerializable(typeof(QueueServer))]
[JsonSerializable(typeof(QueueRawMessage))]
[JsonSerializable(typeof(QueueRawMessage[]))]
[JsonSerializable(typeof(QueueEnqueueResult))]
[JsonSerializable(typeof(ImmutableArray<Queue>))]
[JsonSerializable(typeof(ImmutableArray<QueueRawMessage>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }