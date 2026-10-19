using System.Collections.Generic;
using System.Linq;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Network;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Fights.Triggers;
using Stump.Server.WorldServer.Handlers.Context.RolePlay;
using Spell = Stump.Server.WorldServer.Game.Spells.Spell;
using Stump.Core.Extensions;
using Stump.Server.WorldServer.Game.Arena;
using Stump.Server.WorldServer.Game;

namespace Stump.Server.WorldServer.Handlers.Context
{
    public partial class ContextHandler : WorldHandlerContainer
    {
        [WorldHandler(GameActionFightCastRequestMessage.Id)]
        public static void HandleGameActionFightCastRequestMessage(WorldClient client, GameActionFightCastRequestMessage message)
        {
            if (!client.Character.IsFighting())
                return;

            var fighter = client.Character.Fighter;
            if (client.Character.Fight.FighterPlaying is CompanionActor)
            {
                var spell = client.Character.Companion.GetSpell((int)message.SpellId);
                if (spell != null)
                {

                    client.Character.Companion.CastSpell(spell, client.Character.Fight.Map.Cells[message.CellId]);
                }
            }
            else
            {
                var spell = fighter.IsSlaveTurn() ?
                fighter.GetSlave().GetSpell(message.SpellId) : fighter.GetSpell(message.SpellId);

                if (spell == null)
                    return;

                if (fighter.IsSlaveTurn())
                    fighter.GetSlave().CastSpell(spell, client.Character.Fight.Map.Cells[message.CellId]);
                else
                    fighter.CastSpell(spell, client.Character.Fight.Map.Cells[message.CellId]);
            }
        }

        [WorldHandler(GameActionFightCastOnTargetRequestMessage.Id)]
        public static void HandleGameActionFightCastOnTargetRequestMessage(WorldClient client, GameActionFightCastOnTargetRequestMessage message)
        {
            if (!client.Character.IsFighting())
                return;

            var fighter = client.Character.Fighter;

            var spell = fighter.IsSlaveTurn() ?
                fighter.GetSlave().GetSpell(message.SpellId) : fighter.GetSpell(message.SpellId);

            if (spell == null)
                return;

            var target = client.Character.Fight.GetOneFighter((int)message.TargetId);

            if (target == null)
                return;
            if (client.Character.Fight.FighterPlaying is CompanionActor)
            {
                client.Character.Companion.CastSpell(spell, target.Cell);
            }

            if (target.GetVisibleStateFor(fighter) == GameActionFightInvisibilityStateEnum.INVISIBLE)
            {
                //Impossible de lancer ce sort : la cellule vis�e n'est pas valide !
                client.Character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 193);
                return;
            }

            if (fighter.IsSlaveTurn())
                fighter.GetSlave().CastSpell(spell, target.Cell);
            else
                fighter.CastSpell(spell, target.Cell);
        }


        [WorldHandler(GameFightTurnFinishMessage.Id)]
        public static void HandleGameFightTurnFinishMessage(WorldClient client, GameFightTurnFinishMessage message)
        {
            if (!client.Character.IsFighting())
                return;

            // One window : the turn of a controlled character is ended by its master only,
            // its own idle window is flagged AFK by the client and would pass the turn by itself
            var fighter = client.Character.Fighter;
            // Correctif Nekfeu : on ignore seulement le passage AUTOMATIQUE (fenêtre AFK) ;
            // un clic volontaire sur « passer le tour » depuis la fenêtre du perso contrôlé est accepté.
            if (fighter == fighter.Fight.FighterPlaying && fighter.GetOneWindowMaster() != null && message.IsAfk)
                return;
            if (client.Character.Fight.FighterPlaying is CompanionActor)
            {
                client.Character.Companion.PassTurn();
                return;
            }
            if (client.Character.Fighter.IsSlaveTurn())
            {
                client.Character.Fighter.GetSlave().PassTurn();
                return;
            }

            client.Character.Fighter.PassTurn();
        }

        [WorldHandler(GameFightTurnReadyMessage.Id)]
        public static void HandleGameFightTurnReadyMessage(WorldClient client, GameFightTurnReadyMessage message)
        {
            if (!client.Character.IsFighting())
                return;

            client.Character.Fighter.ToggleTurnReady(message.IsReady);
        }

