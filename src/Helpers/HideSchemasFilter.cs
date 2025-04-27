using Swashbuckle.AspNetCore.SwaggerGen;

public class HideSchemasFilter : IDocumentFilter
{
    private const string NamespaceToHide = "light_quiz_api.Dtos"; // Define the namespace here

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var schemasToRemove = new List<string>();

        // Iterate through all defined schemas in the document components
        foreach (var schema in swaggerDoc.Components.Schemas)
        {
            // The schema key is typically the full type name (due to CustomSchemaIds setting)
            // Check if the schema key starts with the specified namespace followed by a dot.
            // Ensure this matches how your CustomSchemaIds generates keys.
            if (schema.Key.StartsWith(NamespaceToHide + "."))
            {
                schemasToRemove.Add(schema.Key);
            }
        }

        // Remove the collected schemas from the Components.Schemas dictionary
        foreach (var schemaKey in schemasToRemove)
        {
            swaggerDoc.Components.Schemas.Remove(schemaKey);
        }
    }
}