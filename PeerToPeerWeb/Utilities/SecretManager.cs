using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.SecretManager.v1;
using Google.Cloud.SecretManager.V1;

namespace PeerToPeerWeb.Utilities
{
    public class SecretManager
    {
        private SecretManagerServiceClient client;
        private string _projectId;
        public SecretManager(string projectId)
        {
            client = SecretManagerServiceClient.Create();
            _projectId = projectId;

        }

        public string GetSecret(string secretId, string secretVersionId)
        {
            SecretVersionName secretVersionName = new SecretVersionName(_projectId, secretId, secretVersionId);

            // Call the API.
            AccessSecretVersionResponse result = client.AccessSecretVersion(secretVersionName);

            // Convert the payload to a string. Payloads are bytes by default.
            String payload = result.Payload.Data.ToStringUtf8();
            return payload;
        }
    }
}