        [WorldHandler(GameFightReadyMessage.Id)]
        public static void HandleGameFightReadyMessage(WorldClient client, GameFightReadyMessage message)
        {
            if (!client.Character.IsFighting())
                return;

            if (client.Character.Fight is FightPvT)
                return;

            client.Character.Fighter.ToggleReady(message.IsReady);
            client.Character.OnReadyStatusChanged(client.Character.Fighter);
        }

        [WorldHandler(GameContextQuitMessage.Id)]
        public static void HandleGameContextQuitMessage(WorldClient client, GameContextQuitMessage message)
        {
            if (client.Character.IsFighting())
                client.Character.Fighter.LeaveFight();

            else if (client.Character.IsSpectator())
                client.Character.Spectator.Leave();
        }

        [WorldHandler(GameFightPlacementPositionRequestMessage.Id)]
        public static void HandleGameFightPlacementPositionRequestMessage(WorldClient client, GameFightPlacementPositionRequestMessage message)
        {
            if (!client.Character.IsFighting())
                return;

            if (client.Character.Fighter.Position.Cell.Id != message.CellId)
                client.Character.Fighter.ChangePrePlacement(client.Character.Fight.Map.Cells[message.CellId]);
        }

        [WorldHandler(GameRolePlayPlayerFightRequestMessage.Id)]
        public static void HandleGameRolePlayPlayerFightRequestMessage(WorldClient client, GameRolePlayPlayerFightRequestMessage message)
        {
            var target = client.Character.Map.GetActor<Character>((int)message.TargetId);

            if (target == null)
                return;

            if (message.Friendly)
            {
                var reason = client.Character.CanRequestFight(target);
                if (reason != FighterRefusedReasonEnum.FIGHTER_ACCEPTED)
                {
                    SendChallengeFightJoinRefusedMessage(client, client.Character, reason);
                }
                else
                {
                    var fightRequest = new FightRequest(client.Character, target);
                    fightRequest.Open();
                }
            }
            else // agression
            {
                // MYSTIC 17/10 : AvA (deux alliances différentes en mode AvA dans une zone vulnérable)
                if (Game.Alliances.AvAManager.Instance.ZoneVulnerable(client.Character.Map) &&
                    Game.Alliances.AvAManager.Instance.EstEnModeAvA(client.Character) && Game.Alliances.AvAManager.Instance.EstEnModeAvA(target))
                {
                    var raisonAvA = Game.Alliances.AvAManager.Instance.PeutAgresser(client.Character, target);
                    if (raisonAvA != FighterRefusedReasonEnum.FIGHTER_ACCEPTED)
                        SendChallengeFightJoinRefusedMessage(client, client.Character, raisonAvA);
                    else
                        Game.Alliances.AvAManager.Instance.Agresser(client.Character, target);
                    return;
                }

                var reason = client.Character.CanAgress(target);
                if (reason != FighterRefusedReasonEnum.FIGHTER_ACCEPTED)
                {
                    SendChallengeFightJoinRefusedMessage(client, client.Character, reason);
                }
                else
                {
                    
                    foreach(var character in target.Map.GetAllCharacters().Where(x => x != target && x != client.Character))
                        ContextRoleplayHandler.SendGameRolePlayAggressionMessage(character.Client, client.Character, target);
                    //<b>%1</b> agresse <b>%2</b>

                    var fight = FightManager.Instance.CreateAgressionFight(target.Map,
                        client.Character.AlignmentSide, target.AlignmentSide);

                    fight.ChallengersTeam.AddFighter(client.Character.CreateFighter(fight.ChallengersTeam));
                    fight.DefendersTeam.AddFighter(target.CreateFighter(fight.DefendersTeam));

                    fight.StartPlacement();
                }
            }
        }

        [WorldHandler(GameRolePlayPlayerFightFriendlyAnswerMessage.Id)]
        public static void HandleGameRolePlayPlayerFightFriendlyAnswerMessage(WorldClient client, GameRolePlayPlayerFightFriendlyAnswerMessage message)
        {
            if (!client.Character.IsInRequest() || !(client.Character.RequestBox is FightRequest))
                return;

            if (message.Accept)
                client.Character.RequestBox.Accept();
            else if (client.Character == client.Character.RequestBox.Target)
                client.Character.RequestBox.Deny();
            else
                client.Character.RequestBox.Cancel();
        }

