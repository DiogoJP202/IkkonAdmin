using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.Playwright;

const string StabilizationCss =
    """
    *, *::before, *::after {
      animation: none !important;
      transition: none !important;
      scroll-behavior: auto !important;
    }

    .reveal {
      opacity: 1 !important;
      transform: none !important;
    }
    """;

var options = RunnerOptions.Parse(args);
var repoRoot = FindRepoRoot();
var baselineDirectory = Path.GetFullPath(
    options.BaselineDirectory ?? Path.Combine(repoRoot, "IkkonAdmin.VisualTests", "Baselines"));
var artifactDirectory = Path.Combine(repoRoot, "artifacts", "visual-regression");

Directory.CreateDirectory(baselineDirectory);
RecreateDirectory(artifactDirectory);

using var playwright = await Playwright.CreateAsync();
var launchOptions = new BrowserTypeLaunchOptions
{
    Headless = true,
    Channel = options.BrowserChannel
};

await using var browser = await playwright.Chromium.LaunchAsync(launchOptions);

var viewports = new[]
{
    new VisualViewport("desktop", 1440, 1000),
    new VisualViewport("mobile", 390, 844)
};

var failures = new List<string>();
var pageErrors = new List<string>();
var captures = 0;

foreach (var viewport in viewports)
{
    await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
    {
        ViewportSize = new ViewportSize
        {
            Width = viewport.Width,
            Height = viewport.Height
        },
        DeviceScaleFactor = 1,
        Locale = "pt-BR",
        ColorScheme = ColorScheme.Light,
        ReducedMotion = ReducedMotion.Reduce,
        IgnoreHTTPSErrors = true
    });

    var page = await context.NewPageAsync();
    page.PageError += (_, message) =>
    {
        pageErrors.Add($"{viewport.Name}: {message}");
    };

    await NavigateAndSettleAsync(page, BuildUrl(options.BaseUrl, "/?entrada=1"));
    await CaptureAndCompareAsync("entrada", viewport, page);

    await page.Locator("[data-gateway-dismiss]").ClickAsync();
    await page.WaitForTimeoutAsync(250);
    await SettleAsync(page);
    await CaptureAndCompareAsync("home", viewport, page);

    foreach (var route in new[]
             {
                 new VisualRoute("escola", "/escola"),
                 new VisualRoute("eventos", "/eventos"),
                 new VisualRoute("blog", "/blog"),
                 new VisualRoute("auth-login", "/auth/login"),
                 new VisualRoute("aluno-login", "/aluno/login")
             })
    {
        await NavigateAndSettleAsync(page, BuildUrl(options.BaseUrl, route.Path));
        await CaptureAndCompareAsync(route.Name, viewport, page);
    }

    await NavigateAndSettleAsync(page, BuildUrl(options.BaseUrl, "/aluno/login"));
    await page.Locator("#LoginOuEmail").FillAsync("aluno.demo");
    await page.Locator("#Senha").FillAsync("Aluno@123");
    await page.Locator(".auth-submit").ClickAsync();
    await page.WaitForURLAsync(
        "**/area-do-aluno**",
        new PageWaitForURLOptions
        {
            WaitUntil = WaitUntilState.Load,
            Timeout = 30_000
        });
    await SettleAsync(page);

    var studentRoutes = new[]
    {
        new VisualRoute("aluno-dashboard", "/area-do-aluno"),
        new VisualRoute("aluno-perfil", "/area-do-aluno/perfil"),
        new VisualRoute("aluno-turmas", "/area-do-aluno/turmas"),
        new VisualRoute("aluno-aulas", "/area-do-aluno/aulas"),
        new VisualRoute("aluno-frequencia", "/area-do-aluno/frequencia"),
        new VisualRoute("aluno-financeiro", "/area-do-aluno/financeiro"),
        new VisualRoute("aluno-documentos", "/area-do-aluno/documentos"),
        new VisualRoute("aluno-comunicados", "/area-do-aluno/comunicados"),
        new VisualRoute("aluno-eventos", "/area-do-aluno/eventos"),
        new VisualRoute("aluno-conquistas", "/area-do-aluno/conquistas"),
        new VisualRoute(
            "aluno-acesso-indisponivel",
            "/area-do-aluno/acessoindisponivel"),
        new VisualRoute("aluno-configuracoes", "/configuracoes")
    };

    foreach (var route in studentRoutes)
    {
        await NavigateAndSettleAsync(page, BuildUrl(options.BaseUrl, route.Path));
        await ValidateStudentPageAsync(route.Name, viewport, page);
        await CaptureAndCompareAsync(route.Name, viewport, page);

        if (route.Name == "aluno-frequencia")
        {
            await page.Locator("#frequenciaInicio").FillAsync("2026-01-01");
            await page.Locator("#frequenciaFim").FillAsync("2026-12-31");
            await page.Locator(".aluno-portal-filter button[type='submit']").ClickAsync();
            await page.WaitForURLAsync(
                "**/area-do-aluno/frequencia?inicio=2026-01-01&fim=2026-12-31",
                new PageWaitForURLOptions
                {
                    WaitUntil = WaitUntilState.Load,
                    Timeout = 30_000
                });
            await SettleAsync(page);
            await ValidateStudentPageAsync("aluno-frequencia-filtrada", viewport, page);
        }

        if (route.Name == "aluno-dashboard" && viewport.Name == "mobile")
        {
            await ValidateMobileDrawerAsync(viewport, page);
        }
    }

    await NavigateAndSettleAsync(page, BuildUrl(options.BaseUrl, "/area-do-aluno"));
    await page.EvaluateAsync(
        """
        () => {
          document.body.classList.remove("aluno-theme-light");
          document.body.classList.add("aluno-theme-dark");
          document.body.dataset.theme = "dark";
        }
        """);
    await SettleAsync(page);
    await ValidateStudentPageAsync("aluno-dashboard-dark", viewport, page);
    await CaptureAndCompareAsync("aluno-dashboard-dark", viewport, page);

    await NavigateAndSettleAsync(
        page,
        BuildUrl(
            options.BaseUrl,
            "/idioma/alterar?culture=ja-JP&returnUrl=%2Farea-do-aluno"));
    await ValidateStudentPageAsync("aluno-dashboard-ja", viewport, page);
    var documentLanguage = await page.Locator("html").GetAttributeAsync("lang");
    if (!string.Equals(documentLanguage, "ja", StringComparison.OrdinalIgnoreCase))
    {
        failures.Add(
            $"aluno-dashboard-ja-{viewport.Name}: idioma HTML esperado 'ja', " +
            $"encontrado '{documentLanguage ?? "ausente"}'");
    }

    await CaptureAndCompareAsync("aluno-dashboard-ja", viewport, page);

    if (viewport.Name == "mobile")
    {
        await page.Locator("[data-aluno-menu-toggle]").ClickAsync();
    }

    var logoutButton = page.Locator(
        "form[action='/aluno/sair'] button[type='submit']");
    await logoutButton.ScrollIntoViewIfNeededAsync();
    await logoutButton.ClickAsync();
    await page.WaitForURLAsync(
        "**/aluno/login**",
        new PageWaitForURLOptions
        {
            WaitUntil = WaitUntilState.Load,
            Timeout = 30_000
        });
}

