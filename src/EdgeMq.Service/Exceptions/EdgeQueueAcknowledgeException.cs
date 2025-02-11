namespace EdgeMq.Service.Exceptions;

public sealed class EdgeQueueAcknowledgeException(string message) : EdgeQueueException(message) { }