        [WorldHandler(GameFightOptionToggleMessage.Id)]
        public static void HandleGameFightOptionToggleMessage(WorldClient client, GameFightOptionToggleMessage message)
        {
            if (message.Option == (sbyte)FightOptionsEnum.FIGHT_OPTION_SET_TO_PARTY_ONLY && client.Character.IsPartyLeader())
                client.Character.Party.TogglePartyFightRestriction();

            if (!client.Character.IsFighting())
                return;

            if (!client.Character.Fighter.IsTeamLeader())
                return;

            if (!client.Character.Fight.IsStarted)
                client.Character.Team.ToggleOption((FightOptionsEnum)message.Option);

            if (message.Option == (sbyte)FightOptionsEnum.FIGHT_OPTION_SET_SECRET)
                client.Character.Fight.ToggleSpectatorClosed(client.Character, !client.Character.Fight.SpectatorClosed);
        }

        [WorldHandler(GameFightSpectatePlayerRequestMessage.Id)]
        public static void HandleGameFightSpectatePlayerRequestMessage(WorldClient client, GameFightSpectatePlayerRequestMessage message)
        {
            if (client.Character.IsFighting())
                return;

            var player = World.Instance.GetCharacter((int)message.PlayerId);

            if (player == null)
                return;

            if (!client.Character.FriendsBook.IsFriend(player.Account.Id) && !client.Character.Guild.Members.Any(x => x.Character?.Id == player.Id))
                return;

            if (!player.IsFighting())
            {
                SendChallengeFightJoinRefusedMessage(client, client.Character, FighterRefusedReasonEnum.TOO_LATE);
                return;
            }

            var fight = FightManager.Instance.GetFight(player.Fight.Id);

            if (fight == null)
            {
                SendChallengeFightJoinRefusedMessage(client, client.Character, FighterRefusedReasonEnum.TOO_LATE);
                return;
            }

            if (!fight.IsStarted)
                return;

            if (!fight.CanSpectatorJoin(client.Character) || client.Character.IsInFight())
                return;

            ContextRoleplayHandler.SendCurrentMapMessage(client, fight.Map.Id);
            fight.AddSpectator(client.Character.CreateSpectator(fight));
        }

        [WorldHandler(GameFightJoinRequestMessage.Id)]
        public static void HandleGameFightJoinRequestMessage(WorldClient client, GameFightJoinRequestMessage message)
        {
            if (client.Character.IsFighting())
                return;

            var fight = FightManager.Instance.GetFight(message.FightId);

            if (fight == null)
            {
                SendChallengeFightJoinRefusedMessage(client, client.Character, FighterRefusedReasonEnum.TOO_LATE);
                return;
            }

            if (client.Character.Map.Id != ArenaManager.KolizeumMapId && fight.Map != client.Character.Map)
            {
                SendChallengeFightJoinRefusedMessage(client, client.Character, FighterRefusedReasonEnum.WRONG_MAP);
                return;
            }

            if (message.FighterId == 0 && fight.CanSpectatorJoin(client.Character) && !client.Character.IsInFight())
            {
                if (client.Character.Map.Id == ArenaManager.KolizeumMapId)
                    ContextRoleplayHandler.SendCurrentMapMessage(client, fight.Map.Id);

                fight.AddSpectator(client.Character.CreateSpectator(fight));
            }

            if (fight.IsStarted)
                return;

            FightTeam team;
            if (fight.ChallengersTeam.Leader.Id == message.FighterId)
                team = fight.ChallengersTeam;
            else if (fight.DefendersTeam.Leader.Id == message.FighterId)
                team = fight.DefendersTeam;
            else
            {
                SendChallengeFightJoinRefusedMessage(client, client.Character, FighterRefusedReasonEnum.WRONG_MAP);
                return;
            }

            FighterRefusedReasonEnum error;
            if ((error = team.CanJoin(client.Character)) != FighterRefusedReasonEnum.FIGHTER_ACCEPTED)
                SendChallengeFightJoinRefusedMessage(client, client.Character, error);
            else
                team.AddFighter(client.Character.CreateFighter(team));
        }

        [WorldHandler(GameContextKickMessage.Id)]
        public static void HandleGameContextKickMessage(WorldClient client, GameContextKickMessage message)
        {
            if (!client.Character.IsFighting() ||
                !client.Character.Fighter.IsTeamLeader())
                return;

            if (!client.Character.Fight.CanKickPlayer)
                return;

            var target = client.Character.Fight.GetOneFighter<CharacterFighter>((int)message.TargetId);

            if (target == null)
                return;

            if (!target.Character.IsFighting())
                return;

            if (client.Character.Fight != target.Character.Fight)
                return;

            client.Character.Fight.KickFighter(client.Character.Fighter, target);
        }

