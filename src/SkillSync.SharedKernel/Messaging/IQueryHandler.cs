using SkillSync.SharedKernel.Results;

namespace SkillSync.SharedKernel.Messaging;

public interface IQueryHandler<in TQuery, TResponse>
     where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
