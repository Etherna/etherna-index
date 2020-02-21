namespace Etherna.EthernaIndex.ApiApplication.V1.DtoModels
{
    public class SettingsDto
    {
        // Constructors.
        public SettingsDto(string defaultSwarmGatewayUrl)
        {
            DefaultSwarmGatewayUrl = defaultSwarmGatewayUrl;
        }

        // Properties.
        public string DefaultSwarmGatewayUrl { get; }
    }
}