        public static void SendGameFightStartMessage(IPacketReceiver client, IEnumerable<Idol> idols)
        {
            client.Send(new GameFightStartMessage(idols.ToArray()));
        }

        public static void SendGameFightStartingMessage(IPacketReceiver client, FightTypeEnum
        fightTypeEnum, int attackerId, int defenderId, IFight figth)
        {
            client.Send(new GameFightStartingMessage((sbyte)fightTypeEnum, (ushort)figth.Id,
            attackerId,
            defenderId));
        }

        public static void SendGameRolePlayShowChallengeMessage(IPacketReceiver client, IFight fight)
        {
            client.Send(new GameRolePlayShowChallengeMessage(fight.GetFightCommonInformations()));
        }

        public static void SendGameRolePlayRemoveChallengeMessage(IPacketReceiver client, IFight fight)
        {
            client.Send(new GameRolePlayRemoveChallengeMessage((ushort)fight.Id));
        }

        public static void SendGameFightEndMessage(IPacketReceiver client, IFight fight)
        {
            client.Send(new GameFightEndMessage((int)fight.GetFightDuration().TotalMilliseconds, fight.AgeBonus, 0, new FightResultListEntry[0], fight.GetPartiesNameWithOutcome().ToArray()));
        }

        public static void SendGameFightEndMessage(IPacketReceiver client, IFight fight, IEnumerable<FightResultListEntry> results)
        {
            client.Send(new GameFightEndMessage((int)fight.GetFightDuration().TotalMilliseconds, fight.AgeBonus, 0, results.ToArray(), fight.GetPartiesNameWithOutcome().ToArray()));
        }

        public static void SendGameFightJoinMessage(IPacketReceiver client, bool canBeCancelled, bool canSayReady,
            bool isFightStarted, int timeMaxBeforeFightStart, FightTypeEnum fightTypeEnum)
        {
            client.Send(new GameFightJoinMessage(!isFightStarted, canBeCancelled, canSayReady, isFightStarted,
                                                 (short)timeMaxBeforeFightStart, (sbyte)fightTypeEnum));
        }

        public static void SendGameFightSpectatorJoinMessage(IPacketReceiver client, IFight fight)
        {
            client.Send(new GameFightSpectatorJoinMessage(false, false, false, fight.IsStarted,
                fight.IsStarted ? (short)0 : (short)(fight.GetPlacementTimeLeft().TotalMilliseconds / 100), (sbyte)fight.FightType, fight.GetPartiesName().ToArray()));
        }

        public static void SendGameFightSpectateMessage(IPacketReceiver client, IFight fight)
        {
            client.Send(new GameFightSpectateMessage(
                fight.GetBuffs().Select(entry => entry.GetFightDispellableEffectExtendedInformations()).ToArray(),
                fight.GetTriggers().Select(entry => entry.GetHiddenGameActionMark()).ToArray(),
                (ushort)fight.TimeLine.RoundNumber, !fight.IsStarted ? 0 : fight.StartTime.GetUnixTimeStamp(), fight.ActiveIdols.Select(x => x.GetNetworkIdol()).ToArray()));
        }

        public static void SendGameFightTurnResumeMessage(IPacketReceiver client, FightActor fighterPlaying)
        {
            client.Send(new GameFightTurnResumeMessage(fighterPlaying.Id, (uint)fighterPlaying.TurnTime / 100, (uint)fighterPlaying.Fight.GetTurnTimeLeft().TotalMilliseconds / 100));
        }

        public static void SendChallengeFightJoinRefusedMessage(IPacketReceiver client, Character character,
                                                                FighterRefusedReasonEnum reason)
        {
            client.Send(new ChallengeFightJoinRefusedMessage((uint)character.Id, (sbyte)reason));
        }

        public static void SendGameFightHumanReadyStateMessage(IPacketReceiver client, FightActor fighter)
        {
            client.Send(new GameFightHumanReadyStateMessage((uint)fighter.Id, fighter.IsReady));
        }


        public static void SendGameFightSynchronizeMessage(WorldClient client, IFight fight)
        {
            client.Send(new GameFightSynchronizeMessage(
                    fight.GetAllFighters().Select(entry => entry.GetGameFightFighterInformations(client)).ToArray()));
        }

        public static void SendFighterStatsListMessage(IPacketReceiver client, Character character)
        {
            client.Send(new FighterStatsListMessage(character.GetCharacterCharacteristicsInformations()));
        }

