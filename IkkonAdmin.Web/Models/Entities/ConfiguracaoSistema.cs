namespace IkkonAdmin.Web.Models.Entities;

public class ConfiguracaoSistema
{
    public int Id { get; set; }

    public string NomeEscola { get; set; } = "Escola de Taiko Ikkon";
    public string? EmailFinanceiro { get; set; }
    public string? TelefoneContato { get; set; }

    public decimal ValorMensalidadePadrao { get; set; } = 260m;
    public int DiaVencimentoPadrao { get; set; } = 10;
    public int DiasToleranciaAtraso { get; set; } = 0;
    public decimal PercentualMultaAtraso { get; set; } = 2m;
    public decimal PercentualJurosMes { get; set; } = 1m;
    public bool AplicarMultaJurosAutomaticamente { get; set; } = false;
    public bool GerarMensalidadesAutomaticamente { get; set; } = false;

    public bool EnviarLembreteCobranca { get; set; } = true;
    public int DiasAntecedenciaLembrete { get; set; } = 3;
    public string? MensagemBoasVindasPadrao { get; set; }
    public string? ChecklistAdmissaoPadrao { get; set; }

    public bool PermitirDesligamentoComPendencia { get; set; } = true;
    public bool AtualizarNivelAutomaticamenteNaGraduacao { get; set; } = true;

    public DateTime UltimaAtualizacaoUtc { get; set; } = DateTime.UtcNow;
}

