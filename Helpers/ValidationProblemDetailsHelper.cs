using light_quiz_api.Dtos;
using Microsoft.AspNetCore.Mvc;
public static class ValidationProblemDetailsHelper
{
    public static BadRequestObjectResult CreateProblemDetailsFromErrorDetails(List<ErrorDetail> errors)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Failed",
            Type = "https://tools.ietf.org/html/rfc7807",
            Detail = "One or more validation errors occurred."
        };

        var errorDictionary = errors.GroupBy(e => e.PropertyName)
        .ToDictionary(
            group => group.Key,
            group => group.Select(e => e.ErrorMessage).ToArray()
        );

        problemDetails.Extensions["errors"] = errorDictionary;

        return new BadRequestObjectResult(problemDetails);
    }
}