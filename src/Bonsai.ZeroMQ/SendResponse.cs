using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using Bonsai.Expressions;
using NetMQ;

namespace Bonsai.ZeroMQ
{
    /// <summary>
    /// Represents an operator that computes the result of a remote request
    /// asynchronously and transmits the response through the corresponding socket.
    /// </summary>
    [Description("Computes the result of a remote request asynchronously and transmits the response through the corresponding socket.")]
    public class SendResponse : WorkflowExpressionBuilder
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 1, upperBound: 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="SendResponse"/> class.
        /// </summary>
        public SendResponse()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendResponse"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public SendResponse(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        /// <inheritdoc/>
        public override Range<int> ArgumentRange => argumentRange;

        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.FirstOrDefault();
            if (source == null)
            {
                throw new InvalidOperationException("There must be at least one input to the response workflow.");
            }

            var inputParameter = Expression.Parameter(typeof(IObservable<NetMQMessage>));
            return BuildWorkflow(arguments, inputParameter, selectorBody =>
            {
                var selector = Expression.Lambda(selectorBody, inputParameter);
                var selectorObservableType = selector.ReturnType.GetGenericArguments()[0];
                if (selectorObservableType != typeof(NetMQMessage) &&
                    selectorObservableType != typeof(NetMQFrame) &&
                    selectorObservableType != typeof(string) &&
                    selectorObservableType != typeof(byte[]))
                {
                    throw new InvalidOperationException($"The type of the response workflow is not compatible with {typeof(NetMQMessage)}.");
                }

                return Expression.Call(GetType(), nameof(Process), null, source, selector);
            });
        }

        static IObservable<NetMQMessage> Process(IObservable<ResponseContext> source, Func<IObservable<NetMQMessage>, IObservable<NetMQMessage>> selector)
        {
            return source.SelectMany(context => selector(Observable
                .Return(context.Request))
                .LastAsync()
                .Do(context.Response));
        }

        static IObservable<NetMQMessage> Process(
            IObservable<ResponseContext> source,
            Func<IObservable<NetMQMessage>, IObservable<byte[]>> selector)
        {
            return Process(source, selector, (response, buffer) => response.Append(buffer));
        }

        static IObservable<NetMQMessage> Process(
            IObservable<ResponseContext> source,
            Func<IObservable<NetMQMessage>, IObservable<string>> selector)
        {
            return Process(source, selector, (response, message) => response.Append(message, SendReceiveConstants.DefaultEncoding));
        }

        static IObservable<NetMQMessage> Process(
            IObservable<ResponseContext> source,
            Func<IObservable<NetMQMessage>, IObservable<NetMQFrame>> selector)
        {
            return Process(source, selector, (response, frame) => response.Append(frame));
        }

        static IObservable<NetMQMessage> Process<TFrame>(
            IObservable<ResponseContext> source,
            Func<IObservable<NetMQMessage>, IObservable<TFrame>> selector,
            Action<NetMQMessage, TFrame> appendFrame)
        {
            return source.SelectMany(context => selector(Observable
                .Return(context.Request))
                .ToList()
                .Select(frames =>
                {
                    var delimiter = context.Request.FrameCount - 1;
                    for (; delimiter >= 0; delimiter--)
                    {
                        if (context.Request[delimiter].IsEmpty)
                        {
                            break;
                        }
                    }

                    var response = new NetMQMessage(frames.Count + delimiter + 1);
                    if (delimiter >= 0)
                    {
                        for (int i = 0; i <= delimiter; i++)
                        {
                            response.Append(context.Request[i]);
                        }
                    }

                    foreach (var frame in frames)
                    {
                        appendFrame(response, frame);
                    }

                    return response;
                })
                .Do(context.Response));
        }
    }
}
