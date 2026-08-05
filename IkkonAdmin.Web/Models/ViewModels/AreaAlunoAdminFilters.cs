using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Pagination;

namespace IkkonAdmin.Web.Models.ViewModels;

public sealed class AulaAdminFilter : PageRequest
{
    public DateOnly? Inicio { get; set; }
    public DateOnly? Fim { get; set; }
    public int? TurmaId { get; set; }
    public int? InstrutorId { get; set; }
    public StatusAulaEnum? Status { get; set; }

    public bool HasActiveFilters => Inicio.HasValue || Fim.HasValue || TurmaId.HasValue ||
                                    InstrutorId.HasValue || Status.HasValue;
}

public sealed class FrequenciaAdminFilter : PageRequest
{
    public DateOnly? Inicio { get; set; }
    public DateOnly? Fim { get; set; }
    public int? TurmaId { get; set; }
    public int? InstrutorId { get; set; }
    public bool? Preenchida { get; set; }

    public bool HasActiveFilters => Inicio.HasValue || Fim.HasValue || TurmaId.HasValue ||
                                    InstrutorId.HasValue || Preenchida.HasValue;
}

public sealed class DocumentoAdminFilter : PageRequest
{
    public int? AlunoId { get; set; }
    public int? TipoId { get; set; }
    public DocumentoStatusEnum? Status { get; set; }
    public DateOnly? PrazoAte { get; set; }
    public bool? PossuiEnvio { get; set; }

    public bool HasActiveFilters => AlunoId.HasValue || TipoId.HasValue || Status.HasValue ||
                                    PrazoAte.HasValue || PossuiEnvio.HasValue;
}

public sealed class ComunicadoAdminFilter : PageRequest
{
    public string? Busca { get; set; }
    public ComunicadoAlvoTipoEnum? Publico { get; set; }
    public int? TurmaId { get; set; }
    public bool? Importante { get; set; }
    public bool? Fixado { get; set; }
    public DateOnly? Inicio { get; set; }
    public DateOnly? Fim { get; set; }

    public bool HasActiveFilters => !string.IsNullOrWhiteSpace(Busca) || Publico.HasValue ||
                                    TurmaId.HasValue || Importante.HasValue || Fixado.HasValue ||
                                    Inicio.HasValue || Fim.HasValue;
}

public sealed class EventoAdminFilter : PageRequest
{
    public string? Busca { get; set; }
    public EventoAlunoTipoEnum? Tipo { get; set; }
    public ComunicadoAlvoTipoEnum? Publico { get; set; }
    public DateOnly? Inicio { get; set; }
    public DateOnly? Fim { get; set; }
    public bool? Importante { get; set; }
    public bool? Proximo { get; set; }

    public bool HasActiveFilters => !string.IsNullOrWhiteSpace(Busca) || Tipo.HasValue ||
                                    Publico.HasValue || Inicio.HasValue || Fim.HasValue ||
                                    Importante.HasValue || Proximo.HasValue;
}

public sealed class ConquistaAdminFilter : PageRequest
{
    public int? AlunoId { get; set; }
    public int? InsigniaId { get; set; }
    public string? Categoria { get; set; }
    public InsigniaOrigemEnum? Origem { get; set; }
    public DateOnly? Inicio { get; set; }
    public DateOnly? Fim { get; set; }

    public bool HasActiveFilters => AlunoId.HasValue || InsigniaId.HasValue ||
                                    !string.IsNullOrWhiteSpace(Categoria) || Origem.HasValue ||
                                    Inicio.HasValue || Fim.HasValue;
}
