using System;

namespace EdgeMq.Service.Exceptions;

public class EdgeQueueException(string message) : Exception(message) { }