        public static void SendGameFightNewRoundMessage(IPacketReceiver client, int roundNumber)
        {
            client.Send(new GameFightNewRoundMessage((uint)roundNumber));
        }

        public static void SendGameFightTurnListMessage(IPacketReceiver client, IFight fight)
        {
            client.Send(new GameFightTurnListMessage(fight.GetAliveFightersIds().Select(x => (double)x).ToArray(), fight.GetDeadFightersIds().Select(x => (double)x).ToArray()));
        }

        public static void SendGameFightTurnStartMessage(IPacketReceiver client, int id, int waitTime)
        {
            client.Send(new GameFightTurnStartMessage(id, (uint)waitTime));
        }

        public static void SendGameFightTurnStartPlayingMessage(IPacketReceiver client)
        {
            client.Send(new GameFightTurnStartPlayingMessage());
        }

        public static void SendGameFightTurnFinishMessage(IPacketReceiver client)
        {
            client.Send(new GameFightTurnFinishMessage());
        }

        public static void SendGameFightTurnEndMessage(IPacketReceiver client, FightActor fighter)
        {
            client.Send(new GameFightTurnEndMessage(fighter.Id));
        }

        public static void SendGameFightUpdateTeamMessage(IPacketReceiver client, IFight fight, FightTeam team)
        {
            client.Send(new GameFightUpdateTeamMessage(
                            (ushort)fight.Id,
                            team.GetFightTeamInformations()));
        }

        public static void SendGameFightShowFighterMessage(WorldClient client, FightActor fighter)
        {
            client.Send(new GameFightShowFighterMessage(fighter.GetGameFightFighterInformations(client)));
        }

        public static void SendGameFightRefreshFighterMessage(WorldClient client, FightActor fighter)
        {
            client.Send(new GameFightRefreshFighterMessage(fighter.GetGameFightFighterInformations(client)));
        }

        public static void SendGameFightRemoveTeamMemberMessage(IPacketReceiver client, FightActor fighter)
        {
            client.Send(new GameFightRemoveTeamMemberMessage((ushort)fighter.Fight.Id, (sbyte)fighter.Team.Id, fighter.Id));
        }

        public static void SendGameFightLeaveMessage(IPacketReceiver client, FightActor fighter)
        {
            client.Send(new GameFightLeaveMessage(fighter.Id));
        }

        public static void SendGameFightLeaveMessage(IPacketReceiver client, FightSpectator spectator)
        {
            client.Send(new GameFightLeaveMessage(spectator.Character.Id));
        }

        public static void SendGameFightPlacementPossiblePositionsMessage(IPacketReceiver client, IFight fight, sbyte team)
        {
            client.Send(new GameFightPlacementPossiblePositionsMessage(
                            fight.ChallengersTeam.PlacementCells.Select(entry => (ushort)entry.Id).ToArray(),
                            fight.DefendersTeam.PlacementCells.Select(entry => (ushort)entry.Id).ToArray(),
                            team));
        }

        public static void SendGameFightOptionStateUpdateMessage(IPacketReceiver client, FightTeam team, FightOptionsEnum option, bool state)
        {
            client.Send(new GameFightOptionStateUpdateMessage((ushort)team.Fight.Id, (sbyte)team.Id, (sbyte)option, state));
        }

        public static void SendGameActionFightSpellCastMessage(IPacketReceiver client, ActionsEnum actionId, FightActor caster, FightActor target,
                                                               Cell cell, FightSpellCastCriticalEnum critical, bool silentCast,
                                                               Spell spell, short[] portals)
        {
            client.Send(new GameActionFightSpellCastMessage((ushort)actionId, caster.Id, silentCast, true, target == null ? 0 : target.Id, silentCast ? (short)-1 : cell.Id, (sbyte)(critical),
                                                             (ushort)spell.Id, (sbyte)spell.CurrentLevel, portals
                                                             ));
        }

        public static void SendGameActionFightSpellCastMessage(IPacketReceiver client, ActionsEnum actionId, FightActor caster, FightActor target,
                                                       Cell cell, FightSpellCastCriticalEnum critical, bool silentCast,
                                                       short spellId, sbyte spellLevel, short[] portals)
        {
            client.Send(new GameActionFightSpellCastMessage((ushort)actionId, caster.Id, silentCast, false, target == null ? 0 : target.Id, cell.Id, (sbyte)(critical),
                                                            (ushort)spellId, spellLevel, portals));
        }