failures.AddRange(pageErrors.Select(error => $"JavaScript: {error}"));

if (failures.Count > 0)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine("Regressão visual detectada:");
    foreach (var failure in failures)
    {
        Console.Error.WriteLine($"- {failure}");
    }

    Console.Error.WriteLine($"Capturas e diffs: \"{artifactDirectory}\".");
    return 1;
}

if (options.UpdateBaseline)
{
    Console.WriteLine(
        $"Baseline visual atualizado: {captures} capturas em \"{baselineDirectory}\".");
    return 0;
}

Console.WriteLine();
Console.WriteLine($"Regressão visual aprovada: {captures} capturas dentro da tolerância.");
return 0;

async Task CaptureAndCompareAsync(
    string pageName,
    VisualViewport viewport,
    IPage page)
{
    var fileName = $"{pageName}-{viewport.Name}.png";
    var actualPath = Path.Combine(artifactDirectory, fileName);
    var baselinePath = Path.Combine(baselineDirectory, fileName);

    await page.ScreenshotAsync(new PageScreenshotOptions
    {
        Path = actualPath,
        FullPage = false,
        Animations = ScreenshotAnimations.Disabled
    });

    captures++;

    if (options.UpdateBaseline)
    {
        await CopyFileWithRetryAsync(actualPath, baselinePath);
        Console.WriteLine($"[baseline] {fileName}");
        return;
    }

    if (!File.Exists(baselinePath))
    {
        failures.Add($"{fileName}: baseline ausente");
        Console.WriteLine($"[falha] {fileName}: baseline ausente");
        return;
    }

    var comparison = await CompareImagesAsync(
        page,
        baselinePath,
        actualPath,
        Path.Combine(artifactDirectory, $"diff-{fileName}"),
        options.ChannelDeltaThreshold);

    var accepted = comparison.SameSize &&
                   comparison.ChangedRatio <= options.MaxChangedRatio;
    var status = accepted ? "ok" : "falha";
    Console.WriteLine(
        $"[{status}] {fileName}: {comparison.ChangedRatio:P4} dos pixels alterados");

    if (!accepted)
    {
        failures.Add(
            comparison.SameSize
                ? $"{fileName}: {comparison.ChangedRatio:P4} dos pixels alterados"
                : $"{fileName}: dimensão {comparison.ExpectedSize} esperada, " +
                  $"{comparison.ActualSize} encontrada");
    }
}

