namespace ML.GameCommands
{
    public class RespawnPlayer : GameCommandHandler
    {
        public ML.Player.PlayerController player;

        public override void PerformInteraction()
        {
            player.Respawn();
        }
    }
}