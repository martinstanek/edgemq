using System.Linq;
using EdgeMq.Service.Configuration;

namespace EdgeMq.Service.Validation;

public static class Validations
{
    private const int EdgeMaxQueueNameLenght = 20;
    private const int EdgeMinQueueNameLenght = 2;

    public static bool IsQueueNameValid(string queueName)
    {
        return queueName.Length >= EdgeMinQueueNameLenght
               && queueName.Length <= EdgeMaxQueueNameLenght
               && queueName.All(ch => char.IsAsciiLetterOrDigit(ch) || ch.Equals('-'));
    }

    public static bool IsConfigurationValid(EdgeQueueConfiguration configuration)
    {
        var result = true;

        result &= configuration.BufferConfiguration.MaxMessageCount > 0;
        result &= configuration.BufferConfiguration.MaxMessageSizeBytes > 0;
        result &= configuration.BufferConfiguration.MaxPayloadSizeBytes > 0;
        result &= configuration.StoreConfiguration.MaxMessageCount > 0;
        result &= configuration.StoreConfiguration.MaxMessageSizeBytes > 0;
        result &= configuration.StoreConfiguration.DefaultBatchSize > 0;
        result &= configuration.EdgeProcessingMessagesDelayMs > 10;
        result &= !string.IsNullOrWhiteSpace(configuration.StoreConfiguration.Path);
        result &= IsQueueNameValid(configuration.Name);

        return result;
    }
}