using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<BlogCategory> BlogCategories => Set<BlogCategory>();
    public DbSet<BlogTag> BlogTags => Set<BlogTag>();
    public DbSet<BlogPostTag> BlogPostTags => Set<BlogPostTag>();
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
    public DbSet<InventarioItem> InventarioItens => Set<InventarioItem>();
    public DbSet<InventarioMovimentacao> InventarioMovimentacoes => Set<InventarioMovimentacao>();
    public DbSet<GoogleAgendaConexao> GoogleAgendaConexoes => Set<GoogleAgendaConexao>();
    public DbSet<TurmaHorario> TurmaHorarios => Set<TurmaHorario>();
    public DbSet<TurmaInstrutor> TurmaInstrutores => Set<TurmaInstrutor>();
    public DbSet<Aula> Aulas => Set<Aula>();
    public DbSet<FrequenciaAluno> FrequenciasAlunos => Set<FrequenciaAluno>();
    public DbSet<EventoAlunoPortal> EventosAlunoPortal => Set<EventoAlunoPortal>();
    public DbSet<EventoAlunoPortalAlvo> EventosAlunoPortalAlvos => Set<EventoAlunoPortalAlvo>();
    public DbSet<Comunicado> Comunicados => Set<Comunicado>();
    public DbSet<ComunicadoAlvo> ComunicadosAlvos => Set<ComunicadoAlvo>();
    public DbSet<ComunicadoLeitura> ComunicadosLeituras => Set<ComunicadoLeitura>();
    public DbSet<DocumentoTipo> DocumentoTipos => Set<DocumentoTipo>();
    public DbSet<DocumentoSolicitacao> DocumentoSolicitacoes => Set<DocumentoSolicitacao>();
    public DbSet<DocumentoEnvio> DocumentoEnvios => Set<DocumentoEnvio>();
    public DbSet<Insignia> Insignias => Set<Insignia>();
    public DbSet<AlunoInsignia> AlunoInsignias => Set<AlunoInsignia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.Entity<UsuarioSistema>().HasQueryFilter(x => !x.Excluido);
    }
}
