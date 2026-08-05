using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Infrastructure.Auditing;
using IkkonAdmin.Web.Infrastructure.Files;
using IkkonAdmin.Web.Infrastructure.Localization;
using IkkonAdmin.Web.Infrastructure.Maintenance;
using IkkonAdmin.Web.Infrastructure.Security;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
var culturaPadrao = CultureInfo.GetCultureInfo("pt-BR");
var culturaIngles = CultureInfo.GetCultureInfo("en-US");
var culturaJapones = CultureInfo.GetCultureInfo("ja-JP");

CultureInfo.DefaultThreadCurrentCulture = culturaPadrao;
CultureInfo.DefaultThreadCurrentUICulture = culturaPadrao;

builder.Services.AddControllersWithViews();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.AddSingleton<IViewTextService, ViewTextService>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, HttpCurrentUserService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IPrivateFileStorageService, LocalPrivateFileStorageService>();
builder.Services.AddSingleton<IDocumentFileValidator, DocumentFileValidator>();
builder.Services.AddScoped<IAuditLogger, EfAuditLogger>();
builder.Services.AddSingleton<IBlogPostActionAuthorizer, BlogPostActionAuthorizer>();
builder.Services.AddDataProtection();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var culturasSuportadas = new[] { culturaPadrao, culturaIngles, culturaJapones };

    options.DefaultRequestCulture = new RequestCulture(culturaPadrao);
    options.SupportedCultures = culturasSuportadas;
    options.SupportedUICultures = culturasSuportadas;
    options.RequestCultureProviders =
    [
        new PublicPathRequestCultureProvider(),
        new QueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    ];
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options
        .UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            })
        .ConfigureWarnings(warnings =>
            warnings
                .Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)
                .Ignore(RelationalEventId.BoolWithDefaultWarning)));

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.AccessDeniedPath = "/auth/acesso-negado";
        options.LogoutPath = "/auth/logout";
        options.Cookie.Name = "ikkonadmin.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization(options => options.AddIkkonPolicies());

builder.Services.AddScoped<IAlunoQueryService, AlunoQueryService>();
builder.Services.AddScoped<IAlunoService, AlunoService>();
builder.Services.AddScoped<ITurmaQueryService, TurmaQueryService>();
builder.Services.AddScoped<ITurmaService, TurmaService>();
builder.Services.AddScoped<IFinanceiroQueryService, FinanceiroQueryService>();
builder.Services.AddScoped<IFinanceiroService, FinanceiroService>();
builder.Services.AddScoped<IAdmissaoQueryService, AdmissaoQueryService>();
builder.Services.AddScoped<IAdmissaoService, AdmissaoService>();
builder.Services.AddScoped<IDesligamentoQueryService, DesligamentoQueryService>();
builder.Services.AddScoped<IDesligamentoService, DesligamentoService>();
builder.Services.AddScoped<IGraduacaoQueryService, GraduacaoQueryService>();
builder.Services.AddScoped<IGraduacaoService, GraduacaoService>();
builder.Services.AddScoped<IDashboardQueryService, DashboardQueryService>();
builder.Services.AddScoped<IConfiguracaoSistemaProvider, ConfiguracaoSistemaProvider>();
builder.Services.AddScoped<IConfiguracaoQueryService, ConfiguracaoQueryService>();
builder.Services.AddScoped<IConfiguracaoService, ConfiguracaoService>();
builder.Services.AddScoped<IAdminPainelQueryService, AdminPainelQueryService>();
builder.Services.AddScoped<IAdminPainelService, AdminPainelService>();
builder.Services.AddScoped<IUserSettingsQueryService, UserSettingsQueryService>();
builder.Services.AddScoped<IUserSettingsService, UserSettingsService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAreaAlunoContextService, AreaAlunoContextService>();
builder.Services.AddScoped<IAreaAlunoPerfilService, AreaAlunoPerfilService>();
builder.Services.AddScoped<IAreaAlunoFinanceiroService, AreaAlunoFinanceiroService>();
builder.Services.AddScoped<IAreaAlunoTurmasService, AreaAlunoTurmasService>();
builder.Services.AddScoped<IAreaAlunoFrequenciaService, AreaAlunoFrequenciaService>();
builder.Services.AddScoped<IAreaAlunoEventosService, AreaAlunoEventosService>();
builder.Services.AddScoped<IAreaAlunoDocumentosService, AreaAlunoDocumentosService>();
builder.Services.AddScoped<IAreaAlunoComunicadosService, AreaAlunoComunicadosService>();
builder.Services.AddScoped<IAreaAlunoConquistasService, AreaAlunoConquistasService>();
builder.Services.AddScoped<IAreaAlunoAulasAdminService, AreaAlunoAulasAdminService>();
builder.Services.AddScoped<IAreaAlunoDocumentoAdminService, AreaAlunoDocumentoAdminService>();
builder.Services.AddScoped<IAreaAlunoComunicadoAdminService, AreaAlunoComunicadoAdminService>();
builder.Services.AddScoped<IAreaAlunoEventoAdminService, AreaAlunoEventoAdminService>();
builder.Services.AddScoped<IAreaAlunoConquistaAdminService, AreaAlunoConquistaAdminService>();
builder.Services.AddScoped<IAreaAlunoService, AreaAlunoService>();
builder.Services.AddScoped<IAreaAlunoAdminService, AreaAlunoAdminService>();
builder.Services.AddScoped<IInventarioQueryService, InventarioQueryService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IBlogAdminQueryService, BlogAdminQueryService>();
builder.Services.AddScoped<IBlogLanguageService, BlogLanguageService>();
builder.Services.AddScoped<IBlogDateTimeService, BlogDateTimeService>();
builder.Services.AddScoped<IBlogTextService, BlogTextService>();
builder.Services.AddScoped<IBlogLookupService, BlogLookupService>();
builder.Services.AddScoped<IBlogSlugService, BlogSlugService>();
builder.Services.AddScoped<IBlogTagService, BlogTagService>();
builder.Services.AddScoped<IBlogWorkflowService, BlogWorkflowService>();
builder.Services.AddScoped<IBlogPublicService, BlogPublicService>();
builder.Services.AddScoped<IBlogVersionService, BlogVersionService>();
builder.Services.AddScoped<IBlogCategoriaService, BlogCategoriaService>();
builder.Services.AddScoped<IBlogMediaService, BlogMediaService>();
builder.Services.AddScoped<IBlogContentSanitizer, BlogContentSanitizer>();
builder.Services.AddScoped<IPublicSeoService, PublicSeoService>();
builder.Services.Configure<GoogleAgendaOptions>(builder.Configuration.GetSection("GoogleAgenda"));
builder.Services.AddScoped<IGoogleAgendaConnectionService, GoogleAgendaConnectionService>();
builder.Services.AddHttpClient<IGoogleAgendaService, GoogleAgendaService>();
builder.Services.AddScoped<IPasswordHasher<UsuarioSistema>, PasswordHasher<UsuarioSistema>>();
builder.Services.Configure<OperationalMaintenanceOptions>(
    builder.Configuration.GetSection(OperationalMaintenanceOptions.SectionName));
