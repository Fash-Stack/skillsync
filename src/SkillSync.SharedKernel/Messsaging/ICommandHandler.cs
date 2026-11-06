using SkillSync.SharedKernel.Results;

namespace SkillSync.SharedKernel.Messsaging;

public interface ICommandHandler<in TCommand>
     where TCommand : ICommand
{
    Task<Result> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandhandler<in TCommand, TResponse>
     where TCommand : ICommand<TResponse>
{ 
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}
