using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.Entities;

namespace IkkonAdmin.Web.Services;

public interface ITurmaService
{
    Task<OperationResult<int>> CriarAsync(Turma turma, IReadOnlyCollection<int> alunosIds, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarAsync(int id, Turma turmaAtualizada, IReadOnlyCollection<int> alunosIds, CancellationToken cancellationToken = default);
}
