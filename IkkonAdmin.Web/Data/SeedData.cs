using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Data;

public static class SeedData
{
    public static void Initialize(ApplicationDbContext context)
    {
        InitializeStructural(context);
        InitializeDemoData(context);
    }

    public static void InitializeStructural(ApplicationDbContext context)
    {
        if (!context.ConfiguracoesSistema.Any())
        {
            context.ConfiguracoesSistema.Add(new ConfiguracaoSistema
            {
                NomeEscola = "Escola de Taiko Ikkon",
                ValorMensalidadePadrao = 260m,
                DiaVencimentoPadrao = 10,
                DiasToleranciaAtraso = 0,
                PercentualMultaAtraso = 2m,
                PercentualJurosMes = 1m,
                AplicarMultaJurosAutomaticamente = false,
                GerarMensalidadesAutomaticamente = false,
                EnviarLembreteCobranca = true,
                DiasAntecedenciaLembrete = 3,
                PermitirDesligamentoComPendencia = true,
                AtualizarNivelAutomaticamenteNaGraduacao = true,
                UltimaAtualizacaoUtc = DateTime.UtcNow
            });

            context.SaveChanges();
        }

        SeedAccessControl(context);
    }

    private static void InitializeDemoData(ApplicationDbContext context)
    {
        if (context.Alunos.Any())
        {
            SeedInventario(context);
            SeedUsuariosSistema(context);
            return;
        }

        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var competenciaAtual = new DateOnly(hoje.Year, hoje.Month, 1);
        var competenciaAnterior = competenciaAtual.AddMonths(-1);

        var turmas = new List<Turma>
        {
            new()
            {
                Nome = "Taiko Iniciante A",
                Modalidade = "Taiko",
                Horario = "Seg/Qua - 19:30",
                Ativa = true
            },
            new()
            {
                Nome = "Taiko Intermediaria",
                Modalidade = "Taiko",
                Horario = "Ter/Qui - 20:00",
                Ativa = true
            },
            new()
            {
                Nome = "Shinobue Base",
                Modalidade = "Shinobue",
                Horario = "Sabado - 10:00",
                Ativa = true
            }
        };

        context.Turmas.AddRange(turmas);
        context.SaveChanges();

        var alunos = new List<Aluno>
        {
            new()
            {
                NomeCompleto = "Rafael Sato",
                CPF = "111.222.333-44",
                RG = "12.345.678-9",
                DataNascimento = new DateOnly(1999, 5, 2),
                Celular = "(11) 99999-1001",
                Email = "rafael.sato@email.com",
                Endereco = "Rua das Flores, 100",
                ContatoEmergencia = "Mae - (11) 98888-1001",
                DataEntrada = hoje.AddMonths(-10),
                Status = StatusAlunoEnum.Ativo,
                TurmaId = turmas[0].Id
            },
            new()
            {
                NomeCompleto = "Marina Tanaka",
                CPF = "222.333.444-55",
                RG = "23.456.789-0",
                DataNascimento = new DateOnly(2001, 10, 12),
                Celular = "(11) 99999-1002",
                Email = "marina.tanaka@email.com",
                Endereco = "Av. Central, 210",
                ContatoEmergencia = "Pai - (11) 97777-1002",
                DataEntrada = hoje.AddMonths(-6),
                Status = StatusAlunoEnum.Ativo,
                TurmaId = turmas[1].Id
            },
            new()
            {
                NomeCompleto = "Kenji Mori",
                CPF = "333.444.555-66",
                RG = "34.567.890-1",
                DataNascimento = new DateOnly(1995, 3, 21),
                Celular = "(11) 99999-1003",
                Email = "kenji.mori@email.com",
                Endereco = "Rua Harmonia, 77",
                ContatoEmergencia = "Esposa - (11) 96666-1003",
                DataEntrada = hoje.AddMonths(-14),
                Status = StatusAlunoEnum.Ativo,
                TurmaId = turmas[2].Id
            },
            new()
            {
                NomeCompleto = "Paula Lima",
                CPF = "444.555.666-77",
                DataNascimento = new DateOnly(2003, 8, 6),
                Celular = "(11) 99999-1004",
                DataEntrada = hoje.AddMonths(-2),
                Status = StatusAlunoEnum.Inativo,
                TurmaId = turmas[0].Id,
                Observacoes = "Pausou por motivos pessoais."
            },
            new()
            {
                NomeCompleto = "Bruno Dias",
                CPF = "555.666.777-88",
                DataNascimento = new DateOnly(1998, 12, 30),
                Celular = "(11) 99999-1005",
                DataEntrada = hoje.AddYears(-1),
                Status = StatusAlunoEnum.Desligado,
                TurmaId = turmas[1].Id
            }
        };

        context.Alunos.AddRange(alunos);
        context.SaveChanges();

        context.AlunosTurmas.AddRange(
            alunos
                .Where(x => x.TurmaId.HasValue)
                .Select(x => new AlunoTurma
                {
                    AlunoId = x.Id,
                    TurmaId = x.TurmaId!.Value,
                    DataVinculo = DateTime.UtcNow
                }));

        // Exemplo de aluno em mais de uma turma simultaneamente.
        context.AlunosTurmas.Add(new AlunoTurma
        {
            AlunoId = alunos[0].Id,
            TurmaId = turmas[2].Id,
            DataVinculo = DateTime.UtcNow
        });

        context.SaveChanges();

        context.Descontos.Add(new Desconto
        {
            AlunoId = alunos[2].Id,
            Nome = "Desconto Shinobue",
            Tipo = "Mensalidade Shinobue",
            Percentual = 15m,
            VigenciaInicio = competenciaAnterior,
            Ativo = true,
            Observacoes = "Desconto recorrente para modalidade shinobue."
        });

        context.AcordosFinanceiros.Add(new AcordoFinanceiro
        {
            AlunoId = alunos[0].Id,
            Descricao = "Acordo familiar",
            ValorMensalAcordado = 220m,
            InicioVigencia = competenciaAnterior,
            Ativo = true
        });

        var mensalidades = new List<Mensalidade>
        {
            new()
            {
                AlunoId = alunos[0].Id,
                Competencia = competenciaAnterior,
                DataVencimento = competenciaAnterior.AddDays(9),
                DataPagamento = competenciaAnterior.AddDays(8),
                ValorBase = 260m,
                ValorFinal = 220m,
                Status = StatusMensalidadeEnum.Pago
            },
            new()
            {
                AlunoId = alunos[0].Id,
                Competencia = competenciaAtual,
                DataVencimento = competenciaAtual.AddDays(9),
                ValorBase = 260m,
                ValorFinal = 220m,
                Status = StatusMensalidadeEnum.Pendente
            },
            new()
            {
                AlunoId = alunos[1].Id,
                Competencia = competenciaAtual,
                DataVencimento = competenciaAtual.AddDays(9),
                DataPagamento = competenciaAtual.AddDays(7),
                ValorBase = 260m,
                ValorFinal = 260m,
                Status = StatusMensalidadeEnum.Pago
            },
            new()
            {
                AlunoId = alunos[2].Id,
                Competencia = competenciaAnterior,
                DataVencimento = competenciaAnterior.AddDays(9),
                ValorBase = 250m,
                ValorFinal = 212.50m,
                Status = StatusMensalidadeEnum.Atrasado
            },
            new()
            {
                AlunoId = alunos[2].Id,
                Competencia = competenciaAtual,
                DataVencimento = competenciaAtual.AddDays(9),
                ValorBase = 250m,
                ValorFinal = 212.50m,
                Status = StatusMensalidadeEnum.Pendente
            }
        };

        context.Mensalidades.AddRange(mensalidades);
        context.SaveChanges();

        context.Pagamentos.AddRange(
            new Pagamento
            {
                AlunoId = alunos[0].Id,
                MensalidadeId = mensalidades[0].Id,
                DataPagamento = DateTime.Today.AddDays(-20),
                ValorPago = 220m,
                FormaPagamento = FormaPagamentoEnum.Pix
            },
            new Pagamento
            {
                AlunoId = alunos[1].Id,
                MensalidadeId = mensalidades[2].Id,
                DataPagamento = DateTime.Today.AddDays(-3),
                ValorPago = 260m,
                FormaPagamento = FormaPagamentoEnum.Transferencia
            });

        context.Admissoes.Add(new Admissao
        {
            AlunoId = alunos[3].Id,
            NomeInteressado = alunos[3].NomeCompleto,
            DataAulaExperimental = hoje.AddDays(-12),
            DataMatricula = hoje.AddDays(-9),
            Status = StatusAdmissaoEnum.EmAndamento,
            ContratoAssinado = true,
            PagamentoInicialConfirmado = false,
            IntegracaoConcluida = false,
            ChecklistObservacoes = "Falta confirmar inclusao no grupo da turma."
        });

        context.Desligamentos.Add(new Desligamento
        {
            AlunoId = alunos[4].Id,
            DataSolicitacao = hoje.AddDays(-40),
            Motivo = "Mudanca de cidade.",
            PendenciaFinanceira = 0m,
            MultaRescisoria = 0m,
            RequerimentoRecebido = true,
            DataConfirmacao = hoje.AddDays(-35),
            AcessosRemovidos = true,
            Observacoes = "Processo concluido sem pendencias."
        });

        var exame = new ExameGraduacao
        {
            DataExame = hoje.AddDays(-25),
            Local = "Dojo Principal",
            NivelPretendido = NivelGraduacaoEnum.Intermediario
        };

        context.ExamesGraduacao.Add(exame);
        context.SaveChanges();

        context.Graduacoes.Add(new Graduacao
        {
            AlunoId = alunos[1].Id,
            ExameGraduacaoId = exame.Id,
            DataResultado = hoje.AddDays(-23),
            ResultadoAprovado = true,
            NivelAnterior = NivelGraduacaoEnum.Basico,
            NivelNovo = NivelGraduacaoEnum.Intermediario,
            CertificadoEmitido = true,
            OmamoriAtualizado = true
        });

        context.HistoricosAlunos.AddRange(
            new HistoricoAluno
            {
                AlunoId = alunos[0].Id,
                DataEvento = DateTime.Today.AddDays(-2),
                TipoEvento = "Financeiro",
                Descricao = "Pagamento da mensalidade de marco registrado."
            },
            new HistoricoAluno
            {
                AlunoId = alunos[1].Id,
                DataEvento = DateTime.Today.AddDays(-10),
                TipoEvento = "Graduacao",
                Descricao = "Aprovada para o nivel Intermediario."
            },
            new HistoricoAluno
            {
                AlunoId = alunos[3].Id,
                DataEvento = DateTime.Today.AddDays(-8),
                TipoEvento = "Admissao",
                Descricao = "Contrato assinado e admissao em andamento."
            },
            new HistoricoAluno
            {
                AlunoId = alunos[4].Id,
                DataEvento = DateTime.Today.AddDays(-35),
                TipoEvento = "Desligamento",
                Descricao = "Desligamento confirmado e acessos removidos."
            });

        context.SaveChanges();

        SeedInventario(context);
        SeedUsuariosSistema(context);
    }

