namespace EdgeMq.Service1.Exceptions;

public sealed class EdgeQueueInputException(string message) : EdgeQueueException(message) { }