async Task NavigateAndSettleAsync(IPage page, string url)
{
    var response = await page.GotoAsync(
        url,
        new PageGotoOptions
        {
            WaitUntil = WaitUntilState.Load,
            Timeout = 30_000
        });

    if (response is null || !response.Ok)
    {
        throw new InvalidOperationException(
            $"A rota visual não respondeu com sucesso: {url} " +
            $"({response?.Status.ToString(CultureInfo.InvariantCulture) ?? "sem resposta"}).");
    }

    await page.WaitForTimeoutAsync(900);
    await SettleAsync(page);
}

async Task SettleAsync(IPage page)
{
    await page.AddStyleTagAsync(new PageAddStyleTagOptions
    {
        Content = StabilizationCss
    });
    await page.EvaluateAsync(
        """
        async () => {
          await document.fonts.ready;
          document.querySelectorAll(".reveal").forEach(element => {
            element.classList.add("is-visible");
          });
          window.scrollTo(0, 0);
        }
        """);
    await page.WaitForTimeoutAsync(50);
}

async Task ValidateStudentPageAsync(
    string pageName,
    VisualViewport viewport,
    IPage page)
{
    var measurements = await page.EvaluateAsync<StudentPageMeasurements>(
        """
        () => ({
          headingCount: document.querySelectorAll("h1").length,
          horizontalOverflow: Math.max(
            document.documentElement.scrollWidth,
            document.body.scrollWidth
          ) - window.innerWidth
        })
        """);

    if (measurements.HeadingCount != 1)
    {
        failures.Add(
            $"{pageName}-{viewport.Name}: esperado um h1, " +
            $"encontrados {measurements.HeadingCount}");
    }

    if (measurements.HorizontalOverflow > 1)
    {
        failures.Add(
            $"{pageName}-{viewport.Name}: overflow horizontal de " +
            $"{measurements.HorizontalOverflow}px");
    }
}

async Task ValidateMobileDrawerAsync(VisualViewport viewport, IPage page)
{
    var toggle = page.Locator("[data-aluno-menu-toggle]");
    await toggle.ClickAsync();

    if (!string.Equals(
            await toggle.GetAttributeAsync("aria-expanded"),
            "true",
            StringComparison.Ordinal))
    {
        failures.Add($"aluno-dashboard-{viewport.Name}: gaveta não abriu");
    }

    await page.Keyboard.PressAsync("Escape");
    var drawerClosed = string.Equals(
        await toggle.GetAttributeAsync("aria-expanded"),
        "false",
        StringComparison.Ordinal);
    var focusReturned = await toggle.EvaluateAsync<bool>(
        "element => document.activeElement === element");

    if (!drawerClosed || !focusReturned)
    {
        failures.Add(
            $"aluno-dashboard-{viewport.Name}: Escape não fechou a gaveta " +
            "com retorno de foco");
    }
}

