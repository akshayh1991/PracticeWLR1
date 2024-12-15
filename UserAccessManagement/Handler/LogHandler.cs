using MediatR;
using Serilog;

namespace UserAccessManagement.Handler
{

    public class InfoLogCommand : IRequest<Unit>
    {
        public string Message { get; set; }

        public object[]? Properties { get; set; }
    }

    public class ErrorLogCommand : IRequest<Unit>
    {
        public string Message { get; set; } = string.Empty;
        public Exception? Exception { get; set; }

    }


    public class InfoLogCommandHandler : IRequestHandler<InfoLogCommand, Unit>
    {
        public Task<Unit> Handle(InfoLogCommand request, CancellationToken cancellationToken)
        {

            if (request.Properties != null && request.Properties.Length > 0)
            {
                Log.Information(request.Message, request.Properties);
            }
            else
            {
                Log.Information(request.Message);
            }
            return Task.FromResult(Unit.Value); // Return Unit.Value for void commands
        }
    }

    public class ErrorLogCommandHandler : IRequestHandler<ErrorLogCommand, Unit>
    {
        public Task<Unit> Handle(ErrorLogCommand request, CancellationToken cancellationToken)
        {
            Log.Error(request.Exception, request.Message);
            return Task.FromResult(Unit.Value);
        }
    }

}