    private static void SeedInventario(ApplicationDbContext context)
    {
        if (context.InventarioItens.Any())
        {
            return;
        }

        context.InventarioItens.AddRange(
            new InventarioItem
            {
                Nome = "Nagado principal do dojo",
                CodigoInterno = "TAIKO-NAGADO-001",
                Categoria = InventarioCategoriaEnum.Taiko,
                Tipo = "Nagado",
                Descricao = "Taiko principal usado em aulas e apresentações.",
                Quantidade = 1,
                Status = InventarioStatusEnum.Disponivel,
                EstadoConservacao = InventarioEstadoConservacaoEnum.Bom,
                Localizacao = "Dojo - sala principal",
                DisponivelParaAula = true,
                DisponivelParaEvento = true,
                DataAquisicao = DateOnly.FromDateTime(DateTime.Today.AddYears(-4)),
                ValorEstimado = 4500m,
                Observacoes = "Revisar cordas antes de eventos externos.",
                CriadoEmUtc = DateTime.UtcNow,
                Ativo = true
            },
            new InventarioItem
            {
                Nome = "Par de bachis reserva",
                CodigoInterno = "BACHI-RES-001",
                Categoria = InventarioCategoriaEnum.Bachi,
                Tipo = "Bachi",
                Quantidade = 6,
                Status = InventarioStatusEnum.Disponivel,
                EstadoConservacao = InventarioEstadoConservacaoEnum.Bom,
                Localizacao = "Armário de materiais",
                DisponivelParaAula = true,
                DisponivelParaEvento = true,
                CriadoEmUtc = DateTime.UtcNow,
                Ativo = true
            },
            new InventarioItem
            {
                Nome = "Shime em manutenção",
                CodigoInterno = "TAIKO-SHIME-002",
                Categoria = InventarioCategoriaEnum.Taiko,
                Tipo = "Shime",
                Quantidade = 1,
                Status = InventarioStatusEnum.Manutencao,
                EstadoConservacao = InventarioEstadoConservacaoEnum.PrecisaManutencao,
                Localizacao = "Manutenção",
                DisponivelParaAula = false,
                DisponivelParaEvento = false,
                Observacoes = "Pele precisa de revisão antes de voltar para uso.",
                CriadoEmUtc = DateTime.UtcNow,
                Ativo = true
            });

        context.SaveChanges();
    }