static async Task<VisualComparison> CompareImagesAsync(
    IPage page,
    string expectedPath,
    string actualPath,
    string diffPath,
    byte channelDeltaThreshold)
{
    var expectedDataUrl =
        $"data:image/png;base64,{Convert.ToBase64String(await File.ReadAllBytesAsync(expectedPath))}";
    var actualDataUrl =
        $"data:image/png;base64,{Convert.ToBase64String(await File.ReadAllBytesAsync(actualPath))}";

    var comparison = await page.EvaluateAsync<VisualComparison>(
        """
        async ({ expectedDataUrl, actualDataUrl, channelDeltaThreshold }) => {
          const loadImage = source => new Promise((resolve, reject) => {
            const image = new Image();
            image.onload = () => resolve(image);
            image.onerror = reject;
            image.src = source;
          });

          const [expected, actual] = await Promise.all([
            loadImage(expectedDataUrl),
            loadImage(actualDataUrl)
          ]);
          const expectedSize = `${expected.width}x${expected.height}`;
          const actualSize = `${actual.width}x${actual.height}`;

          if (expected.width !== actual.width || expected.height !== actual.height) {
            return {
              sameSize: false,
              changedRatio: 1,
              expectedSize,
              actualSize,
              diffDataUrl: null
            };
          }

          const sourceCanvas = document.createElement("canvas");
          sourceCanvas.width = expected.width;
          sourceCanvas.height = expected.height;
          const sourceContext = sourceCanvas.getContext("2d", {
            willReadFrequently: true
          });

          sourceContext.drawImage(expected, 0, 0);
          const expectedPixels = sourceContext.getImageData(
            0,
            0,
            expected.width,
            expected.height
          ).data;
          sourceContext.clearRect(0, 0, sourceCanvas.width, sourceCanvas.height);
          sourceContext.drawImage(actual, 0, 0);
          const actualPixels = sourceContext.getImageData(
            0,
            0,
            actual.width,
            actual.height
          ).data;

          const diffCanvas = document.createElement("canvas");
          diffCanvas.width = expected.width;
          diffCanvas.height = expected.height;
          const diffContext = diffCanvas.getContext("2d");
          const diffImage = diffContext.createImageData(
            expected.width,
            expected.height
          );
          let changedPixels = 0;

          for (let index = 0; index < expectedPixels.length; index += 4) {
            const delta = Math.max(
              Math.abs(expectedPixels[index] - actualPixels[index]),
              Math.abs(expectedPixels[index + 1] - actualPixels[index + 1]),
              Math.abs(expectedPixels[index + 2] - actualPixels[index + 2]),
              Math.abs(expectedPixels[index + 3] - actualPixels[index + 3])
            );
            const changed = delta > channelDeltaThreshold;

            if (changed) {
              changedPixels++;
              diffImage.data[index] = 239;
              diffImage.data[index + 1] = 56;
              diffImage.data[index + 2] = 64;
            } else {
              diffImage.data[index] = Math.floor(actualPixels[index] / 3);
              diffImage.data[index + 1] = Math.floor(actualPixels[index + 1] / 3);
              diffImage.data[index + 2] = Math.floor(actualPixels[index + 2] / 3);
            }

            diffImage.data[index + 3] = 255;
          }

          const changedRatio = changedPixels / (expected.width * expected.height);
          let diffDataUrl = null;
          if (changedPixels > 0) {
            diffContext.putImageData(diffImage, 0, 0);
            diffDataUrl = diffCanvas.toDataURL("image/png");
          }

          return {
            sameSize: true,
            changedRatio,
            expectedSize,
            actualSize,
            diffDataUrl
          };
        }
        """,
        new
        {
            expectedDataUrl,
            actualDataUrl,
            channelDeltaThreshold
        });

    if (!string.IsNullOrWhiteSpace(comparison.DiffDataUrl))
    {
        var separatorIndex = comparison.DiffDataUrl.IndexOf(',');
        var base64 = comparison.DiffDataUrl[(separatorIndex + 1)..];
        await File.WriteAllBytesAsync(diffPath, Convert.FromBase64String(base64));
    }

    return comparison;
}

static string BuildUrl(string baseUrl, string route)
{
    return $"{baseUrl.TrimEnd('/')}/{route.TrimStart('/')}";
}

