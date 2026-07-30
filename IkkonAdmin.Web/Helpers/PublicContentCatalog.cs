using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Services;

namespace IkkonAdmin.Web.Helpers;

public static class PublicContentCatalog
{
    public static IReadOnlyList<PublicFaqItemViewModel> StudentFaq(IViewTextService i18n) =>
    [
        new(
            i18n[
                "Preciso ser descendente de japoneses para estudar taiko?",
                "Do I need Japanese ancestry to study taiko?",
                "和太鼓を学ぶには日系人である必要がありますか？"],
            i18n[
                "Não. A escola é aberta para todos, sem qualquer restrição de origem. Nosso compromisso é com o respeito à cultura e com o aprendizado acessível.",
                "No. The school is open to everyone, with no restriction on background. Our commitment is to cultural respect and accessible learning.",
                "いいえ。教室は出自に関係なく、どなたにも開かれています。私たちは文化への敬意と、学びやすい指導を大切にしています。"]),
        new(
            i18n[
                "Participar de apresentações públicas é obrigatório?",
                "Is performing in public required?",
                "公開公演への参加は必須ですか？"],
            i18n[
                "Não é obrigatório. Quem deseja participar de eventos e shows recebe preparação, mas o aluno pode focar apenas no processo de aula e desenvolvimento pessoal.",
                "It is not required. Students who want to join events and shows receive preparation, but they can also focus only on classes and personal development.",
                "必須ではありません。イベントやショーに参加したい生徒には準備の機会がありますが、レッスンと自己成長だけに集中することもできます。"]),
        new(
            i18n[
                "Nunca estudei música. Ainda assim posso começar?",
                "I have never studied music. Can I still start?",
                "音楽を学んだことがなくても始められますか？"],
            i18n[
                "Sim. Ensinamos desde o absoluto zero, com progressão técnica clara em ritmo, coordenação, postura e leitura musical básica.",
                "Yes. We teach from the very beginning, with a clear technical progression in rhythm, coordination, posture, and basic music reading.",
                "はい。リズム、コーディネーション、姿勢、基本的な楽譜の読み方まで、まったく初めての方にも段階的に指導します。"]),
        new(
            i18n[
                "Qual é a idade mínima para começar?",
                "What is the minimum age to start?",
                "何歳から始められますか？"],
            i18n[
                "Avaliamos caso a caso, mas em geral por volta dos 10 anos a criança já consegue acompanhar bem a dinâmica das aulas.",
                "We evaluate each case individually, but around age 10 children can usually follow the class dynamics well.",
                "一人ひとり確認しますが、一般的には10歳前後からレッスンの流れについていきやすくなります。"])
    ];
}
