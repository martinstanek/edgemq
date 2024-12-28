namespace EdgeMq.Service1.Exceptions;

public sealed class EdgeQueueAcknowledgeException(string message) : EdgeQueueException(message) { }