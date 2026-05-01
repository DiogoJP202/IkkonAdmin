# Usuários de Acesso (Ambiente de Desenvolvimento)

Este documento lista os usuários padrão do sistema **IkkonAdmin** para cada tipo de acesso.

## Credenciais

| Tipo de Usuário | Tipo de Login na Tela | Login/Usuário | Senha | Destino Após Login |
|---|---|---|---|---|
| Admin | Administrador | `funcionario.admin` | `Ikkon@123` | `/admin/painel` |
| Funcionário | Funcionário | `funcionario.operacional` | `Func@123` | `/admin` |
| Aluno | Área do Aluno | `aluno.demo` | `Aluno@123` | `/area-do-aluno` |

## Observações

- Essas credenciais são semeadas automaticamente em `Data/SeedData.cs`.
- Os acessos acima são destinados apenas para **desenvolvimento/demonstração**.
- Em produção, altere todas as senhas padrão imediatamente.
- Após mudanças de roles/permissões, faça logout/login para atualizar as claims da sessão.
- O login administrativo fica em `/auth/login`; o login do aluno fica separado em `/aluno/login`.
