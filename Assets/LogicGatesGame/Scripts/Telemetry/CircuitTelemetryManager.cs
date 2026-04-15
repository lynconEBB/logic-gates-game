namespace LogicGatesGame.Scripts
{
    public class CircuitTelemetryManager : TelemetryManager
    {
        public const string KeyConnections = "connections";
        public const string KeyGates = "gates";

        protected override void RegisterKeys()
        {
            RegisterKey(KeyConnections);
            RegisterKey(KeyGates);
        }
    }
}
