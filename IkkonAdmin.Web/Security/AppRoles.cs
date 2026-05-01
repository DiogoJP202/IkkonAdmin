using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Security;

public static class AppRoles
{
    public const string Admin = "ROLE_ADMIN";
    public const string Funcionario = "ROLE_FUNCIONARIO";
    public const string Aluno = "ROLE_ALUNO";

    public static string FromTipoAcesso(TipoAcessoEnum tipoAcesso) => tipoAcesso switch
    {
        TipoAcessoEnum.Admin => Admin,
        TipoAcessoEnum.Funcionario => Funcionario,
        TipoAcessoEnum.Aluno => Aluno,
        _ => throw new ArgumentOutOfRangeException(nameof(tipoAcesso), tipoAcesso, "Tipo de acesso invalido.")
    };
}
