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
                if (selectorObservableType != typeof(NetMQMessage))
                {
                    throw new InvalidOperationException($"The specified response workflow must have a single output of type {typeof(NetMQMessage)}.");
                }

                return Expression.Call(GetType(), nameof(Process), null, source, selector);
            });
        }

        static IObservable<NetMQMessage> Process(IObservable<ResponseContext> source, Func<IObservable<NetMQMessage>, IObservable<NetMQMessage>> selector)
        {
            return source.SelectMany(async request =>
            {
                var message = request.Request;
                var response = await selector(Observable.Return(message));
                request.Response.OnNext(response);
                request.Response.OnCompleted();
                return response;
            });
        }
    }
}
