using System.Collections.Generic;
using System.Text;

namespace EdgeMq.Service.Store.FileSystem;

public static class StoreMessageBinaryConverter
{
    private const byte Delimiter = 0xFF;

    public static StoreMessage FromBytes(byte[] bytes)
    {
        var buffer = new List<byte>();
        var chunks = new Stack<string>();
        string chunkString;

        for (var i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] == Delimiter)
            {
                chunkString = Encoding.UTF8.GetString(buffer.ToArray());
                chunks.Push(chunkString);
                buffer.Clear();

                continue;
            }

            buffer.Add(bytes[i]);
        }

        chunkString = Encoding.UTF8.GetString(buffer.ToArray());
        chunks.Push(chunkString);

        var message = new StoreMessage
        {
            Payload = chunks.Pop()
        };

        var headers = new Dictionary<string, string>();

        while (chunks.Count > 0)
        {
            var value = chunks.Pop();
            var key = chunks.Pop();
            headers.Add(key, value);
        }

        return message with { Headers = headers };
    }

    public static byte[] FromStoreMessage(StoreMessage message)
    {
        var headersArray = new List<byte>();

        foreach (var messageHeader in message.Headers)
        {
            headersArray.AddRange(Encoding.UTF8.GetBytes(messageHeader.Key));
            headersArray.Add(Delimiter);
            headersArray.AddRange(Encoding.UTF8.GetBytes(messageHeader.Value));
            headersArray.Add(Delimiter);
        }

        headersArray.AddRange(Encoding.UTF8.GetBytes(message.Payload));

        return headersArray.ToArray();
    }
}