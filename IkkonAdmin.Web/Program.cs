using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDataProtection();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options
        .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
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

builder.Services.AddAuthorization(options =>
{
    static void AddFuncionarioPermissionPolicy(Microsoft.AspNetCore.Authorization.AuthorizationOptions options, string policyName, params string[] permissoes)
    {
        options.AddPolicy(
            policyName,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireAssertion(context =>
                    context.User.IsInRole(AppRoles.Admin) ||
                    (context.User.IsInRole(AppRoles.Funcionario) &&
                     permissoes.Any(permissao => context.User.HasClaim(AppClaimTypes.Permissao, permissao)))));
    }

    static void AddAuthenticatedPermissionPolicy(Microsoft.AspNetCore.Authorization.AuthorizationOptions options, string policyName, string permissao)
    {
        options.AddPolicy(
            policyName,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireAssertion(context =>
                    context.User.IsInRole(AppRoles.Admin) ||
                    context.User.HasClaim(AppClaimTypes.Permissao, permissao)));
    }

    static void AddAdminPermissionPolicy(Microsoft.AspNetCore.Authorization.AuthorizationOptions options, string policyName, string permissao)
    {
        options.AddPolicy(
            policyName,
            policy => policy
                .RequireRole(AppRoles.Admin));
    }

    options.AddPolicy(
        AuthorizationPolicies.Funcionario,
        policy => policy.RequireRole(AppRoles.Funcionario, AppRoles.Admin));

    options.AddPolicy(
        AuthorizationPolicies.Aluno,
        policy => policy.RequireRole(AppRoles.Aluno));

    options.AddPolicy(
        AuthorizationPolicies.Admin,
        policy => policy.RequireRole(AppRoles.Admin));

    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.DashboardView, AppPermissions.DashboardView);

    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.AlunosView, AppPermissions.AlunosView);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.AlunosCreate, AppPermissions.AlunosCreate);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.AlunosEdit, AppPermissions.AlunosEdit);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.AlunosDelete, AppPermissions.AlunosDelete);

    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.TurmasView, AppPermissions.TurmasView);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.TurmasCreate, AppPermissions.TurmasCreate);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.TurmasEdit, AppPermissions.TurmasEdit);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.TurmasDelete, AppPermissions.TurmasDelete);

    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.FinanceiroView, AppPermissions.FinanceiroView);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.FinanceiroCreate, AppPermissions.FinanceiroCreate);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.FinanceiroEdit, AppPermissions.FinanceiroEdit);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.FinanceiroDelete, AppPermissions.FinanceiroDelete);

    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.AdmissoesView, AppPermissions.AdmissoesView);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.AdmissoesCreate, AppPermissions.AdmissoesCreate);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.AdmissoesEdit, AppPermissions.AdmissoesEdit);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.AdmissoesDelete, AppPermissions.AdmissoesDelete);

    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.DesligamentosView, AppPermissions.DesligamentosView);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.DesligamentosCreate, AppPermissions.DesligamentosCreate);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.DesligamentosEdit, AppPermissions.DesligamentosEdit);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.DesligamentosDelete, AppPermissions.DesligamentosDelete);

    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.GraduacoesView, AppPermissions.GraduacoesView);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.GraduacoesCreate, AppPermissions.GraduacoesCreate);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.GraduacoesEdit, AppPermissions.GraduacoesEdit);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.GraduacoesDelete, AppPermissions.GraduacoesDelete);

    AddAuthenticatedPermissionPolicy(options, AuthorizationPolicies.ConfiguracoesView, AppPermissions.ConfiguracoesView);
    AddAuthenticatedPermissionPolicy(options, AuthorizationPolicies.ConfiguracoesEdit, AppPermissions.ConfiguracoesEdit);

    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.GoogleAgendaView, AppPermissions.GoogleAgendaView, AppPermissions.GoogleAgendaManage);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.GoogleAgendaCreate, AppPermissions.GoogleAgendaCreate, AppPermissions.GoogleAgendaManage);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.GoogleAgendaEdit, AppPermissions.GoogleAgendaEdit, AppPermissions.GoogleAgendaManage);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.GoogleAgendaDelete, AppPermissions.GoogleAgendaDelete, AppPermissions.GoogleAgendaManage);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.GoogleAgendaManage, AppPermissions.GoogleAgendaManage);

    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.InventarioView, AppPermissions.InventarioView, AppPermissions.InventarioManage);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.InventarioCreate, AppPermissions.InventarioCreate, AppPermissions.InventarioManage);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.InventarioEdit, AppPermissions.InventarioEdit, AppPermissions.InventarioManage);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.InventarioDelete, AppPermissions.InventarioDelete, AppPermissions.InventarioManage);
    AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.InventarioManage, AppPermissions.InventarioManage);

    AddAdminPermissionPolicy(options, AuthorizationPolicies.AdminGerenciarUsuarios, AppPermissions.GerenciarUsuarios);
    AddAdminPermissionPolicy(options, AuthorizationPolicies.AdminGerenciarCargos, AppPermissions.GerenciarCargos);
    AddAdminPermissionPolicy(options, AuthorizationPolicies.AdminEditarPermissoes, AppPermissions.EditarPermissoes);
    AddAdminPermissionPolicy(options, AuthorizationPolicies.AdminVisualizarDados, AppPermissions.VisualizarDados);
    AddAdminPermissionPolicy(options, AuthorizationPolicies.AdminGerenciarSistema, AppPermissions.GerenciarSistema);
});

builder.Services.AddScoped<IAlunoService, AlunoService>();
builder.Services.AddScoped<ITurmaService, TurmaService>();
builder.Services.AddScoped<IFinanceiroService, FinanceiroService>();
builder.Services.AddScoped<IAdmissaoService, AdmissaoService>();
builder.Services.AddScoped<IDesligamentoService, DesligamentoService>();
builder.Services.AddScoped<IGraduacaoService, GraduacaoService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IConfiguracaoService, ConfiguracaoService>();
builder.Services.AddScoped<IAdminPainelService, AdminPainelService>();
builder.Services.AddScoped<IUserSettingsService, UserSettingsService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAreaAlunoService, AreaAlunoService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.Configure<GoogleAgendaOptions>(builder.Configuration.GetSection("GoogleAgenda"));
builder.Services.AddHttpClient<IGoogleAgendaService, GoogleAgendaService>();
builder.Services.AddScoped<IPasswordHasher<UsuarioSistema>, PasswordHasher<UsuarioSistema>>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    DatabaseBootstrap.EnsureDatabaseReady(dbContext);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/admin/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    application = "IkkonAdmin",
    checkedAtUtc = DateTimeOffset.UtcNow
}))
.AllowAnonymous();

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
