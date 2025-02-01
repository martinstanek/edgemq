namespace EdgeMq.Service.Exceptions;

public sealed class EdgeConfigurationException(string message) : EdgeQueueException(message);