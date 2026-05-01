using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<AlunoTurma> AlunosTurmas => Set<AlunoTurma>();
    public DbSet<Turma> Turmas => Set<Turma>();
    public DbSet<Mensalidade> Mensalidades => Set<Mensalidade>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();
    public DbSet<Desconto> Descontos => Set<Desconto>();
    public DbSet<AcordoFinanceiro> AcordosFinanceiros => Set<AcordoFinanceiro>();
    public DbSet<Admissao> Admissoes => Set<Admissao>();
    public DbSet<Desligamento> Desligamentos => Set<Desligamento>();
    public DbSet<Graduacao> Graduacoes => Set<Graduacao>();
    public DbSet<ExameGraduacao> ExamesGraduacao => Set<ExameGraduacao>();
    public DbSet<HistoricoAluno> HistoricosAlunos => Set<HistoricoAluno>();
    public DbSet<ConfiguracaoSistema> ConfiguracoesSistema => Set<ConfiguracaoSistema>();
    public DbSet<UsuarioSistema> UsuariosSistema => Set<UsuarioSistema>();
    public DbSet<RoleSistema> RolesSistema => Set<RoleSistema>();
    public DbSet<PermissaoSistema> PermissoesSistema => Set<PermissaoSistema>();
    public DbSet<UsuarioRole> UsuariosRoles => Set<UsuarioRole>();
    public DbSet<RolePermissao> RolesPermissoes => Set<RolePermissao>();
    public DbSet<UsuarioPermissao> UsuariosPermissoes => Set<UsuarioPermissao>();
    public DbSet<AuditoriaLog> AuditoriaLogs => Set<AuditoriaLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.Entity<UsuarioSistema>().HasQueryFilter(x => !x.Excluido);
    }
}