        public static void SendGameActionFightNoSpellCastMessage(IPacketReceiver client, Spell spell)
        {
            client.Send(new GameActionFightNoSpellCastMessage(spell.Id == 0 ? 0 : (uint)spell.CurrentSpellLevel.Id));
        }

        public static void SendGameActionFightModifyEffectsDurationMessage(IPacketReceiver client, short actionId, FightActor source, FightActor target, short delta)
        {
            client.Send(new GameActionFightModifyEffectsDurationMessage((ushort)actionId, source.Id, target.Id, delta));
        }

        public static void SendGameActionFightDispellableEffectMessage(IPacketReceiver client, Buff buff, bool update = false)
        {
            client.Send(new GameActionFightDispellableEffectMessage(update ? (ushort)ActionsEnum.ACTION_CHARACTER_UPDATE_BOOST : (ushort)buff.GetActionId(), buff.Caster.Id, buff.GetAbstractFightDispellableEffect()));
        }

        public static void SendGameActionFightMarkCellsMessage(IPacketReceiver client, MarkTrigger trigger, bool visible = true)
        {
            var action = ActionsEnum.ACTION_FIGHT_ADD_GLYPH_CASTING_SPELL;

            switch (trigger.Type)
            {
                case GameActionMarkTypeEnum.WALL:
                case GameActionMarkTypeEnum.GLYPH:
                    action = trigger.TriggerType == TriggerType.OnTurnEnd ?
                        ActionsEnum.ACTION_FIGHT_ADD_GLYPH_CASTING_SPELL_ENDTURN :
                        ActionsEnum.ACTION_FIGHT_ADD_GLYPH_CASTING_SPELL;
                    break;
                case GameActionMarkTypeEnum.TRAP:
                    action = ActionsEnum.ACTION_FIGHT_ADD_TRAP_CASTING_SPELL;
                    break;
            }

            client.Send(new GameActionFightMarkCellsMessage((ushort)action, trigger.Caster.Id, visible ? trigger.GetGameActionMark() : trigger.GetHiddenGameActionMark()));
        }

        public static void SendGameActionFightUnmarkCellsMessage(IPacketReceiver client, MarkTrigger trigger)
        {
            client.Send(new GameActionFightUnmarkCellsMessage(310, trigger.Caster.Id, trigger.Id));
        }

        public static void SendGameActionFightTriggerGlyphTrapMessage(IPacketReceiver client, MarkTrigger trigger, FightActor target, Spell triggeredSpell)
        {
            var action = trigger.Type == GameActionMarkTypeEnum.GLYPH ? ActionsEnum.ACTION_FIGHT_TRIGGER_GLYPH : ActionsEnum.ACTION_FIGHT_TRIGGER_TRAP;
            client.Send(new GameActionFightTriggerGlyphTrapMessage((ushort)action, trigger.Caster.Id, trigger.Id, (ushort)target.Cell.Id, target.Id, (ushort)triggeredSpell.Id));
        }

        public static void SendGameFightTurnReadyRequestMessage(IPacketReceiver client, FightActor current)
        {
            client.Send(new GameFightTurnReadyRequestMessage(current.Id));
        }

        public static void SendSlaveSwitchContextMessage(IPacketReceiver client, SummonedFighter actor)
        {
            sbyte slotIndex = 0;
            client.Send(new SlaveSwitchContextMessage(actor.Summoner.Id, actor.Id, actor.Spells.Select(x => x.Value.GetSpellItem()).ToArray(),
                actor.GetSlaveCharacteristicsInformations(), actor.Spells.Select(x => new ShortcutSpell(slotIndex++, (ushort)x.Value.Template.Id)).ToArray()));
        }
        /// <summary>MYSTIC : One Window -> invocation d'un esclave controlee depuis la fenetre du maitre.</summary>
        public static void SendSlaveSwitchContextMessage(IPacketReceiver client, CharacterFighter master, SummonedFighter actor)
        {
            sbyte slotIndex = 0;
            client.Send(new SlaveSwitchContextMessage(master.Id, actor.Id, actor.Spells.Select(x => x.Value.GetSpellItem()).ToArray(),
                actor.GetSlaveCharacteristicsInformations(), actor.Spells.Select(x => new ShortcutSpell(slotIndex++, (ushort)x.Value.Template.Id)).ToArray()));
        }

