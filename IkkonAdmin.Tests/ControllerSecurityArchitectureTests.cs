using IkkonAdmin.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace IkkonAdmin.Tests;

public class ControllerSecurityArchitectureTests
{
    [Fact]
    public void EndpointsMutaveis_ExigemValidacaoAntiforgery()
    {
        var controllerTypes = typeof(HomeController).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && typeof(Controller).IsAssignableFrom(type));
        var missingProtection = new List<string>();

        foreach (var controllerType in controllerTypes)
        {
            var controllerProtected = controllerType
                .GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true)
                .Length > 0;

            foreach (var method in controllerType.GetMethods()
                         .Where(method => method.IsPublic && method.DeclaringType == controllerType))
            {
                var httpMethods = method
                    .GetCustomAttributes(inherit: true)
                    .OfType<IActionHttpMethodProvider>()
                    .SelectMany(attribute => attribute.HttpMethods)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (!httpMethods.Overlaps(["POST", "PUT", "PATCH", "DELETE"]))
                {
                    continue;
                }

                var methodProtected = method
                    .GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true)
                    .Length > 0;

                if (!controllerProtected && !methodProtected)
                {
                    missingProtection.Add($"{controllerType.Name}.{method.Name}");
                }
            }
        }

        Assert.True(
            missingProtection.Count == 0,
            $"Endpoints mutáveis sem ValidateAntiForgeryToken: {string.Join(", ", missingProtection)}");
    }
}
