using System.Globalization;
using IkkonAdmin.Web.Helpers;

namespace IkkonAdmin.Web.Services;

public interface IViewTextService
{
    bool IsEnglish { get; }
    bool IsJapanese { get; }
    bool IsPortuguese { get; }
    string CurrentCulture { get; }
    string CurrentLanguageSegment { get; }
    string ToggleCulture { get; }
    string ToggleLabel { get; }
    string LocalizePath(string path);
    string PathForCulture(string path, string culture);
    string Term(object? value);
    string this[string ptBr, string enUs] { get; }
    string this[string ptBr, string enUs, string jaJp] { get; }
}

public sealed class ViewTextService : IViewTextService
{
    private static readonly IReadOnlyDictionary<string, string> EnglishTerms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Aberta"] = "Open",
        ["Agendada"] = "Scheduled",
        ["Aprovado"] = "Approved",
        ["Aprovada"] = "Approved",
        ["Ativo"] = "Active",
        ["Atrasado"] = "Overdue",
        ["Atrasada"] = "Overdue",
        ["Automatica"] = "Automatic",
        ["Automática"] = "Automatic",
        ["Cancelada"] = "Canceled",
        ["Cancelado"] = "Canceled",
        ["Conquista"] = "Achievement",
        ["Desligado"] = "Inactive",
        ["Dinheiro"] = "Cash",
        ["Domingo"] = "Sunday",
        ["Em admissão"] = "In admission",
        ["EmAdmissao"] = "In admission",
        ["Enviado"] = "Sent",
        ["Enviada"] = "Sent",
        ["Friday"] = "Friday",
        ["Monday"] = "Monday",
        ["Falta"] = "Absent",
        ["FaltaJustificada"] = "Excused absence",
        ["Falta justificada"] = "Excused absence",
        ["Inativo"] = "Inactive",
        ["Manual"] = "Manual",
        ["Pago"] = "Paid",
        ["Paga"] = "Paid",
        ["Pendente"] = "Pending",
        ["Pix"] = "Pix",
        ["Presente"] = "Present",
        ["Quarta"] = "Wednesday",
        ["Quinta"] = "Thursday",
        ["Realizada"] = "Completed",
        ["Recusado"] = "Rejected",
        ["Recusada"] = "Rejected",
        ["Sábado"] = "Saturday",
        ["Saturday"] = "Saturday",
        ["Segunda"] = "Monday",
        ["Sexta"] = "Friday",
        ["Solicitado"] = "Requested",
        ["Solicitada"] = "Requested",
        ["Sunday"] = "Sunday",
        ["Terça"] = "Tuesday",
        ["Thursday"] = "Thursday",
        ["Tuesday"] = "Tuesday",
        ["Wednesday"] = "Wednesday"
    };

    public bool IsEnglish => CultureInfo.CurrentUICulture.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase);

    public bool IsJapanese => CultureInfo.CurrentUICulture.Name.StartsWith("ja", StringComparison.OrdinalIgnoreCase);

    public bool IsPortuguese => !IsEnglish && !IsJapanese;

    public string CurrentCulture => IsJapanese ? "ja-JP" : IsEnglish ? "en-US" : "pt-BR";

    public string CurrentLanguageSegment => PublicSiteLocales.ForCulture(CurrentCulture).Segment;

    public string ToggleCulture => IsEnglish ? "pt-BR" : "en-US";

    public string ToggleLabel => IsEnglish ? "PT" : "EN";

    public string LocalizePath(string path) => PublicSiteLocales.LocalizePath(path, CurrentCulture);

    public string PathForCulture(string path, string culture) => PublicSiteLocales.LocalizePath(path, culture);

    public string Term(object? value)
    {
        var text = value?.ToString();
        if (string.IsNullOrWhiteSpace(text) || !IsEnglish)
        {
            return text ?? string.Empty;
        }

        return EnglishTerms.TryGetValue(text, out var translated) ? translated : text;
    }

    public string this[string ptBr, string enUs] => IsEnglish ? enUs : ptBr;

    public string this[string ptBr, string enUs, string jaJp] => IsJapanese ? jaJp : IsEnglish ? enUs : ptBr;
}
