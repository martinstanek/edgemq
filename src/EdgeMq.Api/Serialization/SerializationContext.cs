using System.Collections.Generic;
using System.Text.Json.Serialization;
using EdgeMq.Model;

namespace EdgeMq.Api.Serialization;

[JsonSerializable(typeof(QueueMetrics))]
[JsonSerializable(typeof(QueueRawMessage))]
[JsonSerializable(typeof(QueueRawMessage[]))]
[JsonSerializable(typeof(IReadOnlyCollection<QueueRawMessage>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }