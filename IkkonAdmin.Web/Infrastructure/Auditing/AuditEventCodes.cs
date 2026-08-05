namespace IkkonAdmin.Web.Infrastructure.Auditing;

public static class AuditEventCodes
{
    public const string LoginSuccess = "LOGIN_SUCESSO";
    public const string SensitiveAccessDenied = "ACESSO_SENSIVEL_NEGADO";

    public const string DocumentUploaded = "DOCUMENTO_ENVIADO";
    public const string DocumentDownloaded = "DOCUMENTO_BAIXADO";
    public const string DocumentApproved = "DOCUMENTO_APROVADO";
    public const string DocumentRejected = "DOCUMENTO_RECUSADO";

    public const string AttendanceRecorded = "FREQUENCIA_REGISTRADA";
    public const string AttendanceCorrected = "FREQUENCIA_CORRIGIDA";

    public const string PaymentRecorded = "PAGAMENTO_REGISTRADO";
    public const string MonthlyFeeValueChanged = "MENSALIDADE_VALOR_ALTERADO";
    public const string MonthlyFeeStatusChanged = "MENSALIDADE_STATUS_ALTERADO";

    public const string UserCreated = "CRIAR_USUARIO";
    public const string UserEdited = "EDITAR_USUARIO";
    public const string UserActivated = "ATIVAR_USUARIO";
    public const string UserDeactivated = "DESATIVAR_USUARIO";
    public const string UserDeleted = "EXCLUIR_USUARIO";
    public const string UserAccessChanged = "EDITAR_ACESSOS";
    public const string RoleCreated = "CRIAR_ROLE";
    public const string RoleEdited = "EDITAR_ROLE";
    public const string RoleActivated = "ATIVAR_ROLE";
    public const string RoleDeactivated = "DESATIVAR_ROLE";
    public const string RoleDeleted = "EXCLUIR_ROLE";
}