builder.Services.AddHostedService<OperationalMaintenanceHostedService>();

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    DatabaseBootstrap.EnsureDatabaseReady(dbContext);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Erro ao executar DatabaseBootstrap.EnsureDatabaseReady no startup.");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/admin/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/erro/{0}");
app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        var hasVersion = context.Context.Request.Query.ContainsKey("v");
        context.Context.Response.Headers.CacheControl = hasVersion
            ? "public,max-age=31536000,immutable"
            : "public,max-age=604800";
    }
});
app.UseRequestLocalization();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapMethods("/health", new[] { "GET", "HEAD" }, () => Results.Ok(new
{
    status = "ok",
    application = "IkkonAdmin",
    checkedAtUtc = DateTimeOffset.UtcNow
}))
.AllowAnonymous();

app.MapControllerRoute(
    name: "localized-home",
    pattern: "{culture:regex(^(pt|en|ja)$)}",
    defaults: new { controller = "Institucional", action = "Index" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "localized-sobre",
    pattern: "{culture:regex(^(pt|en|ja)$)}/sobre",
    defaults: new { controller = "Institucional", action = "Sobre" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "localized-taiko",
    pattern: "{culture:regex(^(pt|en|ja)$)}/taiko",
    defaults: new { controller = "Institucional", action = "Taiko" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "localized-escola",
    pattern: "{culture:regex(^(pt|en|ja)$)}/escola",
    defaults: new { controller = "Institucional", action = "Escola" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "localized-eventos",
    pattern: "{culture:regex(^(pt|en|ja)$)}/eventos",
    defaults: new { controller = "Institucional", action = "Eventos" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "localized-contato",
    pattern: "{culture:regex(^(pt|en|ja)$)}/contato",
    defaults: new { controller = "Institucional", action = "Contato" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "landing",
    pattern: "",
    defaults: new { controller = "Institucional", action = "Index" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "escola",
    pattern: "escola",
    defaults: new { controller = "Institucional", action = "Escola" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "eventos",
    pattern: "eventos",
    defaults: new { controller = "Institucional", action = "Eventos" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "sobre",
    pattern: "sobre",
    defaults: new { controller = "Institucional", action = "Sobre" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "taiko",
    pattern: "taiko",
    defaults: new { controller = "Institucional", action = "Taiko" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "contato",
    pattern: "contato",
    defaults: new { controller = "Institucional", action = "Contato" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "institucional",
    pattern: "institucional/{action=Index}/{id?}",
    defaults: new { controller = "Institucional" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "auth",
    pattern: "auth/{action=Login}/{id?}",
    defaults: new { controller = "Auth" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "area-do-aluno",
    pattern: "area-do-aluno/{action=Index}/{id?}",
    defaults: new { controller = "AlunoArea" })
    .WithStaticAssets()
    .RequireAuthorization(AuthorizationPolicies.Aluno);

app.MapControllerRoute(
    name: "configuracoes",
    pattern: "configuracoes/{action=Index}/{id?}",
    defaults: new { controller = "Configuracoes" })
    .WithStaticAssets()
    .RequireAuthorization();

app.MapControllerRoute(
    name: "aluno",
    pattern: "aluno/{action=Index}/{id?}",
    defaults: new { controller = "AlunoArea" })
    .WithStaticAssets()
    .RequireAuthorization(AuthorizationPolicies.Aluno);

app.MapControllerRoute(
    name: "admin-painel",
    pattern: "admin/painel/{action=Index}/{id?}",
    defaults: new { controller = "PainelAdmin" })
    .WithStaticAssets()
    .RequireAuthorization(AuthorizationPolicies.Admin);

app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets()
    .RequireAuthorization(AuthorizationPolicies.Funcionario);

app.Run();
