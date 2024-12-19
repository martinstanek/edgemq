namespace EdgeMQ.Service.Exceptions;

public sealed class EdgeQueueInputException(string message) : EdgeQueueException(message) { }