using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.Entities;

public class UsuarioSistema
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Login { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string LoginNormalizado { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(150)]
    public string? EmailNormalizado { get; set; }

    [StringLength(30)]
    public string? Telefone { get; set; }

    [StringLength(300)]
    public string? FotoPerfilUrl { get; set; }

    [Required, StringLength(200)]
    public string NomeExibicao { get; set; } = string.Empty;

    [Required]
    public string SenhaHash { get; set; } = string.Empty;

    public TipoAcessoEnum TipoAcesso { get; set; } = TipoAcessoEnum.Funcionario;

    public TemaPreferenciaEnum TemaPreferencia { get; set; } = TemaPreferenciaEnum.Claro;
    public IdiomaPreferenciaEnum IdiomaPreferencia { get; set; } = IdiomaPreferenciaEnum.PtBr;
    public bool NotificarEmail { get; set; } = true;
    public bool NotificarSistema { get; set; } = true;
    public bool Ativo { get; set; } = true;
    public bool Excluido { get; set; }
    public DateTime? DataExclusaoUtc { get; set; }
    public int? ExcluidoPorUsuarioId { get; set; }

    public int? AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public DateTime DataCriacaoUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UltimoLoginUtc { get; set; }

    public ICollection<UsuarioRole> UsuarioRoles { get; set; } = new List<UsuarioRole>();
    public ICollection<UsuarioPermissao> UsuarioPermissoes { get; set; } = new List<UsuarioPermissao>();
    public ICollection<AuditoriaLog> LogsComoAutor { get; set; } = new List<AuditoriaLog>();
    public ICollection<AuditoriaLog> LogsComoAfetado { get; set; } = new List<AuditoriaLog>();
    public ICollection<TurmaInstrutor> TurmasComoInstrutor { get; set; } = new List<TurmaInstrutor>();
    public ICollection<Aula> AulasComoInstrutor { get; set; } = new List<Aula>();
}
