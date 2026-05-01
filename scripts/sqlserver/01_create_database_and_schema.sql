IF DB_ID(N'IkkonAdminDb') IS NULL
BEGIN
    CREATE DATABASE [IkkonAdminDb];
END;
GO

USE [IkkonAdminDb];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [ExamesGraduacao] (
    [Id] int NOT NULL IDENTITY,
    [DataExame] date NOT NULL,
    [Local] nvarchar(150) NULL,
    [NivelPretendido] int NOT NULL,
    [Observacoes] nvarchar(max) NULL,
    CONSTRAINT [PK_ExamesGraduacao] PRIMARY KEY ([Id])
);

CREATE TABLE [Turmas] (
    [Id] int NOT NULL IDENTITY,
    [Nome] nvarchar(100) NOT NULL,
    [Modalidade] nvarchar(80) NOT NULL,
    [Horario] nvarchar(100) NULL,
    [Ativa] bit NOT NULL,
    [Observacoes] nvarchar(max) NULL,
    CONSTRAINT [PK_Turmas] PRIMARY KEY ([Id])
);

CREATE TABLE [Alunos] (
    [Id] int NOT NULL IDENTITY,
    [NomeCompleto] nvarchar(150) NOT NULL,
    [DataNascimento] date NULL,
    [RG] nvarchar(20) NULL,
    [CPF] nvarchar(14) NOT NULL,
    [Endereco] nvarchar(200) NULL,
    [Celular] nvarchar(20) NULL,
    [Email] nvarchar(150) NULL,
    [ContatoEmergencia] nvarchar(150) NULL,
    [DataEntrada] date NOT NULL,
    [Status] int NOT NULL,
    [Observacoes] nvarchar(max) NULL,
    [TurmaId] int NULL,
    CONSTRAINT [PK_Alunos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Alunos_Turmas_TurmaId] FOREIGN KEY ([TurmaId]) REFERENCES [Turmas] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [AcordosFinanceiros] (
    [Id] int NOT NULL IDENTITY,
    [AlunoId] int NOT NULL,
    [Descricao] nvarchar(150) NOT NULL,
    [ValorMensalAcordado] decimal(10,2) NOT NULL,
    [InicioVigencia] date NOT NULL,
    [FimVigencia] date NULL,
    [Ativo] bit NOT NULL,
    [Observacoes] nvarchar(max) NULL,
    CONSTRAINT [PK_AcordosFinanceiros] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AcordosFinanceiros_Alunos_AlunoId] FOREIGN KEY ([AlunoId]) REFERENCES [Alunos] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Admissoes] (
    [Id] int NOT NULL IDENTITY,
    [AlunoId] int NULL,
    [NomeInteressado] nvarchar(150) NOT NULL,
    [DataAulaExperimental] date NOT NULL,
    [DataMatricula] date NULL,
    [Status] int NOT NULL,
    [ContratoAssinado] bit NOT NULL,
    [PagamentoInicialConfirmado] bit NOT NULL,
    [IntegracaoConcluida] bit NOT NULL,
    [ChecklistObservacoes] nvarchar(max) NULL,
    CONSTRAINT [PK_Admissoes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Admissoes_Alunos_AlunoId] FOREIGN KEY ([AlunoId]) REFERENCES [Alunos] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [Descontos] (
    [Id] int NOT NULL IDENTITY,
    [AlunoId] int NOT NULL,
    [Nome] nvarchar(80) NOT NULL,
    [Tipo] nvarchar(80) NULL,
    [Percentual] decimal(5,2) NULL,
    [ValorFixo] decimal(10,2) NULL,
    [VigenciaInicio] date NOT NULL,
    [VigenciaFim] date NULL,
    [Ativo] bit NOT NULL,
    [Observacoes] nvarchar(max) NULL,
    CONSTRAINT [PK_Descontos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Descontos_Alunos_AlunoId] FOREIGN KEY ([AlunoId]) REFERENCES [Alunos] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Desligamentos] (
    [Id] int NOT NULL IDENTITY,
    [AlunoId] int NOT NULL,
    [DataSolicitacao] date NOT NULL,
    [Motivo] nvarchar(400) NOT NULL,
    [PendenciaFinanceira] decimal(10,2) NOT NULL,
    [MultaRescisoria] decimal(10,2) NOT NULL,
    [RequerimentoRecebido] bit NOT NULL,
    [DataConfirmacao] date NULL,
    [AcessosRemovidos] bit NOT NULL,
    [Observacoes] nvarchar(max) NULL,
    CONSTRAINT [PK_Desligamentos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Desligamentos_Alunos_AlunoId] FOREIGN KEY ([AlunoId]) REFERENCES [Alunos] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Graduacoes] (
    [Id] int NOT NULL IDENTITY,
    [AlunoId] int NOT NULL,
    [ExameGraduacaoId] int NULL,
    [DataResultado] date NOT NULL,
    [ResultadoAprovado] bit NOT NULL,
    [NivelAnterior] int NOT NULL,
    [NivelNovo] int NULL,
    [CertificadoEmitido] bit NOT NULL,
    [OmamoriAtualizado] bit NOT NULL,
    [Observacoes] nvarchar(max) NULL,
    CONSTRAINT [PK_Graduacoes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Graduacoes_Alunos_AlunoId] FOREIGN KEY ([AlunoId]) REFERENCES [Alunos] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Graduacoes_ExamesGraduacao_ExameGraduacaoId] FOREIGN KEY ([ExameGraduacaoId]) REFERENCES [ExamesGraduacao] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [HistoricoAlunos] (
    [Id] int NOT NULL IDENTITY,
    [AlunoId] int NOT NULL,
    [DataEvento] datetime2 NOT NULL,
    [TipoEvento] nvarchar(80) NOT NULL,
    [Descricao] nvarchar(500) NOT NULL,
    CONSTRAINT [PK_HistoricoAlunos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HistoricoAlunos_Alunos_AlunoId] FOREIGN KEY ([AlunoId]) REFERENCES [Alunos] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Mensalidades] (
    [Id] int NOT NULL IDENTITY,
    [AlunoId] int NOT NULL,
    [Competencia] date NOT NULL,
    [DataVencimento] date NOT NULL,
    [DataPagamento] date NULL,
    [ValorBase] decimal(10,2) NOT NULL,
    [ValorFinal] decimal(10,2) NOT NULL,
    [Status] int NOT NULL,
    [Observacoes] nvarchar(max) NULL,
    CONSTRAINT [PK_Mensalidades] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Mensalidades_Alunos_AlunoId] FOREIGN KEY ([AlunoId]) REFERENCES [Alunos] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Pagamentos] (
    [Id] int NOT NULL IDENTITY,
    [MensalidadeId] int NOT NULL,
    [AlunoId] int NOT NULL,
    [DataPagamento] datetime2 NOT NULL,
    [ValorPago] decimal(10,2) NOT NULL,
    [FormaPagamento] int NOT NULL,
    [Comprovante] nvarchar(max) NULL,
    [Observacoes] nvarchar(max) NULL,
    CONSTRAINT [PK_Pagamentos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Pagamentos_Alunos_AlunoId] FOREIGN KEY ([AlunoId]) REFERENCES [Alunos] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Pagamentos_Mensalidades_MensalidadeId] FOREIGN KEY ([MensalidadeId]) REFERENCES [Mensalidades] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AcordosFinanceiros_AlunoId] ON [AcordosFinanceiros] ([AlunoId]);