        public static void SendSlaveSwitchContextMessage(IPacketReceiver client, CharacterFighter master, CharacterFighter slave)
        {
            var character = slave.Character;
            // Correctif Nekfeu : même liste que celle envoyée au compte du personnage (InventorySpellsHandler.SendSpellListMessage) :
            // seulement les sorts actifs (Position 63). GetSpells() envoyait aussi les variantes inactives → barre de sorts fausse.
            client.Send(new SlaveSwitchContextMessage(master.Id, slave.Id, GetOneWindowSlaveSpells(slave).Select(x => x.GetSpellItem()).ToArray(),
                character.GetCharacterCharacteristicsInformations(), character.Shortcuts.SpellsShortcuts.Values.Select(x => x.GetNetworkShortcut()).ToArray()));
            slave.OneWindowContextSent = true;
        }

        /// <summary>
        /// Correctif Nekfeu (one window) : les bonus de sort (portée, coût en PA, ligne de vue) ne sont pas envoyés comme
        /// buff de combat mais dans les caractéristiques du personnage, et seulement à sa propre fenêtre
        /// (ex. Karcham/Chamrak du Pandawa : +3/+5 PO pour jeter). Le client du maître garde les caractéristiques de
        /// l'esclave reçues au premier changement de contrôle → le sort restait à 1 case.
        /// Envoie au client du maître ce bonus comme buff de sort ciblant l'esclave : le client l'applique à ses
        /// caractéristiques, et le retire avec le GameActionFightDispellEffectMessage envoyé à tous à la fin du buff.
        /// </summary>
        public static void SendOneWindowSpellModification(SpellBuff buff)
        {
            if (buff.Delay != 0 || buff.BoostedSpell == null)
                return;

            var slave = buff.Target as CharacterFighter;
            var master = slave?.GetOneWindowMaster();
            if (master == null || !slave.OneWindowContextSent || master.Character.Client == null)
                return;

            ActionsEnum action;
            var delta = buff.Boost;
            switch (buff.Effect.EffectId)
            {
                case EffectsEnum.Effect_SpellRangeIncrease:
                    action = ActionsEnum.ACTION_BOOST_SPELL_RANGE;
                    break;
                case EffectsEnum.Effect_294:
                    action = ActionsEnum.ACTION_DEBOOST_SPELL_RANGE;
                    break;
                case EffectsEnum.Effect_ApCostReduce:
                    action = ActionsEnum.ACTION_BOOST_SPELL_AP_COST;
                    break;
                case EffectsEnum.Effect_SpellObstaclesDisable:
                    // plusieurs sorts d'un coup (Spellos) : non géré
                    if (buff.Spellos != null)
                        return;
                    action = ActionsEnum.ACTION_BOOST_SPELL_NOLINEOFSIGHT;
                    delta = 1;
                    break;
                default:
                    return;
            }

            master.Character.Client.Send(new GameActionFightDispellableEffectMessage((ushort)action, buff.Caster.Id,
                new FightTemporarySpellBoostEffect((uint)buff.Id, slave.Id, buff.Duration, (sbyte)buff.Dispellable, (ushort)buff.Spell.Id,
                    (uint)buff.Effect.Id, 0, delta, (ushort)buff.BoostedSpell.Id)));
        }

        /// <summary>
        /// Correctif Nekfeu (one window) : le client n'applique la relance globale d'un sort lancé par un allié
        /// qu'à son propre personnage et à ses invocations, jamais aux personnages joués en one window.
        /// Envoie au client du maître la relance globale restante de chaque sort de l'esclave (valeur absolue).
        /// À appeler pendant une séquence, une fois le SlaveSwitchContextMessage envoyé.
        /// </summary>
        /// <summary>
        /// Sorts d'un personnage joué en one window, identiques à ceux de sa propre barre (sorts actifs uniquement).
        /// </summary>
        static IEnumerable<Game.Spells.CharacterSpell> GetOneWindowSlaveSpells(CharacterFighter slave) =>
            slave.Character.Spells.GetPlayableSpells().Where(x => x.Record.Position == 63);

