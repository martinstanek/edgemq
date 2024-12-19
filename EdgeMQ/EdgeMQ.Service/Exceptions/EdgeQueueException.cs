using System;

namespace EdgeMQ.Service.Exceptions;

public class EdgeQueueException(string message) : Exception(message) { }