    private static void SeedUsuariosSistema(ApplicationDbContext context)
    {
        var passwordHasher = new PasswordHasher<UsuarioSistema>();
        var possuiAlteracoes = false;

        var adminExistente = context.UsuariosSistema
            .IgnoreQueryFilters()
            .FirstOrDefault(x => x.LoginNormalizado == Normalizar("funcionario.admin"));

        if (adminExistente is null)
        {
            var usuarioFuncionario = new UsuarioSistema
            {
                Login = "funcionario.admin",
                LoginNormalizado = Normalizar("funcionario.admin"),
                Email = "admin@ikkonadmin.local",
                EmailNormalizado = Normalizar("admin@ikkonadmin.local"),
                Telefone = "(11) 90000-0001",
                NomeExibicao = "Administrador Ikkon",
                TipoAcesso = TipoAcessoEnum.Admin,
                Ativo = true,
                DataCriacaoUtc = DateTime.UtcNow
            };

            usuarioFuncionario.SenhaHash = passwordHasher.HashPassword(usuarioFuncionario, "Ikkon@123");
            context.UsuariosSistema.Add(usuarioFuncionario);
            possuiAlteracoes = true;
        }
        else
        {
            if (adminExistente.Excluido)
            {
                adminExistente.Excluido = false;
                adminExistente.DataExclusaoUtc = null;
                adminExistente.ExcluidoPorUsuarioId = null;
                possuiAlteracoes = true;
            }

            if (adminExistente.TipoAcesso != TipoAcessoEnum.Admin)
            {
                adminExistente.TipoAcesso = TipoAcessoEnum.Admin;
                possuiAlteracoes = true;
            }

            if (!adminExistente.Ativo)
            {
                adminExistente.Ativo = true;
                possuiAlteracoes = true;
            }
        }

        if (!context.UsuariosSistema.Any(x => x.LoginNormalizado == Normalizar("funcionario.operacional")))
        {
            var usuarioFuncionario = new UsuarioSistema
            {
                Login = "funcionario.operacional",
                LoginNormalizado = Normalizar("funcionario.operacional"),
                Email = "funcionario@ikkonadmin.local",
                EmailNormalizado = Normalizar("funcionario@ikkonadmin.local"),
                Telefone = "(11) 90000-0002",
                NomeExibicao = "Funcionário Operacional",
                TipoAcesso = TipoAcessoEnum.Funcionario,
                Ativo = true,
                DataCriacaoUtc = DateTime.UtcNow
            };

            usuarioFuncionario.SenhaHash = passwordHasher.HashPassword(usuarioFuncionario, "Func@123");
            context.UsuariosSistema.Add(usuarioFuncionario);
            possuiAlteracoes = true;
        }

        var alunoBase = context.Alunos
            .OrderBy(x => x.Id)
            .FirstOrDefault(x => x.Status == StatusAlunoEnum.Ativo);

        if (alunoBase is not null && !context.UsuariosSistema.Any(x => x.AlunoId == alunoBase.Id))
        {
            var loginAluno = "aluno.demo";
            var emailAluno = !string.IsNullOrWhiteSpace(alunoBase.Email)
                ? alunoBase.Email
                : "aluno.demo@ikkonadmin.local";

            var usuarioAluno = new UsuarioSistema
            {
                Login = loginAluno,
                LoginNormalizado = Normalizar(loginAluno),
                Email = emailAluno,
                EmailNormalizado = Normalizar(emailAluno),
                Telefone = alunoBase.Celular,
                NomeExibicao = alunoBase.NomeCompleto,
                TipoAcesso = TipoAcessoEnum.Aluno,
                Ativo = true,
                AlunoId = alunoBase.Id,
                DataCriacaoUtc = DateTime.UtcNow
            };

            usuarioAluno.SenhaHash = passwordHasher.HashPassword(usuarioAluno, "Aluno@123");
            context.UsuariosSistema.Add(usuarioAluno);
            possuiAlteracoes = true;
        }

        if (possuiAlteracoes)
        {
            context.SaveChanges();
        }

    }