CREATE INDEX [IX_Admissoes_AlunoId] ON [Admissoes] ([AlunoId]);

CREATE UNIQUE INDEX [IX_Alunos_CPF] ON [Alunos] ([CPF]);

CREATE INDEX [IX_Alunos_TurmaId] ON [Alunos] ([TurmaId]);

CREATE INDEX [IX_Descontos_AlunoId] ON [Descontos] ([AlunoId]);

CREATE INDEX [IX_Desligamentos_AlunoId] ON [Desligamentos] ([AlunoId]);

CREATE INDEX [IX_Graduacoes_AlunoId] ON [Graduacoes] ([AlunoId]);

CREATE INDEX [IX_Graduacoes_ExameGraduacaoId] ON [Graduacoes] ([ExameGraduacaoId]);

CREATE INDEX [IX_HistoricoAlunos_AlunoId] ON [HistoricoAlunos] ([AlunoId]);

CREATE UNIQUE INDEX [IX_Mensalidades_AlunoId_Competencia] ON [Mensalidades] ([AlunoId], [Competencia]);

CREATE INDEX [IX_Pagamentos_AlunoId] ON [Pagamentos] ([AlunoId]);

CREATE INDEX [IX_Pagamentos_MensalidadeId] ON [Pagamentos] ([MensalidadeId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260421025358_InitialCreate', N'10.0.6');

COMMIT;
GO


