namespace IkkonAdmin.Web.Helpers;

public static class AlunoPortalPresentation
{
    public static string StatusTone(object? value)
    {
        var status = value?.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(status))
        {
            return "neutral";
        }

        return status.ToUpperInvariant() switch
        {
            "ATIVO" or
            "ATIVA" or
            "PAGO" or
            "PAGA" or
            "APROVADO" or
            "APROVADA" or
            "PRESENTE" or
            "REALIZADA" => "success",

            "ATRASADO" or
            "ATRASADA" or
            "RECUSADO" or
            "RECUSADA" or
            "FALTA" or
            "FALTANAOJUSTIFICADA" or
            "INATIVO" or
            "INATIVA" => "danger",

            "PENDENTE" or
            "FALTAJUSTIFICADA" => "warning",

            "AGENDADA" or
            "ABERTA" or
            "ENVIADO" or
            "ENVIADA" or
            "SOLICITADO" or
            "SOLICITADA" or
            "EMADMISSAO" => "info",

            _ => "neutral"
        };
    }
}
