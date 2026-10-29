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
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [NombreCompleto] nvarchar(max) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [Dimensiones] (
    [Id] int NOT NULL IDENTITY,
    [Codigo] nvarchar(450) NOT NULL,
    [Nombre] nvarchar(max) NOT NULL,
    [Descripcion] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Dimensiones] PRIMARY KEY ([Id])
);

CREATE TABLE [TiposSistema] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NOT NULL,
    [Descripcion] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_TiposSistema] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(128) NOT NULL,
    [ProviderKey] nvarchar(128) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Empresas] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NOT NULL,
    [Sector] nvarchar(max) NOT NULL,
    [Municipio] nvarchar(max) NOT NULL,
    [ApplicationUserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_Empresas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Empresas_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Formularios] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NOT NULL,
    [Version] nvarchar(max) NOT NULL,
    [Activo] bit NOT NULL,
    [TipoSistemaId] int NOT NULL,
    CONSTRAINT [PK_Formularios] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Formularios_TiposSistema_TipoSistemaId] FOREIGN KEY ([TipoSistemaId]) REFERENCES [TiposSistema] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Evaluaciones] (
    [Id] int NOT NULL IDENTITY,
    [EmpresaId] int NOT NULL,
    [FormularioId] int NOT NULL,
    [FechaInicio] datetime2 NOT NULL,
    [FechaFinalizacion] datetime2 NULL,
    [Estado] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Evaluaciones] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Evaluaciones_Empresas_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [Empresas] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Evaluaciones_Formularios_FormularioId] FOREIGN KEY ([FormularioId]) REFERENCES [Formularios] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Preguntas] (
    [Id] int NOT NULL IDENTITY,
    [FormularioId] int NOT NULL,
    [DimensionId] int NOT NULL,
    [Texto] nvarchar(max) NOT NULL,
    [ReferenciaTecnica] nvarchar(max) NULL,
    [CriterioCumplimiento] nvarchar(max) NULL,
    [Activa] bit NOT NULL,
    [Orden] int NOT NULL,
    CONSTRAINT [PK_Preguntas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Preguntas_Dimensiones_DimensionId] FOREIGN KEY ([DimensionId]) REFERENCES [Dimensiones] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Preguntas_Formularios_FormularioId] FOREIGN KEY ([FormularioId]) REFERENCES [Formularios] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Diagnosticos] (
    [Id] int NOT NULL IDENTITY,
    [EvaluacionId] int NOT NULL,
    [FechaGeneracion] datetime2 NOT NULL,
    [PorcentajeGeneral] decimal(18,2) NOT NULL,
    [Resumen] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Diagnosticos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Diagnosticos_Evaluaciones_EvaluacionId] FOREIGN KEY ([EvaluacionId]) REFERENCES [Evaluaciones] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Respuestas] (
    [Id] int NOT NULL IDENTITY,
    [EvaluacionId] int NOT NULL,
    [PreguntaId] int NOT NULL,
    [Valor] int NOT NULL,
    [Observaciones] nvarchar(max) NULL,
    [FechaRegistro] datetime2 NOT NULL,
    CONSTRAINT [PK_Respuestas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Respuestas_Evaluaciones_EvaluacionId] FOREIGN KEY ([EvaluacionId]) REFERENCES [Evaluaciones] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Respuestas_Preguntas_PreguntaId] FOREIGN KEY ([PreguntaId]) REFERENCES [Preguntas] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Hallazgos] (
    [Id] int NOT NULL IDENTITY,
    [DiagnosticoId] int NOT NULL,
    [PreguntaId] int NOT NULL,
    [Descripcion] nvarchar(max) NOT NULL,
    [Prioridad] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Hallazgos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Hallazgos_Diagnosticos_DiagnosticoId] FOREIGN KEY ([DiagnosticoId]) REFERENCES [Diagnosticos] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Hallazgos_Preguntas_PreguntaId] FOREIGN KEY ([PreguntaId]) REFERENCES [Preguntas] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ResultadosDimension] (
    [Id] int NOT NULL IDENTITY,
    [DiagnosticoId] int NOT NULL,
    [DimensionId] int NOT NULL,
    [PreguntasAplicables] int NOT NULL,
    [PreguntasCumplidas] int NOT NULL,
    [PorcentajeCumplimiento] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_ResultadosDimension] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ResultadosDimension_Diagnosticos_DiagnosticoId] FOREIGN KEY ([DiagnosticoId]) REFERENCES [Diagnosticos] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ResultadosDimension_Dimensiones_DimensionId] FOREIGN KEY ([DimensionId]) REFERENCES [Dimensiones] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Recomendaciones] (
    [Id] int NOT NULL IDENTITY,
    [PreguntaId] int NOT NULL,
    [HallazgoId] int NULL,
    [Titulo] nvarchar(max) NOT NULL,
    [Descripcion] nvarchar(max) NOT NULL,
    [Prioridad] nvarchar(max) NOT NULL,
    [Activa] bit NOT NULL,
    CONSTRAINT [PK_Recomendaciones] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Recomendaciones_Hallazgos_HallazgoId] FOREIGN KEY ([HallazgoId]) REFERENCES [Hallazgos] ([Id]),
    CONSTRAINT [FK_Recomendaciones_Preguntas_PreguntaId] FOREIGN KEY ([PreguntaId]) REFERENCES [Preguntas] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE UNIQUE INDEX [IX_Diagnosticos_EvaluacionId] ON [Diagnosticos] ([EvaluacionId]);

CREATE UNIQUE INDEX [IX_Dimensiones_Codigo] ON [Dimensiones] ([Codigo]);

CREATE INDEX [IX_Empresas_ApplicationUserId] ON [Empresas] ([ApplicationUserId]);

CREATE INDEX [IX_Evaluaciones_EmpresaId] ON [Evaluaciones] ([EmpresaId]);

CREATE INDEX [IX_Evaluaciones_FormularioId] ON [Evaluaciones] ([FormularioId]);

CREATE INDEX [IX_Formularios_TipoSistemaId] ON [Formularios] ([TipoSistemaId]);

CREATE INDEX [IX_Hallazgos_DiagnosticoId] ON [Hallazgos] ([DiagnosticoId]);

CREATE INDEX [IX_Hallazgos_PreguntaId] ON [Hallazgos] ([PreguntaId]);

CREATE INDEX [IX_Preguntas_DimensionId] ON [Preguntas] ([DimensionId]);

CREATE INDEX [IX_Preguntas_FormularioId] ON [Preguntas] ([FormularioId]);

CREATE INDEX [IX_Recomendaciones_HallazgoId] ON [Recomendaciones] ([HallazgoId]);

CREATE INDEX [IX_Recomendaciones_PreguntaId] ON [Recomendaciones] ([PreguntaId]);

CREATE UNIQUE INDEX [IX_Respuestas_EvaluacionId_PreguntaId] ON [Respuestas] ([EvaluacionId], [PreguntaId]);

CREATE INDEX [IX_Respuestas_PreguntaId] ON [Respuestas] ([PreguntaId]);

CREATE INDEX [IX_ResultadosDimension_DiagnosticoId] ON [ResultadosDimension] ([DiagnosticoId]);

CREATE INDEX [IX_ResultadosDimension_DimensionId] ON [ResultadosDimension] ([DimensionId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261028214929_CrearModeloInicial', N'10.0.14');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Preguntas] ADD [TipoRespuesta] nvarchar(max) NOT NULL DEFAULT N'';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261029032224_AgregarTipoRespuestaPregunta', N'10.0.14');

COMMIT;
GO