    private static void SeedAccessControl(ApplicationDbContext context)
    {
        var possuiAlteracoes = false;

        var rolesByCode = context.RolesSistema
            .IgnoreQueryFilters()
            .ToDictionary(x => x.Codigo, StringComparer.OrdinalIgnoreCase);

        var roleDefinitions = new[]
        {
            new RoleSistema
            {
                Codigo = AppRoles.Admin,
                Nome = "Administrador",
                Descricao = "Acesso completo ao painel administrativo.",
                TipoAcesso = TipoAcessoEnum.Admin,
                Ativo = true,
                IsSistema = true
            },
            new RoleSistema
            {
                Codigo = AppRoles.Funcionario,
                Nome = "Funcionário",
                Descricao = "Acesso operacional interno.",
                TipoAcesso = TipoAcessoEnum.Funcionario,
                Ativo = true,
                IsSistema = true
            },
            new RoleSistema
            {
                Codigo = AppRoles.Aluno,
                Nome = "Aluno",
                Descricao = "Acesso à área do aluno.",
                TipoAcesso = TipoAcessoEnum.Aluno,
                Ativo = true,
                IsSistema = true
            }
        };

        foreach (var definition in roleDefinitions)
        {
            if (rolesByCode.TryGetValue(definition.Codigo, out var existente))
            {
                existente.Nome = definition.Nome;
                existente.Descricao = definition.Descricao;
                existente.TipoAcesso = definition.TipoAcesso;
                existente.Ativo = true;
                existente.IsSistema = true;
            }
            else
            {
                context.RolesSistema.Add(definition);
                rolesByCode[definition.Codigo] = definition;
                possuiAlteracoes = true;
            }
        }

        if (possuiAlteracoes)
        {
            context.SaveChanges();
            possuiAlteracoes = false;
        }

        var permissionsByCode = context.PermissoesSistema
            .IgnoreQueryFilters()
            .ToDictionary(x => x.Codigo, StringComparer.OrdinalIgnoreCase);

        foreach (var definition in AppPermissions.Definicoes)
        {
            if (permissionsByCode.TryGetValue(definition.Codigo, out var existentePermissao))
            {
                existentePermissao.Nome = definition.Nome;
                existentePermissao.Descricao = definition.Descricao;
                existentePermissao.Ativo = true;
                existentePermissao.IsSistema = true;
            }
            else
            {
                var permissao = new PermissaoSistema
                {
                    Codigo = definition.Codigo,
                    Nome = definition.Nome,
                    Descricao = definition.Descricao,
                    Ativo = true,
                    IsSistema = true,
                    DataCriacaoUtc = DateTime.UtcNow
                };

                context.PermissoesSistema.Add(permissao);
                permissionsByCode[definition.Codigo] = permissao;
                possuiAlteracoes = true;
            }
        }

        if (possuiAlteracoes)
        {
            context.SaveChanges();
            possuiAlteracoes = false;
        }

        var adminRoleId = rolesByCode[AppRoles.Admin].Id;
        var funcionarioRoleId = rolesByCode[AppRoles.Funcionario].Id;
        var alunoRoleId = rolesByCode[AppRoles.Aluno].Id;

        var rolePermissoes = context.RolesPermissoes.ToList();

        void SincronizarPermissoesRole(int roleId, IReadOnlyCollection<string> codigosPermissoes)
        {
            var permissaoIdsDesejadas = codigosPermissoes
                .Where(codigo => permissionsByCode.ContainsKey(codigo))
                .Select(codigo => permissionsByCode[codigo].Id)
                .ToHashSet();

            var atuais = rolePermissoes.Where(x => x.RoleId == roleId).ToList();

            foreach (var atual in atuais.Where(x => !permissaoIdsDesejadas.Contains(x.PermissaoId)))
            {
                context.RolesPermissoes.Remove(atual);
                rolePermissoes.Remove(atual);
                possuiAlteracoes = true;
            }

            foreach (var permissaoId in permissaoIdsDesejadas)
            {
                if (atuais.Any(x => x.PermissaoId == permissaoId))
                {
                    continue;
                }

                var novo = new RolePermissao
                {
                    RoleId = roleId,
                    PermissaoId = permissaoId,
                    DataVinculoUtc = DateTime.UtcNow
                };

                context.RolesPermissoes.Add(novo);
                rolePermissoes.Add(novo);
                possuiAlteracoes = true;
            }
        }

        var permissoesAdmin = AppPermissions.Definicoes
            .Select(x => x.Codigo)
            .ToArray();

        var permissoesFuncionario = new[]
        {
            AppPermissions.DashboardView,
            AppPermissions.AlunosView,
            AppPermissions.AlunosCreate,
            AppPermissions.AlunosEdit,
            AppPermissions.TurmasView,
            AppPermissions.TurmasCreate,
            AppPermissions.TurmasEdit,
            AppPermissions.FinanceiroView,
            AppPermissions.FinanceiroCreate,
            AppPermissions.FinanceiroEdit,
            AppPermissions.AdmissoesView,
            AppPermissions.AdmissoesCreate,
            AppPermissions.AdmissoesEdit,
            AppPermissions.DesligamentosView,
            AppPermissions.DesligamentosCreate,
            AppPermissions.DesligamentosEdit,
            AppPermissions.GraduacoesView,
            AppPermissions.GraduacoesCreate,
            AppPermissions.GraduacoesEdit,
            AppPermissions.ConfiguracoesView,
            AppPermissions.ConfiguracoesEdit,
            AppPermissions.BlogView,
            AppPermissions.BlogCreate,
            AppPermissions.BlogEdit,
            AppPermissions.BlogPublish,
            AppPermissions.BlogArchive,
            AppPermissions.BlogFeature,
            AppPermissions.BlogCategoryManage,
            AppPermissions.BlogTagManage,
            AppPermissions.AreaAlunoView,
            AppPermissions.AreaAlunoManage,
            AppPermissions.FrequenciaView,
            AppPermissions.FrequenciaCreate,
            AppPermissions.FrequenciaEdit,
            AppPermissions.DocumentosView,
            AppPermissions.DocumentosCreate,
            AppPermissions.DocumentosEdit,
            AppPermissions.DocumentosApprove,
            AppPermissions.ComunicadosView,
            AppPermissions.ComunicadosCreate,
            AppPermissions.ComunicadosEdit,
            AppPermissions.EventosAlunoView,
            AppPermissions.EventosAlunoCreate,
            AppPermissions.EventosAlunoEdit,
            AppPermissions.ConquistasView,
            AppPermissions.ConquistasCreate,
            AppPermissions.ConquistasEdit,
            AppPermissions.AulasView,
            AppPermissions.AulasCreate,
            AppPermissions.AulasEdit
        };

        var permissoesAluno = new[]
        {
            AppPermissions.ConfiguracoesView,
            AppPermissions.ConfiguracoesEdit
        };

        SincronizarPermissoesRole(adminRoleId, permissoesAdmin);
        SincronizarPermissoesRole(funcionarioRoleId, permissoesFuncionario);
        SincronizarPermissoesRole(alunoRoleId, permissoesAluno);

        if (possuiAlteracoes)
        {
            context.SaveChanges();
            possuiAlteracoes = false;
        }

        var usuarios = context.UsuariosSistema
            .IgnoreQueryFilters()
            .Where(x => !x.Excluido)
            .ToList();

        var userRoles = context.UsuariosRoles.ToList();
        var rolesById = context.RolesSistema
            .IgnoreQueryFilters()
            .ToDictionary(x => x.Id);

        foreach (var usuario in usuarios)
        {
            var roleCodigo = AppRoles.FromTipoAcesso(usuario.TipoAcesso);
            var roleId = rolesByCode[roleCodigo].Id;

            var existentesUsuario = userRoles
                .Where(x => x.UsuarioId == usuario.Id)
                .ToList();

            var possuiRoleAtiva = existentesUsuario.Any(x =>
                rolesById.TryGetValue(x.RoleId, out var roleAtual) && roleAtual.Ativo);

            if (possuiRoleAtiva)
            {
                continue;
            }

            if (existentesUsuario.Any(x => x.RoleId == roleId))
            {
                continue;
            }

            var novoVinculo = new UsuarioRole
            {
                UsuarioId = usuario.Id,
                RoleId = roleId,
                DataVinculoUtc = DateTime.UtcNow
            };

            context.UsuariosRoles.Add(novoVinculo);
            userRoles.Add(novoVinculo);
            possuiAlteracoes = true;
        }

        if (possuiAlteracoes)
        {
            context.SaveChanges();
        }
    }

    private static string Normalizar(string valor)
    {
        return valor.Trim().ToUpperInvariant();
    }
}
