using MessagePipe;
using UnityEngine;

namespace HoyarCreation.TriadCluster
{
    // This script auto-initializes at app launch
    public static class TriadClusterEventInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void InitMessagePipe()
        {
            Debug.Log("[InitMessagePipe]");

            var builder = new BuiltinContainerBuilder();

            builder.AddMessagePipe(/* configure option */);
            AddMessageBrokers(builder);

            // AddMessageHandlerFilter: Register for filter, also exists RegisterAsyncMessageHandlerFilter, Register(Async)RequestHandlerFilter
            // builder.AddMessageHandlerFilter<MyFilter<int>>();

            // create provider and set to Global(to enable diagnostics window and global fucntion)
            var provider = builder.BuildServiceProvider();
            GlobalMessagePipe.SetProvider(provider);
        }

        private static void AddMessageBrokers(BuiltinContainerBuilder builder)
        {
            // also exists AddMessageBroker<TKey, TMessage>, AddRequestHandler, AddAsyncRequestHandler
            // AddMessageBroker: Register for IPublisher<T>/ISubscriber<T>, includes async and buffered.
            builder.AddMessageBroker<UpdateInputPointsEvent>();
            builder.AddMessageBroker<OnTriangleDownEvent>();
            builder.AddMessageBroker<OnTriangleStayEvent>();
            builder.AddMessageBroker<OnTriangleUpEvent>();
        }
    }
}