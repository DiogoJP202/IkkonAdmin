using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.Entities;

namespace IkkonAdmin.Web.Services;

public interface IAlunoService
{
    Task<OperationResult<int>> CriarAsync(Aluno aluno, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarAsync(int id, Aluno alunoAtualizado, CancellationToken cancellationToken = default);
    Task<OperationResult> AlterarStatusAsync(int id, StatusAlunoEnum status, CancellationToken cancellationToken = default);
}