static async Task CopyFileWithRetryAsync(string sourcePath, string destinationPath)
{
    IOException? lastException = null;

    for (var attempt = 1; attempt <= 6; attempt++)
    {
        try
        {
            File.Copy(sourcePath, destinationPath, overwrite: true);
            return;
        }
        catch (IOException exception)
        {
            lastException = exception;
            if (attempt < 6)
            {
                await Task.Delay(attempt * 250);
            }
        }
    }

    throw new IOException(
        $"Não foi possível atualizar o baseline visual '{destinationPath}'.",
        lastException);
}

static string FindRepoRoot()
{
    foreach (var startDirectory in new[]
             {
                 Directory.GetCurrentDirectory(),
                 AppContext.BaseDirectory
             })
    {
        var directory = new DirectoryInfo(startDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "IkkonAdmin.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }
    }

    throw new DirectoryNotFoundException(
        "Não foi possível localizar a raiz do repositório (IkkonAdmin.slnx).");
}

static void RecreateDirectory(string path)
{
    if (Directory.Exists(path))
    {
        Directory.Delete(path, recursive: true);
    }

    Directory.CreateDirectory(path);
}

internal sealed record VisualRoute(string Name, string Path);

internal sealed record VisualViewport(string Name, int Width, int Height);

internal sealed record VisualComparison
{
    [JsonPropertyName("sameSize")]
    public bool SameSize { get; init; }

    [JsonPropertyName("changedRatio")]
    public double ChangedRatio { get; init; }

    [JsonPropertyName("expectedSize")]
    public string ExpectedSize { get; init; } = string.Empty;

    [JsonPropertyName("actualSize")]
    public string ActualSize { get; init; } = string.Empty;

    [JsonPropertyName("diffDataUrl")]
    public string? DiffDataUrl { get; init; }
}

internal sealed record StudentPageMeasurements
{
    [JsonPropertyName("headingCount")]
    public int HeadingCount { get; init; }

    [JsonPropertyName("horizontalOverflow")]
    public int HorizontalOverflow { get; init; }
}

internal sealed record RunnerOptions(
    bool UpdateBaseline,
    string BaseUrl,
    string? BaselineDirectory,
    string? BrowserChannel,
    double MaxChangedRatio,
    byte ChannelDeltaThreshold)
{
    public static RunnerOptions Parse(string[] arguments)
    {
        var updateBaseline = false;
        var baseUrl = "http://localhost:5037";
        string? baselineDirectory = null;
        string? browserChannel = null;
        var maxChangedRatio = 0.001;
        byte channelDeltaThreshold = 8;

        for (var index = 0; index < arguments.Length; index++)
        {
            switch (arguments[index])
            {
                case "update":
                case "--update":
                    updateBaseline = true;
                    break;
                case "compare":
                    updateBaseline = false;
                    break;
                case "--base-url":
                    baseUrl = ReadValue(arguments, ref index, "--base-url");
                    break;
                case "--baseline-dir":
                    baselineDirectory = ReadValue(arguments, ref index, "--baseline-dir");
                    break;
                case "--channel":
                    browserChannel = ReadValue(arguments, ref index, "--channel");
                    break;
                case "--max-changed-ratio":
                    maxChangedRatio = double.Parse(
                        ReadValue(arguments, ref index, "--max-changed-ratio"),
                        CultureInfo.InvariantCulture);
                    break;
                case "--channel-delta":
                    channelDeltaThreshold = byte.Parse(
                        ReadValue(arguments, ref index, "--channel-delta"),
                        CultureInfo.InvariantCulture);
                    break;
                default:
                    throw new ArgumentException(
                        $"Argumento visual desconhecido: {arguments[index]}");
            }
        }

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
        {
            throw new ArgumentException($"URL base inválida: {baseUrl}");
        }

        if (maxChangedRatio is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxChangedRatio),
                "A tolerância deve estar entre 0 e 1.");
        }

        return new RunnerOptions(
            updateBaseline,
            baseUrl,
            baselineDirectory,
            browserChannel,
            maxChangedRatio,
            channelDeltaThreshold);
    }

    private static string ReadValue(
        IReadOnlyList<string> arguments,
        ref int index,
        string option)
    {
        index++;
        if (index >= arguments.Count || string.IsNullOrWhiteSpace(arguments[index]))
        {
            throw new ArgumentException($"A opção {option} exige um valor.");
        }

        return arguments[index];
    }
}
