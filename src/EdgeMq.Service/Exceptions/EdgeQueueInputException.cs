namespace EdgeMq.Service.Exceptions;

public sealed class EdgeQueueInputException(string message) : EdgeQueueException(message) { }