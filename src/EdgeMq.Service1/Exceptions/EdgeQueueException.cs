using System;

namespace EdgeMq.Service1.Exceptions;

public class EdgeQueueException(string message) : Exception(message) { }