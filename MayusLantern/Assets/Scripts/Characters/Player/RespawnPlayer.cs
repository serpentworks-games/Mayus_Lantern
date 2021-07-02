namespace ML.GameCommands
{
    public class RespawnPlayer : GameCommandHandler
    {
        public ML.Characters.Player.PlayerController player;

        public override void PerformInteraction()
        {
            player.Respawn();
        }
    }
}