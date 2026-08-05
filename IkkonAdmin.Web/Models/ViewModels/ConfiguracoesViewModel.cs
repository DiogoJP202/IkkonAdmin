using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Models.ViewModels;

public class ConfiguracoesIndexViewModel
{
    public ConfiguracoesFormViewModel Form { get; set; } = new();
    public ConfiguracoesResumoViewModel Resumo { get; set; } = new();
}

public class ConfiguracoesFormViewModel
{
    [Required, StringLength(160)]
    public string NomeEscola { get; set; } = string.Empty;

    [StringLength(150), EmailAddress]
    public string? EmailFinanceiro { get; set; }

    [StringLength(30)]
    public string? TelefoneContato { get; set; }

    [Range(0, 99999)]
    public decimal ValorMensalidadePadrao { get; set; } = 260m;

    [Range(1, 28)]
    public int DiaVencimentoPadrao { get; set; } = 10;

    [Range(0, 15)]
    public int DiasToleranciaAtraso { get; set; }

    [Range(0, 50)]
    public decimal PercentualMultaAtraso { get; set; } = 2m;

    [Range(0, 20)]
    public decimal PercentualJurosMes { get; set; } = 1m;

    public bool AplicarMultaJurosAutomaticamente { get; set; }
    public bool GerarMensalidadesAutomaticamente { get; set; }
    public bool GerarAulasAutomaticamente { get; set; } = true;
    public bool AvaliarConquistasAutomaticamente { get; set; } = true;

    [Range(1, 52)]
    public int HorizonteGeracaoAulasSemanas { get; set; } = 8;

    public TimeOnly HorarioAutomacoesAreaAluno { get; set; } = new(3, 30);

    public bool EnviarLembreteCobranca { get; set; } = true;

    [Range(0, 30)]
    public int DiasAntecedenciaLembrete { get; set; } = 3;

    [StringLength(1000)]
    public string? MensagemBoasVindasPadrao { get; set; }

    [StringLength(1000)]
    public string? ChecklistAdmissaoPadrao { get; set; }

    public bool PermitirDesligamentoComPendencia { get; set; } = true;
    public bool AtualizarNivelAutomaticamenteNaGraduacao { get; set; } = true;
    public DateTime? UltimaAtualizacaoUtc { get; set; }
}

public class ConfiguracoesResumoViewModel
{
    public int AlunosAtivos { get; set; }
    public int TurmasAtivas { get; set; }
    public int MensalidadesAtrasadas { get; set; }
    public int DesligamentosEmAberto { get; set; }
    public int ExamesProximos30Dias { get; set; }
}
