using System.Text.Json.Serialization;
using EdgeMq.Model;

namespace EdgeMq.Api.Serialization;

[JsonSerializable(typeof(QueueMetrics))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }