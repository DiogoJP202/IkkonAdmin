using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.Extensions.Options;

namespace IkkonAdmin.Web.Services;

public class GoogleAgendaService(
    HttpClient httpClient,
    IOptions<GoogleAgendaOptions> options,
    IWebHostEnvironment environment,
    ILogger<GoogleAgendaService> logger,
    IGoogleAgendaConnectionService connectionService,
    IClock clock) : IGoogleAgendaService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly GoogleAgendaOptions options = options.Value;

    private string? accessToken;
    private DateTimeOffset accessTokenExpiresAt;

    public string? CalendarId => options.CalendarId;

    public Task<bool> PossuiConexaoOAuthAsync(CancellationToken cancellationToken = default)
    {
        return connectionService.PossuiConexaoOAuthAsync(cancellationToken);
    }

    public async Task<string> GerarUrlAutorizacaoAsync(
        string redirectUri,
        string state,
        CancellationToken cancellationToken = default)
    {
        var credentials = await LerCredenciaisAsync(cancellationToken);
        if (credentials.OAuthClient is null)
        {
            throw new GoogleAgendaConfigurationException("O arquivo configurado não é um OAuth Client web. Use o JSON com a chave 'web'.");
        }

        var client = credentials.OAuthClient;
        var callback = ResolverRedirectUri(redirectUri);
        var authUri = string.IsNullOrWhiteSpace(client.AuthUri)
            ? "https://accounts.google.com/o/oauth2/auth"
            : client.AuthUri;

        var query = new Dictionary<string, string>
        {
            ["client_id"] = client.ClientId ?? string.Empty,
            ["redirect_uri"] = callback,
            ["response_type"] = "code",
            ["scope"] = GoogleAgendaConstants.CalendarScope,
            ["access_type"] = "offline",
            ["prompt"] = "consent",
            ["state"] = state
        };

        return $"{authUri}?{MontarQueryString(query)}";
    }

    public async Task ConcluirAutorizacaoOAuthAsync(
        string code,
        string redirectUri,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new GoogleAgendaConfigurationException("Código de autorização do Google não informado.");
        }

        var credentials = await LerCredenciaisAsync(cancellationToken);
        if (credentials.OAuthClient is null)
        {
            throw new GoogleAgendaConfigurationException("O arquivo configurado não é um OAuth Client web.");
        }

        var client = credentials.OAuthClient;
        var tokenUri = string.IsNullOrWhiteSpace(client.TokenUri)
            ? "https://oauth2.googleapis.com/token"
            : client.TokenUri;

        using var tokenResponse = await httpClient.PostAsync(
            tokenUri,
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["client_id"] = client.ClientId ?? string.Empty,
                ["client_secret"] = client.ClientSecret ?? string.Empty,
                ["redirect_uri"] = ResolverRedirectUri(redirectUri)
            }),
            cancellationToken);

        await EnsureSuccessAsync(tokenResponse, "conectar OAuth do Google Agenda", cancellationToken);

        await using var stream = await tokenResponse.Content.ReadAsStreamAsync(cancellationToken);
        var tokenPayload = await JsonSerializer.DeserializeAsync<GoogleTokenResponse>(stream, JsonOptions, cancellationToken)
            ?? throw new GoogleAgendaConfigurationException("Não foi possível obter token do Google Agenda.");

        if (string.IsNullOrWhiteSpace(tokenPayload.RefreshToken))
        {
            throw new GoogleAgendaConfigurationException("O Google não retornou refresh token. Revogue o acesso anterior no Google ou reconecte usando prompt=consent.");
        }

        await connectionService.SubstituirConexaoAtivaAsync(
            tokenPayload.RefreshToken,
            tokenPayload.Scope ?? GoogleAgendaConstants.CalendarScope,
            usuarioId,
            cancellationToken);
    }

    public async Task DesconectarOAuthAsync(int? usuarioId, CancellationToken cancellationToken = default)
    {
        await connectionService.DesconectarOAuthAsync(usuarioId, cancellationToken);
        accessToken = null;
        accessTokenExpiresAt = DateTimeOffset.MinValue;
    }

    public async Task<IReadOnlyList<GoogleAgendaEventoViewModel>> ListarEventosAsync(
        GoogleAgendaFiltroViewModel filtro,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthorizationAsync(cancellationToken);

        var inicio = filtro.Inicio.ToDateTime(TimeOnly.MinValue);
        var fim = filtro.Fim.ToDateTime(TimeOnly.MaxValue);
        var url = new StringBuilder();
        url.Append("https://www.googleapis.com/calendar/v3/calendars/");
        url.Append(Uri.EscapeDataString(ObterCalendarId()));
        url.Append("/events?singleEvents=true&orderBy=startTime");
        url.Append("&timeMin=").Append(Uri.EscapeDataString(FormataUtc(inicio)));
        url.Append("&timeMax=").Append(Uri.EscapeDataString(FormataUtc(fim)));

        using var response = await httpClient.GetAsync(url.ToString(), cancellationToken);
        await EnsureSuccessAsync(response, "listar eventos", cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<GoogleEventsResponse>(stream, JsonOptions, cancellationToken)
            ?? new GoogleEventsResponse();

        return payload.Items
            .Select(MapearEvento)
            .Where(x => !filtro.Tipo.HasValue || x.Tipo == filtro.Tipo.Value)
            .OrderBy(x => x.Inicio)
            .ToList();
    }

    public async Task<GoogleAgendaEventoViewModel?> ObterEventoAsync(string eventoId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(eventoId))
        {
            return null;
        }

        await EnsureAuthorizationAsync(cancellationToken);

        using var response = await httpClient.GetAsync(MontarEventoUrl(eventoId), cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, "obter evento", cancellationToken);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<GoogleEventResource>(stream, JsonOptions, cancellationToken);
        return payload is null ? null : MapearEvento(payload);
    }

    public async Task<GoogleAgendaEventoViewModel> CriarEventoAsync(
        GoogleAgendaEventoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthorizationAsync(cancellationToken);

        var content = JsonContent(model);
        using var response = await httpClient.PostAsync(MontarCalendarioEventosUrl(), content, cancellationToken);
        await EnsureSuccessAsync(response, "criar evento", cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<GoogleEventResource>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Google Agenda não retornou o evento criado.");

        return MapearEvento(payload);
    }

    public async Task<GoogleAgendaEventoViewModel> AtualizarEventoAsync(
        string eventoId,
        GoogleAgendaEventoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthorizationAsync(cancellationToken);

        var content = JsonContent(model);
        using var response = await httpClient.PutAsync(MontarEventoUrl(eventoId), content, cancellationToken);
        await EnsureSuccessAsync(response, "atualizar evento", cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<GoogleEventResource>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Google Agenda não retornou o evento atualizado.");

        return MapearEvento(payload);
    }

    public async Task ExcluirEventoAsync(string eventoId, CancellationToken cancellationToken = default)
    {
        await EnsureAuthorizationAsync(cancellationToken);

        using var response = await httpClient.DeleteAsync(MontarEventoUrl(eventoId), cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return;
        }

        await EnsureSuccessAsync(response, "excluir evento", cancellationToken);
    }

    private async Task EnsureAuthorizationAsync(CancellationToken cancellationToken)
    {
        if (accessToken is not null && accessTokenExpiresAt > new DateTimeOffset(clock.UtcNow).AddMinutes(2))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return;
        }

        var credentials = await LerCredenciaisAsync(cancellationToken);
        if (credentials.ServiceAccount is not null)
        {
            await AutorizarComServiceAccountAsync(credentials.ServiceAccount, cancellationToken);
            return;
        }

        if (credentials.OAuthClient is not null)
        {
            await AutorizarComOAuthAsync(credentials.OAuthClient, cancellationToken);
            return;
        }

        throw new GoogleAgendaConfigurationException("Credenciais do Google Agenda não reconhecidas.");
    }

    private async Task AutorizarComServiceAccountAsync(
        GoogleServiceAccountCredentials credentials,
        CancellationToken cancellationToken)
    {
        var tokenUri = string.IsNullOrWhiteSpace(credentials.TokenUri)
            ? "https://oauth2.googleapis.com/token"
            : credentials.TokenUri;

        var assertion = CriarJwtAssertion(credentials, tokenUri);
        using var tokenResponse = await httpClient.PostAsync(
            tokenUri,
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                ["assertion"] = assertion
            }),
            cancellationToken);

        await EnsureSuccessAsync(tokenResponse, "autenticar no Google", cancellationToken);
        var tokenPayload = await LerTokenResponseAsync(tokenResponse, cancellationToken);
        AplicarAccessToken(tokenPayload);
    }

    private async Task AutorizarComOAuthAsync(GoogleOAuthClientCredentials credentials, CancellationToken cancellationToken)
    {
        var refreshToken = await connectionService.ObterRefreshTokenAtivoAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new GoogleAgendaConfigurationException("Conecte o Google Agenda pelo botão 'Conectar Google Agenda' antes de carregar eventos.");
        }

        var tokenUri = string.IsNullOrWhiteSpace(credentials.TokenUri)
            ? "https://oauth2.googleapis.com/token"
            : credentials.TokenUri;

        using var tokenResponse = await httpClient.PostAsync(
            tokenUri,
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = credentials.ClientId ?? string.Empty,
                ["client_secret"] = credentials.ClientSecret ?? string.Empty
            }),
            cancellationToken);

        await EnsureSuccessAsync(tokenResponse, "renovar token do Google Agenda", cancellationToken);
        var tokenPayload = await LerTokenResponseAsync(tokenResponse, cancellationToken);
        AplicarAccessToken(tokenPayload);
    }

    private async Task<GoogleTokenResponse> LerTokenResponseAsync(HttpResponseMessage tokenResponse, CancellationToken cancellationToken)
    {
        await using var stream = await tokenResponse.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<GoogleTokenResponse>(stream, JsonOptions, cancellationToken)
            ?? throw new GoogleAgendaConfigurationException("Não foi possível obter token do Google Agenda.");
    }

    private void AplicarAccessToken(GoogleTokenResponse tokenPayload)
    {
        accessToken = tokenPayload.AccessToken;
        accessTokenExpiresAt = new DateTimeOffset(clock.UtcNow).AddSeconds(Math.Max(60, tokenPayload.ExpiresIn - 60));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    private async Task<GoogleCredentialsDocument> LerCredenciaisAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.CalendarId))
        {
            throw new GoogleAgendaConfigurationException("Configure GoogleAgenda:CalendarId no appsettings.");
        }

        var path = !string.IsNullOrWhiteSpace(options.OAuthClientSecretsPath)
            ? options.OAuthClientSecretsPath
            : options.CredentialsPath;

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new GoogleAgendaConfigurationException("Configure GoogleAgenda:CredentialsPath ou GoogleAgenda:OAuthClientSecretsPath com o caminho do JSON do Google.");
        }

        if (!Path.IsPathRooted(path))
        {
            path = Path.Combine(environment.ContentRootPath, path);
        }

        if (!File.Exists(path))
        {
            throw new GoogleAgendaConfigurationException($"Arquivo de credenciais não encontrado em: {path}");
        }

        await using var stream = File.OpenRead(path);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;

        if (root.TryGetProperty("web", out var webElement))
        {
            var oauthClient = webElement.Deserialize<GoogleOAuthClientCredentials>(JsonOptions);
            if (oauthClient is null || string.IsNullOrWhiteSpace(oauthClient.ClientId) || string.IsNullOrWhiteSpace(oauthClient.ClientSecret))
            {
                throw new GoogleAgendaConfigurationException("Credenciais OAuth inválidas. Verifique client_id e client_secret.");
            }

            return new GoogleCredentialsDocument(null, oauthClient);
        }

        var serviceAccount = root.Deserialize<GoogleServiceAccountCredentials>(JsonOptions);
        if (serviceAccount is not null &&
            !string.IsNullOrWhiteSpace(serviceAccount.ClientEmail) &&
            !string.IsNullOrWhiteSpace(serviceAccount.PrivateKey))
        {
            return new GoogleCredentialsDocument(serviceAccount, null);
        }

        throw new GoogleAgendaConfigurationException("Credenciais do Google Agenda inválidas. Use JSON de service account ou OAuth Client web.");
    }

    private string ResolverRedirectUri(string redirectUri)
    {
        return !string.IsNullOrWhiteSpace(options.RedirectUri)
            ? options.RedirectUri
            : redirectUri;
    }

    private string CriarJwtAssertion(GoogleServiceAccountCredentials credentials, string tokenUri)
    {
        var now = new DateTimeOffset(clock.UtcNow).ToUnixTimeSeconds();
        var header = new { alg = "RS256", typ = "JWT" };
        var payload = new
        {
            iss = credentials.ClientEmail,
            scope = GoogleAgendaConstants.CalendarScope,
            aud = tokenUri,
            exp = now + 3600,
            iat = now
        };

        var unsignedToken = $"{Base64Url(JsonSerializer.SerializeToUtf8Bytes(header))}.{Base64Url(JsonSerializer.SerializeToUtf8Bytes(payload))}";
        using var rsa = RSA.Create();
        rsa.ImportFromPem(credentials.PrivateKey);
        var signature = rsa.SignData(Encoding.ASCII.GetBytes(unsignedToken), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return $"{unsignedToken}.{Base64Url(signature)}";
    }

    private static string Base64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private HttpContent JsonContent(GoogleAgendaEventoFormViewModel model)
    {
        var payload = new GoogleEventResource
        {
            Summary = model.Titulo.Trim(),
            Description = LimparTextoOpcional(model.Descricao),
            Location = LimparTextoOpcional(model.Local),
            Start = new GoogleEventDateTime
            {
                DateTime = model.Inicio,
                TimeZone = options.TimeZone
            },
            End = new GoogleEventDateTime
            {
                DateTime = model.Fim,
                TimeZone = options.TimeZone
            },
            ExtendedProperties = new GoogleExtendedProperties
            {
                Private = new Dictionary<string, string>
                {
                    ["ikkonTipo"] = model.Tipo.ToString()
                }
            }
        };

        return new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");
    }

    private static GoogleAgendaEventoViewModel MapearEvento(GoogleEventResource evento)
    {
        var inicio = evento.Start?.DateTime
            ?? (evento.Start?.Date is not null ? evento.Start.Date.Value.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue);
        var fim = evento.End?.DateTime
            ?? (evento.End?.Date is not null ? evento.End.Date.Value.ToDateTime(TimeOnly.MinValue) : inicio);
        var diaInteiro = evento.Start?.Date is not null;

        var tipo = GoogleAgendaTipoEventoEnum.Outro;
        if (evento.ExtendedProperties?.Private is not null &&
            evento.ExtendedProperties.Private.TryGetValue("ikkonTipo", out var tipoTexto) &&
            Enum.TryParse<GoogleAgendaTipoEventoEnum>(tipoTexto, true, out var tipoParseado))
        {
            tipo = tipoParseado;
        }

        return new GoogleAgendaEventoViewModel
        {
            Id = evento.Id ?? string.Empty,
            Titulo = evento.Summary ?? "(Sem título)",
            Descricao = evento.Description,
            Local = evento.Location,
            Inicio = inicio,
            Fim = fim,
            DiaInteiro = diaInteiro,
            Tipo = tipo,
            Status = evento.Status,
            HtmlLink = evento.HtmlLink
        };
    }

    private string MontarCalendarioEventosUrl()
    {
        return $"https://www.googleapis.com/calendar/v3/calendars/{Uri.EscapeDataString(ObterCalendarId())}/events";
    }

    private string MontarEventoUrl(string eventoId)
    {
        return $"{MontarCalendarioEventosUrl()}/{Uri.EscapeDataString(eventoId)}";
    }

    private string ObterCalendarId()
    {
        return !string.IsNullOrWhiteSpace(options.CalendarId)
            ? options.CalendarId
            : throw new GoogleAgendaConfigurationException("Configure GoogleAgenda:CalendarId no appsettings.");
    }

    private static string FormataUtc(DateTime dateTime)
    {
        return DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime().ToString("O");
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, string operacao, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var detalhe = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogWarning(
            "Falha ao {Operacao} no Google Agenda. Status: {StatusCode}. Corpo: {Body}",
            operacao,
            (int)response.StatusCode,
            detalhe);

        throw new InvalidOperationException($"Não foi possível {operacao} no Google Agenda. Verifique a configuração e tente novamente.");
    }

    private static string MontarQueryString(Dictionary<string, string> values)
    {
        return string.Join("&", values.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));
    }

    private static string? LimparTextoOpcional(string? texto)
    {
        var valor = texto?.Trim();
        return string.IsNullOrWhiteSpace(valor) ? null : valor;
    }

    private sealed record GoogleCredentialsDocument(
        GoogleServiceAccountCredentials? ServiceAccount,
        GoogleOAuthClientCredentials? OAuthClient);

    private sealed class GoogleServiceAccountCredentials
    {
        [JsonPropertyName("client_email")]
        public string? ClientEmail { get; set; }

        [JsonPropertyName("private_key")]
        public string? PrivateKey { get; set; }

        [JsonPropertyName("token_uri")]
        public string? TokenUri { get; set; }
    }

    private sealed class GoogleOAuthClientCredentials
    {
        [JsonPropertyName("client_id")]
        public string? ClientId { get; set; }

        [JsonPropertyName("client_secret")]
        public string? ClientSecret { get; set; }

        [JsonPropertyName("auth_uri")]
        public string? AuthUri { get; set; }

        [JsonPropertyName("token_uri")]
        public string? TokenUri { get; set; }
    }

    private sealed class GoogleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }

    private sealed class GoogleEventsResponse
    {
        public List<GoogleEventResource> Items { get; set; } = new();
    }

    private sealed class GoogleEventResource
    {
        public string? Id { get; set; }
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? Status { get; set; }
        public string? HtmlLink { get; set; }
        public GoogleEventDateTime? Start { get; set; }
        public GoogleEventDateTime? End { get; set; }
        public GoogleExtendedProperties? ExtendedProperties { get; set; }
    }

    private sealed class GoogleEventDateTime
    {
        public DateTime? DateTime { get; set; }
        public DateOnly? Date { get; set; }
        public string? TimeZone { get; set; }
    }

    private sealed class GoogleExtendedProperties
    {
        public Dictionary<string, string>? Private { get; set; }
    }
}
