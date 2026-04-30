using CaseyHub.API.Evaluators;
using CaseyHub.Models.DTOs.Internal.PermitChecker;

namespace CaseyHub.API.Services;
public interface IPermitEvaluatorService
{
    Task<EvaluationResponseDto> EvaluateAsync(EvaluationContext ctx, Guid? userId = null);
}