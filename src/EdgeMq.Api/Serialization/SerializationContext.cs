using System.Text.Json.Serialization;
using EdgeMq.Model;

namespace EdgeMq.Api.Serialization;

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }