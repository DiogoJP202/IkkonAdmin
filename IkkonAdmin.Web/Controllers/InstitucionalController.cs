using IkkonAdmin.Web.Helpers;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[AllowAnonymous]
public class InstitucionalController(IViewTextService i18n) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        SetPageSeo(
            i18n[
                "IKKON SPTD | Escola de Taiko em São Paulo",
                "IKKON SPTD | Taiko School in Sao Paulo",
                "IKKON SPTD | サンパウロの和太鼓教室"],
            i18n[
                "Escola de taiko em São Paulo com aulas de percussão japonesa e grupo artístico para shows, festivais e eventos. Conheça o IKKON SPTD.",
                "Taiko school in Sao Paulo offering Japanese percussion classes and a performance group for shows, festivals, and events. Discover IKKON SPTD.",
                "サンパウロの和太鼓教室IKKON SPTD。初心者向けレッスンから、祭りやイベントでの和太鼓公演まで行っています。"],
            "home",
            "/");

        return View();
    }

    [HttpGet]
    public IActionResult Escola()
    {
        SetPageSeo(
            i18n[
                "Escola de Taiko em São Paulo | IKKON SPTD",
                "Taiko School in Sao Paulo | IKKON SPTD",
                "サンパウロの和太鼓教室 | IKKON SPTD"],
            i18n[
                "Aulas de taiko em São Paulo para iniciantes e alunos em evolução, com prática em grupo, ritmo, técnica, postura e cultura japonesa.",
                "Taiko classes in Sao Paulo for beginners and developing students, with group practice, rhythm, technique, posture, and Japanese culture.",
                "サンパウロで初心者から経験者まで学べる和太鼓教室。リズム、技術、姿勢、合奏、日本文化を段階的に学びます。"],
            "escola",
            "/escola",
            i18n["Escola", "School", "教室"]);

        var faqItems = PublicContentCatalog.StudentFaq(i18n);
        GetStructuredData().Add(PublicSeoHelper.FaqPage(
            faqItems.Select(item => (item.Question, item.Answer))));
        GetStructuredData().Add(PublicSeoHelper.Courses(
            Request,
            PublicSiteLocales.AbsoluteUrl(Request, i18n.LocalizePath("/escola")),
        [
            (
                "Taiko",
                i18n[
                    "Técnica, postura, precisão rítmica, escuta coletiva e presença.",
                    "Technique, posture, rhythmic precision, ensemble listening, and presence.",
                    "打ち方、姿勢、リズムの正確さ、仲間の音を聴く力、表現を学びます。"]),
            (
                "Fue",
                i18n[
                    "Respiração, afinação, fraseado e controle sonoro no repertório japonês.",
                    "Breath, tuning, phrasing, and sound control in Japanese repertoire.",
                    "日本のレパートリーを通じて、呼吸、音程、フレーズ、音色を学びます。"]),
            (
                i18n["Teoria Musical", "Music Theory", "音楽理論"],
                i18n[
                    "Leitura rítmica, estrutura musical e compreensão de arranjos.",
                    "Rhythmic reading, musical structure, and understanding arrangements.",
                    "リズムの読み方、音楽構造、アレンジの理解を学びます。"])
        ]));

        return View();
    }

    [HttpGet]
    public IActionResult Eventos()
    {
        SetPageSeo(
            i18n[
                "Apresentações de Taiko para Eventos | IKKON SPTD",
                "Taiko Performances for Events | IKKON SPTD",
                "イベント向け和太鼓公演 | IKKON SPTD"],
            i18n[
                "Apresentações de taiko para eventos culturais, festivais, empresas, escolas e celebrações em São Paulo. Solicite uma proposta ao IKKON.",
                "Taiko performances for cultural events, festivals, companies, schools, and celebrations in Sao Paulo. Request an IKKON proposal.",
                "サンパウロを中心に、文化イベント、祭り、企業、学校、式典向けの和太鼓公演を行います。IKKONへご相談ください。"],
            "eventos",
            "/eventos",
            i18n["Eventos", "Events", "イベント"]);

        GetStructuredData().Add(PublicSeoHelper.MusicGroup(
            Request,
            PublicSiteLocales.AbsoluteUrl(Request, i18n.LocalizePath("/eventos"))));

        return View();
    }

    [HttpGet]
    public IActionResult Sobre()
    {
        SetPageSeo(
            i18n[
                "Sobre o IKKON São Paulo Taiko Dojo",
                "About IKKON Sao Paulo Taiko Dojo",
                "IKKONサンパウロ太鼓道場について"],
            i18n[
                "Conheça o IKKON São Paulo Taiko Dojo, escola e grupo artístico dedicado ao ensino, à prática coletiva e às apresentações de taiko desde 2015.",
                "Meet IKKON Sao Paulo Taiko Dojo, a school and performance group dedicated to taiko education, ensemble practice, and performances since 2015.",
                "2015年から和太鼓の指導、合奏、公演に取り組む、IKKONサンパウロ太鼓道場についてご紹介します。"],
            "sobre",
            "/sobre",
            i18n["Sobre o IKKON", "About IKKON", "IKKONについて"],
            "AboutPage");

        return View();
    }

    [HttpGet]
    public IActionResult Taiko()
    {
        SetPageSeo(
            i18n[
                "O que é Taiko? História, prática e cultura | IKKON",
                "What Is Taiko? Practice and Culture | IKKON",
                "和太鼓とは？文化と演奏 | IKKON"],
            i18n[
                "Entenda o que é taiko, como funciona a prática em grupo e como os tambores japoneses conectam música, movimento, disciplina e cultura.",
                "Learn what taiko is, how ensemble practice works, and how Japanese drums connect music, movement, discipline, and culture.",
                "和太鼓とは何か、合奏の魅力、音楽・身体表現・規律・日本文化とのつながりをわかりやすく紹介します。"],
            "taiko",
            "/taiko",
            i18n["O que é taiko", "What is taiko", "和太鼓とは"]);

        return View();
    }

    [HttpGet]
    public IActionResult Contato()
    {
        SetPageSeo(
            i18n[
                "Contato e Localização | IKKON SPTD",
                "Contact and Location | IKKON SPTD",
                "お問い合わせ・所在地 | IKKON SPTD"],
            i18n[
                "Fale com o IKKON SPTD sobre aulas de taiko, apresentações e projetos culturais. Rua Domingos de Morais, 2975, São Paulo.",
                "Contact IKKON SPTD about taiko classes, performances, and cultural projects. Rua Domingos de Morais, 2975, Sao Paulo.",
                "和太鼓レッスン、公演、文化プロジェクトについてIKKON SPTDへお問い合わせください。サンパウロ、Rua Domingos de Morais 2975。"],
            "contato",
            "/contato",
            i18n["Contato", "Contact", "お問い合わせ"],
            "ContactPage");

        return View();
    }

    private void SetPageSeo(
        string title,
        string description,
        string publicSection,
        string publicPath,
        string? breadcrumbLabel = null,
        string schemaType = "WebPage")
    {
        var canonicalPath = i18n.LocalizePath(publicPath);
        var canonicalUrl = PublicSiteLocales.AbsoluteUrl(Request, canonicalPath);
        var locale = PublicSiteLocales.ForCulture(i18n.CurrentCulture);

        ViewData["Title"] = title;
        ViewData["Description"] = description;
        ViewData["CanonicalPath"] = canonicalPath;
        ViewData["CanonicalUrl"] = canonicalUrl;
        ViewData["PublicSection"] = publicSection;
        ViewData["JapanesePublicEnabled"] = true;
        ViewData["IncludeSiteIdentitySchema"] = publicPath is "/" or "/sobre";
        ViewData["StructuredData"] = new List<string>
        {
            PublicSeoHelper.WebPage(
                Request,
                canonicalUrl,
                title,
                description,
                locale.Hreflang,
                schemaType)
        };

        if (string.IsNullOrWhiteSpace(breadcrumbLabel))
        {
            return;
        }

        var homeLabel = i18n["Início", "Home", "ホーム"];
        var homeUrl = PublicSiteLocales.AbsoluteUrl(Request, i18n.LocalizePath("/"));
        ViewData["Breadcrumbs"] = new List<PublicBreadcrumbItemViewModel>
        {
            new(homeLabel, i18n.LocalizePath("/")),
            new(breadcrumbLabel)
        };
        GetStructuredData().Add(PublicSeoHelper.Breadcrumbs(
        [
            (homeLabel, homeUrl),
            (breadcrumbLabel, canonicalUrl)
        ]));
    }

    private List<string> GetStructuredData() =>
        (List<string>)ViewData["StructuredData"]!;
}
