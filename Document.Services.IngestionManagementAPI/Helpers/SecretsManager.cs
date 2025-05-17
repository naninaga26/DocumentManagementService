using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon;
using System.Text.Json;

public static class SecretsManagerHelper
{
    public static async Task<string> GetConnectionStringAsync(string secretName, RegionEndpoint region)
    {
        try
        {
            using var client = new AmazonSecretsManagerClient(region);

            var request = new GetSecretValueRequest
            {
                SecretId = secretName
            };

            var response = await client.GetSecretValueAsync(request);

            var secretJson = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);

            return secretJson["ConnectionString"];

        }
        catch (Exception ex)
        {
            // Handle exceptions as needed
            Console.WriteLine($"Error retrieving secret: {ex.Message}");
            throw ex;
        }
    }
}