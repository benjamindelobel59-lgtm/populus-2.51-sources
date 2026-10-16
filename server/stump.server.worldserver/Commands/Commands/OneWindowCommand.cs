using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Commands.Commands.Patterns;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Handlers.Context;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>
    /// One window + master : the other characters of the same IP follow this one map by map, join its fights,
    /// and their turns are played from this client.
    /// </summary>
    public class OneWindowCommand : InGameCommand
    {
        public OneWindowCommand()
        {
            Aliases = new[] { "onewindow", "ow" };
            Description = "Master + one window : vos personnages vous suivent et vous jouez leurs tours depuis cette fenêtre.";
            RequiredRole = RoleEnum.Player;
        }

        public override void Execute(GameTrigger trigger)
        {
            var character = trigger.Character;

            if (character.OneWindow)
            {
                character.OneWindow = false;
                MultiLeader.Disable(character);
                character.SendServerMessage("One Window désactivé : chaque personnage joue depuis sa fenêtre et ne vous suit plus.");
                return;
            }

            // only one master per IP, otherwise two windows would fight for the same turns
            foreach (var other in WorldServer.Instance.FindClients(x => x.IP == character.Client.IP && x.Character != null && x.Character != character))
            {
                if (!other.Character.OneWindow)
                    continue;
                other.Character.OneWindow = false;
                other.Character.SendServerMessage("One Window désactivé : c'est maintenant " + character.Name + " le maître.");
            }

            character.OneWindow = true;
            MultiLeader.Enable(character);
            character.SendServerMessage("One Window activé : vos personnages vous suivent de map en map, rejoignent vos combats et vous jouez leurs tours depuis cette fenêtre.");

            // Correctif Nekfeu : activé en plein combat pendant le tour d'un de ses personnages,
            // le contrôle n'était donné qu'au début du tour suivant → personne ne pouvait passer ce tour.
            if (character.IsFighting() && character.Fighter != null)
            {
                var playing = character.Fight.FighterPlaying as CharacterFighter;
                if (playing != null && playing != character.Fighter && playing.GetOneWindowMaster() == character.Fighter)
                {
                    ContextHandler.SendSlaveSwitchContextMessage(character.Client, character.Fighter, playing);

                    using (character.Fight.StartSequence(SequenceTypeEnum.SEQUENCE_SPELL))
                        ContextHandler.SendOneWindowGlobalCooldowns(character.Client, playing);
                }
            }
        }
    }
}