        public static void SendOneWindowGlobalCooldowns(IPacketReceiver client, CharacterFighter slave)
        {
            var round = slave.Fight.TimeLine.RoundNumber;

            foreach (var spell in GetOneWindowSlaveSpells(slave))
            {
                var level = spell.CurrentSpellLevel;
                if (level == null)
                    continue;

                // MYSTIC : relance initiale (sort pas disponible les premiers tours) : la fenetre du maitre ne la connait pas
                // pour un esclave → on l'envoie tant que le sort n'a jamais ete lance
                if (level.InitialCooldown > 0 && slave.SpellHistory.GetEntries(x => x.Spell.SpellId == level.SpellId).LastOrDefault() == null)
                {
                    var initial = slave.SpellHistory.GetSpellCooldown(level);
                    if (initial > 0)
                    {
                        client.Send(new GameActionFightSpellCooldownVariationMessage((ushort)ActionsEnum.ACTION_CHARACTER_ADD_SPELL_COOLDOWN,
                            slave.Id, slave.Id, (ushort)spell.Id, (short)initial));
                        continue;
                    }
                }

                if (level.GlobalCooldown == 0)
                    continue;

                var remaining = 0;
                foreach (var ally in slave.Team.Fighters.OfType<CharacterFighter>())
                {
                    if (ally == slave)
                        continue;

                    var entry = ally.SpellHistory.GetEntries(x => x.Spell.SpellId == level.SpellId).LastOrDefault();
                    if (entry == null)
                        continue;

                    var globalCooldown = entry.Spell.GlobalCooldown == -1
                        ? (int)(entry.Spell.MinCastInterval - entry.CharacterSpellDelayReduction)
                        : (int)(entry.Spell.GlobalCooldown - entry.CharacterSpellDelayReduction);

                    remaining = System.Math.Max(remaining, globalCooldown - entry.GetElapsedRounds(round));
                }

                if (remaining > 0)
                    client.Send(new GameActionFightSpellCooldownVariationMessage((ushort)ActionsEnum.ACTION_CHARACTER_ADD_SPELL_COOLDOWN,
                        slave.Id, slave.Id, (ushort)spell.Id, (short)remaining));
            }
        }

        public static void SendSlaveSwitchContextMessage(IPacketReceiver client, CompanionActor actor)
        {
            sbyte slotIndex = 0;
            client.Send(new SlaveSwitchContextMessage(actor.Master.Id, actor.Id, actor.CompanionSpell.Select(x => x.GetSpellItem()).ToArray(),
                actor.GetCompanionStatsMessage(), actor.CompanionSpell.Select(x => new ShortcutSpell(slotIndex++, (ushort)x.Template.Id)).ToArray()));
        }
        public static void SendGameFightResumeMessage(IPacketReceiver client, CharacterFighter fighter)
        {
            var slaves = fighter.Summons.Where(x => x.IsControlled());

            if (slaves.Any())
            {
                client.Send(new GameFightResumeWithSlavesMessage(
                    fighter.Fight.GetBuffs().Select(entry => entry.GetFightDispellableEffectExtendedInformations()).ToArray(),
                    fighter.Fight.GetTriggers().Select(entry => entry.GetGameActionMark(fighter)).ToArray(),
                    (ushort)fighter.Fight.TimeLine.RoundNumber,
                    !fighter.Fight.IsStarted ? 0 : fighter.Fight.StartTime.GetUnixTimeStamp(),
                    fighter.Fight.ActiveIdols.Select(x => x.GetNetworkIdol()).ToArray(),
                    fighter.SpellHistory.GetCooldowns(),
                    (sbyte)fighter.SummonedCount,
                    (sbyte)fighter.BombsCount,
                    slaves.Select(x => new GameFightResumeSlaveInfo(x.Id, x.SpellHistory.GetCooldowns(), (sbyte)x.SummonedCount, (sbyte)x.BombsCount)).ToArray()));
            }
            else
            {
                client.Send(new GameFightResumeMessage(
                    fighter.Fight.GetBuffs().Select(entry => entry.GetFightDispellableEffectExtendedInformations()).ToArray(),
                    fighter.Fight.GetTriggers().Select(entry => entry.GetGameActionMark(fighter)).ToArray(),
                    (ushort)fighter.Fight.TimeLine.RoundNumber,
                    !fighter.Fight.IsStarted ? 0 : fighter.Fight.StartTime.GetUnixTimeStamp(),
                    fighter.Fight.ActiveIdols.Select(x => x.GetNetworkIdol()).ToArray(),
                    fighter.SpellHistory.GetCooldowns(),
                    (sbyte)fighter.SummonedCount,
                    (sbyte)fighter.BombsCount));
            }
        }

        public static void SendGameActionFightKillMessage(IPacketReceiver client, FightActor source, FightActor target)
        {
            client.Send(new GameActionFightKillMessage((ushort)ActionsEnum.ACTION_CHARACTER_KILL, source.Id, target.Id));
        }
    }
}