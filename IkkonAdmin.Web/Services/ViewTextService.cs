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
        ["Ativa"] = "Active",
        ["Atrasado"] = "Overdue",
        ["Atrasada"] = "Overdue",
        ["Automatica"] = "Automatic",
        ["Automática"] = "Automatic",
        ["AulaEspecial"] = "Special class",
        ["Aula especial"] = "Special class",
        ["Apresentacao"] = "Performance",
        ["Atividade"] = "Activity",
        ["Boleto"] = "Bank slip",
        ["Cancelada"] = "Canceled",
        ["Cancelado"] = "Canceled",
        ["Cartao"] = "Card",
        ["Claro"] = "Light",
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
        ["Feriado"] = "Holiday",
        ["Funcionario"] = "Staff",
        ["Funcionário"] = "Staff",
        ["Inativo"] = "Inactive",
        ["Inativa"] = "Inactive",
        ["Manual"] = "Manual",
        ["Outro"] = "Other",
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
        ["Reposicao"] = "Make-up class",
        ["Reuniao"] = "Meeting",
        ["Sábado"] = "Saturday",
        ["Saturday"] = "Saturday",
        ["Segunda"] = "Monday",
        ["Sexta"] = "Friday",
        ["Solicitado"] = "Requested",
        ["Solicitada"] = "Requested",
        ["Sunday"] = "Sunday",
        ["Terça"] = "Tuesday",
        ["Thursday"] = "Thursday",
        ["Transferencia"] = "Bank transfer",
        ["Tuesday"] = "Tuesday",
        ["Wednesday"] = "Wednesday",
        ["Aluno"] = "Student",
        ["Administrador"] = "Administrator",
        ["Admin"] = "Administrator",
        ["Conta"] = "Account",
        ["Escuro"] = "Dark",
        ["Exame"] = "Exam",
        ["Acesso total ao painel administrativo"] = "Full access to the admin panel",
        ["Gestão de usuários e permissões"] = "User and permission management",
        ["Controle de configurações e auditoria"] = "Settings and audit control",
        ["Acesso ao painel administrativo interno"] = "Access to the internal admin panel",
        ["Gestão de alunos, turmas e financeiro"] = "Student, group, and finance management",
        ["Visualização de indicadores operacionais"] = "Operational indicator overview",
        ["Acesso à área exclusiva do aluno"] = "Access to the private student area",
        ["Consulta de dados e histórico pessoal"] = "Access to personal data and history",
        ["Recebimento de notificações e comunicados"] = "Receive notifications and announcements"
    };

    private static readonly IReadOnlyDictionary<string, string> JapaneseTerms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Aberta"] = "受付中",
        ["Agendada"] = "予定",
        ["Aprovado"] = "承認済み",
        ["Aprovada"] = "承認済み",
        ["Ativo"] = "有効",
        ["Ativa"] = "有効",
        ["Atrasado"] = "期限超過",
        ["Atrasada"] = "期限超過",
        ["Automatica"] = "自動",
        ["Automática"] = "自動",
        ["AulaEspecial"] = "特別レッスン",
        ["Aula especial"] = "特別レッスン",
        ["Apresentacao"] = "公演",
        ["Atividade"] = "活動",
        ["Boleto"] = "銀行振込票",
        ["Cancelada"] = "キャンセル",
        ["Cancelado"] = "キャンセル",
        ["Cartao"] = "カード",
        ["Claro"] = "ライト",
        ["Conquista"] = "実績",
        ["Desligado"] = "退会",
        ["Dinheiro"] = "現金",
        ["Domingo"] = "日曜日",
        ["Em admissão"] = "入会手続き中",
        ["EmAdmissao"] = "入会手続き中",
        ["Enviado"] = "提出済み",
        ["Enviada"] = "提出済み",
        ["Escuro"] = "ダーク",
        ["Exame"] = "審査",
        ["Falta"] = "欠席",
        ["FaltaJustificada"] = "届出済み欠席",
        ["Falta justificada"] = "届出済み欠席",
        ["Feriado"] = "祝日",
        ["Friday"] = "金曜日",
        ["Funcionário"] = "スタッフ",
        ["Funcionario"] = "スタッフ",
        ["Inativo"] = "無効",
        ["Inativa"] = "無効",
        ["Manual"] = "手動",
        ["Monday"] = "月曜日",
        ["Outro"] = "その他",
        ["Pago"] = "支払済み",
        ["Paga"] = "支払済み",
        ["Pendente"] = "保留",
        ["Pix"] = "Pix",
        ["Presente"] = "出席",
        ["Quarta"] = "水曜日",
        ["Quinta"] = "木曜日",
        ["Realizada"] = "実施済み",
        ["Recusado"] = "差し戻し",
        ["Recusada"] = "差し戻し",
        ["Reposicao"] = "振替",
        ["Reuniao"] = "ミーティング",
        ["Sábado"] = "土曜日",
        ["Saturday"] = "土曜日",
        ["Segunda"] = "月曜日",
        ["Sexta"] = "金曜日",
        ["Solicitado"] = "提出依頼",
        ["Solicitada"] = "提出依頼",
        ["Sunday"] = "日曜日",
        ["Terça"] = "火曜日",
        ["Thursday"] = "木曜日",
        ["Transferencia"] = "銀行振込",
        ["Tuesday"] = "火曜日",
        ["Wednesday"] = "水曜日",
        ["Aluno"] = "生徒",
        ["Administrador"] = "管理者",
        ["Admin"] = "管理者",
        ["Conta"] = "アカウント",
        ["Acesso total ao painel administrativo"] = "管理画面へのすべてのアクセス",
        ["Gestão de usuários e permissões"] = "ユーザーと権限の管理",
        ["Controle de configurações e auditoria"] = "設定と監査の管理",
        ["Acesso ao painel administrativo interno"] = "内部管理画面へのアクセス",
        ["Gestão de alunos, turmas e financeiro"] = "生徒、クラス、会計の管理",
        ["Visualização de indicadores operacionais"] = "運営指標の確認",
        ["Acesso à área exclusiva do aluno"] = "生徒専用エリアへのアクセス",
        ["Consulta de dados e histórico pessoal"] = "個人情報と履歴の確認",
        ["Recebimento de notificações e comunicados"] = "通知とお知らせの受信"
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
        if (string.IsNullOrWhiteSpace(text))
        {
            return text ?? string.Empty;
        }

        if (IsJapanese)
        {
            return JapaneseTerms.TryGetValue(text, out var translated) ? translated : text;
        }

        if (IsEnglish)
        {
            return EnglishTerms.TryGetValue(text, out var translated) ? translated : text;
        }

        return text;
    }

    public string this[string ptBr, string enUs] => IsEnglish ? enUs : ptBr;

    public string this[string ptBr, string enUs, string jaJp] => IsJapanese ? jaJp : IsEnglish ? enUs : ptBr;
}
