namespace IkkonAdmin.Web.Services;

public interface IBlogDateTimeService
{
    DateTime ConvertSaoPauloLocalToUtc(DateTime localDateTime);
    DateTime? ConvertUtcToSaoPauloLocal(DateTime? utcDateTime);
    DateTime ConvertSaoPauloDateOnlyToUtcStart(DateOnly date);
    DateTime ConvertSaoPauloDateOnlyToUtcEndExclusive(DateOnly date);
}
