namespace Etherna.EthernaIndex.ApiApplication.V1.DtoModels
{
    public class SettingsDto
    {
        // Constructors.
        public SettingsDto(
            string defaultSwarmGatewayUrl,
            string version)
        {
            DefaultSwarmGatewayUrl = defaultSwarmGatewayUrl ?? throw new System.ArgumentNullException(nameof(defaultSwarmGatewayUrl));
            Version = version ?? throw new System.ArgumentNullException(nameof(version));
        }

        // Properties.
        public string DefaultSwarmGatewayUrl { get; }
        public string Version { get; }
    }
}
