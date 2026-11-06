using SkillSync.SharedKernel.Results;

namespace SkillSync.SharedKernel.Messsaging;

public interface IQueryHandler<in TQuery, TResponse>
     where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
