using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class AdmissaoService(ApplicationDbContext dbContext) : IAdmissaoService
{
    public async Task<IReadOnlyList<Admissao>> ListarAsync(
        string? busca = null,
        StatusAdmissaoEnum? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Admissoes
            .AsNoTracking()
            .Include(x => x.Aluno)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var buscaTexto = busca.Trim();
            query = query.Where(x =>
                x.NomeInteressado.Contains(buscaTexto) ||
                (x.Aluno != null && x.Aluno.NomeCompleto.Contains(buscaTexto)));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return await query
            .OrderByDescending(x => x.DataAulaExperimental)
            .ThenBy(x => x.NomeInteressado)
            .ToListAsync(cancellationToken);
    }

    public Task<Admissao?> ObterDetalhesAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Admissoes
            .AsNoTracking()
            .Include(x => x.Aluno)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Turma>> ListarTurmasAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Turmas
            .AsNoTracking()
            .Where(x => x.Ativa)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CriarAsync(Admissao admissao, CancellationToken cancellationToken = default)
    {
        admissao.NomeInteressado = admissao.NomeInteressado.Trim();
        admissao.ChecklistObservacoes = LimparOpcional(admissao.ChecklistObservacoes);

        if (admissao.Status == StatusAdmissaoEnum.Matriculado)
        {
            admissao.Status = StatusAdmissaoEnum.EmAndamento;
        }

        await dbContext.Admissoes.AddAsync(admissao, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return admissao.Id;
    }

    public async Task<bool> AtualizarProcessoAsync(
        int id,
        StatusAdmissaoEnum status,
        bool contratoAssinado,
        bool pagamentoInicialConfirmado,
        bool integracaoConcluida,
        string? checklistObservacoes,
        CancellationToken cancellationToken = default)
    {
        var admissao = await dbContext.Admissoes
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (admissao is null)
        {
            return false;
        }

        if (status == StatusAdmissaoEnum.Matriculado && !admissao.AlunoId.HasValue)
        {
            return false;
        }

        admissao.Status = status;
        admissao.ContratoAssinado = contratoAssinado;
        admissao.PagamentoInicialConfirmado = pagamentoInicialConfirmado;
        admissao.IntegracaoConcluida = integracaoConcluida;
        admissao.ChecklistObservacoes = LimparOpcional(checklistObservacoes);

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<AdmissaoMatriculaResultado> CriarMatriculaAsync(
        int admissaoId,
        AdmissaoMatriculaInput input,
        CancellationToken cancellationToken = default)
    {
        var admissao = await dbContext.Admissoes
            .FirstOrDefaultAsync(x => x.Id == admissaoId, cancellationToken);

        if (admissao is null)
        {
            return new AdmissaoMatriculaResultado { Erro = "Admissão não encontrada." };
        }

        if (admissao.AlunoId.HasValue)
        {
            return new AdmissaoMatriculaResultado { Erro = "Esta admissão já possui matrícula vinculada." };
        }

        var cpf = SomenteDigitos(input.CPF);
        if (cpf.Length != 11)
        {
            return new AdmissaoMatriculaResultado { Erro = "Informe um CPF valido com 11 digitos." };
        }

        var cpfJaExiste = await dbContext.Alunos
            .AnyAsync(x => x.CPF == cpf || x.CPF.Replace(".", string.Empty).Replace("-", string.Empty) == cpf, cancellationToken);

        if (cpfJaExiste)
        {
            return new AdmissaoMatriculaResultado { Erro = "Já existe um aluno cadastrado com esse CPF." };
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var novoAluno = new Aluno
        {
            NomeCompleto = admissao.NomeInteressado,
            CPF = cpf,
            RG = LimparOpcional(input.RG),
            DataNascimento = input.DataNascimento,
            Endereco = LimparOpcional(input.Endereco),
            Celular = LimparOpcional(input.Celular),
            Email = LimparOpcional(input.Email),
            ContatoEmergencia = LimparOpcional(input.ContatoEmergencia),
            DataEntrada = DateOnly.FromDateTime(DateTime.Today),
            TurmaId = input.TurmaId,
            Status = StatusAlunoEnum.Ativo,
            Observacoes = LimparOpcional(input.ObservacoesAluno)
        };

        await dbContext.Alunos.AddAsync(novoAluno, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (novoAluno.TurmaId.HasValue)
        {
            dbContext.AlunosTurmas.Add(new AlunoTurma
            {
                AlunoId = novoAluno.Id,
                TurmaId = novoAluno.TurmaId.Value,
                DataVinculo = DateTime.UtcNow
            });
        }

        admissao.AlunoId = novoAluno.Id;
        admissao.DataMatricula = DateOnly.FromDateTime(DateTime.Today);
        admissao.Status = StatusAdmissaoEnum.Matriculado;

        dbContext.HistoricosAlunos.Add(new HistoricoAluno
        {
            AlunoId = novoAluno.Id,
            DataEvento = DateTime.Now,
            TipoEvento = "Admissao",
            Descricao = $"Matricula criada a partir da admissao #{admissao.Id}."
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new AdmissaoMatriculaResultado
        {
            Sucesso = true,
            AlunoId = novoAluno.Id
        };
    }

    private static string SomenteDigitos(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return string.Empty;
        }

        return new string(valor.Where(char.IsDigit).ToArray());
    }

    private static string? LimparOpcional(string? texto)
    {
        var valor = texto?.Trim();
        return string.IsNullOrWhiteSpace(valor) ? null : valor;
    }
}
